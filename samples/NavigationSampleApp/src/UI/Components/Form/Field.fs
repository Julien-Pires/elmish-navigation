namespace Calendar.UI.Components

open Fable.ReactNative
open Fable.ReactNative.Props

[<RequireQualifiedAccess>]
module Field =
    module P = Props
    module R = Helpers
    module H = ComponentHelpers

    type FieldProperties =
        | Style of IStyle list

    let block props childrens =
        fun walkChildren ->
            let elements = childrens |> List.map walkChildren
            let style = props |> H.findProp (function Style style -> Some style) []
            R.view [
                P.ViewProperties.Style style ] elements
        |> FormElements.Container