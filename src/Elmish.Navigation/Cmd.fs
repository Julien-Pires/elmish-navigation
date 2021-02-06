[<RequireQualifiedAccess>]
module Cmd
    open Elmish

    let cast cmd =
        cmd |> Cmd.map (fun (msg: obj) -> msg :?> 'Msg)