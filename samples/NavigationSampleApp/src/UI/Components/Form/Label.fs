namespace Calendar.UI.Components

open Fable.ReactNative
open Fable.ReactNative.Props

[<RequireQualifiedAccess>]
module Label =
    module P = Props
    module R = Helpers
    module H = ComponentHelpers

    type Label =
        | Style of IStyle list

    type Error =
        | Style of IStyle list

    let label text props =
        let style = props |> H.findProp (function Label.Style style -> Some style) []
        R.text [ P.TextProperties.Style style ] text |> FormElements.Control

    let error id props =
        let render = fun text ->
            let style = props |> H.findProp (function Label.Style style -> Some style) []
            R.text [ P.TextProperties.Style style ] text
        Error(render, id)