namespace Calendar.UI.Components

open Fable.ReactNative

[<RequireQualifiedAccess>]
module Label =
    module P = Props
    module R = Helpers

    let label text props =
        R.text [] text |> FormElements.Control

    let error id props =
        let render = fun text -> R.text [] text
        Error(render, id)