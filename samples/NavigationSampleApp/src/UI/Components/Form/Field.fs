namespace Calendar.UI.Components

open Fable.ReactNative

[<RequireQualifiedAccess>]
module Field =
    module P = Props
    module R = Helpers

    let block props childrens =
        fun f ->
            let elements = childrens |> List.map f
            R.view [] elements
        |> FormElements.Container