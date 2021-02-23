namespace Calendar.UI.Components

open Fable.ReactNative

[<RequireQualifiedAccess>]
module Label =
    module P = Props
    module R = Helpers

    let label text =
        R.text [] text |> FormElements.Control

    let error text =
        R.text [] text |> FormElements.Control