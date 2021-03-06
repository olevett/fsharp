module Generators.CanonicalData

open System
open System.IO
open System.Collections.Generic
open Serilog
open LibGit2Sharp
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Options

let [<Literal>] private ProblemSpecificationsGitUrl = "https://github.com/exercism/problem-specifications.git";
let [<Literal>] private ProblemSpecificationsBranch = "master";
let [<Literal>] private ProblemSpecificationsRemote = "origin";
let [<Literal>] private ProblemSpecificationsRemoteBranch = ProblemSpecificationsRemote + "/" + ProblemSpecificationsBranch;

let private cloneRepository options =
    if (Directory.Exists(options.CanonicalDataDirectory)) then
        ()
    else
        Log.Information("Cloning repository...")

        Repository.Clone(ProblemSpecificationsGitUrl, options.CanonicalDataDirectory) |> ignore

        Log.Information("Repository cloned.")

let private updateToLatestVersion options =
    Log.Information("Updating repository to latest version...");

    use repository = new Repository(options.CanonicalDataDirectory)
    
    Commands.Fetch(repository, ProblemSpecificationsRemote, Seq.empty, FetchOptions(), null)
    
    let remoteBranch = repository.Branches.[ProblemSpecificationsRemoteBranch];
    repository.Reset(ResetMode.Hard, remoteBranch.Tip);

    Log.Information("Updated repository to latest version.");

let private downloadData options =
    if options.SkipUpdateCanonicalData then
        ()
    else
        cloneRepository options
        updateToLatestVersion options    

let private readCanonicalData options exercise = 
    let exerciseCanonicalDataPath = Path.Combine(options.CanonicalDataDirectory, "exercises", exercise, "canonical-data.json")
    File.ReadAllText(exerciseCanonicalDataPath)

type CanonicalDataConverter() =
    inherit JsonConverter()

    let createCanonicalDataCasePropertiesFromJToken (jToken: JToken) =
        jToken.ToObject<IDictionary<string, obj>>()
        |> Dict.toMap

    let createDescriptionPathFromJToken (jToken: JToken): string list =
        let descriptionFromJToken (currentToken: JToken) =
            match currentToken.SelectToken("description") with
            | null ->  None
            | description -> Some (description.ToObject<string>())

        jToken
        |> Json.parentsAndSelf
        |> List.choose descriptionFromJToken

    let createCanonicalDataCaseFromJToken (jToken: JToken) =
        { Properties = createCanonicalDataCasePropertiesFromJToken jToken
          DescriptionPath = createDescriptionPathFromJToken jToken }

    let createCanonicalDataCasesFromJToken (jToken: JToken) =  
        jToken.["cases"].SelectTokens("$..*[?(@.property)]")
        |> Seq.map createCanonicalDataCaseFromJToken
        |> Seq.toList

    let createCanonicalDataFromJToken (jToken: JToken) =
        { Exercise = jToken.["exercise"].Value<string>()
          Version = jToken.["version"].Value<string>()
          Cases = createCanonicalDataCasesFromJToken jToken }

    override __.WriteJson(_: JsonWriter, _: obj, _: JsonSerializer) = failwith "Not supported"

    override __.ReadJson(reader: JsonReader, _: Type, _: obj, _: JsonSerializer) =
        let jToken = JToken.ReadFrom(reader)
        createCanonicalDataFromJToken jToken :> obj

    override __.CanConvert(objectType: Type) = objectType = typeof<CanonicalData>

let private convertCanonicalData canonicalDataContents = 
    JsonConvert.DeserializeObject<CanonicalData>(canonicalDataContents, CanonicalDataConverter()) 

let parseCanonicalData options = 
    downloadData options 
    readCanonicalData options >> convertCanonicalData