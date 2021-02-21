namespace Calendar.Modules

type Result<'a, 'b> =
    | Ok of 'a
    | Errors of 'b list

module Result =
    let map f x =
        match x with
        | Ok value ->
            Ok (f value)
        | Errors errors ->
            Errors errors

    let bind f x =
        match x with
        | Ok value ->
            f value
        | Errors errors ->
            Errors errors

    let apply fResult xResult =
        match fResult, xResult with
        | Ok f, Ok x -> Ok (f x)
        | Ok _, Errors errors -> Errors errors
        | Errors errors, Ok _ -> Errors errors
        | Errors errors1, Errors errors2 -> Errors (errors1@errors2)

module ResultSymbols =
    let (<!>) = Result.map
    let (<*>) = Result.apply