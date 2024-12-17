namespace Hlf.Transpiler;

public enum ValueKind
{
    Void,
    Scoreboard,
    Nbt,
    EntityTag, // A tag on an entity
    Block, // An actual block in the world storing a block type
    Constant,
}