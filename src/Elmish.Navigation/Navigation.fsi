namespace Elmish.Navigation
  open Elmish

  module Navigation = begin
    val init : NavigationState
  end

  module Program = begin
    val withNavigation :
      pages:(string * Navigation.Page<'a,'b>) list ->
        updateState:('model -> NavigationState -> 'model * Cmd<'msg>) ->
          getState:('model -> NavigationState) ->
            mapAppCommand:('msg -> Cmd<Navigable<'msg,'b>>) ->
              program:Program<unit,'model,'msg,
                              (Navigation<'a,'b> -> 'c)> ->
                Program<unit,'model,Navigable<'msg,'b>,'c>
  end
