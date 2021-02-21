module Fable.ReactNative.Helpers

open Fable.React
open Fable.Core
open Fable.Core.JsInterop
open Fable.ReactNative.Props

let inline SafeAreaView (props : IViewProperties list) (elems : ReactElement list) : ReactElement =
    ofImport "SafeAreaView" "react-native" (keyValueList CaseRules.LowerFirst props) elems