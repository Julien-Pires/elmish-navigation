namespace Elmish.Navigation

open System
open Elmish

type MessageSource<'msg> =
    | App of 'msg
    | Page of 'msg * PageId
    with
        static member Upcast(msg: MessageSource<obj>) =
            match msg with
            | App msg -> App (msg :?> 'b)
            | Page (msg, pageId) -> Page ((msg :?> 'b), pageId)

/// <summary>
/// Represents a wrapper for an application message or a page message
/// </summary>
type ProgramMsg<'msg, 'args> =
    | Message of MessageSource<'msg>
    | Navigation of NavigationMessage<'args>

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

    let pushPage state page = {
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

    let createNavigationParams source parameters = { 
        Source = source
        Parameters = parameters }

    let navigate state pages name parameters =
        getTemplate pages name
        |> Option.bind (fun template ->
            let navigationParameters = createNavigationParams (Some name) parameters
            let (model, initCmd) = template.Init()
            let (model, navigateCmd) = template.OnNavigate model navigationParameters
            let pageId = PageId(Guid.NewGuid())
            let pageModel = {
                Id = pageId
                Name = name
                Model = model }
            let state = pushPage state pageModel
            let cmds = 
                Cmd.batch [initCmd; navigateCmd] 
                |> Cmd.map (fun cmd -> Page(cmd, pageId))
                |> Cmd.map MessageSource.Upcast<_>
            Some(state, cmds))
        |> Option.defaultValue (state, [])

    let navigateBack state pages parameters =
        getCurrentPage state
        |> Option.bind (fun previousPage -> 
            popPage state
            |> getCurrentPage
            |> Option.bind (fun page ->
                let template = pages |> Map.find page.Name
                let navigationParameters = createNavigationParams (Some previousPage.Name) parameters
                let (model, cmd) = template.OnNavigate page.Model navigationParameters
                let state = updatePage state page.Id model
                Some(state, cmd |> Cmd.map ((fun cmd -> Page(cmd, page.Id)) >> MessageSource.Upcast<_>))))
        |> Option.defaultValue (state, [])

    let applyNavigation pages msg model =
        let state = model.Navigation
        match msg with
        | Navigate (page, parameters) -> navigate state pages page parameters
        | NavigateBack parameters -> navigateBack state pages parameters
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

    let update userUpdate pages msg model =
        match msg with
        | Message source -> applyUpdate userUpdate pages source model
        | Navigation msg -> applyNavigation pages msg model
        |> fun (model, cmds) -> model, cmds |> Cmd.map Message

    let empty = {
        Pages = Map.empty
        Stack = [] }

    let init model = {
        Model = model
        Navigation = empty }

[<RequireQualifiedAccess>]
module Program =
    let map = App

    let wrapInit userInit = 
        userInit() 
        ||> fun model cmd -> 
            Navigation.init model, cmd |> Cmd.map (App >> Message)

    let wrapUpdate userUpdate pages msg model =
        Navigation.update userUpdate pages msg model

    let withNavigation pages program =
        program |>
        Program.map
            (fun init _ -> wrapInit init)
            (fun update msg model -> wrapUpdate update pages msg model)
            (fun view model dispatch -> view (model.Model) (App >> Message >> dispatch)) 
            (fun setState model dispatch -> setState (model.Model) (App >> Message >> dispatch))
            (fun subscribe model -> subscribe (model.Model) |> Cmd.map (App >> Message))
