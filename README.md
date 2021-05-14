# Elmish-Navigation

[![Build status](https://ci.appveyor.com/api/projects/status/h0c83n5niwc8gm35/branch/main?svg=true)](https://ci.appveyor.com/project/Takumii/elmish-navigation/branch/main)

**Elmish-navigation** ease the navigation for application that follows MVU pattern. It makes the creation and management of pages and models less tedious.

The library allow to create each page as if it were an independant application with their own init, update and view workflow.

## Installation

The library is fully written in F# and use dotnet SDK. It can be installed through `dotnet nuget`:
```
dotnet add package Elmish.Navigation
```

or `paket`:
```
paket add Elmish.Navigation
```

## How-To

**Elmish-navigation** is a plugin that extends `Program` type from **Elmish**. Like others `Program` plugin, you must call it once the program is created. The name of the method is `withNavigation`. 

Here is the minimum setup to use it:

```fsharp
module App

open Elmish
open Elmish.Navigation

type Model = {
    Counter: int }

let init () = 
    { Counter = 0 }, []

let update msg model =
    model, []

module R = Fable.ReactNative.Helpers
 
let view model dispatch navigation =
    R.view [][]

Program.mkProgram init update view
|> Program.withNavigation [] "Home"
|> Program.withConsoleTrace
|> Program.run
```

`Program.withNavigation` takes two arguments:
* Pages: It's a list of name associated to a page template.
* Default: Name of the default page used at the first screen.

Few notes regarding the way the library handle messages and pages. We make a clear distinction between message sent by the App and message sent from a page. 

We called `App` the init/update/view that you have at the root level of your application, the ones passed to the `mkProgram` method when you init the app. You will probably use the app methods to handle messags and views that are global to your application.

And we called `Page` the individuals init/update/view of each pages. Messages, models and views from `Page` are usually constrained to the page and does not leak outside.

To resume, when sending messages from the update or dispatch of the `App`, they will be received by the `update` method of the `App`. And when sending from a `Page`, it will be received by the `update` method of the same page.