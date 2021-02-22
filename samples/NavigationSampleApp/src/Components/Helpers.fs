namespace Calendar.Components

module ComponentHelpers =
    let findProp prop defaultValue props =
        match props |> List.tryPick prop with
        | Some x -> x
        | None -> defaultValue