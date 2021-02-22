namespace Calendar.Components

open Fable.Core
open Fable.Core.JsInterop
open Fable.React

module R = Fable.ReactNative.Helpers
module P = Fable.ReactNative.Props

module Props =
    type TextInputWithError =
        | Label of string
        | Error of string list

    type Checkbox =
        | IsChecked of bool
        | OnPress of (bool -> unit)

open Props

module Helpers =
    let findProp prop defaultValue props =
        match props |> List.tryPick prop with
        | Some x -> x
        | None -> defaultValue

    let inputBox props childrens =
        let label = props |> findProp (function Label text -> Some (Some text) | _ -> None) None
        let errors = props |> findProp (function Error errors -> Some errors | _ -> None) []
        R.view [] [
            if label.IsSome then
                R.text [] label.Value
            yield! childrens
            if errors.Length > 0 then
                yield! errors |> List.map (fun error -> R.text [] error) ]

    let inline checkbox (props : Props.Checkbox list) : ReactElement =
        ofImport "default" "react-native-bouncy-checkbox" (keyValueList CaseRules.LowerFirst props) []