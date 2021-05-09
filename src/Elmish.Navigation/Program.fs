namespace Elmish.Navigation

open Elmish

[<RequireQualifiedAccess>]
module Program =
    module Internal = Navigation.Program

    let wrapInit defaultPage userInit args = 
        let (model, cmd) = userInit args
        Internal.init model, Cmd.batch [ Cmd.ofMsg (Navigate defaultPage |> Navigation)
                                         Cmd.map (App >> Message) cmd ]

    let wrapUpdate pages userUpdate msg model =
        Internal.update userUpdate pages msg model

    let wrapView pages navigator userView model dispatch =
        Internal.view userView pages model dispatch navigator

    let wrapSetState userSetState model dispatch =
        userSetState (model.Model) (App >> Message >> dispatch)

    let wrapSubscribe (navigator: Navigator<_,_>) userSubscribe model =
        let stream dispatch =
            navigator.Received
            |> Observable.subscribe (fun msg ->
                msg
                |> Navigation 
                |> dispatch)
            |> ignore
        let userCmds = userSubscribe (model.Model)
        Cmd.batch [ Cmd.ofSub stream
                    Cmd.map (App >> Message) userCmds ]

    let withNavigation pages defaultPage program =
        let pages =
            pages 
            |> List.map (fun (name, page) -> (PageName name, page))
            |> Map.ofList
        let navigator = Navigator()
        program |>
        Program.map
            (wrapInit defaultPage)
            (wrapUpdate pages)
            (wrapView pages navigator)
            wrapSetState
            (wrapSubscribe navigator)