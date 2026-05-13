namespace Aivana.App

open System

type Protocol =
    | Rdp = 0
    | Ssh = 1
    | Vnc = 2

type ConnectionHealth =
    | Online = 0
    | Warning = 1
    | Offline = 2

[<CLIMutable>]
type ConnectionProfile =
    {
        Id: Guid
        Name: string
        Host: string
        Port: int
        Protocol: Protocol
        Username: string
        Group: string
        IsFavorite: bool
        Health: ConnectionHealth
        LastConnected: Nullable<DateTimeOffset>
        Tags: string array
    }

module ConnectionProfile =
    let create name host port protocol =
        {
            Id = Guid.NewGuid()
            Name = name
            Host = host
            Port = port
            Protocol = protocol
            Username = ""
            Group = "General"
            IsFavorite = false
            Health = ConnectionHealth.Offline
            LastConnected = Nullable()
            Tags = [||]
        }

module Profiles =
    let sample =
        [
            { ConnectionProfile.create "Production Gateway" "rdp.prod.internal" 3389 Protocol.Rdp with
                Username = "ops-admin"
                Group = "Production"
                IsFavorite = true
                Health = ConnectionHealth.Online
                LastConnected = Nullable(DateTimeOffset.Now.AddMinutes(-24.0))
                Tags = [| "rdp"; "critical" |] }
            { ConnectionProfile.create "Build Agent SSH" "build-07.internal" 22 Protocol.Ssh with
                Username = "builder"
                Group = "Engineering"
                Health = ConnectionHealth.Warning
                LastConnected = Nullable(DateTimeOffset.Now.AddHours(-3.0))
                Tags = [| "ssh"; "ci" |] }
            { ConnectionProfile.create "Support Lab VNC" "lab-vnc-02.local" 5900 Protocol.Vnc with
                Username = "support"
                Group = "Support"
                Health = ConnectionHealth.Offline
                Tags = [| "vnc"; "lab" |] }
        ]

    let private contains (needle: string) (value: string) =
        value.Contains(needle, StringComparison.OrdinalIgnoreCase)

    let filter (searchText: string) (profiles: seq<ConnectionProfile>) =
        let query = if isNull searchText then "" else searchText.Trim()

        profiles
        |> Seq.filter (fun profile ->
            String.IsNullOrWhiteSpace(query)
            || contains query profile.Name
            || contains query profile.Host
            || contains query profile.Group
            || profile.Tags |> Array.exists (contains query))
        |> Seq.sortBy (fun profile -> (if profile.IsFavorite then 0 else 1), profile.Name)
        |> Seq.toList

[<CLIMutable>]
type DashboardSummary =
    {
        TotalConnections: int
        OnlineConnections: int
        WarningConnections: int
        OfflineConnections: int
    }

module DashboardSummary =
    let fromProfiles (profiles: seq<ConnectionProfile>) =
        let items = profiles |> Seq.toList
        {
            TotalConnections = items.Length
            OnlineConnections = items |> List.filter (fun p -> p.Health = ConnectionHealth.Online) |> List.length
            WarningConnections = items |> List.filter (fun p -> p.Health = ConnectionHealth.Warning) |> List.length
            OfflineConnections = items |> List.filter (fun p -> p.Health = ConnectionHealth.Offline) |> List.length
        }
