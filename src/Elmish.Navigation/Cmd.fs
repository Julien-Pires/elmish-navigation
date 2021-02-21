namespace Elmish.Navigation

type CmdMsg =
    static member OfMsg (msg) = Message msg

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