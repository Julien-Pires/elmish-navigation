namespace Calendar.UI.Components

open Fable.React
open Fable.ReactNative

type IValue =
    abstract member Value : unit -> 'a

type IInputState =
    abstract member Get: 'a -> 'b -> 'b
    abstract member Set: 'a -> 'b -> IInputState

type Form<'name, 'error when 'name: comparison> = {
    Fields: Map<'name, IValue>
    Errors: 'error list
    State: Map<'name, IInputState> }

module Form =
    let empty = {
        Fields = Map.empty
        Errors = []
        State = Map.empty }

    let buildValue value =
        { new IValue with  member __.Value() = (value :> obj) :?> 'a }

    let rec initState (state: Map<_, IValue>) =
        { new IInputState with
            member __.Get key defaultValue =
                let key = (key :> obj) :?> 'a
                match state |> Map.tryFind key with
                | Some x -> x.Value()
                | None -> defaultValue

            member __.Set key value =
                let key = (key :> obj) :?> 'a
                let value = buildValue value
                let newState = state |> Map.add key value
                initState newState }

    let getInputState id form =
        match form.State |> Map.tryFind id with
        | Some state -> state
        | None -> initState Map.empty

    let setInputState id state form =
        let state = form.State |> Map.add id state
        { form with State = state }

    let getRawValue name form =
        form.Fields |> Map.tryFind name

    let getValue name defaultValue form =
        match form.Fields |> Map.tryFind name with
        | Some value -> value.Value()
        | None -> defaultValue

    let setValue name value form =
        let fields = form.Fields |> Map.add name value
        { form with Fields = fields }

    let setErrors errors form =
        { form with Errors = errors }

module Props =
    type FormType<'name, 'error when 'name: comparison> = Form<'name, 'error>

    type Form<'name, 'error when 'name: comparison> =
        | Form of FormType<'name, 'error>
        | OnChange of (FormType<'name, 'error> -> unit)
        | Errors of ('name * ('error -> string option)) list

type FormElements<'name, 'value when 'name: comparison> =
    | Control of ReactElement
    | Container of ((FormElements<'name, 'value> -> ReactElement) -> ReactElement)
    | Input of (IValue option -> IInputState -> ((IValue * IInputState) -> unit) -> ReactElement) * 'name
    | Error of (string -> ReactElement) * 'name

module Helpers =
    module H = ComponentHelpers
    module R = Helpers

    let rec walkElement onChange errorHandlers form = function
        | Control control -> control
        | Container childrens -> childrens (walkElement onChange errorHandlers form)
        | Input (input, id) ->
            let value = form |> Form.getRawValue id
            let state = form |> Form.getInputState id
            input value state (fun (value, state) -> 
                form 
                |> Form.setInputState id state
                |> Form.setValue id value 
                |> onChange)
        | Error (label, labelId) ->
            errorHandlers
            |> List.tryPick (fun (id, handler) -> if labelId = id then Some handler else None)
            |> Option.bind(fun handler -> form.Errors |> List.tryPick handler)
            |> function
                | Some msg -> label msg
                | None -> label ""

    let form props childrens =
        let form = props |> H.findProp (function Props.Form form -> Some form | _ -> None) Form.empty
        let onChange = props |> H.findProp (function Props.OnChange onChange -> Some onChange | _ -> None) ignore
        let errorHandlers = props |> H.findProp (function Props.Errors handlers -> Some handlers | _ -> None) []
        R.view [] (childrens |> List.map (walkElement onChange errorHandlers form))