﻿module Day16_attempt2
open Day16
open System.Collections.Generic

let addToArray array item = 
    Array.append array [| item |]


let isShorter distance maybeDistance = 
    match maybeDistance with 
        | None -> true
        | Some distance2 -> 
            if distance < distance2 then true else false

let rec findDistanceRec (target:Valve) (destination:Valve) (visitedValves: Valve array) (calculatedDistances: Dictionary<(string * string), int>) = 
    let mutable index = 0
    let mutable foundPath = false
    let mutable keepLooping = true
    let mutable shortestDistance = None
    while (keepLooping) do 
        let connection = target.Connections[index]
        if target.Id = connection.Id then () // don't want to go back from where we came from
        else if contains connection visitedValves then () // don't want to go through the same valve twice
        else if connection.Id = destination.Id then 
            shortestDistance <- 1 |> Some
            foundPath <- true
        else 
            let key1 = (connection.Id, destination.Id)
            let key2 = (destination.Id, connection.Id)

            let found1, value1 = calculatedDistances.TryGetValue key1
            let found2, value2 = calculatedDistances.TryGetValue key2

            if found1
            then 
                if isShorter value1 shortestDistance 
                then 
                    shortestDistance <- Some (value1 + 1)
                else ()
            else if found2
            then 
                if isShorter value2 shortestDistance 
                then shortestDistance <- Some (value2 + 1) 
                else ()
            else 
                let distanceFromConnectionToDestination = findDistanceRec connection destination (addToArray visitedValves target) calculatedDistances
                match distanceFromConnectionToDestination with 
                    | None -> ()
                    | Some distance -> 
                        let newDistance = distance + 1
                        if isShorter newDistance shortestDistance 
                        then shortestDistance <- Some newDistance 
                        else ()
            ()

        index <- index + 1
        if index = target.Connections.Length then keepLooping <- false
        else if foundPath then keepLooping <- false 
        else ()

    if shortestDistance.IsSome then 
        calculatedDistances[(target.Id, destination.Id)] <- shortestDistance.Value
        ()
    else
        ()
    
    shortestDistance

let findDistance target destination calculatedDistances = 
    let distance = findDistanceRec target destination Array.empty calculatedDistances
    distance

let rec calculateReleasedPressure startValve nonZeroValves calculatedDistances timeLeft pressure maxPressure = 
    let mutable a = maxPressure

    for nextValve in nonZeroValves do 
        let distance = findDistance startValve nextValve calculatedDistances
        let newTimeLeft = timeLeft - distance.Value - 1
        if newTimeLeft >= 0
        then 
            let newPressure = pressure + nextValve.Rate * newTimeLeft
            if newPressure > a then a <- newPressure else ()

            if (newTimeLeft < 3) 
            then () 
            else 
                let nonZeroValvesWithoutNextValve = nonZeroValves |> Array.filter (fun v -> v.Id <> nextValve.Id)
                let maxPressure = if newPressure > maxPressure then newPressure else maxPressure
                let newMaxPressure = calculateReleasedPressure nextValve nonZeroValvesWithoutNextValve calculatedDistances newTimeLeft newPressure maxPressure
                if newMaxPressure > a then a <- newMaxPressure else ()
        else
            ()
    a

let part1 (input: string[]) = 
    let valves = Day16.parseValves input
    let nonZeroValves = valves |> Seq.filter (fun valve -> valve.Rate <> 0) |> Array.ofSeq
    let startValve = getStartValve valves
    let calculatedDistances = new Dictionary<(string * string), int>()
    let timeLeft = 30
    let maxPressure = calculateReleasedPressure startValve nonZeroValves calculatedDistances timeLeft 0 0
    printfn "%d" maxPressure
    0

let part2 (input: string[]) = 
    // parse
    printfn "parsing..."
    let valves = parseValves input 
    let startValve = valves |> getStartValve
    let nonZeroValves = valves |> Seq.filter (fun valve -> valve.Rate <> 0) |> Array.ofSeq
    let calculatedDistances = new Dictionary<(string * string), int>()

    let mutable ourMaxPressure = 0

    let splitTargetValves = splitTargetValves nonZeroValves

    for (mine, elephants) in splitTargetValves do 
        printfn $"finding max pressure. I have {Seq.length mine} valves, elephant has {Seq.length elephants}"
        let myPressure = calculateReleasedPressure startValve mine calculatedDistances 26 0 0
        let elephantsPressure = calculateReleasedPressure startValve elephants calculatedDistances 26 0 0
        printfn $"my pressure: {myPressure}. elephant: {elephantsPressure}. total {myPressure + elephantsPressure}"
        let ourPressure = myPressure + elephantsPressure
        if ourPressure > ourMaxPressure then ourMaxPressure <- ourPressure else ()
    printfn "%d" ourMaxPressure
    ourMaxPressure

let part1TestInput () =
    part1 (inputReader.readLines "Day16/testInput.txt" |> Array.ofSeq)

let part1RealInput () =
    part1 (inputReader.readLines "Day16/input.txt" |> Array.ofSeq)

let part2TestInput () =
    part2 (inputReader.readLines "Day16/testInput.txt" |> Array.ofSeq)