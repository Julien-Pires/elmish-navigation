namespace Calendar.UI.Elements

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.React

module Props =
    type DateTimePickerMode =
        | Date
        | Time
        | Datetime
        | Countdown

    type DateTimePickerDisplay =
        | Default
        | Spinner
        | Inline

    type DateTimePicker =
        | Mode of string
        | Display of string
        | Value of DateTime
        | OnChange of (string -> string -> unit)

module Helpers =
    module R = Fable.ReactNative.Helpers
    module P = Fable.ReactNative.Props  

    let inline dateTimePicker (props : Props.DateTimePicker list) : ReactElement =
        ofImport "default" "@react-native-community/datetimepicker" (keyValueList CaseRules.LowerFirst props) []