



module Cmd
val cast : cmd:Elmish.Cmd<obj> -> Elmish.Cmd<'Msg>

namespace Elmish.Navigation
  module Navigation = begin
    type PageName = | PageName of string
    type OnNavigationParameters<'Params> =
      { Source: PageName option
        Parameters: 'Params option }
      with
        member Map : unit -> OnNavigationParameters<'a>
      end
    type Dispatch<'Msg> = 'Msg -> unit
    type Init<'Model,'Msg> = unit -> 'Model * Cmd<'Msg>
    type Update<'Model,'Msg> = 'Msg -> 'Model -> 'Model * Cmd<'Msg>
    type View<'Model,'Msg,'View> = 'Model -> Dispatch<'Msg> -> 'View
    type OnNavigate<'Model,'Msg,'Params> =
      'Model -> OnNavigationParameters<'Params> -> 'Model * Cmd<'Msg>
    type Page<'View,'Params> =
      class
        new : init:Init<obj,obj> * update:Update<obj,obj> *
              view:View<obj,obj,'View> * onNavigate:OnNavigate<obj,obj,'Params> ->
                Page<'View,'Params>
        member Init : Init<obj,obj>
        member
          OnNavigate : (obj -> OnNavigationParameters<'Params> -> obj * Cmd<obj>)
        member Update : (obj -> obj -> obj * Cmd<obj>)
        member View : (obj -> Dispatch<obj> -> 'View)
        static member
          Create : init:Init<'Model,'Msg> * view:View<'Model,'Msg,'View> *
                   ?update:Update<'Model,'Msg> *
                   ?onNavigate:OnNavigate<'Model,'Msg,'Params> ->
                     Page<'View,'Params>
      end
    type Pages<'View,'Params> = Map<PageName,Page<'View,'Params>>
    type PageModel =
      { Name: PageName
        Model: obj }
    type NavigationState =
      { Stack: PageModel list }
    type NavigationMessage<'Params> =
      | Navigate of PageName
      | NavigateParams of PageName * 'Params option
      | NavigateBack
      | NavigateBackParams of 'Params option
    type Navigable<'msg,'Params> =
      | AppMsg of 'msg
      | PageMsg of 'msg
      | NavigationMsg of NavigationMessage<'Params>
    type Navigation<'Page,'Params> =
      { Dispatch: Dispatch<NavigationMessage<'Params>>
        CurrentPage: 'Page option }
    val currentPage : navigationState:NavigationState -> PageModel option
    val navigate :
      name:PageName ->
        navigationParameters:OnNavigationParameters<'Params> ->
          navigationState:NavigationState ->
            pages:Pages<'View,'Params> -> NavigationState * Cmd<'a>
    val navigateBack :
      navigationParameters:OnNavigationParameters<'Params> ->
        navigationState:NavigationState ->
          pages:Pages<'View,'Params> -> NavigationState * Cmd<'a>
    val update :
      msg:'a ->
        navigationState:NavigationState ->
          pages:Pages<'View','Params> -> NavigationState * Cmd<'b>
    val view :
      dispatch:(Navigable<'a,'a0> -> unit) ->
        navigationState:NavigationState ->
          pages:Pages<'View,'Params> -> 'View option
    val init : NavigationState
  end
  module Program = begin
    val ( |Navigate|_| ) :
      _arg1:Navigation.NavigationMessage<'a> ->
        (Navigation.PageName * 'a option) option
    val ( |NavigateBack|_| ) :
      _arg1:Navigation.NavigationMessage<'a> -> 'a option option
    val map : model:'a * cmd:Cmd<'b> -> 'a * Cmd<Navigation.Navigable<'b,'c>>
    val createNavigationParams :
      parameters:'a option ->
        navigationState:Navigation.NavigationState ->
          Navigation.OnNavigationParameters<'a>
    val init :
      userInit:(unit -> 'a * Cmd<'b>) ->
        unit -> 'a * Cmd<Navigation.Navigable<'b,'c>>
    val update :
      getState:('a -> Navigation.NavigationState) ->
        updateState:('a -> Navigation.NavigationState -> 'b * Cmd<'c>) ->
          pages:Navigation.Pages<'d,'e> ->
            userUpdate:('f -> 'a -> 'b * Cmd<'c>) ->
              msg:Navigation.Navigable<'f,'e> ->
                model:'a -> 'b * Cmd<Navigation.Navigable<'c,'g>>
    val view :
      getState:('a -> Navigation.NavigationState) ->
        pages:Navigation.Pages<'b,'c> ->
          userView:('a -> ('d -> unit) -> Navigation.Navigation<'b,'e> -> 'f) ->
            model:'a -> dispatch:(Navigation.Navigable<'d,'e> -> unit) -> 'f
    val setState :
      userSetState:('a -> ('b -> 'c) -> 'd) ->
        model:'a -> dispatch:(Navigation.Navigable<'b,'e> -> 'c) -> 'd
    val subs :
      userSubscribe:('a -> Cmd<'b>) ->
        model:'a -> Cmd<Navigation.Navigable<'b,'c>>
    val start :
      program:Program<unit,'a,'b,(Navigation.Navigation<'View,'Params> -> 'c)> ->
        pages:(string * Navigation.Page<'View,'Params>) list ->
          updateState:('a -> Navigation.NavigationState -> 'a * Cmd<'b>) ->
            getState:('a -> Navigation.NavigationState) ->
              Program<unit,'a,Navigation.Navigable<'b,'Params>,'c>
    val withNavigation :
      pages:(string * Navigation.Page<'a,'b>) list ->
        updateState:('model -> Navigation.NavigationState -> 'model * Cmd<'msg>) ->
          getState:('model -> Navigation.NavigationState) ->
            program:Program<unit,'model,'msg,
                            (Navigation.Navigation<'a,'b> -> 'c)> ->
              Program<unit,'model,Navigation.Navigable<'msg,'b>,'c>
  end

