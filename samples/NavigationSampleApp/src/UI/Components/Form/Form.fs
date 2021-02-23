namespace Calendar.UI.Components

open Fable.React
open Fable.ReactNative

type IValue =
    abstract member Value : unit -> 'a

type Form<'name when 'name: comparison> = {
    Fields: Map<'name, IValue> }

module Form =
    let empty = {
        Fields = Map.empty }

    let buildValue value =
        { new IValue with  member __.Value() = (value :> obj) :?> 'a }

    let getRawValue name form =
        form.Fields |> Map.tryFind name

    let getValue name defaultValue form =
        match form.Fields |> Map.tryFind name with
        | Some value -> value.Value()
        | None -> defaultValue

    let setValue name value form =
        let fields = form.Fields |> Map.add name value
        { form with Fields = fields }

module Props =
    type FormType<'name when 'name: comparison> = Form<'name>

    type Form<'name when 'name: comparison> =
        | Form of FormType<'name>
        | OnChange of (FormType<'name> -> unit)

module H = ComponentHelpers
module P = Props
module R = Helpers

type FormElements<'name, 'value when 'name: comparison> =
    | Control of ReactElement
    | Container of ((FormElements<'name, 'value> -> ReactElement) -> ReactElement)
    | Input of (IValue option -> (IValue -> unit) -> ReactElement) * 'name

module Helpers =
    let rec walkElement onChange form = function
        | Control control -> control
        | Container childrens -> childrens (walkElement onChange form)
        | Input (input, id) ->
            let value = form |> Form.getRawValue id
            input value (fun x -> form |> Form.setValue id x |> onChange)

    let form props childrens =
        let form = props |> ComponentHelpers.findProp (function Props.Form form -> Some form | _ -> None) Form.empty
        let onChange = props |> ComponentHelpers.findProp (function Props.OnChange onChange -> Some onChange | _ -> None) ignore
        R.view [] (childrens |> List.map (walkElement onChange form))