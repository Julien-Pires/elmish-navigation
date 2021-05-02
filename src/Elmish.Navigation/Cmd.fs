namespace Elmish.Navigation

open Elmish

[<RequireQualifiedAccess>]
type CmdMsg =
    static member Navigate(page, ?args) =
        match args with
        | Some args -> NavigateParams (page, Some args)
        | None -> Navigate page
        |> Navigation

    static member NavigateBack(?args) = 
        match args with
        | Some args -> NavigateBackParams (Some args)
        | None -> NavigateBack
        |> Navigation

[<RequireQualifiedAccess>]
type Cmd =
    static member Navigate(page, ?args) = CmdMsg.Navigate(page, args) |> Cmd.ofMsg

    static member NavigateBack(?args) = CmdMsg.NavigateBack(args) |> Cmd.ofMsg