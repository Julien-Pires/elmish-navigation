namespace Calendar.UI.Components

open Fable.Core
open Fable.Core.JsInterop
open Fable.React

module Props =
    type DatePicker =
        | Mode of string
        | Selected of string
        | Current of string
        | OnSelectedChange of (string -> unit)

module Helpers =
    module R = Fable.ReactNative.Helpers
    module P = Fable.ReactNative.Props  

    let inline datePicker (props : Props.DatePicker list) : ReactElement =
        ofImport "default" "react-native-modern-datepicker" (keyValueList CaseRules.LowerFirst props) []