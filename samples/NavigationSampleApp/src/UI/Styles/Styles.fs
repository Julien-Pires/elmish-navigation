namespace Calendar.UI.Styles

open Fable.ReactNative
open Fable.ReactNative.Props

module Styles =
    module P = Fable.ReactNative.Props

    let stylesheet styles = styles |> (Seq.cast<IStyle> >> List.ofSeq)

    let pageBackground = stylesheet [
        P.BackgroundColor Theme.color.Primary ] @ [
            P.FlexStyle.Height (unbox "100%") ]

    let roundedSurface = stylesheet [
        P.BackgroundColor Theme.color.Surface
        P.BorderRadius 46. ] @ [
        P.FlexStyle.Flex 1.
        P.FlexStyle.Padding (unbox 23.) ]

    let textInput = stylesheet [
        P.BackgroundColor Theme.color.OnSurface
        P.BorderRadius 6. ] @ [ 
        P.FlexStyle.Padding (unbox 12.) ]

    let label = stylesheet [
        P.TextStyle.Color Theme.color.Primary
        P.TextStyle.FontWeight FontWeight.Bold ] @ [
        P.FlexStyle.MarginBottom (unbox 18.) ]

    let error = stylesheet [
        P.TextStyle.Color Theme.color.Error ]