namespace ChironB.Benchmarks

open Chiron
open BenchmarkDotNet.Attributes
open System.Text

module Examples =
    module E = Chiron.Serialization.Json.Encode
    module EI = Chiron.Inference.Json.Encode
    module Inline =
        module Explicit =
            type Testing =
                { one: int option
                  two: bool
                  three: int }
                static member Encode (x: Testing, jObj: JsonObject): JsonObject =
                    jObj
                    |> E.optional E.int "1" x.one
                    |> E.required E.bool "2" x.two
                    |> E.required E.int "3" x.three
                static member ToJson (x: Testing): Json =
                    Testing.Encode (x, JsonObject.empty)
                    |> E.jsonObject
            let testObject = { one = None; two = true; three = 42 }

        module Inferred =
            type Testing =
                { one: int option
                  two: bool
                  three: int }
                static member Encode (x: Testing, jObj: JsonObject): JsonObject =
                    jObj
                    |> EI.optional "1" x.one
                    |> EI.required "2" x.two
                    |> EI.required "3" x.three
                static member ToJson (x: Testing): Json =
                    Testing.Encode (x, JsonObject.empty)
                    |> Chiron.Inference.Json.encode
            let testObject = { one = None; two = true; three = 42 }

    module InModule =
        module Explicit =
            type Testing =
                { one: int option
                  two: bool
                  three: int }
            module Testing =
                let encode x jObj =
                    jObj
                    |> E.optional E.int "1" x.one
                    |> E.required E.bool "2" x.two
                    |> E.required E.int "3" x.three
            type Testing with
                static member Encode (x: Testing, jObj: JsonObject): JsonObject =
                    Testing.encode x jObj
                static member ToJson (x: Testing): Json =
                    E.buildWith Testing.encode x
            let testObject = { one = None; two = true; three = 42 }

        module Inferred =
            type Testing =
                { one: int option
                  two: bool
                  three: int }
            module Testing =
                let encode x jObj =
                    jObj
                    |> EI.optional "1" x.one
                    |> EI.required "2" x.two
                    |> EI.required "3" x.three
            type Testing with
                static member ToJson (x: Testing): Json =
                    E.buildWith Testing.encode x
            let testObject = { one = None; two = true; three = 42 }

    module Obsolete =
        open ChironObsolete
        module ComputationExpression =
            type Testing =
                { one: int option
                  two: bool
                  three: int }
                static member ToJson (x: Testing): Json<unit> = json {
                    do! Json.writeUnlessDefault "1" None x.one
                    do! Json.write "2" x.two
                    do! Json.write "3" x.three
                }
            let testObject = { one = None; two = true; three = 42 }

        module Operators =
            open ChironObsolete.Operators
            type Testing =
                { one: int option
                  two: bool
                  three: int }
                static member ToJson (x: Testing): Json<unit> =
                       Json.writeUnlessDefault "1" None x.one
                    *> Json.write "2" x.two
                    *> Json.write "3" x.three
            let testObject = { one = None; two = true; three = 42 }

[<Config(typeof<CoreConfig>)>]
type Encoding () =
    [<Benchmark>]
    member x.Inline_Explicit () =
        Inference.Json.encode Examples.Inline.Explicit.testObject

    [<Benchmark>]
    member x.InModule_Explicit () =
        Inference.Json.encode Examples.InModule.Explicit.testObject

    [<Benchmark>]
    member x.Inline_Inferred () =
        Inference.Json.encode Examples.Inline.Inferred.testObject

    [<Benchmark>]
    member x.InModule_Inferred () =
        Inference.Json.encode Examples.InModule.Inferred.testObject

    [<Benchmark(Baseline=true)>]
    member x.Version6_ComputationExpression () =
        ChironObsolete.Mapping.Json.serialize Examples.Obsolete.ComputationExpression.testObject

    [<Benchmark>]
    member x.Version6_Operators () =
        ChironObsolete.Mapping.Json.serialize Examples.Obsolete.Operators.testObject

module D = Samples.Json.Decode

[<Config(typeof<CoreConfig>)>]
type DecodeMedium () =
    [<Benchmark>]
    member x.DeserializeA () =
        D.complexType Samples.Constants.jsons.[0]

    [<Benchmark>]
    member x.DeserializeB () =
        D.complexType Samples.Constants.jsons.[1]

    [<Benchmark>]
    member x.DeserializeAWithError () =
        D.childType Samples.Constants.jsons.[0]

    [<Benchmark>]
    member x.DeserializeBWithError () =
        D.childType Samples.Constants.jsons.[1]
