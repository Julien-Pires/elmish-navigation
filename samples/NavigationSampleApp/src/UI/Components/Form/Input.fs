namespace Calendar.UI.Components

open System
open Fable.ReactNative
open Fable.ReactNative.Props

[<RequireQualifiedAccess>]
module Input =
    module P = Props
    module R = Helpers
    module H = ComponentHelpers

    type Text =
        | Default of string
        | Style of IStyle list

    type Checkbox =
        | Default of bool
        | Style

    type DateTime =
        | Default of DateTimeOffset
        | Style

    type DateTimeState =
        | IsEditing

    let private buildText multiline id props =
        let render = fun (value: IValue option) state onChange ->
            let style = props |> H.findProp (function Text.Style style -> Some style | _ -> None) []
            let defaultValue = props |> H.findProp (function Text.Default text -> Some text | _ -> None) ""
            let value = match value with Some x -> x.Value() | _ -> defaultValue
            R.textInput [
                P.TextInput.Value value
                P.TextInput.Multiline multiline
                P.TextInput.OnChangeText ((fun arg -> Form.buildValue arg, state) >> onChange)
                P.TextInput.TextInputProperties.Style style ]
        Input (render, id)

    let text id props = buildText false id props

    let multiline id props = buildText true id props

    let switch id props =
        let render = fun (value: IValue option) state onChange ->
            let defaultValue = props |> H.findProp (function Checkbox.Default value -> Some value | _ -> None) false
            let value = match value with Some x -> x.Value() | _ -> defaultValue
            R.switch [
                P.SwitchProperties.Value value
                P.SwitchProperties.OnValueChange ((fun arg -> Form.buildValue arg, state) >> onChange) ]
        Input (render, id)

    let datetime id props =
        let render = fun (value: IValue option) (state: IInputState) onChange ->
            let isEditing = state.Get IsEditing false
            let defaultValue = props |> H.findProp (function DateTime.Default text -> Some text | _ -> None) DateTimeOffset.Now
            let value = match value with Some x -> x | _ -> Form.buildValue defaultValue
            R.view [] [
                R.touchableWithoutFeedback [
                    P.TouchableWithoutFeedbackProperties.OnPress (
                        fun _ -> value, state.Set IsEditing true
                        >> onChange) ] [
                    R.text [] (value.Value<DateTimeOffset>().ToString("yyyy-MM-dd")) ]

                if isEditing then
                    let date = value.Value<DateTimeOffset>().ToString("yyyy-MM-dd")
                    R.datePicker [
                        P.DatePicker.Current date
                        P.DatePicker.Selected date
                        P.DatePicker.OnSelectedChange (fun x ->
                            let oldDate = value.Value()
                            let newDate = DateTimeOffset.Parse(x)
                            match oldDate <> newDate with
                            | true -> (Form.buildValue newDate, state) |> onChange
                            | false -> ()) ] ]              
        Input (render, id)