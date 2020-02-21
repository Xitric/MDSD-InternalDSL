# Introduction
This is an internal DSL developed for describing property based tests within C#. It was developed for the course Model Driven Software Development at the University of Southern Denmark.

## DSL Overview
The DSL allows for creating named tests. Each test performs a configurable number of random samples from a composable input generator. These inputs are provided to the properties of the test. A property will compare the output from a function under test against an arbitrary number of expectations. Such expectations can be comparisons with the outputs of other functions or simply literals. These expectations can further be composed into recursive hierarchies and combined with the boolean operators `and` and `or`.

An example instance is seen below:

```csharp
Test("Test of Add function")
    .Samples(10000)
    .Generator(Pair(
        PosSmallInteger, PosSmallInteger))
    .Property("Returns the sum of its input")
        .Given(i => i.Item1 > 0)
        .Given(i => i.Item2 > 0)
        .Then(i => Add(i.Item1, i.Item2).I())
        .BeginBlock()
            .Equals(i => i.Item1 + i.Item2)
            .Or()
            .Equals(i => i.Item2 + i.Item1)
        .EndBlock()
        .And()
        .BeginBlock()
            .IsGreaterThan(0)
            .And()
            .BeginBlock()
                .IsNotEqual(int.MaxValue)
                .Or()
                .IsNotEqual(1)
            .EndBlock()
        .EndBlock()
    .Property("Is not the difference")
        .Then(i => Add(i.Item1, i.Item2).I())
        .IsNotEqual(i => i.Item1 - i.Item2)
    .Build();
```

# DSL design
- Method chaining, function nesting, and nested lambdas
- Hierarchy for generators and boolean comparisons

## Progressive interfaces
- Progressive interfaces

## Alternatives
- Multiple types of DSL for evaluating pros and cons

## Generators
- How generators are made

# Parsing
- Context variables (stacks, nesting depth)

## Generics and primitives
- Generics in general and how it helps us (autocomplete)
- Primitives and autoboxing for generics

# Separation of concerns
- Separation of DSL, semantic model, and execution
