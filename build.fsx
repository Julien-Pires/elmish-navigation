#r "paket:
storage: packages
nuget Fake.IO.FileSystem
nuget Fake.DotNet.Cli
nuget Fake.Core.Target
nuget Fake.Tools.Git
nuget Fake.DotNet.FSFormatting //"
#if !FAKE
#load ".fake/build.fsx/intellisense.fsx"
#r "Facades/netstandard"
#endif

open Fake
open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators
open System.IO

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
            let branch = Environment.environVar "APPVEYOR_REPO_BRANCH"
            match branch with
            | "master" -> options
            | _ -> 
                let jobNumber = Environment.environVar "APPVEYOR_JOB_NUMBER"
                { options with VersionSuffix = Some $"beta{jobNumber}" }) project
    )
)

// Target.create "PublishNuget" (fun _ ->
//     let exec dir =
//         DotNet.exec (fun a ->
//             a.WithCommon (withWorkDir dir)
//         )

//     let args = sprintf "push Fable.Elmish.%s.nupkg -s nuget.org -k %s" (string release.SemVer) (Environment.environVar "nugetkey")
//     let result = exec "src/bin/Release" "nuget" args
//     if (not result.OK) then failwithf "%A" result.Errors

//     let args = sprintf "push Elmish.%s.nupkg -s nuget.org -k %s" (string release.SemVer) (Environment.environVar "nugetkey")
//     let result = exec "netstandard/bin/Release" "nuget" args
//     if (not result.OK) then
//         failwithf "%A" result.Errors
// )

// Build order
"Clean"
  ==> "Restore"
  ==> "Build"
  ==> "Package"

// start build
Target.runOrDefault "Package"
