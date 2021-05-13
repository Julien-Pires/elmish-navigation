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

Here is the minimal setup to use the library.
```fsharp
let a = 10
```