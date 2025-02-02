# Variables

Variables can be defined like in other C-style languages:
```csharp
<VariableType> <variableName> = <initialValue>;
```

An example would be:
```csharp
int a = 16;
```

## Type system

Since HLF is statically-typed, a variable must be assigned a datatype when it is declared.
This datatype must not change during execution.

Variables can be of any type that is not a compile-time-constant type. See [Types](types.md)

## Type inference

In case you don't want to write the correct datatype everytime you declare a variable,
you can let the transpiler infer its type from the initial value. To do this, declare a variable using the `var` or `val` keywords.

````csharp
var block = BlockType("stone");
````

You can make a variable immutable by using the `val`-keyword:

````csharp
val i = 16;
i = 14; // This will throw an error at compile-time
````
