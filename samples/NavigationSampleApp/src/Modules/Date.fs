namespace Calendar.Modules

open System

type SingleDate = private SingleDate of DateTime

type RangeDate = private {
    Start: DateTime 
    End: DateTime }

type Date =
    | AllDay of SingleDate
    | Range of RangeDate

type EventError =
    | NameEmpty
    | EndDateInvalid

type EventName = private EventName of string

type EventDescription = private EventDescription of string

type Event = {
    Name: EventName
    Description: EventDescription
    Date: Date }

module Event =
    open ResultSymbols

    let createAllDay date =
        AllDay (SingleDate date)

    let createRange startDate endDate =
        match endDate < startDate with
        | true -> Ok <| Range { Start = startDate; End = endDate }
        | false -> Errors [ EndDateInvalid ]

    let createDate startDate endDate isAllDay =
        match isAllDay with
        | true -> Ok (createAllDay startDate)
        | false -> createRange startDate endDate

    let createName name =
        if String.IsNullOrWhiteSpace name then
            Errors [ NameEmpty ]
        else
            Ok (EventName name)

    let createDescription description =
        Ok (EventDescription description)

    let createEvent name description date = {
        Name = name
        Description = description
        Date = date }

    let createEventA name description startDate endDate isAllDay =
        let nameResult = createName name
        let descriptionResult = createDescription description
        let dateResult = createDate startDate endDate isAllDay
        createEvent 
            <!> nameResult 
            <*> descriptionResult 
            <*> dateResult
        