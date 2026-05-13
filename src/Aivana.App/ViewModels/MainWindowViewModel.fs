namespace Aivana.App.ViewModels

open System.Collections.ObjectModel
open Aivana.App

type MainWindowViewModel() =
    inherit ViewModelBase()

    let navigationItems = [| "Connections"; "Health"; "Settings" |]
    let connections = ObservableCollection<ConnectionProfile>(Profiles.sample)
    let mutable selectedSection = "Connections"
    let mutable searchText = ""
    let mutable quickConnectHost = "rdp.prod.internal"
    let mutable quickConnectProtocol = "RDP"

    member _.NavigationItems = navigationItems
    member _.Connections = connections

    member this.SelectedSection
        with get () = selectedSection
        and set value =
            if this.SetProperty(&selectedSection, value) then
                this.OnPropertyChanged(nameof this.IsConnectionsSelected)
                this.OnPropertyChanged(nameof this.IsHealthSelected)
                this.OnPropertyChanged(nameof this.IsSettingsSelected)

    member this.SearchText
        with get () = searchText
        and set value =
            if this.SetProperty(&searchText, value) then
                this.OnPropertyChanged(nameof this.FilteredConnections)
                this.OnPropertyChanged(nameof this.VisibleConnectionCount)

    member this.QuickConnectHost
        with get () = quickConnectHost
        and set value = this.SetProperty(&quickConnectHost, value) |> ignore

    member this.QuickConnectProtocol
        with get () = quickConnectProtocol
        and set value = this.SetProperty(&quickConnectProtocol, value) |> ignore

    member _.FilteredConnections =
        connections |> Profiles.filter searchText

    member this.VisibleConnectionCount = this.FilteredConnections.Length

    member _.Summary =
        connections |> DashboardSummary.fromProfiles

    member _.IsConnectionsSelected = selectedSection = "Connections"
    member _.IsHealthSelected = selectedSection = "Health"
    member _.IsSettingsSelected = selectedSection = "Settings"
