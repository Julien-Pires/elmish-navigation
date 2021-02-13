namespace Elmish.Navigation

open Elmish

[<RequireQualifiedAccess>]
module Navigation =
    let (|Navigate|_|) = function
        | Navigate page -> Some(PageName page, None)
        | NavigateParams (page, parameters) -> Some(PageName page, parameters)
        | _ -> None

    let (|NavigateBack|_|) = function
        | NavigateBack -> Some None
        | NavigateBackParams parameters -> Some parameters
        | _ -> None

    let currentPage navigationState =
        match navigationState.Stack with
        | [] -> None
        | head::_ -> Some head

    let createNavigationParams parameters navigationState =
        match currentPage navigationState with
        | Some pageModel -> 
            { Source = Some (pageModel.Name)
              Parameters = parameters }
        | None ->
            { Source = None
              Parameters = parameters }

    let navigate name parameters navigationState pages =
        let page = pages |> Map.find name
        let (model, initCmd) = page.Init()
        let navigationParameters = createNavigationParams parameters navigationState
        let (model, navigateCmd) = page.OnNavigate model navigationParameters
        let pageModel = {
            Name = name
            Model = model }
        { navigationState with Stack = pageModel::navigationState.Stack }, 
        Cmd.batch [initCmd; navigateCmd] |> Cmd.map (Message.Upcast >> Message)

    let navigateBack parameters navigationState pages =
        match navigationState.Stack with
        | _::previousPages ->
            let (previousPages, cmd) =
                match previousPages with
                | previousPage::tail ->
                    let page = pages |> Map.find previousPage.Name
                    let navigationParameters = createNavigationParams parameters navigationState
                    let (model, navigateCmd) = page.OnNavigate previousPage.Model navigationParameters
                    let previousPage = { previousPage with Model = model }
                    previousPage::tail, navigateCmd
                | [] -> previousPages, []
            { navigationState with Stack = previousPages }, cmd |> Cmd.map (Message.Upcast >> Message)
        | [] -> { navigationState with Stack = [] }, []

    let updateMsg userUpdate msg model =
        match msg with
        | Message (Message.Message msg) ->
            userUpdate msg model
            ||> fun model cmd -> model, (cmd |> Cmd.map Effect) 
        | Effect (Message.Message msg) -> 
            userUpdate msg model
            ||> fun model cmd -> model, (cmd |> Cmd.map Message) 
        | _ -> model, []

    let updateCurrentPage page model pageModel navigationState cmd =
        match navigationState.Stack with
        | _::tail ->
            { navigationState with Stack = { page with Model = pageModel}::tail }
        | _ -> navigationState
        |> fun navigationState -> model, navigationState, cmd

    let updateNavigation pages msg model navigationState cmd =
        match msg with
        | Message (Navigation msg)
        | Effect (Navigation msg) ->
            match msg with
            | Navigate (page, parameters) -> Some (navigate page parameters)
            | NavigateBack parameters -> Some (navigateBack parameters)
            | _ -> None
            |> function
                | Some navigate ->
                    let (navigationState, cmd) = navigate navigationState pages
                    model, navigationState, cmd
                | None -> model, navigationState, cmd
        | _ -> model, navigationState, cmd

    let update userUpdate mapCommand navigationState pages msg model =
        match msg with
        | App msg ->
            updateMsg userUpdate msg model
            ||> fun model cmd -> model, navigationState, cmd
            |||> updateNavigation pages msg
            |||> fun model navigationState cmd -> model, navigationState, cmd |> Cmd.map App
        | Page msg ->
            match currentPage navigationState with
            | Some page ->
                let template = pages |> Map.find page.Name
                updateMsg template.Update (msg |> Command.Downcast<_,_>) page.Model
                ||> fun model cmd -> model, navigationState, cmd
                |||> updateCurrentPage page model
                |||> updateNavigation pages msg
                |||> fun model navigationState cmd -> model, navigationState, cmd |> Cmd.map (Command.Upcast<_> >> Page)
            | None -> model, navigationState, []

    let view dispatch navigationState pages = 
        match currentPage navigationState with
        | Some { Name = name; Model = model } ->
            let page = pages |> Map.find name
            Some (page.View model ((fun msg -> msg :?> 'a) >> Message.Message >> Message >> Page >> dispatch))
        | None -> None

    let init = { Stack = [] }

[<RequireQualifiedAccess>]
module Program =
    let init userInit () = 
        userInit() |> Cmd.map (Message >> App)

    let wrapUpdate userUpdate mapCommand getState updateState pages msg model =
        let navigationState = getState model
        let (model, navigationState, cmd) = Navigation.update userUpdate mapCommand navigationState pages msg model
        let (model, updateCmd) = updateState navigationState model
        model, Cmd.batch [cmd; updateCmd |> Cmd.map App]

    let wrapView getState pages userView model dispatch =
        let navigationState = getState model
        let currentPage = Navigation.view dispatch navigationState pages
        userView model (Message.Message >> Message >> App >> dispatch) currentPage

    let setState userSetState model dispatch =
        userSetState model (App >> dispatch)

    let subs userSubscribe model =
        userSubscribe model |> fun cmd -> cmd |> Cmd.map App

    let makeProgramWithNavigation init update view mapCommand updateState getState pages =
        let pages =
            pages
            |> List.map (fun (name, page) -> (PageName name, page))
            |> Map.ofList
        let update = wrapUpdate update mapCommand getState updateState pages
        let view = wrapView getState pages view
        Program.mkProgram init update view
