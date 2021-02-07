namespace Elmish.Navigation
  open Elmish

  type PageName = | PageName of string

  type PageModel =
    { Name: PageName
      Model: obj }

  type OnNavigationParameters<'Params> =
    { Source: PageName option
      Parameters: 'Params option }

  type NavigationState =
    { Stack: PageModel list }

  type NavigationMessage<'Params> =
    | Navigate of PageName
    | NavigateParams of PageName * 'Params option
    | NavigateBack
    | NavigateBackParams of 'Params option

  type Navigable<'msg, 'Params> =
    | AppMsg of 'msg
    | PageMsg of 'msg
    | NavigationMsg of NavigationMessage<'Params>

  type Navigation<'Page,'Params> =
    { Dispatch: Dispatch<NavigationMessage<'Params>>
      CurrentPage: 'Page option }

  type Dispatch<'Msg> = 'Msg -> unit

  type Init<'Model,'Msg> = unit -> 'Model * Cmd<'Msg>

  type Update<'Model,'Msg> = 'Msg -> 'Model -> 'Model * Cmd<'Msg>

  type View<'Model,'Msg,'View> = 'Model -> Dispatch<'Msg> -> 'View

  type OnNavigate<'Model,'Msg,'Params> =
    'Model -> OnNavigationParameters<'Params> -> 'Model * Cmd<'Msg>

  type Page<'View, 'Args> = {
    Init : Init<obj,obj>
    Update : Update<obj, obj>
    View : View<obj, obj, 'View>
    OnNavigate : OnNavigate<obj, obj, 'Args> }
    with
      static member
        Create : init:Init<'Model,'Msg> * view:View<'Model,'Msg,'View> *
                 ?update:Update<'Model,'Msg> *
                 ?onNavigate:OnNavigate<'Model,'Msg,'Params> ->
                   Page<'View,'Params>
    end

  type private Pages<'View, 'Params> = Map<PageName, Page<'View, 'Params>>
