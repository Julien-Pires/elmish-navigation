namespace NavigationSampleApp

open Fable.React
open Elmish.Navigation

type Class =
    | Knight
    | Wizard
    | Necromancer

type Race =
    | Human
    | Elves
    | Ork
    | Dwarf

type Name = Name of string

type Character = {
    Name: Name
    Class: Class }

type NavigationArgs = 
    | CharacterCreated

type View = (NavigationMessage<NavigationArgs> -> unit) -> ReactElement