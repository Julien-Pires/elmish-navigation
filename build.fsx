#r "paket:
storage: packages
nuget Fake.IO.FileSystem
nuget Fake.DotNet.Cli
nuget Fake.Core.Target
nuget Fake.Tools.Git
nuget Fake.DotNet.FSFormatting
nuget Fake.BuildServer.AppVeyor"
#if !FAKE
#load ".fake/build.fsx/intellisense.fsx"
#r "Facades/netstandard"
#endif

open System.IO
open Fake
open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators

module AppVeyor = Fake.BuildServer.AppVeyor

// Filesets
let solution = "src/Elmish.Navigation.sln"
let projects = !! "src/**/*.fsproj"

Target.create "Clean" (fun _ ->
    projects
    |> Seq.iter (fun project ->
        let dir = Path.GetDirectoryName project 
        Shell.cleanDir $"{dir}/obj"
        Shell.cleanDir $"{dir}/bin"
    )
)

Target.create "Restore" (fun _ ->
    DotNet.restore id solution
)

Target.create "Build" (fun _ ->
    DotNet.build id solution
)

// --------------------------------------------------------------------------------------
// Build a NuGet package
Target.create "Package" (fun _ ->
    projects
    |> Seq.iter (fun project -> 
        DotNet.pack (fun options ->
            let branch = AppVeyor.Environment.RepoBranch
            match branch with
            | "master" -> options
            | _ -> 
                let jobNumber = AppVeyor.Environment.BuildNumber
                { options with VersionSuffix = Some $"alpha.{jobNumber}" }) project
    )
)

Target.create "PublishNuget" (fun _ ->
    let packages = !! "src./**/*.nupkg"
    packages |> Seq.iter (fun package ->
        DotNet.nugetPush (fun options -> 
            let pushParams = options.PushParams
            { options with PushParams = { pushParams with ApiKey = Some (Environment.environVar "NUGET_API_KEY") }}) package)
)

// Build order
"Clean"
  ==> "Restore"
  ==> "Build"
  ==> "Package"
  ==> "PublishNuget"

// start build
Target.runOrDefault "PublishNuget"
