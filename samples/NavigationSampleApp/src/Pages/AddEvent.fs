namespace NavigationSampleApp.Pages

open System
open Elmish
open Elmish.Navigation
open Fable.ReactNative
open NavigationSampleApp
open Calendar.Components
open Calendar.Modules

module R = Helpers
module P = Props

module AddEvent =
    type InputName =
        | Name
        | Description
        | AllDay
        | Start
        | End

    type Model = {
        Form: Form<InputName, EventError> }

    type Msg =
        | Update of Form<InputName, EventError>
        | Validate

    let onError = function
        | NameEmpty -> (Name, "Name cannot be empty")
        | EndDateInvalid -> (End, "End date must be superior to start date")

    let init () = {
        Form =
            Form.empty onError
            |> Form.input {
                Name = Name
                Label = "Name"
                Default = "" }
            |> Form.input {
                Name = Description
                Label = "Description"
                Default = "" }
            |> Form.input {
                Name = AllDay
                Label = "All day"
                Default = false }
            |> Form.input {
                Name = Start
                Label = "Start date"
                Default = DateTime.Now }
            |> Form.input {
                Name = End
                Label = "End date"
                Default = DateTime.Now.AddHours(1.) }}, []

    let update msg model =
        match msg with
        | Update form -> { model with Form = form }, []
        | Validate ->
            let name = model.Form |> Form.getValue Name
            let description = model.Form |> Form.getValue Description
            let allDay = model.Form |> Form.getValue AllDay
            let startDate = model.Form |> Form.getValue Start
            let endDate = model.Form |> Form.getValue End
            let result = 
                Event.createEventA name description startDate endDate allDay
            match result with
            | Ok event ->
                model, CmdMsg.NavigateBack(EventAdded event) |> Cmd.ofMsg
            | Errors errors ->
                let form =
                    model.Form
                    |> Form.resetErrors
                    |> Form.applyErrors errors
                { model with Form = form }, []

    let inputBox label errors childrens =
        R.inputBox [
            P.TextInputWithError.Label label
            P.TextInputWithError.Error errors ] childrens

    let textInput label value errors onChange =
        inputBox label errors [
            R.textInput [
                P.TextInput.Value value
                P.TextInput.OnChangeText onChange  ]]

    let dateInput label value errors onChange =
        inputBox label errors [
            R.textInput [
                P.TextInput.Value value
                P.TextInput.OnChangeText onChange  ]]

    let checkBox label value errors onChange =
        inputBox label errors [
            R.checkbox [
                P.Checkbox.IsChecked value
                P.Checkbox.OnPress onChange ]]

    let view model dispatch =
        R.view [] [
            R.form [
                P.Form model.Form
                P.OnChange (Update >> dispatch) ] [
                R.formInput [
                    P.Name Name
                    P.Render (textInput "Name") ]
                R.formInput [
                    P.Name Description
                    P.Render (textInput "Description") ]
                R.formInput [
                    P.Name AllDay
                    P.Render (checkBox "All day") ]
                R.formInput [
                    P.Name Start
                    P.Render (dateInput "Start date") ]
                R.formInput [
                    P.Name End
                    P.Render (dateInput "End date") ]] 

            R.button [
                P.ButtonProperties.Title "Add"
                P.ButtonProperties.OnPress (fun _ -> dispatch Validate) ] []]

    let page = Page.Create(init, view, update)