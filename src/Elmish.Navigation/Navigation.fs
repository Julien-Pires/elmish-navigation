namespace Elmish.Navigation

open Elmish

type Update<'a> =
    | Update of 'a
    | NoUpdate

[<RequireQualifiedAccess>]
module Update =
    let bind f m =
        match m with
        | Update x -> f x
        | NoUpdate -> NoUpdate

    let defaultValue value m =
        match m with
        | Update x -> x
        | NoUpdate -> value

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

    let applyUpdate userUpdate msg model =
        match msg with
        | Message msg -> 
            userUpdate msg model 
            ||> fun model cmd -> Update (model, cmd)
        | _ -> NoUpdate

    let applyNavigation pages msg navigationState =
        match msg with
        | Navigation (Navigate (page, parameters)) -> Some (navigate page parameters)
        | Navigation (NavigateBack parameters) -> Some (navigateBack parameters)
        | _ -> None
        |> function
            | Some navigate ->
                let (navigationState, cmd) = navigate navigationState pages
                Update (navigationState, cmd)
            | None -> NoUpdate

    let updateCurrentPage page navigationState model =
        match navigationState.Stack with
        | _::tail ->
            { navigationState with Stack = { page with Model = model }::tail }
        | _ -> navigationState

    let update userUpdate navigationState pages msg (model: 'Y when 'Y :> INavigationModel<'Y>) =
        (match msg with
        | App msg ->
            let model, cmd =
                applyUpdate userUpdate msg model
                |> Update.defaultValue (model, [])
            msg, model, navigationState, cmd |> Cmd.map App
        | Page msg ->
            match currentPage navigationState with
            | Some page ->
                let template = pages |> Map.find page.Name
                let model, navigationState, cmd =
                    applyUpdate template.Update (msg |> Message.Downcast<_,_>) page.Model
                    |> Update.bind (fun (model, cmd) -> Update (updateCurrentPage page navigationState model, cmd))
                    |> Update.bind (fun (state, cmd) -> (model.UpdateNavigation(state), state, cmd) |> Update)
                    |> Update.defaultValue (model, navigationState, [])
                msg, model, navigationState, cmd |> Cmd.map (Message.Upcast<_> >> Page)
            | None -> msg, model, navigationState, [])
        |> fun (msg, model, navigationState, updateCmd) ->
            let model, navigationCmd =
                navigationState
                |> applyNavigation pages msg
                |> Update.bind (fun (state, cmd) -> (model.UpdateNavigation(state), cmd) |> Update)
                |> Update.defaultValue (model, [])
            model, Cmd.batch [
                updateCmd
                navigationCmd |> Cmd.map Page ]

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

    let wrapUpdate userUpdate getState pages msg model =
        let navigationState = getState model
        Navigation.update userUpdate navigationState pages msg model

    let wrapView getState pages userView model dispatch =
        printfn "Call view => %A" model
        let navigationState = getState model
        let currentPage = Navigation.view dispatch navigationState pages
        userView model (Message >> App >> dispatch) currentPage

    let mkProgramWithNavigation init update view mapCommand getState pages =
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
            (wrapUpdate update getState pages)
            (wrapView getState pages view)

    let mkSimpleWithNavigation init update view getState pages =
        let pages =
            pages
            |> List.map (fun (name, page) -> (PageName name, page))
            |> Map.ofList
        Program.mkProgram
            (wrapInit init)
            (wrapUpdate update getState pages)
            (wrapView getState pages view)
