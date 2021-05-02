namespace Elmish.Navigation

open Elmish

[<RequireQualifiedAccess>]
module Navigation =
    let (|Navigate|NavigateBack|) = function
        | Navigate page -> Navigate (PageName page, None)
        | NavigateParams (page, parameters) -> Navigate (PageName page, parameters)
        | NavigateBack -> NavigateBack None
        | NavigateBackParams parameters -> NavigateBack parameters

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
        { navigationState with Stack = pageModel::navigationState.Stack }, Cmd.batch [initCmd; navigateCmd]

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
            { navigationState with Stack = previousPages }, cmd
        | [] -> { navigationState with Stack = [] }, []

    let applyNavigation pages msg navigationState =
        match msg with
        | Navigate (page, parameters) -> navigate page parameters
        | NavigateBack parameters -> navigateBack parameters
        |> fun f -> f navigationState pages

    let updateCurrentPage page navigationState model =
        match navigationState.Stack with
        | _::tail ->
            { navigationState with Stack = { page with Model = model }::tail }
        | _ -> navigationState

    let update userUpdate navigationState pages msg (model: 'Y when 'Y :> INavigationModel<'Y>) =
        match msg with
        | App msg ->
            let model, cmd = userUpdate msg model
            model, cmd |> Cmd.map App
        | Page msg ->
            match currentPage navigationState with
            | Some page ->
                let template = pages |> Map.find page.Name
                template.Update (msg :> obj) page.Model
                ||> fun model cmd -> updateCurrentPage page navigationState model, cmd
                ||> fun state cmd -> model.UpdateNavigation(state), cmd |> Cmd.map ((fun cmd -> cmd :?> 'b) >> Page)
            | None -> model, []
        | Navigation msg ->
            navigationState
            |> applyNavigation pages msg
            ||> fun state cmd -> 
                model.UpdateNavigation(state), cmd |> Cmd.map ((fun cmd -> cmd :?> 'b) >> Page)

    let view dispatch navigationState pages =
        match currentPage navigationState with
        | Some { Name = name; Model = model } ->
            let page = pages |> Map.find name
            Some (page.View model ((fun msg -> msg :?> 'a) >> Page >> dispatch))
        | None -> None

    let empty = { Stack = [] }

[<RequireQualifiedAccess>]
module Program =
    let map = App

    let wrapInit userInit = 
        userInit() ||> fun model cmd -> model, cmd |> Cmd.map App

    let wrapUpdate userUpdate pages msg (model: 'Y when 'Y :> INavigationModel<'Y>) =
        let navigationState = model.GetNavigation()
        Navigation.update userUpdate navigationState pages msg model

    let withNavigation pages (program: Program<_,_,_,_>) : Program<_,_,_,_> =
        program |>
        Program.map
            (fun init _ -> wrapInit init)
            (fun update msg model -> wrapUpdate update pages msg model)
            (fun view model dispatch -> view model (App >> dispatch)) 
            (fun setState model dispatch -> setState model (App >> dispatch))
            (fun subscribe model -> subscribe model |> Cmd.map App)
