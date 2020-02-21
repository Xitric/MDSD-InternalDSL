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
The DSL has been designed using a combination of design patterns for fluent interfaces. Most prominently is method chaining, which is started by using a static method `Test()`. This method can be statically imported in the client to reduce noise in the syntax.

For the generators I have opted for nested functions. This allows for me to quickly compose generators together without using any sort of context variables. Instead, I make use of statically imported functions such as `Pair()` and `List()` that simply take primitive generators as inputs and return new generators.

```csharp
var crazyGenerator = List(
    Pair(
        Triplet(Str, PosInteger, Character), 
        Array(
            Pair(Double, Float)
            )
        )
    )
```

I also experimented with nested lambdas for composing the boolean expressions inside of properties. The nested lambda expressions make for a simpler implementation, but also a noisier syntax. More on this [later](#Alternatives). Regardless, the DSL has a hierarchic structure for both generators and boolean expressions.

## Progressive interfaces
To assist the user as much as possible, I made use of progressive interfaces when creating the builder. For instance, the `Test()` method returns an interface that only exposes the methods `Samples()` and `Generator()`. The `Generator()` method then returns an interface exposing the methods `Property()` and `Build()`. This results in the IDE only proposing methods that are legal in the current part of the DSL.

Similarly, for composing boolean expressions it is not possible to call two comparison methods such as `Equals()` and `IsGreaterThan()` unless they are separated by an operator method such as `And()`. There are also limitations on `BeginBlock()` and `EndBlock()`. While these interfaces do not protect against all illegal expressions (such as mismatching begins and ends), they do help the user substantially.

## Alternatives
The implementation of the method chaining builder for boolean expressions makes use of two stacks to keep track of how the expressions are composed. This is further supported by a number of context variables. To experiment with a simpler implementation, I opted for nested lambdas. This implementation removed the need for context variables entirely, at the expense of a noisier syntax. The lambda-based DSL is available by using `ThenLambda()` in place of `Then()`:

```csharp
.Property("Returns the sum of its input")
    .Given(i => i.Item1 > 0)
    .Given(i => i.Item2 > 0)
    .ThenLambda(i => Add(i.Item1, i.Item2).I())
    .Satisfies(b => b.And(
        _ => b.Or(
            __ => b.Equals(i => i.Item1 + i.Item2),
            __ => b.Equals(i => i.Item2 + i.Item1)
            ),
        _ => b.And(
            __ => b.IsGreaterThan(0),
            __ => b.Or(
                ___ => b.IsNotEqual(int.MaxValue),
                ___ => b.IsNotEqual(1)
                )
            )
        )
    )
```

I have decided that the method chaining approach works best in this case, because of the syntax of C#. The nested lambda approach might be preferable in other languages. It was interesting to realize how much more complexity was required for implementing a DSL that was easier to use.

# Parsing
Generating the semantic model from the DSL is handled by various builders. These builders are primarily divided into separate classes because of how the generic types become available over time, e.g. `FluentTestBuilder -> FluentTestBuilder<TInput> -> FluentTestBuilder<TInput, TOutput>`.

The class `FluentTestBuilder<TInput, TOutput>` is responsible for building up the boolean expresisons for method chaining. It uses a stack for boolean comparisons and another for operators, as well as context variables for keeping track of nesting depth and parentheses. Over time, it will pop comparisons and operators off of these stacks and combine them into compound expressions using the composite pattern.

## Generics and primitives
The use of generics helps greatly with code completion and writing legal DSL. For instance, the input generator passed to the `Generator()` method affects the input type provided to functions such as `Then()` and `Equals()`. The function provided to `Then()` further determines the required return type from methods used to compose the boolean expressions.

The way this is implemented does not allow for using primitives as return types. Thus, I needed to box primitives in reference types - I basically reinvented Java's autoboxing. All that is required is to explicitly specify the type in the call to `Then()` or `ThenLambda()`:

```csharp
.Then(i => Add(i.Item1, i.Item2).I())
```

The call `.I()` tells that the return type must be boxed in an `Integer` reference type. By using C# implicit conversions, the return types of all subsequent lambdas are automatically boxed. This is handled by `PrimitiveWrappers.cs`:

```csharp
public class Integer : Wrapper<int>
{
    ...
    public static implicit operator Integer(int i) => new Integer(i);
    ...
}
```

# Separation of concerns
I have divided the project into SemanticModel, Builder, and Executor. The Builder directory contains everything related to the DSL itself. The Executor directory contains classes for using the semantic model to run actual unit tests and validating the semantic model. The SemanticModel directory only contains a set of data classes.

This separation of concerns proved very difficult because of the use of generics. I believe that the result is aceptable, albeit not perfect. The benefit is that the semantic model can be used for both execution and validation (as I have done), and even code generation in the future.
