namespace Calendar.UI.Pages

open System
open Elmish
open Elmish.Navigation
open Fable.ReactNative
open Calendar.UI
open Calendar.UI.Components
open Calendar.Modules

module AddEvent =
    module R = Helpers
    module P = Props
    module D = Calendar.UI.Design.Design

    type InputName =
        | Title
        | Description
        | AllDay
        | Start
        | End

    type Model = {
        Form: Form<InputName, EventError> }

    type Msg =
        | Update of Form<InputName, EventError>
        | Validate

    let init () = {
        Form = Form.empty }, []

    let update msg model =
        match msg with
        | Update form -> { model with Form = form }, []
        | Validate ->
            let title = model.Form |> Form.getValue Title ""
            let description = model.Form |> Form.getValue Description ""
            let allDay = model.Form |> Form.getValue AllDay false
            let startDate = model.Form |> Form.getValue Start DateTimeOffset.Now
            let endDate = model.Form |> Form.getValue End (DateTimeOffset.Now.AddHours(1.))
            let result = Event.createEventA title description startDate endDate allDay
            match result with
            | Ok event ->
                model, CmdMsg.NavigateBack(EventAdded event) |> Cmd.ofMsg
            | Errors errors ->
                { model with Form = Form.setErrors errors model.Form }, []

    let view model dispatch =
        D.background [
            D.header [
                R.view [
                    P.ViewProperties.Style [ P.FlexStyle.Height (unbox 20.)]] []
            ]
            D.surface [
                D.SurfaceProperties.Scrollable true ] [
                R.form [
                    P.Form model.Form
                    P.OnChange (Update >> dispatch)
                    P.Errors[
                        Title, function NameEmpty -> Some "Name cannot be empty" | _ -> None
                        End, function EndDateInvalid -> Some "End date must be superior to start date" | _ -> None ] ] [
                    D.textInput Title "Title" [
                        Input.Placeholder "Write the title" ]
                    D.switch AllDay "AllDay" []
                    D.datePicker Start "Start Date" []
                    D.datePicker End "End Date" []
                    D.textArea Description "Description" [] ]

                R.button [
                    P.ButtonProperties.Title "Add"
                    P.ButtonProperties.OnPress (fun _ -> dispatch Validate) ] [] ]]

    let page = Page.Create(init, view, update)