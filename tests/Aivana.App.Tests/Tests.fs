module Aivana.App.Tests

open System
open System.IO
open Aivana.App
open Xunit

[<Fact>]
let ``new profile defaults to offline general profile`` () =
    let profile = ConnectionProfile.create "Test RDP" "server.local" 3389 Protocol.Rdp

    Assert.Equal("Test RDP", profile.Name)
    Assert.Equal("server.local", profile.Host)
    Assert.Equal(3389, profile.Port)
    Assert.Equal(Protocol.Rdp, profile.Protocol)
    Assert.Equal("General", profile.Group)
    Assert.Equal(ConnectionHealth.Offline, profile.Health)
    Assert.False(profile.IsFavorite)

[<Fact>]
let ``profile search matches tags and keeps favorites first`` () =
    let profiles =
        [
            { ConnectionProfile.create "Lab VNC" "lab.local" 5900 Protocol.Vnc with Tags = [| "support" |] }
            { ConnectionProfile.create "Critical RDP" "prod.local" 3389 Protocol.Rdp with IsFavorite = true; Tags = [| "critical" |] }
        ]

    let result = Profiles.filter "critical" profiles

    Assert.Single(result) |> ignore
    Assert.Equal("Critical RDP", result.Head.Name)

[<Fact>]
let ``profile storage round trips json`` () =
    let filePath = Path.Combine(Path.GetTempPath(), $"aivana-{Guid.NewGuid():N}.json")
    let profiles = [ { ConnectionProfile.create "SSH" "build.local" 22 Protocol.Ssh with Username = "builder" } ]

    try
        ProfileStorage.save filePath profiles
        let loaded = ProfileStorage.load filePath

        Assert.Single(loaded) |> ignore
        Assert.Equal("SSH", loaded.Head.Name)
        Assert.Equal("build.local", loaded.Head.Host)
        Assert.Equal(Protocol.Ssh, loaded.Head.Protocol)
        Assert.Equal("builder", loaded.Head.Username)
    finally
        if File.Exists(filePath) then
            File.Delete(filePath)
