use std::borrow::Cow;
use std::io;

use ironrdp_core::{Encode, WriteCursor, cast_length, decode};
use ironrdp_pdu::gcc::{EncryptionMethod, ServerSecurityData};
use ironrdp_pdu::mcs::{McsMessage, SendDataIndication, SendDataRequest};
use ironrdp_pdu::rdp::headers::{BasicSecurityHeader, BasicSecurityHeaderFlags};
use ironrdp_pdu::rdp::server_license::ServerCertificate;
use ironrdp_pdu::x224::X224;
use md5::Digest as _;
use num_bigint::BigUint;
use rand::RngCore as _;

use crate::{ConnectorError, ConnectorErrorExt as _, ConnectorResult, custom_err};

const RANDOM_SIZE: usize = 32;
const MAC_SIZE: usize = 8;

#[derive(Debug, Clone)]
pub struct StandardSecurityContext {
    mac_key: Vec<u8>,
    encrypt: Rc4,
    decrypt: Rc4,
}

impl StandardSecurityContext {
    pub fn new(server_security: &ServerSecurityData) -> ConnectorResult<(Self, SecurityExchangePdu)> {
        let server_random = server_security
            .server_random
            .ok_or_else(|| ConnectorError::general("standard security server random is missing"))?;
        let public_key = server_public_key(&server_security.server_cert)?;

        let mut client_random = [0u8; RANDOM_SIZE];
        rand::rng().fill_bytes(&mut client_random);
        let encrypted_client_random = encrypt_with_public_key(&client_random, &public_key)
            .map_err(|err| custom_err!("StandardSecurityExchange", err))?;

        let keys = session_keys(&client_random, &server_random, server_security.encryption_method);
        Ok((
            Self {
                mac_key: keys.mac_key,
                encrypt: Rc4::new(&keys.client_encrypt_key),
                decrypt: Rc4::new(&keys.server_encrypt_key),
            },
            SecurityExchangePdu {
                encrypted_client_random,
            },
        ))
    }

    pub fn decrypt_user_data(&mut self, user_data: &[u8]) -> ConnectorResult<Vec<u8>> {
        let (security_header, decrypted_payload) = self.decrypt_payload(user_data)?;
        if !security_header.flags.contains(BasicSecurityHeaderFlags::ENCRYPT) {
            return Ok(user_data.to_vec());
        }

        let mut decrypted = Vec::with_capacity(BasicSecurityHeader::FIXED_PART_SIZE + decrypted_payload.len());
        let mut flags = security_header.flags;
        flags.remove(BasicSecurityHeaderFlags::ENCRYPT);
        flags.remove(BasicSecurityHeaderFlags::SECURE_CHECKSUM);
        if flags.is_empty() {
            flags.insert(BasicSecurityHeaderFlags::LICENSE_PKT);
        }
        let header = BasicSecurityHeader { flags };
        decrypted.extend_from_slice(&ironrdp_core::encode_vec(&header).map_err(ConnectorError::encode)?);
        decrypted.extend_from_slice(&decrypted_payload);
        Ok(decrypted)
    }

    fn decrypt_plain_user_data(&mut self, user_data: &[u8]) -> ConnectorResult<Vec<u8>> {
        let (security_header, decrypted_payload) = self.decrypt_payload(user_data)?;
        if security_header.flags.contains(BasicSecurityHeaderFlags::ENCRYPT) {
            Ok(decrypted_payload)
        } else {
            Ok(user_data.to_vec())
        }
    }

    fn decrypt_payload(&mut self, user_data: &[u8]) -> ConnectorResult<(BasicSecurityHeader, Vec<u8>)> {
        if user_data.len() < BasicSecurityHeader::FIXED_PART_SIZE {
            return Ok((BasicSecurityHeader { flags: BasicSecurityHeaderFlags::empty() }, user_data.to_vec()));
        }

        let security_header = decode::<BasicSecurityHeader>(user_data).map_err(ConnectorError::decode)?;
        if !security_header.flags.contains(BasicSecurityHeaderFlags::ENCRYPT) {
            return Ok((security_header, user_data.to_vec()));
        }

        let encrypted_offset = BasicSecurityHeader::FIXED_PART_SIZE + MAC_SIZE;
        if user_data.len() < encrypted_offset {
            return Err(ConnectorError::general("encrypted standard security packet is truncated"));
        }

        let encrypted_payload = &user_data[encrypted_offset..];
        let decrypted_payload = self.decrypt.process(encrypted_payload);
        Ok((security_header, decrypted_payload))
    }

    pub fn encrypt_user_data(&mut self, user_data: &[u8]) -> ConnectorResult<Vec<u8>> {
        if user_data.len() < BasicSecurityHeader::FIXED_PART_SIZE {
            return Ok(user_data.to_vec());
        }

        let security_header = decode::<BasicSecurityHeader>(user_data).map_err(ConnectorError::decode)?;
        let payload = &user_data[BasicSecurityHeader::FIXED_PART_SIZE..];
        let mac = salted_mac(&self.mac_key, payload);
        let encrypted_payload = self.encrypt.process(payload);

        let mut encrypted = Vec::with_capacity(BasicSecurityHeader::FIXED_PART_SIZE + MAC_SIZE + encrypted_payload.len());
        let header = BasicSecurityHeader {
            flags: security_header.flags | BasicSecurityHeaderFlags::ENCRYPT,
        };
        encrypted.extend_from_slice(&ironrdp_core::encode_vec(&header).map_err(ConnectorError::encode)?);
        encrypted.extend_from_slice(&mac[..MAC_SIZE]);
        encrypted.extend_from_slice(&encrypted_payload);
        Ok(encrypted)
    }

