module App

open Elmish
open Elmish.React
open Elmish.ReactNative
open Elmish.ReactNative.Expo
open Elmish.Navigation
open Fable.ReactNative
open NavigationSampleApp.Pages

type Model = {
    Navigation: Navigation.NavigationState }

type Msg =
    | Update of NavigationState

let init () = 
    { Navigation = Navigation.init }, []

let update msg model =
    match msg with
    | Update navigation ->
        { model with Navigation = navigation }, []

module R = Fable.ReactNative.Helpers
module P = Fable.ReactNative.Props
open Fable.ReactNative.Props

let view model dispatch page =
    R.view[] []

let pages = [
    "Home", Home.page ]

Program.mkSimpleWithNavigation init update view Msg.Update (fun model -> model.Navigation) pages
|> Program.withConsoleTrace
|> Program.withReactNativeExpo
|> Program.run