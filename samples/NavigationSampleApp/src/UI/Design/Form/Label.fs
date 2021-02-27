namespace Calendar.UI.Design

open Calendar.UI.Styles
open Calendar.UI.Components

module Design =
    let label text =
        Label.label text [
            Label.Style Styles.label ]

    let error id =
        Label.error id [
            Label.Style Styles.error ]