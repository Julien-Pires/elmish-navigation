module App

open Elmish
open Elmish.React
open Elmish.ReactNative
open Elmish.ReactNative.Expo
open Elmish.Navigation
open Fable.ReactNative

// A very simple app which increments a number when you press a button

type Model = {
    Navigation: Navigation.NavigationState }

type Message =
    | Increment 

let init () = { Navigation = Navigation.init }, Cmd.none

let updateNavigation model navigationState =
    { model with Navigation = navigationState }, []

let update msg model =
    model, Cmd.none 

module R = Fable.ReactNative.Helpers
module P = Fable.ReactNative.Props
open Fable.ReactNative.Props

let view model dispatch navigation =
    R.view [] []

let pages = []

Program.mkProgram init update view
|> Program.withConsoleTrace
|> Program.withNavigation pages updateNavigation (fun model -> model.Navigation)
|> Program.withReactNativeExpo
|> Program.run