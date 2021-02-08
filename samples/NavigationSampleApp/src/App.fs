module App

open Elmish
open Elmish.React
open Elmish.ReactNative
open Elmish.ReactNative.Expo
open Elmish.Navigation
open Fable.ReactNative
open NavigationSampleApp.Pages

// A very simple app which increments a number when you press a button

type Model = {
    Navigation: Navigation.NavigationState }

type Message =
    | Increment
    | Decrement

let init () = 
    { Navigation = Navigation.init }, []

let updateNavigation model navigationState =
    { model with Navigation = navigationState }, []

let update msg model =
    match msg with
    | Decrement -> model, []
    | _ -> model, []

module R = Fable.ReactNative.Helpers
module P = Fable.ReactNative.Props
open Fable.ReactNative.Props

let view model dispatch navigation =
    match navigation.CurrentPage with
    | Some page ->
        R.view [] [
            page navigation.Dispatch ]
    | None ->
        R.view [] [
            R.button [
                P.ButtonProperties.Title "Press Me"
                P.ButtonProperties.OnPress (fun _ -> dispatch Decrement)
            ] [
                R.text [] "Press" ]
        ]


let pages = [
    "Home", Home.page
    "CharacterCreation", CharacterCreation.page ]

Program.mkProgram init update view
|> Program.withConsoleTrace
|> Program.withNavigation pages updateNavigation (fun model -> model.Navigation)
|> Program.withReactNativeExpo
|> Program.run