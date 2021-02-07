namespace Elmish.Navigation

open Elmish

[<AutoOpen>]
module Navigation =
    let currentPage navigationState =
        match navigationState.Stack with
        | [] -> None
        | head::_ -> Some head

    let navigate 
        name 
        (navigationParameters: OnNavigationParameters<'Params>) 
        navigationState 
        (pages: Pages<'View, 'Params>) =
        let page = pages |> Map.find name
        let (model, initCmd) = page.Init()
        let (model, navigateCmd) = page.OnNavigate model navigationParameters
        let pageModel = {
            Name = name
            Model = model }
        { navigationState with Stack = pageModel::navigationState.Stack }, Cmd.batch [initCmd; navigateCmd] |> Cmd.cast

    let navigateBack 
        (navigationParameters: OnNavigationParameters<'Params>) 
        navigationState 
        (pages: Pages<'View, 'Params>) =
        match navigationState.Stack with
        | _::previousPages ->
            let (previousPages, cmd) =
                match previousPages with
                | previousPage::tail ->
                    let page = pages |> Map.find previousPage.Name
                    let (model, navigateCmd) = page.OnNavigate previousPage.Model navigationParameters
                    let previousPage = { previousPage with Model = model }
                    previousPage::tail, navigateCmd |> Cmd.cast
                | [] -> previousPages, []
            { navigationState with Stack = previousPages }, cmd 
        | [] -> { navigationState with Stack = [] }, []

    let update msg navigationState (pages: Pages<'View', 'Params>)  =
        match navigationState.Stack with
        | { Name = name; Model = model }::tail ->
            let page = pages |> Map.find name
            let (model, cmd) = page.Update (msg :> obj) model
            let newPage = { Name = name; Model = model }
            { navigationState with Stack = newPage::tail }, cmd |> Cmd.cast
        | [] -> navigationState, []

    let view dispatch navigationState (pages: Pages<'View, 'Params>) = 
        match currentPage navigationState with
        | Some { Name = name; Model = model } ->
            let page = pages |> Map.find name
            Some (page.View model ((fun msg -> msg :?> 'a) >> PageMsg >> dispatch))
        | None -> None

    let init = { Stack = [] }

[<RequireQualifiedAccess>]
module Program =
    let (|Navigate|_|) = function
        | Navigate page -> Some(page, None)
        | NavigateParams (page, parameters) -> Some(page, parameters)
        | _ -> None

    let (|NavigateBack|_|) = function
        | NavigateBack -> Some None
        | NavigateBackParams parameters -> Some parameters
        | _ -> None

    let map (model, cmd) =
        model, cmd |> Cmd.map PageMsg

    let createNavigationParams parameters navigationState =
        match currentPage navigationState with
        | Some pageModel -> 
            { Source = Some (pageModel.Name)
              Parameters = parameters }
        | None ->
            { Source = None
              Parameters = parameters }

    let init userInit () = 
        userInit() |> map

    let update getState updateState pages userUpdate msg model =
        let navigationState = getState model
        match msg with
        | AppMsg msg ->
            userUpdate msg model
            ||> fun model cmd -> model, cmd |> Cmd.map AppMsg
        | PageMsg msg ->
            let (navigationState, updateCmd) = update msg navigationState pages
            let (model, cmd) = updateState model navigationState
            (model, Cmd.batch [updateCmd; cmd]) |> map
        | NavigationMsg msg ->
            let (navigationState, cmd) =
                match msg with
                | Navigate (page, parameters) ->
                    let navigationParameters = createNavigationParams parameters navigationState
                    navigate page navigationParameters navigationState pages
                | NavigateBack parameters ->
                    let navigationParameters = createNavigationParams parameters navigationState
                    navigateBack navigationParameters navigationState pages
                | _ -> navigationState, []
            let (model, updateCmd) = updateState model navigationState
            (model, Cmd.batch [cmd; updateCmd]) |> map

    let view getState pages userView model dispatch =
        let navigationState = getState model
        let page = view dispatch navigationState pages
        let navigation = {
            Dispatch = NavigationMsg >> dispatch
            CurrentPage = page }
        userView model (AppMsg >> dispatch) navigation

    let setState userSetState model dispatch =
        userSetState model (PageMsg >> dispatch)

    let subs userSubscribe model =
        userSubscribe model |> Cmd.map PageMsg

    let start program (pages: (string * Page<'View, 'Params>) list) updateState getState =
        let pages =
            pages 
            |> List.map (fun (name, page) -> (PageName name, page))
            |> Map.ofList
        let update = update getState updateState pages
        let view = view getState pages
        program
        |> Program.map init update view setState subs

    let withNavigation pages updateState getState (program : Program<'a,'model,'msg,'view>)  =
        start program pages updateState getState