namespace Calendar.Modules

open System

type RangeDate = {
    Start: DateTimeOffset
    End: DateTimeOffset }

type Date =
    | Day of RangeDate
    | Hours of RangeDate

type EventError =
    | NameEmpty
    | EndDateInvalid

type EventName = EventName of string

type EventDescription = EventDescription of string

type Event = {
    Id: string
    Name: EventName
    Description: EventDescription
    Date: Date }

module Event =
    open ResultSymbols

    let createRange startDate endDate =
        match endDate > startDate with
        | true -> Ok { Start = startDate; End = endDate }
        | false -> Errors [ EndDateInvalid ]

    let createDate startDate endDate isAllDay =
        createRange startDate endDate
        |> Result.bind (fun range ->
            match isAllDay with
            | true -> Ok(Day range)
            | false -> Ok(Hours range))

    let createName name =
        if String.IsNullOrWhiteSpace name then
            Errors [ NameEmpty ]
        else
            Ok (EventName name)

    let createDescription description =
        Ok (EventDescription description)

    let createEvent name description date = {
        Id = Guid.NewGuid().ToString()
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