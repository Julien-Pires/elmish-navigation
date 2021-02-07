namespace Elmish.Navigation

open Elmish

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

type Page<'View, 'Args> = {
    Init : Init<obj,obj>
    Update : Update<obj, obj>
    View : View<obj, obj, 'View>
    OnNavigate : OnNavigate<obj, obj, 'Args> }
    with
        static member Create
            (
                init: Init<'Model, 'Msg>, 
                view: View<'Model, 'Msg, 'View>, 
                ?update: Update<'Model, 'Msg>, 
                ?onNavigate: OnNavigate<'Model, 'Msg, 'Params>
            ) = 
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
            { Init = init
              Update = update
              View = view
              OnNavigate = onNavigate }

type Pages<'View, 'Params> = Map<PageName, Page<'View, 'Params>>
