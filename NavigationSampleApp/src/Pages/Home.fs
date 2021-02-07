module UI.Pages.Home

open Fable.React
open Fable.ReactNative
open Fable.ReactNative.Props
open Elmish.Navigation
open NavigationSampleApp

module R = Fable.ReactNative.Helpers

type Model = {
    Characters: Character list }

let init () = {
    Characters = [] }, []

let update msg model = model, []

let view model dispatch navigation =
    R.view [] []

let page = Page<View, NavigationArgs>.Create(init, view, update)