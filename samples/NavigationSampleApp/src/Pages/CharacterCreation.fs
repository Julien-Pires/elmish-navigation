namespace NavigationSampleApp.Pages

open Fable.React
open Fable.ReactNative
open Fable.ReactNative.Props
open Elmish.Navigation
open NavigationSampleApp

module CharacterCreation =
    module R = Fable.ReactNative.Helpers
    module P = Fable.ReactNative.Props

    type Model = {
        Characters: Character list }

    type Msg =
        | Cancel

    let init () = {
        Characters = [] }, []

    let update msg model =
        match msg with
        | Cancel -> model, NavigateBack |> Cmd.navigate

    let view model dispatch navigation =
        R.view [] [
             R.button [
                P.ButtonProperties.Title "Cancel"
                P.ButtonProperties.OnPress (fun _ -> dispatch Cancel)
            ] [] ]

    let page = Page<View, NavigationArgs>.Create(init, view, update)