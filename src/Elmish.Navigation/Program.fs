namespace Elmish.Navigation

[<RequireQualifiedAccess>]
module Program =
    let mkProgramWithCmdMsg init update view mapCommand =
        let init = fun () ->
            init()
            ||> fun model cmd -> model, cmd |> List.map mapCommand
        let update = fun msg model ->
            update msg model
            ||> fun model cmd -> model, cmd |> List.map mapCommand
        Program.mkProgram init update view