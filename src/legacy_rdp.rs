use std::io::{Read, Write};
use std::net::{TcpStream, ToSocketAddrs};
use std::time::Duration;

use anyhow::{Context, Result, bail};

use crate::models::ConnectionProfile;

const PROTOCOL_SSL: u32 = 0x0000_0001;
const PROTOCOL_HYBRID: u32 = 0x0000_0002;
const PROTOCOL_HYBRID_EX: u32 = 0x0000_0008;
const SSL_NOT_ALLOWED_BY_SERVER: u32 = 0x0000_0002;
const HYBRID_REQUIRED_BY_SERVER: u32 = 0x0000_0005;

#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum LegacySecurityMode {
    StandardRdp,
    Tls,
    CredSsp,
}

#[derive(Clone, Debug, PartialEq, Eq)]
pub struct LegacySecurityDetection {
    pub mode: LegacySecurityMode,
    pub detail: String,
}

pub fn detect_server_security(profile: &ConnectionProfile) -> Result<LegacySecurityDetection> {
    let response =
        send_negotiation_probe(profile, PROTOCOL_SSL | PROTOCOL_HYBRID | PROTOCOL_HYBRID_EX)
            .with_context(|| format!("legacy security probe {}:{}", profile.host, profile.port))?;
    let mode = parse_negotiation_response(&response)?;
    Ok(LegacySecurityDetection {
        mode,
        detail: describe_mode(mode).to_owned(),
    })
}

fn send_negotiation_probe(profile: &ConnectionProfile, protocols: u32) -> Result<Vec<u8>> {
    let server_addr = (profile.host.as_str(), profile.port)
        .to_socket_addrs()?
        .next()
        .context("socket address not found")?;
    let mut stream =
        TcpStream::connect_timeout(&server_addr, Duration::from_secs(8)).context("TCP connect")?;
    stream
        .set_read_timeout(Some(Duration::from_secs(5)))
        .context("set read timeout")?;
    stream
        .write_all(&build_negotiation_request(protocols))
        .context("write RDP negotiation request")?;

    let mut response = vec![0; 4096];
    let read = stream
        .read(&mut response)
        .context("read RDP negotiation response")?;
    response.truncate(read);
    Ok(response)
}

fn build_negotiation_request(protocols: u32) -> Vec<u8> {
    let mut packet = Vec::with_capacity(19);
    packet.extend_from_slice(&[0x03, 0x00, 0x00, 0x13]);
    packet.extend_from_slice(&[0x0e, 0xe0, 0x00, 0x00, 0x00, 0x00, 0x00]);
    packet.extend_from_slice(&[0x01, 0x00, 0x08, 0x00]);
    packet.extend_from_slice(&protocols.to_le_bytes());
    packet
}

pub fn parse_negotiation_response(packet: &[u8]) -> Result<LegacySecurityMode> {
    if packet.len() < 11 {
        bail!("RDP negotiation response is too short");
    }
    if packet[0] != 0x03 || packet[1] != 0x00 {
        bail!("RDP negotiation response is not a TPKT packet");
    }
    let tpkt_len = u16::from_be_bytes([packet[2], packet[3]]) as usize;
    if tpkt_len > packet.len() {
        bail!("RDP negotiation response length is incomplete");
    }
    let x224_len = packet[4] as usize;
    if 5 + x224_len > tpkt_len {
        bail!("RDP negotiation response has invalid X.224 length");
    }
    if packet[5] & 0xf0 != 0xd0 {
        bail!("RDP negotiation response is not an X.224 connection confirm");
    }

    let nego_offset = 11;
    if nego_offset == tpkt_len {
        return Ok(LegacySecurityMode::StandardRdp);
    }
    if nego_offset + 8 > tpkt_len {
        bail!("RDP negotiation response has truncated negotiation data");
    }

    let nego_type = packet[nego_offset];
    let nego_len = u16::from_le_bytes([packet[nego_offset + 2], packet[nego_offset + 3]]) as usize;
    if nego_len < 8 || nego_offset + nego_len > tpkt_len {
        bail!("RDP negotiation response has invalid negotiation data length");
    }
    let value = u32::from_le_bytes([
        packet[nego_offset + 4],
        packet[nego_offset + 5],
        packet[nego_offset + 6],
        packet[nego_offset + 7],
    ]);

    match nego_type {
        0x02 => selected_protocol_to_mode(value),
        0x03 => failure_code_to_mode(value),
        _ => bail!("RDP negotiation response has unknown negotiation type {nego_type}"),
    }
}

fn selected_protocol_to_mode(protocol: u32) -> Result<LegacySecurityMode> {
    if protocol & (PROTOCOL_HYBRID | PROTOCOL_HYBRID_EX) != 0 {
        Ok(LegacySecurityMode::CredSsp)
    } else if protocol & PROTOCOL_SSL != 0 {
        Ok(LegacySecurityMode::Tls)
    } else if protocol == 0 {
        Ok(LegacySecurityMode::StandardRdp)
    } else {
        bail!("RDP server selected unsupported protocol 0x{protocol:08x}")
    }
}

fn failure_code_to_mode(code: u32) -> Result<LegacySecurityMode> {
    match code {
        SSL_NOT_ALLOWED_BY_SERVER => Ok(LegacySecurityMode::StandardRdp),
        HYBRID_REQUIRED_BY_SERVER => Ok(LegacySecurityMode::CredSsp),
        _ => bail!("RDP negotiation failed with code 0x{code:08x}"),
    }
}

pub fn describe_mode(mode: LegacySecurityMode) -> &'static str {
    match mode {
        LegacySecurityMode::StandardRdp => "Standard RDP Security (legacy RC4, no TLS certificate)",
        LegacySecurityMode::Tls => "Enhanced RDP Security over TLS",
        LegacySecurityMode::CredSsp => "NLA/CredSSP",
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn parses_standard_rdp_security_failure_as_legacy_standard() {
        let failure_pdu = [
            0x03, 0x00, 0x00, 0x13, 0x0e, 0xd0, 0x00, 0x00, 0x12, 0x34, 0x00, 0x03, 0x00, 0x08,
            0x00, 0x02, 0x00, 0x00, 0x00,
        ];

        assert_eq!(
            parse_negotiation_response(&failure_pdu).unwrap(),
            LegacySecurityMode::StandardRdp
        );
    }

    #[test]
    fn parses_tls_response_as_enhanced_tls() {
        let response_pdu = [
            0x03, 0x00, 0x00, 0x13, 0x0e, 0xd0, 0x00, 0x00, 0x12, 0x34, 0x00, 0x02, 0x00, 0x08,
            0x00, 0x01, 0x00, 0x00, 0x00,
        ];

        assert_eq!(
            parse_negotiation_response(&response_pdu).unwrap(),
            LegacySecurityMode::Tls
        );
    }

    #[test]
    fn accepts_plain_x224_confirm_without_negotiation_as_standard_rdp() {
        let standard_confirm = [
            0x03, 0x00, 0x00, 0x0b, 0x06, 0xd0, 0x00, 0x00, 0x12, 0x34, 0x00,
        ];

        assert_eq!(
            parse_negotiation_response(&standard_confirm).unwrap(),
            LegacySecurityMode::StandardRdp
        );
    }
}
