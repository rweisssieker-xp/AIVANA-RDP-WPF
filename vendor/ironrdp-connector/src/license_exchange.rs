use core::fmt::Debug;
use core::panic::RefUnwindSafe;
use core::{fmt, mem};
use std::str;
use std::sync::Arc;

use ironrdp_core::WriteBuf;
use ironrdp_pdu::rdp::server_license::{self, LicenseInformation, LicensePdu, ServerLicenseError};
use ironrdp_pdu::PduHint;
use rand::RngCore as _;
use tracing::{debug, error, info, trace};

use super::{custom_err, general_err, legacy, ConnectorError, ConnectorErrorExt as _};
use crate::reason_err;
use crate::standard_security::{StandardSecurityContext, encode_send_data_request_raw};
use crate::{encode_send_data_request, ConnectorResult, ConnectorResultExt as _, Sequence, State, Written};

#[derive(Default, Debug)]
#[non_exhaustive]
#[cfg_attr(feature = "arbitrary", derive(arbitrary::Arbitrary))]
pub enum LicenseExchangeState {
    #[default]
    Consumed,

    NewLicenseRequest,
    PlatformChallenge {
        encryption_data: server_license::LicenseEncryptionData,
    },
    UpgradeLicense {
        encryption_data: server_license::LicenseEncryptionData,
    },
    LicenseExchanged,
}

impl State for LicenseExchangeState {
    fn name(&self) -> &'static str {
        match self {
            Self::Consumed => "Consumed",
            Self::NewLicenseRequest => "NewLicenseRequest",
            Self::PlatformChallenge { .. } => "PlatformChallenge",
            Self::UpgradeLicense { .. } => "UpgradeLicense",
            Self::LicenseExchanged => "LicenseExchanged",
        }
    }

    fn is_terminal(&self) -> bool {
        matches!(self, Self::LicenseExchanged)
    }

    fn as_any(&self) -> &dyn core::any::Any {
        self
    }
}

/// Client licensing sequence
///
/// Implements the state machine described in MS-RDPELE, section [3.1.5.3.1] Client State Transition.
///
/// [3.1.5.3.1]: https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-rdpele/8f9b860a-3687-401d-b3bc-7e9f5d4f7528
#[derive(Debug)]
#[cfg_attr(feature = "arbitrary", derive(arbitrary::Arbitrary))]
pub struct LicenseExchangeSequence {
    pub state: LicenseExchangeState,
    pub io_channel_id: u16,
    pub username: String,
    pub domain: Option<String>,
    pub hardware_id: [u32; 4],
    pub license_cache: Arc<dyn LicenseCache>,
}

// Use RefUnwindSafe so that types that embed LicenseCache remain UnwindSafe
pub trait LicenseCache: Sync + Send + Debug + RefUnwindSafe {
    fn get_license(&self, license_info: LicenseInformation) -> ConnectorResult<Option<Vec<u8>>>;
    fn store_license(&self, license_info: LicenseInformation) -> ConnectorResult<()>;
}

#[derive(Debug)]
pub(crate) struct NoopLicenseCache;

impl LicenseCache for NoopLicenseCache {
    fn get_license(&self, _license_info: LicenseInformation) -> ConnectorResult<Option<Vec<u8>>> {
        Ok(None)
    }

    fn store_license(&self, _license_info: LicenseInformation) -> ConnectorResult<()> {
        Ok(())
    }
}

impl LicenseExchangeSequence {
    pub fn new(
        io_channel_id: u16,
        username: String,
        domain: Option<String>,
        hardware_id: [u32; 4],
        license_cache: Arc<dyn LicenseCache>,
    ) -> Self {
        Self {
            state: LicenseExchangeState::NewLicenseRequest,
            io_channel_id,
            username,
            domain,
            hardware_id,
            license_cache,
        }
    }
}

