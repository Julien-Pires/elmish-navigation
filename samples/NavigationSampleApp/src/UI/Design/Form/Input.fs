namespace Calendar.UI.Design

open Calendar.UI.Styles
open Calendar.UI.Components

module Design =
    let inputBlock id label input =
        Field.block [
            Field.Style Styles.field ] [
            Design.label label
            input
            Design.error id ]

    let textInput id label props =
        inputBlock id label (Input.text id [
            yield! props
            Input.Style Styles.textInput ])
    
    let textArea id label props = 
        inputBlock id label (Input.multiline id [
            yield! props
            Input.Text.Style Styles.textArea ])

    let switch id label props = 
        inputBlock id label (Input.switch id [
            yield! props ])

    let datePicker id label props = 
        inputBlock id label (Input.datetime id [
            yield! props ])
