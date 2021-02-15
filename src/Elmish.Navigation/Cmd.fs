namespace Elmish.Navigation

type CmdMsg =
    static member OfMsg (msg) = Message msg

    static member Navigate(page, ?args) =
        match args with
        | Some args -> NavigateParams (page, args)
        | None -> Navigate page
        |> Navigation

    static member NavigateBack(?args) = 
        match args with
        | Some args -> NavigateBackParams args
        | None -> NavigateBack
        |> Navigation