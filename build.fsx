open Fake.IO.Shell
#r "paket:
storage: packages
nuget FSharp.Data
nuget Fake.IO.FileSystem
nuget Fake.DotNet.Cli
nuget Fake.DotNet.NuGet
nuget Fake.Core.Target
nuget Fake.Tools.Git
nuget Fake.DotNet.FSFormatting
nuget Fake.BuildServer.AppVeyor"
#if !FAKE
#load ".fake/build.fsx/intellisense.fsx"
#r "Facades/netstandard"
#endif

open System.IO
open FSharp.Data
open Fake
open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.DotNet.NuGet
open Fake.IO
open Fake.IO.Globbing.Operators

module AppVeyor = BuildServer.AppVeyor

type nuspec = XmlProvider<"sample.nuspec">

type Package = {
    Version: string }

type Project = {
    Name: string
    Directory: string
    Package: Package }

let artifactsDir = "artifacts/"
let packagingDir = "packaging/"

// Filesets
let solution = "src/Elmish.Navigation.sln"
let projects = 
    !! "src/**/*.fsproj"
    |> Seq.map (fun project ->
        let projectName = Path.GetFileNameWithoutExtension project
        let projectDir = Path.getDirectory project
        let nuspecContent = File.ReadAllText (Path.Combine [| projectDir; $"{projectName}.nuspec" |])
        let nuspecPackage = nuspec.Parse nuspecContent
        let version =
            let branch = AppVeyor.Environment.RepoBranch
            let semVer = SemVer.parse nuspecPackage.Metadata.Version
            match branch with
            | "master" -> semVer.ToString()
            | _ ->
                let jobNumber = AppVeyor.Environment.BuildNumber
                let nextSemVer = Version.IncPatch semVer
                $"{nextSemVer.ToString()}-alpha{jobNumber}"
        { Name = projectName
          Directory = projectDir
          Package = { Version = version }})

Target.create "Clean" (fun _ ->
    projects
    |> Seq.iter (fun project ->
        Shell.cleanDir $"{project.Directory}/obj"
        Shell.cleanDir $"{project.Directory}/bin")
)

Target.create "Restore" (fun _ ->
    DotNet.restore id solution
)

Target.create "Build" (fun _ ->
    DotNet.build id solution
)

// --------------------------------------------------------------------------------------
// Build a NuGet package
Target.create "Packages" (fun _ ->
    projects
    |> Seq.iter (fun project ->
        cleanDirs [packagingDir]

        !! "**"
        |> GlobbingPattern.setBaseDir "bin/release"
        |> copyFilesWithSubFolder packagingDir

        let nuspecFile = Path.Combine [| project.Directory; $"{project.Name}.nuspec" |]
        NuGet.NuGet (fun p ->
            { p with
                Version = project.Package.Version
                Project = project.Name
                WorkingDir = packagingDir
                OutputPath = artifactsDir
                AccessKey = Environment.environVar "NUGET_API_KEY"
                Publish = true })
            nuspecFile)
)

// Build order
"Clean"
  ==> "Restore"
  ==> "Build"
  ==> "Packages"

// start build
Target.runOrDefault "Packages"
