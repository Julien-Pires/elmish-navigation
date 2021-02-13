namespace Elmish.Navigation
  open Elmish

  module Cmd = begin
    val internal cast : cmd:Elmish.Cmd<obj> -> Elmish.Cmd<'Msg>
  end
