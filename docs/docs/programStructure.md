# Program Structure

An HLF-Program consists of functions, types and variable declarations.<br>
To run some code when the datapack is loaded (or when `/reload` is run), write it in
the `load()` function.

````csharp
void load() {
    // code written here is executed when the datapack loads
}
````


## Event functions

There are 2 event functions in HLF: `load()` and `tick()`. As their names suggest, the first one
runs when the datapack loads, while the second one runs in every game tick.

````csharp
void load() {
    say("Datapack loaded!");
}

void tick() {
    say("Tick!");
}
````

## Global variables

When a variable is declared in the root scope (not in a function body), it is considered a global variable.
This variable can be accessed from all functions and preserves state between function executions.
```csharp
int i = 0; // this is a global variable

void tick() {
    say($"This is the {i}th tick!");
}
```

## Type definitions

You can also define custom types in the root scope. For that, see [structs](structs.md).