    fn encrypt_plain_user_data(&mut self, user_data: &[u8]) -> ConnectorResult<Vec<u8>> {
        let mac = salted_mac(&self.mac_key, user_data);
        let encrypted_payload = self.encrypt.process(user_data);
        let mut encrypted = Vec::with_capacity(BasicSecurityHeader::FIXED_PART_SIZE + MAC_SIZE + encrypted_payload.len());
        let header = BasicSecurityHeader {
            flags: BasicSecurityHeaderFlags::ENCRYPT | BasicSecurityHeaderFlags::SECURE_CHECKSUM,
        };
        encrypted.extend_from_slice(&ironrdp_core::encode_vec(&header).map_err(ConnectorError::encode)?);
        encrypted.extend_from_slice(&mac[..MAC_SIZE]);
        encrypted.extend_from_slice(&encrypted_payload);
        Ok(encrypted)
    }

    pub fn decrypt_server_frame(&mut self, input: &[u8]) -> ConnectorResult<Vec<u8>> {
        let mcs_msg = decode::<X224<McsMessage<'_>>>(input).map_err(ConnectorError::decode)?;
        match mcs_msg.0 {
            McsMessage::SendDataIndication(msg) => {
                let decrypted = self.decrypt_plain_user_data(&msg.user_data)?;
                let pdu = SendDataIndication {
                    initiator_id: msg.initiator_id,
                    channel_id: msg.channel_id,
                    user_data: Cow::Owned(decrypted),
                };
                ironrdp_core::encode_vec(&X224(pdu)).map_err(ConnectorError::encode)
            }
            _ => Ok(input.to_vec()),
        }
    }

    pub fn encrypt_client_frame(&mut self, input: &[u8]) -> ConnectorResult<Vec<u8>> {
        let mcs_msg = decode::<X224<McsMessage<'_>>>(input).map_err(ConnectorError::decode)?;
        match mcs_msg.0 {
            McsMessage::SendDataRequest(msg) => {
                let encrypted = self.encrypt_plain_user_data(&msg.user_data)?;
                let pdu = SendDataRequest {
                    initiator_id: msg.initiator_id,
                    channel_id: msg.channel_id,
                    user_data: Cow::Owned(encrypted),
                };
                ironrdp_core::encode_vec(&X224(pdu)).map_err(ConnectorError::encode)
            }
            _ => Ok(input.to_vec()),
        }
    }
}




#[derive(Debug, Clone)]
pub struct SecurityExchangePdu {
    encrypted_client_random: Vec<u8>,
}

impl Encode for SecurityExchangePdu {
    fn encode(&self, dst: &mut WriteCursor<'_>) -> ironrdp_core::EncodeResult<()> {
        let header = BasicSecurityHeader {
            flags: BasicSecurityHeaderFlags::EXCHANGE_PKT | BasicSecurityHeaderFlags::LICENSE_ENCRYPT_SC,
        };
        header.encode(dst)?;
        dst.write_u32(cast_length!(
            "encryptedClientRandomLen",
            self.encrypted_client_random.len()
        )?);
        dst.write_slice(&self.encrypted_client_random);
        Ok(())
    }

    fn name(&self) -> &'static str {
        "SecurityExchangePdu"
    }

    fn size(&self) -> usize {
        BasicSecurityHeader::FIXED_PART_SIZE + 4 + self.encrypted_client_random.len()
    }
}

pub(crate) fn encode_send_data_request_raw(
    initiator_id: u16,
    channel_id: u16,
    user_data: Vec<u8>,
    buf: &mut ironrdp_core::WriteBuf,
) -> ConnectorResult<usize> {
    let pdu = SendDataRequest {
        initiator_id,
        channel_id,
        user_data: Cow::Owned(user_data),
    };
    let written = ironrdp_core::encode_buf(&X224(pdu), buf).map_err(ConnectorError::encode)?;
    Ok(written)
}

fn server_public_key(server_cert: &[u8]) -> ConnectorResult<Vec<u8>> {
    let certificate = decode::<ServerCertificate>(server_cert).map_err(ConnectorError::decode)?;
    certificate
        .get_public_key()
        .map_err(|err| custom_err!("StandardSecurityCertificate", err))
}

struct SessionKeys {
    mac_key: Vec<u8>,
    client_encrypt_key: Vec<u8>,
    server_encrypt_key: Vec<u8>,
}

