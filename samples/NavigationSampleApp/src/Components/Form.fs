namespace Calendar.Components

open Fable.ReactNative

type IValue =
    abstract member Value : unit -> 'a

type Input = {
    Value: IValue
    Label: string
    Errors: string list }

type ErrorTemplate<'name> = {
    Message: string
    Target: 'name }

type Form<'name, 'error when 'name: comparison> = {
    Fields: Map<'name, Input>
    OnErrors: ('error -> ('name * string)) }

type InputSettings<'name, 'value, 'error when 'name: comparison and 'error: comparison> = {
    Name: 'name
    Label: string
    Default: 'value }

module Form =
    let empty onError = {
        Fields = Map.empty
        OnErrors = onError }

    let private createValue value =
        { new IValue with  member __.Value() = (value :> obj) :?> 'a }

    let private updateInput name input form =
        let fields = form.Fields |> Map.add name input
        { form with Fields = fields }

    let getInput name form =
        form.Fields |> Map.find name

    let input settings (form: Form<_,_>) =
        let fields = form.Fields |> Map.add settings.Name {
            Value = createValue settings.Default
            Label = settings.Label
            Errors = [] }
        { form with Fields = fields }

    let getValue name form =
        let input = getInput name form
        input.Value.Value()

    let setValue name value form =
        let input = getInput name form
        let input = { input with Value = createValue value }
        updateInput name input form

    let applyErrors errors form =
        errors 
        |> List.fold (fun acc error -> 
            let target, message = form.OnErrors error
            let input = getInput target acc
            let input = { input with Errors = message::input.Errors }
            updateInput target input acc) form

    let resetErrors form =
        { form with
            Fields = form.Fields |> Map.fold (fun acc key input ->
                let input = { input with Errors = [] }
                acc |> Map.add key input) Map.empty }

module Props =
    type FormType<'name, 'error when 'name: comparison> = Form<'name, 'error>

    type Form<'name, 'error when 'name: comparison> =
        | Form of FormType<'name,'error>
        | OnChange of (FormType<'name,'error> -> unit)

    type FormInput<'name, 'error, 'value, 'element when 'name: comparison> =
        | Name of 'name
        | Render of ('value -> string list -> ('value -> unit) -> 'element)

module H = ComponentHelpers
module P = Props
module R = Helpers

module Helpers =
    let form props childrens =
        let form = props |> H.findProp (function P.Form form -> Some (Some form) | _ -> None) None
        match form with
        | None -> failwith "Form props must be provided"
        | Some form ->
            let onFormChange = props |> H.findProp (function P.OnChange onChange -> Some onChange | _ -> None) ignore
            let childrens = childrens |> List.map (fun child -> child onFormChange form)
            R.view [] childrens

    let formInput props onChange form =
        let name = props |> H.findProp (function Props.Name name -> Some (Some name) | _ -> None) None
        let render = props |> H.findProp (function Props.Render render -> Some (Some render) | _ -> None) None
        match name, render with
        | None, _ -> failwith "Input name must be provided"
        | _, None -> failwith "Render must be provided"
        | Some name, Some render ->
            let input = Form.getInput name form
            let onChange = (fun value -> form |> Form.setValue name value) >> onChange
            render (input.Value.Value()) input.Errors onChange