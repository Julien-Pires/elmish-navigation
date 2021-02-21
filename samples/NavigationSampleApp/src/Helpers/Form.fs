namespace Calendar.Helpers

type IValue =
    abstract member Get : unit -> 'a

type Input = {
    Value: IValue
    Label: string
    Errors: string list }

type ErrorTemplate<'name> = {
    Message: string
    Target: 'name }

type Form<'name, 'error when 'name: comparison and 'error: comparison> = private {
    Fields: Map<'name, Input>
    Errors: Map<'error, ErrorTemplate<'name>> }

type InputSettings<'name, 'value, 'error when 'name: comparison and 'error: comparison> = {
    Name: 'name
    Label: string
    DefaultValue: 'value }

module Form =
    let empty = {
        Fields = Map.empty
        Errors = Map.empty }

    let private createValue value =
        { new IValue with  member __.Get() = (value :> obj) :?> 'a }

    let private updateInput name input form =
        let fields = form.Fields |> Map.add name input
        { form with Fields = fields }

    let private findError error form =
        form.Errors |> Map.find error

    let getInput name form =
        form.Fields |> Map.find name

    let addInput settings (form: Form<_,_>) =
        let fields = form.Fields |> Map.add settings.Name {
            Value = createValue settings.DefaultValue
            Label = settings.Label
            Errors = [] }
        { form with Fields = fields }

    let addErrors errors (form: Form<_,_>) = 
        let errors = errors |> List.fold (fun acc error ->
            let (key, target, message) = error
            let error = {
                Message = message
                Target = target }
            acc |> Map.add key error) form.Errors
        { form with Errors = errors }

    let setValue name value form =
        let input = getInput name form
        let input = { input with Value = createValue value }
        updateInput name input form

    let setErrors errors form =
        errors 
        |> List.fold (fun acc error -> 
            let error = findError error acc
            let input = getInput error.Target acc
            let input = { input with Errors = error.Message::input.Errors }
            updateInput error.Target input acc) form

    let resetErrors form =
        { form with
            Fields = form.Fields |> Map.fold (fun acc key input ->
                let input = { input with Errors = [] }
                acc |> Map.add key input) Map.empty }
