namespace Calendar.Pages

open System
open Elmish
open Fable.ReactNative
open Elmish.Navigation
open Calendar.Modules

module Home =
    module R = Fable.ReactNative.Helpers
    module P = Fable.ReactNative.Props

    type Model = {
        Events: EventStore }

    type NavigationMsg =
        | AddEvent

    type Msg =
        | Move of NavigationMsg
        
    let init () = {
        Events = EventStore.empty }, []

    let onNavigate model args =
        match args.Parameters with
        | Some (EventAdded event) ->
            let store = model.Events |> EventStore.add event
            { model with Events = store }, []
        | None -> model, []

    let update msg model =
        match msg with
        | Move page ->
            match page with
            | AddEvent -> model, CmdMsg.Navigate "AddEvent" |> Cmd.ofMsg

    let view model dispatch =
        R.view [] [
            R.flatList ([| yield! model.Events.Days |]) [
                P.FlatListProperties.RenderItem (fun { item = item } ->
                    R.view [][
                        let date = DateTimeOffset.FromUnixTimeSeconds(item)
                        R.text [] (date.ToString("d MMM yyyy"))
                        R.view [] [
                            yield! model.Events.EventsDayMapping 
                            |> Map.find item
                            |> Seq.map (fun id ->
                                let event = model.Events.Events |> Map.find id
                                let (EventName name) = event.Name
                                R.text [] name) ]
                    ])]
            R.button [
                P.ButtonProperties.Title "Add Event"
                P.ButtonProperties.OnPress (fun _ -> dispatch <| Move AddEvent) ] [] ]

    let page: Page<Fable.React.ReactElement, NavigationEventArg> = Page.Create(init, view, update, onNavigate)