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
        let navigationParameters = createNavigationParams parameters navigationState
        let (model, initCmd) = page.Init()
        let (model, navigateCmd) = page.OnNavigate model navigationParameters
        let pageModel = {
            Name = name
            Model = model }
        { navigationState with Stack = pageModel::navigationState.Stack }, 
        [initCmd; navigateCmd] |> Cmd.batch |> Cmd.map Message.Upcast

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
            { navigationState with Stack = previousPages }, cmd |> Cmd.map Message.Upcast
        | [] -> { navigationState with Stack = [] }, []

    let updateMsg userUpdate msg model =
        match msg with
        | Message msg -> userUpdate msg model
        | _ -> model, []

    let updateCurrentPage page pageModel navigationState =
        match navigationState.Stack with
        | _::tail ->
            { navigationState with Stack = { page with Model = pageModel }::tail }
        | _ -> navigationState

    let updateNavigation pages msg navigationState =
        match msg with
        | Navigation msg ->
            match msg with
            | Navigate (page, parameters) -> Some (navigate page parameters)
            | NavigateBack parameters -> Some (navigateBack parameters)
            | _ -> None
            |> function
                | Some navigate ->
                    let (navigationState, cmd) = navigate navigationState pages
                    navigationState, cmd
                | None -> navigationState, []
        | _ -> navigationState, []

    let update userUpdate navigationUpdate navigationState pages msg model =
        match msg with
        | App msg ->
            let model, appCmd = updateMsg userUpdate msg model
            let navigationState, pagesCmd = updateNavigation pages msg navigationState
            let navigationUpdateCmd = navigationUpdate navigationState |> Message |> Cmd.ofMsg
            model, [
                appCmd |> Cmd.map App
                pagesCmd |> Cmd.map Page
                navigationUpdateCmd |> Cmd.map App ] |> Cmd.batch
        | Page msg ->
            match currentPage navigationState with
            | Some page ->
                let template = pages |> Map.find page.Name
                let navigationState, updateCmd = 
                    let model, cmd = updateMsg template.Update (msg |> Message.Downcast<_,_>) page.Model
                    (updateCurrentPage page model navigationState), cmd
                let navigationState, navigationCmd = updateNavigation pages msg navigationState
                let navigationUpdateCmd = navigationUpdate navigationState |> Message |> Cmd.ofMsg
                model, [
                    updateCmd |> Cmd.map (Message.Upcast<_> >> Page)
                    navigationCmd |> Cmd.map (Message.Upcast<_> >> Page)
                    navigationUpdateCmd |> Cmd.map App ] |> Cmd.batch
            | None -> model, []

    let view dispatch navigationState pages =
        match currentPage navigationState with
        | Some { Name = name; Model = model } ->
            let page = pages |> Map.find name
            Some (page.View model ((fun msg -> msg :?> 'a) >> Message >> Page >> dispatch))
        | None -> None

    let init = { Stack = [] }

[<RequireQualifiedAccess>]
module Program =
    let wrapInit userInit () = 
        userInit() ||> fun model cmd -> model, cmd |> Cmd.map App

    let wrapUpdate userUpdate getState updateState pages msg model =
        let navigationState = getState model
        Navigation.update userUpdate updateState navigationState pages msg model

    let wrapView getState pages userView model dispatch =
        let navigationState = getState model
        let currentPage = Navigation.view dispatch navigationState pages
        userView model (Message >> App >> dispatch) currentPage

    let mkProgramWithNavigation init update view mapCommand updateState getState pages =
        let pages =
            pages
            |> List.map (fun (name, page) -> (PageName name, page))
            |> Map.ofList
        let init = fun () ->
            init()
            ||> fun model cmd -> model, cmd |> List.map mapCommand
        let update = fun msg model ->
            update msg model
            ||> fun model cmd -> model, cmd |> List.map mapCommand
        Program.mkProgram
            (wrapInit init)
            (wrapUpdate update getState updateState pages)
            (wrapView getState pages view)

    let mkSimpleWithNavigation init update view updateState getState pages =
        let pages =
            pages
            |> List.map (fun (name, page) -> (PageName name, page))
            |> Map.ofList
        Program.mkProgram
            (wrapInit init)
            (wrapUpdate update getState updateState pages)
            (wrapView getState pages view)