impl Sequence for LicenseExchangeSequence {
    fn next_pdu_hint(&self) -> Option<&dyn PduHint> {
        match self.state {
            LicenseExchangeState::Consumed => None,
            LicenseExchangeState::NewLicenseRequest => Some(&ironrdp_pdu::X224_HINT),
            LicenseExchangeState::PlatformChallenge { .. } => Some(&ironrdp_pdu::X224_HINT),
            LicenseExchangeState::UpgradeLicense { .. } => Some(&ironrdp_pdu::X224_HINT),
            LicenseExchangeState::LicenseExchanged => None,
        }
    }

    fn state(&self) -> &dyn State {
        &self.state
    }

    fn step(&mut self, input: &[u8], output: &mut WriteBuf) -> ConnectorResult<Written> {
        self.step_inner(input, output, None)
    }
}

impl LicenseExchangeSequence {
    pub fn step_with_standard_security(
        &mut self,
        input: &[u8],
        output: &mut WriteBuf,
        standard_security: &mut StandardSecurityContext,
    ) -> ConnectorResult<Written> {
        self.step_inner(input, output, Some(standard_security))
    }

    fn step_inner(
        &mut self,
        input: &[u8],
        output: &mut WriteBuf,
        mut standard_security: Option<&mut StandardSecurityContext>,
    ) -> ConnectorResult<Written> {
        let (written, next_state) = match mem::take(&mut self.state) {
            LicenseExchangeState::Consumed => {
                return Err(general_err!(
                    "license exchange sequence state is consumed (this is a bug)",
                ))
            }

            LicenseExchangeState::NewLicenseRequest => {
                let send_data_indication_ctx = legacy::decode_send_data_indication(input)?;
                let decoded_user_data = decode_license_user_data(
                    &send_data_indication_ctx,
                    standard_security.as_deref_mut(),
                )?;
                let license_pdu = ironrdp_core::decode::<LicensePdu>(&decoded_user_data)
                    .map_err(ConnectorError::decode)
                    .map_err(|err| {
                        reason_err!(
                            "LicenseExchangeState::NewLicenseRequest",
                            "decode failed: {err}; raw_user_data={}; decoded_user_data={}",
                            preview_hex(send_data_indication_ctx.user_data),
                            preview_hex(&decoded_user_data)
                        )
                    })?;

                match license_pdu {
                    LicensePdu::ServerLicenseRequest(license_request) => {
                        if standard_security.is_some() && license_request.server_certificate.is_none() {
                            info!("Server license request did not include a certificate; continuing standard-security session");
                            let valid_client = server_license::LicensingErrorMessage::new_valid_client()
                                .map_err(ConnectorError::encode)?;
                            let written = encode_license_pdu(
                                send_data_indication_ctx.initiator_id,
                                send_data_indication_ctx.channel_id,
                                &valid_client.into(),
                                output,
                                standard_security.as_deref_mut(),
                            )?;
                            (
                                Written::from_size(written)?,
                                LicenseExchangeState::PlatformChallenge {
                                    encryption_data: server_license::LicenseEncryptionData::default(),
                                },
                            )
                        } else {
                        let mut rng = rand::rng();
                        let mut client_random = [0u8; server_license::RANDOM_NUMBER_SIZE];
                        rng.fill_bytes(&mut client_random);

                        let mut premaster_secret = [0u8; server_license::PREMASTER_SECRET_SIZE];
                        rng.fill_bytes(&mut premaster_secret);

                        let license_info = license_request
                            .scope_list
                            .iter()
                            .filter_map(|scope| {
                                self.license_cache
                                    .get_license(LicenseInformation {
                                        version: license_request.product_info.version,
                                        scope: scope.0.clone(),
                                        company_name: license_request.product_info.company_name.clone(),
                                        product_id: license_request.product_info.product_id.clone(),
                                        license_info: vec![],
                                    })
                                    .transpose()
                            })
                            .next()
                            .transpose()?;

                        if let Some(info) = license_info {
                            match server_license::ClientLicenseInfo::from_server_license_request(
                                &license_request,
                                &client_random,
                                &premaster_secret,
                                self.hardware_id,
                                info,
                            ) {
                                Ok((client_license_info, encryption_data)) => {
                                    trace!(?encryption_data, "Successfully generated Client License Info");
                                    trace!(message = ?client_license_info, "Send");

                                    let written = encode_license_pdu(
                                        send_data_indication_ctx.initiator_id,
                                        send_data_indication_ctx.channel_id,
                                        &client_license_info.into(),
                                        output,
                                        standard_security.as_deref_mut(),
                                    )?;

                                    trace!(?written, "Written ClientLicenseInfo");

                                    (
                                        Written::from_size(written)?,
                                        LicenseExchangeState::PlatformChallenge { encryption_data },
                                    )
                                }
                                Err(err) => {
                                    return Err(custom_err!("ClientNewLicenseRequest", err));
                                }
                            }
                        } else {
                            let hwid = self.hardware_id;
                            match server_license::ClientNewLicenseRequest::from_server_license_request(
                                &license_request,
                                &client_random,
                                &premaster_secret,
                                &self.username,
                                &format!("{:X}-{:X}-{:X}-{:X}", hwid[0], hwid[1], hwid[2], hwid[3]),
                            ) {
                                Ok((new_license_request, encryption_data)) => {
                                    trace!(?encryption_data, "Successfully generated Client New License Request");
                                    trace!(message = ?new_license_request, "Send");

                                    let written = encode_license_pdu(
                                        send_data_indication_ctx.initiator_id,
                                        send_data_indication_ctx.channel_id,
                                        &new_license_request.into(),
                                        output,
                                        standard_security.as_deref_mut(),
                                    )?;

                                    (
                                        Written::from_size(written)?,
                                        LicenseExchangeState::PlatformChallenge { encryption_data },
                                    )
                                }
                                Err(error) => {
                                    if let ServerLicenseError::InvalidX509Certificate {
                                        source: error,
                                        cert_der,
                                    } = &error
                                    {
                                        struct BytesHexFormatter<'a>(&'a [u8]);

                                        impl fmt::Display for BytesHexFormatter<'_> {
                                            fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
                                                write!(f, "0x")?;
                                                self.0.iter().try_for_each(|byte| write!(f, "{byte:02X}"))
                                            }
                                        }

                                        error!(
                                            %error,
                                            cert_der = %BytesHexFormatter(cert_der),
                                            "Unsupported or invalid X509 certificate received during license exchange step"
                                        );
                                    }

                                    return Err(custom_err!("ClientNewLicenseRequest", error));
                                }
                            }
                        }
                        }
                    }
                    LicensePdu::LicensingErrorMessage(error_message) => {
                        if error_message.error_code != server_license::LicenseErrorCode::StatusValidClient {
                            return Err(custom_err!(
                                "LicensingErrorMessage",
                                ServerLicenseError::from(error_message)
                            ));
                        }
                        info!("Server did not initiate license exchange");
                        (Written::Nothing, LicenseExchangeState::LicenseExchanged)
                    }
                    _ => {
                        return Err(general_err!(
                            "unexpected PDU received during LicenseExchangeState::NewLicenseRequest"
                        ));
                    }
                }
            }

            LicenseExchangeState::PlatformChallenge { encryption_data } => {
                let send_data_indication_ctx = legacy::decode_send_data_indication(input)?;

                let decoded_user_data = decode_license_user_data(
                    &send_data_indication_ctx,
                    standard_security.as_deref_mut(),
                )?;
                let license_pdu = ironrdp_core::decode::<LicensePdu>(&decoded_user_data)
                    .map_err(ConnectorError::decode)
                    .with_context("decode during LicenseExchangeState::PlatformChallenge")?;

                match license_pdu {
                    LicensePdu::ServerPlatformChallenge(challenge) => {
                        debug!(message = ?challenge, "Received");

                        let challenge_response =
                            server_license::ClientPlatformChallengeResponse::from_server_platform_challenge(
                                &challenge,
                                self.hardware_id,
                                &encryption_data,
                            )
                            .map_err(|e| custom_err!("ClientPlatformChallengeResponse", e))?;

                        debug!(message = ?challenge_response, "Send");

                        let written = encode_license_pdu(
                            send_data_indication_ctx.initiator_id,
                            send_data_indication_ctx.channel_id,
                            &challenge_response.into(),
                            output,
                            standard_security.as_deref_mut(),
                        )?;

                        (
                            Written::from_size(written)?,
                            LicenseExchangeState::UpgradeLicense { encryption_data },
                        )
                    }
                    LicensePdu::LicensingErrorMessage(error_message) => {
                        if error_message.error_code != server_license::LicenseErrorCode::StatusValidClient {
                            return Err(custom_err!(
                                "LicensingErrorMessage",
                                ServerLicenseError::from(error_message)
                            ));
                        }
                        debug!(message = ?error_message, "Received");
                        info!("Client licensing completed");
                        (Written::Nothing, LicenseExchangeState::LicenseExchanged)
                    }
                    _ => {
                        return Err(general_err!(
                            "unexpected PDU received during LicenseExchangeState::PlatformChallenge"
                        ));
                    }
                }
            }

            LicenseExchangeState::UpgradeLicense { encryption_data } => {
                let send_data_indication_ctx = legacy::decode_send_data_indication(input)?;

                let decoded_user_data = decode_license_user_data(
                    &send_data_indication_ctx,
                    standard_security.as_deref_mut(),
                )?;
                let license_pdu = ironrdp_core::decode::<LicensePdu>(&decoded_user_data)
                    .map_err(ConnectorError::decode)
                    .with_context("decode during SERVER_NEW_LICENSE/LicenseExchangeState::UpgradeLicense")?;

                match license_pdu {
                    LicensePdu::ServerUpgradeLicense(upgrade_license) => {
                        debug!(message = ?upgrade_license, "Received");

                        upgrade_license
                            .verify_server_license(&encryption_data)
                            .map_err(|e| custom_err!("license verification", e))?;

                        debug!("License verified with success");

                        let license_info = upgrade_license
                            .new_license_info(&encryption_data)
                            .map_err(ConnectorError::decode)?;

                        self.license_cache.store_license(license_info)?
                    }
                    LicensePdu::LicensingErrorMessage(error_message) => {
                        if error_message.error_code != server_license::LicenseErrorCode::StatusValidClient {
                            return Err(custom_err!(
                                "LicensingErrorMessage",
                                ServerLicenseError::from(error_message)
                            ));
                        }

                        debug!(message = ?error_message, "Received");
                        info!("Client licensing completed");
                    }
                    _ => {
                        return Err(general_err!(
                            "unexpected PDU received during LicenseExchangeState::UpgradeLicense"
                        ));
                    }
                }

                (Written::Nothing, LicenseExchangeState::LicenseExchanged)
            }

            LicenseExchangeState::LicenseExchanged => return Err(general_err!("license already exchanged")),
        };

        self.state = next_state;

        Ok(written)
    }
}

