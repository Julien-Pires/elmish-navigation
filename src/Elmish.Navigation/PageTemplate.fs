namespace Elmish.Navigation

open Elmish

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
type PageTemplate<'view, 'pageName, 'args> = {
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
