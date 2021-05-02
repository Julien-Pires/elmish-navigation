namespace Elmish.Navigation

open Elmish

/// <summary>
/// Represents the name of a page
/// </summary>
type PageName = PageName of string

/// <summary>
/// Represents information about an opened page
/// </summary>
type PageModel = {
    Name: PageName
    Model: obj }

/// <summary>
/// Represents the current state for the navigation
/// </summary>
type NavigationState = {
    Stack: PageModel list }

/// <summary>
/// Represents a model that contains a navigation state
/// </summary>
type INavigationModel<'a when 'a :> INavigationModel<'a>> =
    abstract member UpdateNavigation: NavigationState -> 'a
    abstract member GetNavigation: unit -> NavigationState

/// <summary>
/// Represents a message used to navigate
/// </summary>
type NavigationMessage<'args> =
    | Navigate of string
    | NavigateParams of string * 'args option
    | NavigateBack
    | NavigateBackParams of 'args option

/// <summary>
/// Represents a wrapper for an application message or a page message
/// </summary>
type ProgramMsg<'msg, 'args> =
    | App of 'msg
    | Page of 'msg
    | Navigation of NavigationMessage<'args>
    with
        static member Downcast(msg) =
            match msg with
            | App msg  -> App (msg :> obj)
            | Page msg -> Page (msg :> obj)
            | Navigation args -> Navigation args
        static member Upcast(msg: ProgramMsg<obj, 'args>) =
            match msg with
            | App msg -> App (msg :?> 'msg)
            | Page msg -> Page (msg :?> 'msg)
            | Navigation args -> Navigation args

/// <summary>
/// Provides information when a page navigation occurred
/// </summary>
type OnNavigationEventArgs<'args> = {
    Source: PageName option
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
type OnNavigate<'model, 'msg, 'args> = 'model -> OnNavigationEventArgs<'args> -> 'model * Cmd<'msg>

/// <summary>
/// Represents an application page
/// </summary>
type Page<'view, 'args> = {
    Init : Init<obj, obj>
    Update : Update<obj, obj, obj>
    View : View<obj, obj, 'view>
    OnNavigate : OnNavigate<obj, obj, 'args>
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
                ?onNavigate: OnNavigate<'model, 'msg, 'args>,
                ?mapCommand: MapCommand<'msg, 'cmdMsg>
            ) =
            let init = fun () ->
                init() 
                ||> fun model cmd -> model :> obj, (cmd |> Cmd.map (fun cmd -> cmd :> obj))
            let view = fun (model: obj) (dispatch: Dispatch<obj>) -> view (model :?> 'model) dispatch
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

/// <summary>
/// Represents a mapping of page and page name
/// </summary>
type Pages<'view, 'args> = Map<PageName, Page<'view, 'args>>
