namespace Calendar.UI.Design

open Calendar.UI.Styles

module Design =
    module R = Fable.ReactNative.Helpers
    module P = Fable.ReactNative.Props

    let background childrens =
        R.view [
            P.ViewProperties.Style Styles.pageBackground ] childrens