namespace Elmish.Navigation

  open Elmish

  module Navigation = begin
    val empty : NavigationState
  end

  module Program = begin
    val mkProgramWithNavigation :
      init:(unit -> 'a * 'b list) ->
        update:('c -> 'a -> 'a * 'b list) ->
          view:('a -> ('c -> unit) -> 'd option -> 'e) ->
            mapCommand:('b -> Sub<Message<'c,'f>>) ->
              pages:(string * Page<'d,'f>) list ->
                Program<unit,'a,ProgramMsg<'c,'f>,'e>
        when 'a :> INavigationModel<'a>

    val mkSimpleWithNavigation :
      init:(unit -> 'a * Cmd<Message<'b,'c>>) ->
        update:('b -> 'a -> 'a * Cmd<Message<'b,'c>>) ->
          view:('a -> ('b -> unit) -> 'd option -> 'e) ->
            pages:(string * Page<'d,'c>) list ->
              Program<unit,'a,ProgramMsg<'b,'c>,'e>
        when 'a :> INavigationModel<'a>
  end