namespace Calendar.UI.Components

open System
open Fable.ReactNative
open Calendar.UI.Elements

[<RequireQualifiedAccess>]
module Input =
    module P = Props
    module R = Helpers
    module H = ComponentHelpers

    type Text =
        | Default of string
        | Style

    type Checkbox =
        | Default of bool
        | Style

    type DateTime =
        | Default of DateTimeOffset
        | Style

    let private buildText multiline id props =
        let render = fun (value: IValue option) onChange ->
            let defaultValue = props |> H.findProp (function Text.Default text -> Some text | _ -> None) ""
            let value = match value with Some x -> x.Value() | _ -> defaultValue
            R.textInput [
                P.TextInput.Value value
                P.TextInput.Multiline multiline
                P.TextInput.OnChangeText (Form.buildValue >> onChange) ]
        Input (render, id)

    let text id props = buildText false id props

    let multiline id props = buildText true id props

    let checkbox id props =
        let render = fun (value: IValue option) onChange ->
            let defaultValue = props |> H.findProp (function Checkbox.Default value -> Some value | _ -> None) false
            let value = match value with Some x -> x.Value() | _ -> defaultValue
            R.checkbox [
                P.Checkbox.IsChecked value
                P.Checkbox.OnPress (Form.buildValue >> onChange) ]
        Input (render, id)

    let datetime id props =
        let render = fun (value: IValue option) onChange ->
            let defaultValue = props |> H.findProp (function DateTime.Default text -> Some text | _ -> None) DateTimeOffset.Now
            let value = match value with Some x -> x.Value() | _ -> defaultValue
            R.textInput [
                P.TextInput.Value (value.ToString())
                P.TextInput.Multiline false
                P.TextInput.OnChangeText (Form.buildValue >> onChange) ]
        Input (render, id)