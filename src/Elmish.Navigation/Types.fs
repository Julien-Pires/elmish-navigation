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

type Navigable<'msg, 'Params> =
    | AppMsg of 'msg
    | PageMsg of 'msg
    | NavigationMsg of NavigationMessage<'Params>
    with
        static member Cast(msg: Navigable<'msg, 'Params>) =
            match msg with
            | AppMsg msg -> AppMsg (msg :> obj)
            | PageMsg msg -> PageMsg (msg :> obj)
            | NavigationMsg args -> NavigationMsg args
        static member Upcast<'Msg>(msg: Navigable<obj, 'Params>) =
            match msg with
            | AppMsg msg -> AppMsg (msg :?> 'Msg)
            | PageMsg msg -> PageMsg (msg :?> 'Msg)
            | NavigationMsg args -> NavigationMsg args

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

type Init<'Model, 'Msg, 'Args> = unit -> 'Model * Cmd<Navigable<'Msg, 'Args>>

type Update<'Model, 'Msg, 'Args> = 'Msg -> 'Model -> 'Model * Cmd<Navigable<'Msg, 'Args>>

type View<'Model, 'Msg, 'View> = 'Model -> Dispatch<'Msg> -> 'View

type OnNavigate<'Model, 'Msg, 'Args> = 'Model -> OnNavigationParameters<'Args> -> 'Model * Cmd<Navigable<'Msg, 'Args>>

type Page<'View, 'Args> = {
    Init : Init<obj, obj, 'Args>
    Update : Update<obj, obj, 'Args>
    View : View<obj, obj, 'View>
    OnNavigate : OnNavigate<obj, obj, 'Args> }
    with
        static member Create
            (
                init: Init<'Model, 'Msg, 'Args>, 
                view: View<'Model, 'Msg, 'View>, 
                ?update: Update<'Model, 'Msg, 'Args>, 
                ?onNavigate: OnNavigate<'Model, 'Msg, 'Args>
            ) =
            let map (model, cmd: Cmd<Navigable<'Msg, 'Args>>) = 
                model :> obj, cmd |> Cmd.map Navigable<'Msg, 'Args>.Cast
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
            { Init = init
              Update = update
              View = view
              OnNavigate = onNavigate }

type Pages<'View, 'Args> = Map<PageName, Page<'View, 'Args>>
