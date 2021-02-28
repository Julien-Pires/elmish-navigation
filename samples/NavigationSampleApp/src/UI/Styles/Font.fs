namespace Calendar.UI.Styles

module Font =
    module P = Fable.ReactNative.Props

    let private toStyleList = Seq.cast<P.IStyle> >> Seq.toList

    let private buildFontStyle 
        (families: {| Thin: string; Regular: string; Bold: string |}) 
        size = {| 
            Thin = 
                [ P.FontSize (unbox size); P.FontFamily (families.Thin) ] |> toStyleList
            Regular = [ P.FontSize (unbox size); P.FontFamily (families.Regular) ] |> toStyleList
            Bold = [ P.FontSize (unbox size); P.FontFamily (families.Bold) ] |> toStyleList |}

    let useFont families = {|
        H1 = buildFontStyle families 96
        H2 = buildFontStyle families 60
        H3 = buildFontStyle families 48
        H4 = buildFontStyle families 34
        H5 = buildFontStyle families 24
        H6 = buildFontStyle families 20
        Body1 = buildFontStyle families 18
        Body1 = buildFontStyle families 14
        Caption = buildFontStyle families 12 |}