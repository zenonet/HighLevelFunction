# Working with entities

The `Entity`-Datatype allows you to store references to entities in variables.

### Selectors
To get an entity from the game-world, you can use a selector:

```csharp
Entity randomSheep = @e[type=sheep, sort=random, limit=1];
```

However, make sure, your selector can only return one entity (by using a `limit=1` clause)<br>
The transpiler will complain otherwise.

### Summoning entities

You can also create entities yourself using the summon function:

```
Entity notSoRandomSheep = summon("sheep");
```

When summoning an entity, it will be at the coordinates 0 0 0 by default.
Make sure to place it somewhere logical my assigning its Position property:
```csharp
notSoRandomSheep.Position = Vector(0, 63, 0);
```

#### Owned entities

When you summon an entity from HLF, it is considered to be "owned" by your datapack.
If you want your entity to be removed when your datapack is reloaded, you can all the `killOwned()`
function to kill all entities owned by HLF:

```
void main(){
    // Do this in your main function
    killOwned();
}
```