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

### Comparing entities

You can compare entity references using the default comparison operators.
For example, you can check if an entity hit by a raycast is the player like this:

````csharp
Entity hit = raycastForEntity(origin, direction, 1000);
if(hit == @p) {
    say("Player was hit!");
}
````

This comparison compares the UUIDs of the 2 entities.

### Properties of Entities

The Entity-Type has 3 Properties:

- ``Position``
    - The position of the Entity
    - Corresponds to the data property `Pos`
    - When a player's Position property is assigned, HLF falls back to the `/tp` command
      to circumvent player-data-modification-errors.
- ``Forward``
    - A directional vector representing the entities viewing direction.
- ``Motion``
    - The velocity of the entity.
    - Corresponds to the data property `Motion`
    - Setting a player's `Motion` does not work because datapacks can't modify player-data.