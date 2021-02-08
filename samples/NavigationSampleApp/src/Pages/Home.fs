namespace NavigationSampleApp.Pages

open Fable.ReactNative
open Elmish.Navigation
open NavigationSampleApp

module Home =
    module R = Fable.ReactNative.Helpers
    module P = Fable.ReactNative.Props

    type Model = {
        Characters: Character list }

    type Msg =
        | Create

    let init () = {
        Characters = [] }, []

    let update msg model =
        match msg with
        | Create -> model, Navigate "CharacterCreation" |> Cmd.navigate

    let view model dispatch navigation =
        R.view [] [
            R.button [
                P.ButtonProperties.Title "Next"
                P.ButtonProperties.OnPress (fun _ -> dispatch Create)
            ] [] ]

    let page = Page<View, NavigationArgs>.Create(init, view, update)