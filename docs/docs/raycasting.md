# Raycasting

HLF provides functions to easily perform raycasting.

## Raycasting for blocks

You can raycast for blocks using the `raycast()` functions.
It takes the ray-origin, the ray-direction and the [maxSteps](#max-steps) as parameters.

The function returns the position where the ray hit a block. 
If the ray didn't hit anything, the function will return the position where the ray stopped.

Example:

````csharp
Vector origin = Vector(0, 64, 0);
Vector direction = Vector(0.5, -0.5, 0);
Vector hit = raycast(origin, direction, 1000);

// replaces the block where the ray hit the ground with a restone_block
setBlock(hit, BlockType("restone_block"));
````

## Raycasting for entities

You can raycast for entities using the `raycastForEntity()` functions.
It takes the ray-origin, the ray-direction and the [maxSteps](#max-steps) as parameters as well.

The function returns a reference to the entity hit by the ray.
If the ray didn't hit an entity, it will return an _empty entity_. 
There is currently no way of telling if the entity is _empty_. (Ik, this should be fixed asap)

Example:

````csharp
Vector origin = Vector(0, 64, 0);
Vector direction = Vector(0.5, -0.5, 0);
Entity hit = raycastForEntity(origin, direction, 1000);

// kills the entity hit by the ray
kill(hit);
````

## Raycasting from the players cross-hare

Raycasting from the players perspective is luckily very easy thanks to the `Forward` property of the `Entity`-Type.
The `Forward`-property returns a directional Vector specifying the way an entity is looking. 

Example:
````csharp
// Make sure to offset the ray-origin because the players position is at their feet, not their eyes.
Vector origin = @p.Positon + Vector(0, 1.7, 0);
Vector direction = @p.Forward;
Vector hit = raycast(origin, direction, 1000);

// replaces the block where the player is looking with a restone_block
setBlock(hit, BlockType("restone_block"));
````

## Max steps

The maxSteps-value is used to stop the raycast after a certain amount of iterations.
The default stepSize is 0.1 blocks.
This means that the max-length of a ray with a max-steps value of 1000 is 0.1 blocks * 1000 = 100 blocks.