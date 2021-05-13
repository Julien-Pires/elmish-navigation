namespace Elmish.Navigation

type PageId = | PageId of System.Guid

type PageName<'a> = | PageName of 'a

type PageModel<'pageName> = { 
    Id: PageId
    Name: PageName<'pageName>
    Model: obj }

type NavigationState<'pageName> = {
    Pages: Map<PageId,PageModel<'pageName>>
    Stack: PageId list }

type NavigationModel<'model,'pageName> = {
    Navigation: NavigationState<'pageName>
    Model: 'model }

type MessageSource<'msg> =
    | App of 'msg
    | Page of 'msg * PageId
    with
      static member Upcast : msg:MessageSource<obj> -> MessageSource<'a>
    end

type NavigationMessage<'pageName,'args> =
    | Navigate of 'pageName
    | NavigateParams of 'pageName * 'args option
    | NavigateBack
    | NavigateBackParams of 'args option
    with
      static member
        Downcast : msg:NavigationMessage<'a,'b> -> NavigationMessage<'a,obj>
      static member
        Upcast : msg:NavigationMessage<'pageName,obj> ->
                   NavigationMessage<'pageName,'a>
    end

type ProgramMsg<'msg,'pageName,'args> =
    | Message of MessageSource<'msg>
    | Navigation of NavigationMessage<'pageName,'args>

type Navigator<'pageName,'args> =
    class
      new : unit -> Navigator<'pageName,'args>
      member Navigate : page:'pageName -> unit
      member NavigateBackWith : args:'args -> unit
      member NavigateWith : page:'pageName -> args:'args -> unit
      member NavigateBack : unit
      member Received : IEvent<NavigationMessage<'pageName,'args>>
    end

type CurrentPage<'pageName,'view> = {
    Id: PageId
    Name: PageName<'pageName>
    Render: unit -> 'view }

type Navigation<'pageName,'view,'args> = {
    CurrentPage: CurrentPage<'pageName,'view> option
    Navigator: Navigator<'pageName,'args> }

type OnNavigationEventArgs<'pageName,'args> = {
    Source: 'pageName option
    Parameters: 'args option }
    with
      member Map : unit -> OnNavigationEventArgs<'pageName,'a>
    end