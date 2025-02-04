# Loops

There are while and for loops in HLF.

## While loops

You can write while loops like this:

```csharp
while(condition){
    // code here is ran as long as the condition evaluates to true
}
```

## For loops

You can write for-loops like this:

```csharp
for(<initializer>;<condition>;<increment>) {
    // loops content goes here
}
```

When the loop is run, the initializer is run first, it is typically used to initialize a counter variable.
After that the loops begins and runs like this:

- Condition is evaluated, loops stops if the condition is `false`
- Loops content is run
- Increment statement is run


An example of a classic usage of a for loop is:
```csharp
for(int i = 0; i < 5; i++) {
    say(i);
}
```
This code would print all numbers from 0 to 4 to the chat


## Limitations

Loops are limited by the `maxCommandChainLength`-gamerule. By default, it is set to 10000000 when a datapack loads.
This is to prevent unintuitive behaviour. This (hopefully) is a temporary solution.