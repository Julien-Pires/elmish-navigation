namespace Elmish.Navigation

  open Elmish

  [<RequireQualifiedAccessAttribute ()>]
  type CmdMsg =
    class
      static member Navigate : page:string * ?args:'a -> Message<'b,'a>
      static member NavigateBack : ?args:'a -> Message<'b,'a>
      static member Of : msg:'a -> Message<'a,'b>
    end

  [<RequireQualifiedAccessAttribute ()>]
  type Cmd =
    class
      static member Navigate : page:string * ?args:'a -> Cmd<Message<'b,'a option>>
      static member NavigateBack : ?args:'a -> Cmd<Message<'b,'a option>>
      static member Of : msg:'a -> Cmd<Message<'a,'b>>
    end