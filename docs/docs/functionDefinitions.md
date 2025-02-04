# Function definitions

You can define functions like in other languages like this:

````csharp
void myFunction() {
    say("My function was called!");
}
````

## Parameters

HLF supports function parameters. you can define them like this:

```csharp
void yeet(Entity target, float amount) {
    target.Motion = Vector(0, amount, 0);
}
```

## Calling custom functions

You can call custom function just like any other functions:
```csharp
yeet(@e[type=sheep, limit=1], 2);
```

## Calling custom functions from McFunction

When transpiled, custom functions become regular McFunction functions meaning you can
call them using the `/function` command. However, to run a function with parameters from McFunction,
you would need to assign them in the data storage. As of now, there are no tools implemented to let you see which parameters correspond to which data paths. 

## Limitations

As of now, HLF does not support return values from custom functions. This means that
a function can only be defined as `void`.