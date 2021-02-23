namespace Calendar.UI.Elements

open Fable.Core
open Fable.Core.JsInterop
open Fable.React

module Props =
    type Checkbox =
        | IsChecked of bool
        | OnPress of (bool -> unit)

module Helpers =
    module R = Fable.ReactNative.Helpers
    module P = Fable.ReactNative.Props

    let inline checkbox (props : Props.Checkbox list) : ReactElement =
        ofImport "default" "react-native-bouncy-checkbox" (keyValueList CaseRules.LowerFirst props) []

