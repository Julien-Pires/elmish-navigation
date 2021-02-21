namespace NavigationSampleApp.Pages

open Elmish
open Fable.ReactNative
open Elmish.Navigation
open NavigationSampleApp
open Calendar.Modules

module Home =
    module R = Fable.ReactNative.Helpers
    module P = Fable.ReactNative.Props

    type Day = Day of int

    type Events = {
        Days: Day list 
        Events: Map<Day, Event list> }

    type Model = {
        Events: Events }

    type NavigationMsg =
        | AddEvent

    type Msg =
        | Move of NavigationMsg

    let init () = {
        Events = {
            Days = []
            Events = Map.empty } }, []

    let onNavigate model args =
        match args.Parameters with
        | Some (EventAdded event) ->
            model, []
        | None -> model, []

    let update msg model =
        match msg with
        | Move page ->
            match page with
            | AddEvent -> model, CmdMsg.Navigate "AddEvent" |> Cmd.ofMsg

    let view model dispatch =
        R.view [] [
            R.flatList ([||]) [
                P.FlatListProperties.RenderItem (fun { item = item } ->
                    R.view [][])]
            R.button [
                P.ButtonProperties.Title "Add Event"
                P.ButtonProperties.OnPress (fun _ -> dispatch <| Move AddEvent) ] [] ]

    let page: Page<Fable.React.ReactElement, NavigationEventArg> = Page.Create(init, view, update, onNavigate)