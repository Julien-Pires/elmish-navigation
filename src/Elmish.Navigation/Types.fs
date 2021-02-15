namespace Elmish.Navigation

open Elmish

type PageName = PageName of string

type PageModel = {
    Name: PageName
    Model: obj }

type NavigationState = {
    Stack: PageModel list }

type NavigationMessage<'Params> =
    | Navigate of string
    | NavigateParams of string * 'Params option
    | NavigateBack
    | NavigateBackParams of 'Params option

type Message<'Msg, 'Arg> =
    | Message of 'Msg
    | Navigation of NavigationMessage<'Arg>
    with
        static member Downcast(msg) =
            match msg with
            | Message msg -> Message (msg :> obj)
            | Navigation msg -> Navigation msg
        static member Upcast<'Msg>(msg: Message<obj, 'Arg>) =
            match msg with
            | Message msg -> Message (msg :?> 'Msg)
            | Navigation msg -> Navigation msg

type ProgramMsg<'Msg, 'Arg> =
    | App of Message<'Msg, 'Arg>
    | Page of Message<'Msg, 'Arg>

type Navigation<'Page, 'Params> = {
    Dispatch: Dispatch<NavigationMessage<'Params>> 
    CurrentPage: 'Page option }

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

type Init<'Model, 'CmdMsg, 'Args> = unit -> 'Model * Cmd<Message<'CmdMsg, 'Args>>

type MapCommand<'Msg, 'CmdMsg, 'Args> = 'CmdMsg -> Cmd<Message<'Msg, 'Args>>

type Update<'Model, 'Msg, 'CmdMsg, 'Args> = 'Msg -> 'Model -> 'Model * Cmd<Message<'CmdMsg, 'Args>>

type View<'Model, 'Msg, 'View> = 'Model -> Dispatch<'Msg> -> 'View

type OnNavigate<'Model, 'Msg, 'Args> = 'Model -> OnNavigationParameters<'Args> -> 'Model * Cmd<Message<'Msg, 'Args>>

type Page<'View, 'Args> = {
    Init : Init<obj, obj, 'Args>
    Update : Update<obj, obj, obj, 'Args>
    View : View<obj, obj, 'View>
    OnNavigate : OnNavigate<obj, obj, 'Args>
    MapCommand: MapCommand<obj, obj, 'Args> }
    with
        static member Create
            (
                init: Init<'Model, 'CmdMsg, 'Args>,
                view: View<'Model, 'Msg, 'View>, 
                ?update: Update<'Model, 'Msg, 'CmdMsg, 'Args>, 
                ?onNavigate: OnNavigate<'Model, 'Msg, 'Args>,
                ?mapCommand: MapCommand<'Msg, 'CmdMsg, 'Args>
            ) =
            let init = fun () -> init() ||> fun model cmd -> model :> obj, cmd |> Cmd.map Message.Downcast<'CmdMsg, 'Args>
            let view = fun (model: obj) (dispatch: Dispatch<obj>) -> view (model :?> 'Model) dispatch
            let update =
                match update with
                | Some update ->
                    fun (msg: obj) (model: obj) -> 
                        update (msg :?> 'Msg) (model :?> 'Model) 
                        ||> fun model cmd -> model :> obj, cmd |> Cmd.map Message.Downcast<'CmdMsg, 'Args>
                | None -> fun _ (model: obj) -> model, []
            let onNavigate =
                match onNavigate with
                | Some onNavigate ->
                    fun (model: obj) navigationParams -> 
                        onNavigate (model :?> 'Model) navigationParams
                        ||> fun model cmd -> model :> obj, cmd |> Cmd.map Message.Downcast<'Msg, 'Args>
                | None -> fun (model: obj) _ -> model, []
            let mapCommand =
                match mapCommand with
                | Some mapCommand ->
                    fun (msg: obj) -> mapCommand (msg :?> 'CmdMsg) |> fun cmd -> cmd |> Cmd.map Message.Downcast<'Msg, 'Args>
                | None -> fun _ -> []
            { Init = init
              Update = update
              View = view
              OnNavigate = onNavigate
              MapCommand = mapCommand }

type Pages<'View, 'Args> = Map<PageName, Page<'View, 'Args>>
