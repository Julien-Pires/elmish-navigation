namespace Elmish.Navigation
  open Elmish

  module Navigation = begin
    val init : NavigationState
  end

  module Program = begin
    val makeProgramWithNavigation :
      init:('a -> 'b * Cmd<Navigable<'c,'d>>) ->
        update:('c -> 'b -> 'b * Cmd<Navigable<'c,'d>>) ->
          view:('b -> ('c -> unit) -> Navigation<'e,'d> -> 'f) ->
            updateState:('b -> NavigationState -> 'b * Cmd<'c>) ->
              getState:('b -> NavigationState) ->
                pages:(string * Page<'e,'d>) list ->
                  Program<'a,'b,Navigable<'c,'d>,'f>
  end
