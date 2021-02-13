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
    | Navigate of string
    | NavigateParams of string * 'Params option
    | NavigateBack
    | NavigateBackParams of 'Params option

  type Message<'Msg, 'Arg> =
      | Message of 'Msg
      | Navigation of NavigationMessage<'Arg>

  type Command<'Msg, 'Arg> =
      | Message of Message<'Msg, 'Arg>
      | Effect of Message<'Msg, 'Arg>

  type ProgramMsg<'Msg, 'Arg> =
      | App of Command<'Msg, 'Arg>
      | Page of Command<'Msg, 'Arg>

  type Navigable<'msg, 'Params> =
    | AppMsg of 'msg
    | PageMsg of 'msg
    | NavigationMsg of NavigationMessage<'Params>
    with
      static member
        Cast : msg:Navigable<'msg, 'Params> -> Navigable<obj, 'Params>
      static member
        Upcast<'Msg> : msg:Navigable<obj, 'Params> -> Navigable<'Msg, 'Params>

  type Navigation<'Page,'Params> =
    { Dispatch: Dispatch<NavigationMessage<'Params>>
      CurrentPage: 'Page option }

  type Dispatch<'Msg> = 'Msg -> unit

  type Init<'Model, 'Msg, 'Args> = unit -> 'Model * Cmd<Navigable<'Msg, 'Args>>

  type MapCommand<'Msg, 'CmdMsg, 'Args> = 'CmdMsg -> Cmd<Navigable<'Msg, 'Args>>

  type Update<'Model, 'Msg, 'CmdMsg, 'Args> = 'Msg -> 'Model -> 'Model * Cmd<Navigable<'CmdMsg, 'Args>>

  type View<'Model, 'Msg, 'View> = 'Model -> Dispatch<'Msg> -> 'View

  type OnNavigate<'Model, 'Msg, 'Args> =
    'Model -> OnNavigationParameters<'Args> -> 'Model * Cmd<Navigable<'Msg, 'Args>>

  type MapExternalCmd<'Msg, 'Args> = 'Msg -> Cmd<Navigable<'Msg, 'Args>>

  type Page<'View, 'Args> = {
    Init : Init<obj, obj, 'Args>
    Update : Update<obj, obj, obj, 'Args>
    View : View<obj, obj, 'View>
    OnNavigate : OnNavigate<obj, obj, 'Args>
    MapCommand : MapCommand<obj, obj, 'Args> }
    with
      static member
        Create : init:Init<'Model,'Msg, 'Args> * view:View<'Model,'Msg,'View> *
                 ?update:Update<'Model,'Msg, 'CmdMsg, 'Args> *
                 ?onNavigate:OnNavigate<'Model,'Msg,'Args> *
                 ?mapCommand:MapCommand<'Msg, 'CmdMsg, 'Args> ->
                   Page<'View,'Args>
    end

  type private Pages<'View, 'Args> = Map<PageName, Page<'View, 'Args>>
