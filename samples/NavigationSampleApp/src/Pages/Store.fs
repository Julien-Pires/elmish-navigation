namespace Calendar.Pages

open System
open Calendar.Modules

type EventStore = {
    Days: int64 list
    Events: Map<string, Event>
    EventsDayMapping: Map<int64, string Set> }

module EventStore =
    let empty = {
        Days = [] 
        Events = Map.empty
        EventsDayMapping = Map.empty }

    let getDays event =
        let range =
            match event.Date with
            | Day range -> range
            | Hours range -> range
        let diffDays = (range.End - range.Start).Days + 1
        let oneDay = int64(TimeSpan(1, 0, 0, 0).TotalSeconds)
        let startDay = DateTimeOffset(range.Start.Date).ToUnixTimeSeconds()
        Seq.init diffDays int64
        |> Seq.map (fun index -> startDay + (oneDay * index))
        |> Seq.toList

    let add event store =
        let appliedDays = getDays event
        let mapping = appliedDays |> List.fold (fun acc day ->
            match acc |> Map.tryFind day with
            | Some events -> acc |> Map.add day (Set.add event.Id events)
            | None -> acc |> Map.add day (Set [event.Id])) store.EventsDayMapping
        let days =
            appliedDays |> List.fold (fun acc day ->
                match store.EventsDayMapping |> Map.tryFind day with
                | Some _ -> acc
                | None -> day::acc ) store.Days
        { Days = days |> List.sort 
          Events = store.Events |> Map.add event.Id event
          EventsDayMapping = mapping }