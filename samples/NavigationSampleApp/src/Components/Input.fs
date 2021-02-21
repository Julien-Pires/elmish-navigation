namespace Calendar.Components

module R = Fable.ReactNative.Helpers
module P = Fable.ReactNative.Props

module Props =
    type TextInputWithError =
        | Text of string
        | Label of string
        | Error of string list
        | OnChange of (string -> unit)

open Props

module Helpers =
    let findProp prop defaultValue props =
        match props |> List.tryPick prop with
        | Some x -> x
        | None -> defaultValue

    let textInputWithLabel props =
        let text = props |> findProp (function Text text -> Some text | _ -> None) ""
        let label = props |> findProp (function Label text -> Some (Some text) | _ -> None) None
        let errors = props |> findProp (function Error errors -> Some errors | _ -> None) []
        let onChange = props |> findProp (function OnChange change -> Some change | _ -> None) ignore
        R.view [] [
            if label.IsSome then
                R.text [] label.Value
            R.textInput [
                P.TextInput.Value text
                P.TextInput.OnChangeText onChange ]
            if errors.Length > 0 then
                yield! errors |> List.map (fun error -> R.text [] error) ]