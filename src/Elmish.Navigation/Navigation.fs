namespace Elmish.Navigation

open Elmish

[<AutoOpen>]
module Navigation =
    type PageName = PageName of string

    type OnNavigationParameters<'Params> = {
        Source: PageName option
        Parameters: 'Params option } 
        with
            member this.Map<'a>() = 
                let untyped = this.Parameters |> Option.map (fun c -> c :> obj)
                let newParameter = untyped |> Option.map (fun c -> c :?> 'a)
                { Source = this.Source
                  Parameters = newParameter }

    type Dispatch<'Msg> = 'Msg -> unit

    type Init<'Model, 'Msg> = unit -> 'Model * Cmd<'Msg>

    type Update<'Model, 'Msg> = 'Msg -> 'Model -> 'Model * Cmd<'Msg>

    type View<'Model, 'Msg, 'View> = 'Model -> Dispatch<'Msg> -> 'View

    type OnNavigate<'Model, 'Msg, 'Params> = 'Model -> OnNavigationParameters<'Params> -> 'Model * Cmd<'Msg>

    type Page<'View, 'Params>(init: Init<obj, obj>,
                     update: Update<obj, obj>, 
                     view: View<obj, obj, 'View>, 
                     onNavigate: OnNavigate<obj, obj, 'Params>) =
        member _.Init = init

        member _.Update = update

        member _.View = view

        member _.OnNavigate = onNavigate

        static member Create
            (
            init: Init<'Model, 'Msg>, 
            view: View<'Model, 'Msg, 'View>, 
            ?update: Update<'Model, 'Msg>, 
            ?onNavigate: OnNavigate<'Model, 'Msg, 'Params>) = 
            let map (model, cmd) = model :> obj, cmd |> Cmd.map (fun msg -> msg :> obj)
            let init = fun () -> init() |> map
            let view = fun (model: obj) (dispatch: Dispatch<obj>) -> view (model :?> 'Model) dispatch
            let update =
                match update with
                | Some update ->
                    fun (msg: obj) (model: obj) -> update (msg :?> 'Msg) (model :?> 'Model) |> map
                | None -> fun _ (model: obj) -> model, []
            let onNavigate =
                match onNavigate with
                | Some onNavigate ->
                    fun (model: obj) navigationParams -> onNavigate (model :?> 'Model) navigationParams |> map
                | None -> fun (model: obj) _ -> model, []
            Page(init, update, view, onNavigate)

    type Pages<'View, 'Params> = Map<PageName, Page<'View, 'Params>>

    type PageModel = {
        Name: PageName
        Model: obj }

    type NavigationState = {
        Stack: PageModel list }

    type NavigationMessage<'Params> =
        | Navigate of PageName
        | NavigateParams of PageName * 'Params option
        | NavigateBack
        | NavigateBackParams of 'Params option

    type Navigable<'msg, 'Params> =
        | AppMsg of 'msg
        | PageMsg of 'msg
        | NavigationMsg of NavigationMessage<'Params>

    type Navigation<'Page, 'Params> = {
        Dispatch: Dispatch<NavigationMessage<'Params>> 
        CurrentPage: 'Page option }

    let currentPage navigationState =
        match navigationState.Stack with
        | [] -> None
        | head::_ -> Some head

    let navigate name (navigationParameters: OnNavigationParameters<'Params>) navigationState (pages: Pages<'View, 'Params>) =
        let page = pages |> Map.find name
        let (model, initCmd) = page.Init()
        let (model, navigateCmd) = page.OnNavigate model navigationParameters
        let pageModel = {
            Name = name
            Model = model }
        { navigationState with Stack = pageModel::navigationState.Stack }, Cmd.batch [initCmd; navigateCmd] |> Cmd.cast

    let navigateBack (navigationParameters: OnNavigationParameters<'Params>) navigationState (pages: Pages<'View, 'Params>) =
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