namespace Calendar.UI.Styles

open Fable.ReactNative
open Fable.ReactNative.Props

module Styles =
    module P = Fable.ReactNative.Props

    let stylesheet = Seq.cast<IStyle> >> List.ofSeq

    let roundedSurface = stylesheet [
        P.BorderRadius 2.
        P.BackgroundColor Theme.color.Surface ] @ [
        P.Margin (unbox 6.) ]