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
/// Represents a wrapper around an application message or a navigation message
/// </summary>
type Message<'msg, 'args> =
    | Message of 'msg
    | Navigation of NavigationMessage<'args>
    with
        static member Downcast(msg) =
            match msg with
            | Message msg -> Message (msg :> obj)
            | Navigation msg -> Navigation msg
        static member Upcast(msg: Message<obj, 'args>) =
            match msg with
            | Message msg -> Message (msg :?> 'msg)
            | Navigation msg -> Navigation msg

/// <summary>
/// Represents a wrapper for an application message or a page message
/// </summary>
type ProgramMsg<'msg, 'args> =
    | App of Message<'msg, 'args>
    | Page of Message<'msg, 'args>

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
type Init<'model, 'cmdMsg, 'args> = unit -> 'model * Cmd<Message<'cmdMsg, 'args>>

/// <summary>
/// Represents the method used to map a command message to a message
/// </summary>
type MapCommand<'msg, 'cmdMsg, 'args> = 'cmdMsg -> Cmd<Message<'msg, 'args>>

/// <summary>
/// Represents the method used to update the page state
/// </summary>
type Update<'model, 'msg, 'cmdMsg, 'args> = 'msg -> 'model -> 'model * Cmd<Message<'cmdMsg, 'args>>

/// <summary>
/// Represents the method use to render the page
/// </summary>
type View<'model, 'msg, 'view> = 'model -> Dispatch<'msg> -> 'view

/// <summary>
/// Represents the method that is triggred when a page navigation occurred to the page
/// </summary>
type OnNavigate<'model, 'msg, 'args> = 'model -> OnNavigationEventArgs<'args> -> 'model * Cmd<Message<'msg, 'args>>

/// <summary>
/// Represents an application page
/// </summary>
type Page<'view, 'args> = {
    Init : Init<obj, obj, 'args>
    Update : Update<obj, obj, obj, 'args>
    View : View<obj, obj, 'view>
    OnNavigate : OnNavigate<obj, obj, 'args>
    MapCommand: MapCommand<obj, obj, 'args> }
    with
        /// <summary>
        /// Allow to create a new page
        /// </summary>
        static member Create
            (
                init: Init<'model, 'cmdMsg, 'args>,
                view: View<'model, 'msg, 'view>, 
                ?update: Update<'model, 'msg, 'cmdMsg, 'args>, 
                ?onNavigate: OnNavigate<'model, 'msg, 'args>,
                ?mapCommand: MapCommand<'msg, 'cmdMsg, 'args>
            ) =
            let init = fun () -> 
                init() 
                ||> fun model cmd -> model :> obj, (cmd |> Cmd.map Message.Downcast<'cmdMsg, 'args>)
            let view = fun (model: obj) (dispatch: Dispatch<obj>) -> view (model :?> 'model) dispatch
            let update =
                match update with
                | Some update ->
                    fun (msg: obj) (model: obj) -> 
                        update (msg :?> 'msg) (model :?> 'model) 
                        ||> fun model cmd -> model :> obj, (cmd |> Cmd.map Message.Downcast<'cmdMsg, 'args>)
                | None -> fun _ (model: obj) -> model, []
            let onNavigate =
                match onNavigate with
                | Some onNavigate ->
                    fun (model: obj) navigationParams -> 
                        onNavigate (model :?> 'model) navigationParams
                        ||> fun model cmd -> model :> obj, (cmd |> Cmd.map Message.Downcast<'msg, 'args>)
                | None -> fun (model: obj) _ -> model, []
            let mapCommand =
                match mapCommand with
                | Some mapCommand ->
                    fun (msg: obj) -> 
                        mapCommand (msg :?> 'cmdMsg) 
                        |> Cmd.map Message.Downcast<'msg, 'args>
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
