namespace Elmish.Navigation

  open Elmish

  type PageName = | PageName of string

  type PageModel =
    { Name: PageName
      Model: obj }

  type NavigationState =
    { Stack: PageModel list }

  type INavigationModel<'a when 'a :> INavigationModel<'a>> =
    interface
      abstract member GetNavigation : unit -> NavigationState
      abstract member UpdateNavigation : NavigationState -> 'a
    end

  type NavigationMessage<'args> =
    | Navigate of string
    | NavigateParams of string * 'args option
    | NavigateBack
    | NavigateBackParams of 'args option

  type Message<'msg,'args> =
    | Message of 'msg
    | Navigation of NavigationMessage<'args>
    with
      static member Downcast : msg:Message<'a,'b> -> Message<obj,'b>
      static member Upcast : msg:Message<obj,'args> -> Message<'msg,'args>
    end

  type ProgramMsg<'msg,'args> =
    | App of Message<'msg,'args>
    | Page of Message<'msg,'args>

  type OnNavigationEventArgs<'args> =
    { Source: PageName option
      Parameters: 'args option }
    with
      member Map : unit -> OnNavigationEventArgs<'a>
    end

  type Dispatch<'msg> = 'msg -> unit

  type Init<'model,'cmdMsg,'args> = unit -> 'model * Cmd<Message<'cmdMsg,'args>>

  type MapCommand<'msg,'cmdMsg,'args> = 'cmdMsg -> Cmd<Message<'msg,'args>>

  type Update<'model,'msg,'cmdMsg,'args> =
    'msg -> 'model -> 'model * Cmd<Message<'cmdMsg,'args>>

  type View<'model,'msg,'view> = 'model -> Dispatch<'msg> -> 'view

  type OnNavigate<'model,'msg,'args> =
    'model -> OnNavigationEventArgs<'args> -> 'model * Cmd<Message<'msg,'args>>

  type Page<'view,'args> =
    { Init: Init<obj,obj,'args>
      Update: Update<obj,obj,obj,'args>
      View: View<obj,obj,'view>
      OnNavigate: OnNavigate<obj,obj,'args>
      MapCommand: MapCommand<obj,obj,'args> }
    with
      static member
        Create : init:Init<'model,'cmdMsg,'args> * view:View<'model,'msg,'view> *
                 ?update:Update<'model,'msg,'cmdMsg,'args> *
                 ?onNavigate:OnNavigate<'model,'msg,'args> *
                 ?mapCommand:MapCommand<'msg,'cmdMsg,'args> -> Page<'view,'args>
    end

  type Pages<'view,'args> = Map<PageName,Page<'view,'args>>