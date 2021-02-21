namespace NavigationSampleApp.Pages

open System
open Elmish
open Fable.ReactNative
open Elmish.Navigation
open NavigationSampleApp
open Calendar.Helpers
open Calendar.Components
open Calendar.Modules

module AddEvent =
    module R = Helpers
    module P = Props

    type InputName =
        | Name
        | Description
        | Start
        | End

    type Model = {
        Form: Form<InputName, EventError> }

    type Msg =
        | Update of Form<InputName, EventError>
        | Validate

    let init () = {
        Form = 
            Form.empty
            |> Form.addInput {
                Name = Name
                Label = "Name"
                DefaultValue = "" }
            |> Form.addInput {
                Name = Description
                Label = "Description"
                DefaultValue = "" }
            |> Form.addInput {
                Name = Start
                Label = "Start date"
                DefaultValue = DateTime.Now }
            |> Form.addInput {
                Name = End
                Label = "End date"
                DefaultValue = DateTime.Now.AddHours(1.) }
            |> Form.addErrors [
                NameEmpty, Name, "Name cannot be empty"
                EndDateInvalid, End, "End date must be superior to start date" ]}, []

    let update msg model =
        match msg with
        | Update form -> { model with Form = form }, []
        | Validate ->
            let name = model.Form |> Form.getInput Name
            let description = model.Form |> Form.getInput Description
            let startDate = model.Form |> Form.getInput Start
            let result = Event.createEventA (name.Value.Get()) (description.Value.Get()) (startDate.Value.Get()) DateTime.Now false
            match result with
            | Ok event ->
                model, CmdMsg.NavigateBack(EventAdded event) |> Cmd.ofMsg
            | Errors errors ->
                let form = 
                    model.Form
                    |> Form.resetErrors
                    |> Form.setErrors errors
                { model with Form = form }, []

    let textInput (input: Input) onChange =
        R.textInputWithLabel [
            P.TextInputWithError.Label input.Label
            P.TextInputWithError.Text (input.Value.Get())
            P.TextInputWithError.OnChange onChange
            if input.Errors.Length > 0 then
                P.TextInputWithError.Error input.Errors ]

    let dateInput (input: Input) onChange =
        R.textInputWithLabel [
            P.TextInputWithError.Label input.Label
            P.TextInputWithError.Text (input.Value.Get().ToString())
            P.TextInputWithError.OnChange onChange
            if input.Errors.Length > 0 then
                P.TextInputWithError.Error input.Errors ]

    let view model dispatch =
        R.view [] [
            let name = model.Form |> Form.getInput Name
            textInput name (fun arg -> dispatch <| Update (model.Form |> Form.setValue Name arg))

            let description = model.Form |> Form.getInput Description
            textInput description (fun arg -> dispatch <| Update (model.Form |> Form.setValue Description arg))

            let startDate = model.Form |> Form.getInput Start
            dateInput startDate (fun arg -> dispatch <| Update (model.Form |> Form.setValue Start arg))

            R.button [
                P.ButtonProperties.Title "Add"
                P.ButtonProperties.OnPress (fun _ -> dispatch Validate) ] []]

    let page = Page.Create(init, view, update)