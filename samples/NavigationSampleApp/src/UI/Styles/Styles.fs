namespace Calendar.UI.Styles

open Fable.ReactNative
open Fable.ReactNative.Props

module Styles =
    module P = Fable.ReactNative.Props

    let private stylesheet styles = styles |> (Seq.cast<IStyle> >> List.ofSeq)

    let private font = 
        Font.useFont {|
                        Thin = "Poppins-thin"
                        Regular = "Poppins-regular"
                        Bold = "Poppins-bold" |}

    /// Layout Styles

    let pageBackground = stylesheet [
        P.BackgroundColor Theme.color.Primary ] @ [
            P.FlexStyle.Height (unbox "100%") ]

    let roundedSurface = stylesheet [
        P.BackgroundColor Theme.color.Surface
        P.BorderTopLeftRadius 46.
        P.BorderTopRightRadius 46. ] @ [
        P.FlexStyle.Flex 1.
        P.FlexStyle.PaddingLeft (unbox 36.)
        P.FlexStyle.PaddingRight (unbox 36.)
        P.FlexStyle.PaddingTop (unbox 40.) ]

    /// Form Syles

    let field = stylesheet [
        P.FlexStyle.MarginBottom (unbox 18.) ]

    let baseTextInput = stylesheet [
        P.BackgroundColor Theme.color.OnSurface.Light
        P.BorderRadius 12. ] @ [ 
        P.FlexStyle.Padding (unbox 18.) ] @ [
        P.TextStyle.Color Theme.color.OnSurface.Full ] @ font.Body1.Regular

    let textInput = baseTextInput

    let textArea = stylesheet baseTextInput @ [
        P.FlexStyle.Height (unbox 120.)
        P.FlexStyle.PaddingTop (unbox 23.) ]

    let label = stylesheet [
        P.TextStyle.Color Theme.color.Primary
        P.TextStyle.FontWeight FontWeight.Bold ] @ [
        P.FlexStyle.MarginBottom (unbox 18.) ] @ font.H6.Bold

    let error = stylesheet [
        P.TextStyle.Color Theme.color.Error ] @ font.Caption.Regular