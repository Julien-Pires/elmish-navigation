namespace NavigationSampleApp.Pages

open Fable.React
open Fable.ReactNative
open Fable.ReactNative.Props
open Elmish.Navigation
open NavigationSampleApp

module Home =
    module R = Fable.ReactNative.Helpers

    type Model = {
        Characters: Character list }

    let init () = {
        Characters = [] }, []

    let update msg model = model, []

    let view model dispatch navigation =
        R.view [] [
            R.text [] "BOU" ]

    let page = Page<View, NavigationArgs>.Create(init, view, update)