namespace Elmish.Navigation

open Elmish

module Program = begin
    val withNavigation :
      pages:('a * PageTemplate<(Navigator<'a,'b> -> 'c),'a,'b>) list ->
        defaultPage:'a ->
          program:Program<'d,'e,obj,(Navigation<'a,'c,'b> -> 'f)> ->
            Program<'d,NavigationModel<'e,'a>,ProgramMsg<obj,'a,'b>,'f>
        when 'a : comparison
end

