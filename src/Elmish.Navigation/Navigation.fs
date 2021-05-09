namespace Elmish.Navigation

open System
open Elmish

[<RequireQualifiedAccess>]
module Navigation =
    let (|Navigate|NavigateBack|) = function
        | Navigate page -> Navigate (PageName page, None)
        | NavigateParams (page, parameters) -> Navigate (PageName page, parameters)
        | NavigateBack -> NavigateBack None
        | NavigateBackParams parameters -> NavigateBack parameters

    let getPage state id =
        state.Pages.TryFind id

    let getCurrentPage state =
        match state.Stack with
        | [] -> None
        | head::_ -> getPage state head

    let getTemplate pages name =
        pages |> Map.tryFind name

    let pushPage state (page: PageModel<_>) = {
        Pages = state.Pages.Add (page.Id, page)
        Stack = page.Id::state.Stack }

    let popPage state =
        match state.Stack with
        | head::tail -> {
            Pages = state.Pages.Remove head 
            Stack = tail }
        | [] -> state

    let updatePage state id model =
        let page = getPage state id
        match page with
        | Some page ->
            let pagesMap = state.Pages.Add (id, { page with Model = model})
            { state with Pages = pagesMap }
        | None -> state

    let createNavigationEventArgs source parameters = { 
        Source = source
        Parameters = parameters }

    let applyNavigate state pages name parameters =
        getTemplate pages name
        |> Option.map (fun template ->
            let (PageName name) = name
            let navigationEventArgs = createNavigationEventArgs (Some name) parameters
            let (model, initCmd) = template.Init()
            let (model, navigateCmd) = template.OnNavigate model navigationEventArgs
            let pageModel = {
                Id = PageId (Guid.NewGuid())
                Name = PageName name
                Model = model }
            let state = pushPage state pageModel
            let cmds =
                Cmd.batch [ initCmd; navigateCmd ] 
                |> Cmd.map (fun cmd -> Page(cmd, pageModel.Id))
                |> Cmd.map MessageSource.Upcast<_>
            state, cmds)
        |> Option.defaultValue (state, [])

    let applyNavigateBack state pages parameters =
        getCurrentPage state
        |> Option.bind (fun previousPage -> 
            popPage state
            |> getCurrentPage
            |> Option.map (fun page -> previousPage, page))
        |> Option.map (fun (previousPage, currentPage) ->
            let template = pages |> Map.find currentPage.Name
            let (PageName name) = previousPage.Name
            let navigationEventArgs = createNavigationEventArgs (Some name) parameters
            let (model, cmd) = template.OnNavigate currentPage.Model navigationEventArgs
            let state = updatePage state currentPage.Id model
            state, cmd |> Cmd.map ((fun cmd -> Page(cmd, currentPage.Id)) >> MessageSource.Upcast<_>))
        |> Option.defaultValue (state, [])

    let applyNavigation pages msg model =
        let state = model.Navigation
        match msg with
        | Navigate (page, parameters) -> applyNavigate state pages page parameters
        | NavigateBack parameters -> applyNavigateBack state pages parameters
        ||> fun state cmd ->
                { model with Navigation = state }, cmd |> Cmd.map MessageSource.Upcast<_>

    let applyUpdate userUpdate pages msg model =
        match msg with
        | App msg ->
            let appModel, cmd = userUpdate msg (model.Model)
            { model with NavigationModel.Model = appModel }, cmd |> Cmd.map App
        | Page (msg, pageId) ->
            match getPage (model.Navigation) pageId with
            | Some page ->
                let template = pages |> Map.find page.Name
                template.Update (msg :> obj) page.Model
                ||> fun pageModel cmd ->
                        updatePage (model.Navigation) pageId pageModel, cmd
                ||> fun state cmd ->
                        { model with Navigation = state }, 
                        cmd |> Cmd.map ((fun cmd -> Page(cmd, pageId)) >> MessageSource.Upcast<_>)
            | None -> model, []

    module Program =
        let init model = {
            Model = model
            Navigation = {
                Pages = Map.empty
                Stack = [] }}

        let update userUpdate pages msg model =
            match msg with
            | Message source -> applyUpdate userUpdate pages source model
            | Navigation msg -> applyNavigation pages msg model
            |> fun (model, cmds) -> model, cmds |> Cmd.map Message

        let view userView pages model dispatch navigator =
            let dispatch = Message >> dispatch
            let currentPage =
                getCurrentPage (model.Navigation)
                |> Option.bind (fun page -> 
                    let template = getTemplate pages page.Name
                    template |> Option.map (fun c -> page, c))
                |> Option.map (fun (page, template) -> {
                    Id = page.Id
                    Name = page.Name
                    Render = (fun () ->
                        let pageDispatch = fun msg -> Page (msg, page.Id) |> dispatch
                        let pageView = template.View page.Model pageDispatch
                        pageView navigator )})
            let navigation = {
                CurrentPage = currentPage
                Navigator = navigator }
            userView model.Model (App >> dispatch) navigation
