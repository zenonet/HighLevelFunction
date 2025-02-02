# String Interpolation

Sometimes, you might want to write a value to the chat but annotate what it is.
In these cases, you can use string interpolation to interpolate a value into a string.

**Example:**
```csharp
int a = 25;
say($"A has a value of {a}");
```

The inserted value doesn't have to a variable. You can use any expression you like:
```csharp
say($"2^(2+3*2) {pow(2, 2+3*2)}");
```

You can also interpolate multiple values into a string:
```csharp
int r = 2;
float pi = 3.14;
say($"The circle has a radius of {r}, so the area of the circle is {pi*pow(r, 2)}");
```