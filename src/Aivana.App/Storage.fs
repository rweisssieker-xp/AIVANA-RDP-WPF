namespace Aivana.App

open System.IO
open System.Text.Json

module ProfileStorage =
    let private options =
        JsonSerializerOptions(WriteIndented = true)

    let save (filePath: string) (profiles: ConnectionProfile list) =
        let directory = Path.GetDirectoryName(filePath)
        if not (System.String.IsNullOrWhiteSpace(directory)) then
            Directory.CreateDirectory(directory) |> ignore

        let json = JsonSerializer.Serialize(profiles, options)
        File.WriteAllText(filePath, json)

    let load (filePath: string) =
        if File.Exists(filePath) then
            let json = File.ReadAllText(filePath)
            JsonSerializer.Deserialize<ConnectionProfile list>(json, options)
            |> Option.ofObj
            |> Option.defaultValue []
        else
            []
