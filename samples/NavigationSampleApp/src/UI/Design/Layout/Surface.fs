namespace Calendar.UI.Design

open Calendar.UI.Styles

module Design =
    module R = Fable.ReactNative.Helpers
    module P = Fable.ReactNative.Props
    module H = Calendar.UI.Components.ComponentHelpers

    type SurfaceProperties =
        | Scrollable of bool

    let surface props childrens =
        let isScrollable = props |> H.findProp (function Scrollable scrollable -> Some scrollable) false
        R.view [
            P.ViewProperties.Style Styles.roundedSurface ] [
                if isScrollable then
                    R.scrollView [] childrens
                else
                    yield! childrens ]