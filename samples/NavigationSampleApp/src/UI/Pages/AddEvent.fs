namespace Calendar.UI.Pages

open System
open Elmish
open Elmish.Navigation
open Fable.ReactNative
open Calendar.UI
open Calendar.UI.Components
open Calendar.UI.Components.Atoms
open Calendar.Modules

module AddEvent =
    module R = Helpers
    module P = Props
    module C = Components

    type InputName =
        | Name
        | Description
        | AllDay
        | Start
        | End

    type Model = {
        Form: Form<InputName> }

    type Msg =
        | Update of Form<InputName>
        | Validate

    let onError = function
        | NameEmpty -> (Name, "Name cannot be empty")
        | EndDateInvalid -> (End, "End date must be superior to start date")

    let init () = {
        Form = Form.empty }, []

    let update msg model =
        match msg with
        | Update form -> { model with Form = form }, []
        | Validate ->
            let name = model.Form |> Form.getValue Name ""
            let description = model.Form |> Form.getValue Description ""
            let allDay = model.Form |> Form.getValue AllDay false
            let startDate = model.Form |> Form.getValue Start DateTimeOffset.Now
            let endDate = model.Form |> Form.getValue End DateTimeOffset.Now
            let result = Event.createEventA name description startDate endDate allDay
            match result with
            | Ok event ->
                model, CmdMsg.NavigateBack(EventAdded event) |> Cmd.ofMsg
            | Errors errors ->
                model, []

    let view model dispatch =
        C.surface [
            R.scrollView [] [
                R.form [
                    P.Form model.Form
                    P.OnChange (Update >> dispatch) ] [
                    Field.block [] [
                        Label.label "Name"
                        Input.text Name [] ]
                    Field.block [] [
                        Label.label "Description"
                        Input.multiline Description [] ]
                    Field.block [] [
                        Label.label "All day"
                        Input.checkbox AllDay [] ]
                    Field.block [] [
                        Label.label "Start date"
                        Input.datetime Start [] ]
                    Field.block [] [
                        Label.label "End date"
                        Input.datetime End [] ] ]

                R.button [
                    P.ButtonProperties.Title "Add"
                    P.ButtonProperties.OnPress (fun _ -> dispatch Validate) ] []] ]

    let page = Page.Create(init, view, update)