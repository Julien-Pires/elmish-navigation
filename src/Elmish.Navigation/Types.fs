namespace Elmish.Navigation

open System
open Elmish

/// <summary>
/// Represents the unique ID of a page
/// </summary>
type PageId = PageId of Guid

/// <summary>
/// Represents the name of a page
/// </summary>
type PageName<'a> = PageName of 'a

/// <summary>
/// Represents information about an opened page
/// </summary>
type PageModel<'pageName> = {
    Id: PageId
    Name: PageName<'pageName>
    Model: obj }

/// <summary>
/// Represents the current state for the navigation
/// </summary>
type NavigationState<'pageName> = {
    Pages: Map<PageId, PageModel<'pageName>>
    Stack: PageId list }

/// <summary>
/// Represents a model that contains a navigation state
/// </summary>
type NavigationModel<'model, 'pageName> = {
    Navigation: NavigationState<'pageName>
    Model: 'model }

type MessageSource<'msg> =
    | App of 'msg
    | Page of 'msg * PageId
    with
        static member Upcast(msg: MessageSource<obj>) =
            match msg with
            | App msg -> App (msg :?> 'a)
            | Page (msg, pageId) -> Page ((msg :?> 'a), pageId)

/// <summary>
/// Represents a message used to navigate
/// </summary>
type NavigationMessage<'pageName, 'args> =
    | Navigate of 'pageName
    | NavigateParams of 'pageName * 'args option
    | NavigateBack
    | NavigateBackParams of 'args option
    with
        static member Downcast msg =
            match msg with
            | Navigate page -> Navigate page
            | NavigateParams (page, args) -> NavigateParams (page, Option.map (fun args -> args :> obj) args)
            | NavigateBack -> NavigateBack
            | NavigateBackParams args -> NavigateBackParams (Option.map (fun args -> args :> obj) args)
        static member Upcast (msg: NavigationMessage<'pageName, obj>) =
            match msg with
            | Navigate page -> Navigate page
            | NavigateParams (page, args) -> NavigateParams (page, Option.map (fun (args: obj) -> args :?> 'a) args)
            | NavigateBack -> NavigateBack
            | NavigateBackParams args -> NavigateBackParams (Option.map (fun (args: obj) -> args :?> 'a) args)

/// <summary>
/// Represents a wrapper for an application message or a page message
/// </summary>
type ProgramMsg<'msg, 'pageName, 'args> =
    | Message of MessageSource<'msg>
    | Navigation of NavigationMessage<'pageName, 'args>

type Navigator<'pageName, 'args>() =
    let navigationStream = new Event<NavigationMessage<'pageName,'args>>()

    let sendNavigationMsg msg =
        msg |> navigationStream.Trigger

    member _.Received = navigationStream.Publish

    member _.Navigate page =
        Navigate page |> sendNavigationMsg

    member _.NavigateWith page args =
        NavigateParams (page, Some args) |> sendNavigationMsg

    member _.NavigateBackWith args =
        NavigateBackParams (Some args) |> sendNavigationMsg

    member _.NavigateBack =
        NavigateBack |> sendNavigationMsg

type CurrentPage<'pageName, 'view> = {
    Id: PageId
    Name: PageName<'pageName>
    Render: unit -> 'view }

type Navigation<'pageName, 'view, 'args> = {
    CurrentPage: CurrentPage<'pageName, 'view> option
    Navigator: Navigator<'pageName,'args> }

/// <summary>
/// Provides information when a page navigation occurred
/// </summary>
type OnNavigationEventArgs<'pageName, 'args> = {
    Source: 'pageName option
    Parameters: 'args option }
    with
        member this.Map<'a>() = 
            let untyped = this.Parameters |> Option.map (fun c -> c :> obj)
            let newParameter = untyped |> Option.map (fun c -> c :?> 'a)
            { Source = this.Source
              Parameters = newParameter }

/// <summary>
/// Represents the method used to send message
/// </summary>
type Dispatch<'msg> = 'msg -> unit

/// <summary>
/// Represents the method used to initialize a page
/// </summary>
type Init<'model, 'cmdMsg> = unit -> 'model * Cmd<'cmdMsg>

/// <summary>
/// Represents the method used to map a command message to a message
/// </summary>
type MapCommand<'msg, 'cmdMsg> = 'cmdMsg -> Cmd<'msg>

/// <summary>
/// Represents the method used to update the page state
/// </summary>
type Update<'model, 'msg, 'cmdMsg> = 'msg -> 'model -> 'model * Cmd<'cmdMsg>

/// <summary>
/// Represents the method use to render the page
/// </summary>
type View<'model, 'msg, 'view> = 'model -> Dispatch<'msg> -> 'view

/// <summary>
/// Represents the method that is triggred when a page navigation occurred to the page
/// </summary>
type OnNavigate<'model, 'msg, 'pageName, 'args> = 'model -> OnNavigationEventArgs<'pageName, 'args> -> 'model * Cmd<'msg>

/// <summary>
/// Represents an application page
/// </summary>
type Page<'view, 'pageName, 'args> = {
    Init : Init<obj, obj>
    Update : Update<obj, obj, obj>
    View : View<obj, obj, 'view>
    OnNavigate : OnNavigate<obj, obj, 'pageName, 'args>
    MapCommand: MapCommand<obj, obj> }
    with
        /// <summary>
        /// Creates a new page
        /// </summary>
        static member Create
            (
                init: Init<'model, 'cmdMsg>,
                view: View<'model, 'msg, 'view>, 
                ?update: Update<'model, 'msg, 'cmdMsg>, 
                ?onNavigate: OnNavigate<'model, 'msg, 'pageName, 'args>,
                ?mapCommand: MapCommand<'msg, 'cmdMsg>
            ) =
            let init = fun () ->
                init() 
                ||> fun model cmd -> model :> obj, (cmd |> Cmd.map (fun cmd -> cmd :> obj))
            let view = fun (model: obj) (dispatch: Dispatch<obj>) -> 
                view (model :?> 'model) dispatch
            let update =
                match update with
                | Some update ->
                    fun (msg: obj) (model: obj) -> 
                        update (msg :?> 'msg) (model :?> 'model) 
                        ||> fun model cmd -> model :> obj, (cmd |> Cmd.map (fun cmd -> cmd :> obj))
                | None -> fun _ (model: obj) -> model, []
            let onNavigate =
                match onNavigate with
                | Some onNavigate ->
                    fun (model: obj) navigationParams -> 
                        onNavigate (model :?> 'model) navigationParams
                        ||> fun model cmd -> model :> obj, (cmd |> Cmd.map (fun cmd -> cmd :> obj))
                | None -> fun (model: obj) _ -> model, []
            let mapCommand =
                match mapCommand with
                | Some mapCommand ->
                    fun (msg: obj) -> 
                        mapCommand (msg :?> 'cmdMsg) 
                        |> Cmd.map (fun cmd -> cmd :> obj)
                | None -> fun _ -> []
            { Init = init
              Update = update
              View = view
              OnNavigate = onNavigate
              MapCommand = mapCommand }
