namespace Elmish.Navigation

open Elmish

module internal Navigation = begin
    module internal Program = begin
      val init : model:'a -> NavigationModel<'a,'b>
      val update :
        userUpdate:('a -> 'b -> 'b * Cmd<'c>) ->
          pages:Map<PageName<'d>,PageTemplate<'e,'d,'f>> ->
            msg:ProgramMsg<'a,'d,'f> ->
              model:NavigationModel<'b,'d> ->
                NavigationModel<'b,'d> * Cmd<ProgramMsg<'c,'g,'h>>
          when 'd : comparison
      val view :
        userView:('a -> (obj -> unit) -> Navigation<'b,'c,'d> -> 'e) ->
          pages:Map<PageName<'b>,PageTemplate<('f -> 'c),'g,'h>> ->
            model:NavigationModel<'a,'b> ->
              dispatch:(ProgramMsg<obj,'i,'j> -> unit) -> navigator:'f -> 'e
          when 'b : comparison and 'f :> Navigator<'b,'d>
    end
end