fn session_keys(
    client_random: &[u8; RANDOM_SIZE],
    server_random: &[u8; RANDOM_SIZE],
    encryption_method: EncryptionMethod,
) -> SessionKeys {
    let premaster_secret = [&client_random[..24], &server_random[..24]].concat();
    let master_secret = salted_hash48(&premaster_secret, client_random, server_random, [b"A", b"BB", b"CCC"]);
    let session_key_blob = salted_hash48(&master_secret, client_random, server_random, [b"X", b"YY", b"ZZZ"]);
    let mac_key = session_key_blob[..16].to_vec();
    let server_key_128 = final_hash(&session_key_blob[16..32], client_random, server_random);
    let client_key_128 = final_hash(&session_key_blob[32..48], client_random, server_random);
    let key_len = if encryption_method.contains(EncryptionMethod::BIT_40) {
        8
    } else if encryption_method.contains(EncryptionMethod::BIT_56) {
        8
    } else {
        16
    };
    SessionKeys {
        mac_key,
        client_encrypt_key: client_key_128[..key_len].to_vec(),
        server_encrypt_key: server_key_128[..key_len].to_vec(),
    }
}

fn salted_hash48(
    secret: &[u8],
    random_first: &[u8],
    random_second: &[u8],
    salts: [&[u8]; 3],
) -> Vec<u8> {
    [
        salted_hash(secret, random_first, random_second, salts[0]),
        salted_hash(secret, random_first, random_second, salts[1]),
        salted_hash(secret, random_first, random_second, salts[2]),
    ]
    .concat()
}

fn salted_hash(secret: &[u8], random_first: &[u8], random_second: &[u8], salt: &[u8]) -> Vec<u8> {
    let mut sha = sha1::Sha1::new();
    sha.update([salt, secret, random_first, random_second].concat());
    let sha_result = sha.finalize();

    let mut md5 = md5::Md5::new();
    md5.update([secret, sha_result.as_ref()].concat());
    md5.finalize().to_vec()
}

fn final_hash(key: &[u8], client_random: &[u8], server_random: &[u8]) -> Vec<u8> {
    let mut md5 = md5::Md5::new();
    md5.update([key, client_random, server_random].concat());
    md5.finalize().to_vec()
}

fn salted_mac(mac_key: &[u8], data: &[u8]) -> Vec<u8> {
    let data_len = u32::try_from(data.len()).unwrap_or(u32::MAX).to_le_bytes();
    let pad_one = [0x36u8; 40];
    let pad_two = [0x5cu8; 48];

    let mut sha = sha1::Sha1::new();
    sha.update([mac_key, pad_one.as_ref(), data_len.as_ref(), data].concat());
    let sha_result = sha.finalize();

    let mut md5 = md5::Md5::new();
    md5.update([mac_key, pad_two.as_ref(), sha_result.as_ref()].concat());
    md5.finalize().to_vec()
}

fn encrypt_with_public_key(message: &[u8], public_key_der: &[u8]) -> io::Result<Vec<u8>> {
    use der_parser::parse_der;

    let (_, der_object) = parse_der(public_key_der).map_err(|err| {
        io::Error::new(
            io::ErrorKind::InvalidData,
            format!("unable to parse public key from DER: {err:?}"),
        )
    })?;
    let der_object_sequence = der_object.as_sequence().map_err(|err| {
        io::Error::new(
            io::ErrorKind::InvalidData,
            format!("unable to extract public key sequence: {err:?}"),
        )
    })?;
    if der_object_sequence.len() != 2 {
        return Err(io::Error::new(io::ErrorKind::InvalidData, "invalid RSA public key"));
    }

    let n = der_object_sequence[0].as_slice().map_err(|err| {
        io::Error::new(io::ErrorKind::InvalidData, format!("invalid RSA modulus: {err:?}"))
    })?;
    let e = der_object_sequence[1].as_slice().map_err(|err| {
        io::Error::new(io::ErrorKind::InvalidData, format!("invalid RSA exponent: {err:?}"))
    })?;

    let n = BigUint::from_bytes_be(n);
    let e = BigUint::from_bytes_be(e);
    let m = BigUint::from_bytes_le(message);
    let c = m.modpow(&e, &n);

    let mut result = c.to_bytes_le();
    result.resize(result.len() + 8, 0u8);
    Ok(result)
}

#[derive(Debug, Clone)]
struct Rc4 {
    i: usize,
    j: usize,
    state: [u8; 256],
}

impl Rc4 {
    fn new(key: &[u8]) -> Self {
        let mut state = [0u8; 256];
        for (idx, item) in state.iter_mut().enumerate() {
            *item = idx as u8;
        }
        let mut j = 0usize;
        for i in 0..256 {
            j = (j + usize::from(state[i]) + usize::from(key[i % key.len()])) % 256;
            state.swap(i, j);
        }
        Self { i: 0, j: 0, state }
    }

    fn process(&mut self, message: &[u8]) -> Vec<u8> {
        let mut output = Vec::with_capacity(message.len());
        for byte in message {
            self.i = (self.i + 1) % 256;
            self.j = (self.j + usize::from(self.state[self.i])) % 256;
            self.state.swap(self.i, self.j);
            let idx = (usize::from(self.state[self.i]) + usize::from(self.state[self.j])) % 256;
            output.push(self.state[idx] ^ byte);
        }
        output
    }
}
