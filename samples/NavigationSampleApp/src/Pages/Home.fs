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
        | Create -> model, []

    let view model dispatch =
        R.view [] [
            R.button [
                P.ButtonProperties.Title "Next"
                P.ButtonProperties.OnPress (fun _ -> dispatch Create)
            ] [] ]

    let page: Page<Fable.React.ReactElement, obj> = Page.Create(init, view, update)