fn decode_license_user_data(
    ctx: &legacy::SendDataIndicationCtx<'_>,
    standard_security: Option<&mut StandardSecurityContext>,
) -> ConnectorResult<Vec<u8>> {
    if let Some(standard_security) = standard_security {
        standard_security.decrypt_user_data(ctx.user_data)
    } else {
        Ok(ctx.user_data.to_vec())
    }
}

fn encode_license_pdu(
    initiator_id: u16,
    channel_id: u16,
    license_pdu: &LicensePdu,
    output: &mut WriteBuf,
    standard_security: Option<&mut StandardSecurityContext>,
) -> ConnectorResult<usize> {
    if let Some(standard_security) = standard_security {
        let user_data = ironrdp_core::encode_vec(license_pdu).map_err(ConnectorError::encode)?;
        let encrypted = standard_security.encrypt_user_data(&user_data)?;
        encode_send_data_request_raw(initiator_id, channel_id, encrypted, output)
    } else {
        encode_send_data_request::<LicensePdu>(initiator_id, channel_id, license_pdu, output)
    }
}

fn preview_hex(bytes: &[u8]) -> String {
    bytes
        .iter()
        .take(16)
        .map(|byte| format!("{byte:02x}"))
        .collect::<Vec<_>>()
        .join(" ")
}
