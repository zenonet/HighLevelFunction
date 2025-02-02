# Structs

You can define custom data types using structs.
A struct is essentially a compound of other values.
You can define a struct like this:

```csharp
struct MyStructType
{
    string someStringValue;
    int someIntValue;
    int anotherIntValue;
}
```

As of now, default values for struct fields are unfortunately not supported.

## Instantiating struct types

You can instantiate a struct using the `new`-keyword:
```
MyStructType myCustomObject = new MyStructType();
```

When you have instantiated your struct, you can access and assign its fields like this:
```csharp
// Assinging struct fields:
myCustomObject.someStringValue = "This is stored in your struct";
myCustomObject.someIntValue = 0;
myCustomObject.anotherIntValue = 5;

// Getting values froom struct fields
say(myCustomObject.anotherIntValue);

```

## Limitations

Unfortunately, you currently can't store `Entity`s or `BlockType`s in structs.
This has to do with how structs are stored as nbt while BlockTypes and Entities are stored differently.