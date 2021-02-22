module App

open Elmish
open Elmish.React
open Elmish.ReactNative
open Elmish.ReactNative.Expo
open Elmish.Navigation
open Fable.ReactNative
open Calendar.Pages

type Model = {
    Navigation: Navigation.NavigationState }
    with 
        interface INavigationModel<Model> with
            member this.UpdateNavigation(state) = { this with Navigation = state }

let init () = 
    { Navigation = Navigation.init }, CmdMsg.Navigate "Home" |> Cmd.ofMsg

let update msg model =
    model, []

module R = Fable.ReactNative.Helpers
module P = Fable.ReactNative.Props
open Fable.ReactNative.Props
 
let view model dispatch page =
    R.SafeAreaView [] [
        page |> Option.defaultValue (R.view[][]) ]

let pages = [
    "Home", Home.page
    "AddEvent", AddEvent.page ]

Program.mkSimpleWithNavigation init update view (fun model -> model.Navigation) pages
|> Program.withConsoleTrace
|> Program.withReactNativeExpo
|> Program.run