namespace Elmish.Navigation

open Elmish

type Dispatch<'msg> = 'msg -> unit

type Init<'model,'cmdMsg> = unit -> 'model * Cmd<'cmdMsg>

type MapCommand<'msg,'cmdMsg> = 'cmdMsg -> Cmd<'msg>

type Update<'model,'msg,'cmdMsg> = 'msg -> 'model -> 'model * Cmd<'cmdMsg>

type View<'model,'msg,'view> = 'model -> Dispatch<'msg> -> 'view

type OnNavigate<'model,'msg,'pageName,'args> = 'model -> OnNavigationEventArgs<'pageName,'args> -> 'model * Cmd<'msg>

type PageTemplate<'view,'pageName,'args> = { 
    Init: Init<obj,obj>
    Update: Update<obj,obj,obj>
    View: View<obj,obj,'view>
    OnNavigate: OnNavigate<obj,obj,'pageName,'args>
    MapCommand: MapCommand<obj,obj> }
    with
      static member
        Create : init:Init<'model,'cmdMsg> * view:View<'model,'msg,'view> *
                 ?update:Update<'model,'msg,'cmdMsg> *
                 ?onNavigate:OnNavigate<'model,'msg,'pageName,'args> *
                 ?mapCommand:MapCommand<'msg,'cmdMsg> ->
                   PageTemplate<'view,'pageName,'args>
    end