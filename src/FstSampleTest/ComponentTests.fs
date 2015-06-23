﻿module SampleComponentTests

open FsUnit
open NUnit.Framework
open SampleComponents
open Storm
open FsJson
open System.Threading
open System

let conf = {PidDir=""; TaskId=""; Json= jval ""}
let next = jval ["command",NEXT]

[<Test>]
let ``spout emits``() = 
    let results = ref []
    let s = spout (StormTest.simpleSpoutRunner (fun x -> results := x :: results.Value) [next; next]) conf
    Async.RunSynchronously s
    match results.Value with
    | x::y::[] -> ()
    | _ -> Assert.Fail "Must have emitted exactly two!"


[<Test>]
let ``bolt adds``() = 
    let results = ref []
    let b = addOneBolt (StormTest.autoAckBoltRunner [tuple [123]] Console.WriteLine) (fun tag desc -> Console.WriteLine("{0}: {1}", tag, desc)) (fun t -> results := t :: results.Value) conf
    Async.RunSynchronously (b, 2000)
    match results.Value with
    | x::[] -> Assert.AreEqual (124, x?tuple.[0].ValI)
    | _ -> Assert.Fail "Head, no tail, that's the deal!"


[<Test>]
let ``bolt consumes``() = 
    let results = ref []
    let b = resultBolt (StormTest.autoAckBoltRunner [tuple [123]] Console.WriteLine) (fun tag desc -> results := (tag,desc) :: results.Value) conf
    Async.RunSynchronously (b, 2000)
    match results.Value with
    | x::[] -> Assert.AreEqual (("x", sprintf "%A" 123), x )
    | _ -> Assert.Fail "Head, no tail, that's the deal!"
    