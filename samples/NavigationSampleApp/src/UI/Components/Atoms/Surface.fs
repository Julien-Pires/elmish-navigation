namespace Calendar.UI.Components.Atoms

open Calendar.UI.Styles

module Components =
    module R = Fable.ReactNative.Helpers
    module P = Fable.ReactNative.Props

    let surface childrens =
        R.view [
            P.ViewProperties.Style Styles.roundedSurface ] childrens