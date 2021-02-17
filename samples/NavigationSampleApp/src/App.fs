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
    with 
        interface INavigationModel<Model> with
            member this.UpdateNavigation(state) = { this with Navigation = state }

type Msg =
    | No

let init () = 
    { Navigation = Navigation.init }, CmdMsg.Navigate "Home" |> Cmd.ofMsg

let update msg model =
    match msg with
    | No -> model, []

module R = Fable.ReactNative.Helpers
module P = Fable.ReactNative.Props
open Fable.ReactNative.Props
 
let view model dispatch page =
    R.view[
        P.ViewProperties.Style [
            Margin (unbox 200.)
        ]
    ] [
        page |> Option.defaultValue (R.view[][
            R.button [
                P.ButtonProperties.Title "Action"
                P.ButtonProperties.OnPress (fun _ -> dispatch No)
            ] []
        ]) ]

let pages = [
    "Home", Home.page ]

Program.mkSimpleWithNavigation init update view (fun model -> model.Navigation) pages
|> Program.withConsoleTrace
|> Program.withReactNativeExpo
|> Program.run