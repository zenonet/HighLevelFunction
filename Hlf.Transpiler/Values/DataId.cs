using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public abstract class DataId
{
    public HlfType Type { get; set; }
    public bool IsVariable { get; set; }
    public bool IsImmutable { get; set; }

    public abstract string Generate(GeneratorOptions options);


    /// <summary>
    /// Try to generate an implicit conversion to fix up a type mismatch. It the conversion isn't possible, don't do anything so that the type mismatch is caught after
    /// the call to GenerateImplicitConversion()
    /// </summary>
    /// <param name="options">Generator Options</param>
    /// <param name="targetType">The type the value should have</param>
    /// <param name="dataId">The dataId to be transformed</param>
    /// <returns>The code needed to transform the value</returns>
    public static string GenerateImplicitConversion(GeneratorOptions options, HlfType targetType, ref DataId dataId)
    {
        if (dataId.Type == targetType) return "";
        if (dataId.Type.TryGenerateImplicitConversion(options, targetType, dataId, out string code, out DataId newId))
        {
            dataId = newId;
            return code;
        }

        return "";
    }
    
    public DataId ConvertImplicitly(GeneratorOptions options, HlfType targetType, out string code)
    {
        if (Type == targetType)
        {
            code = "";
            return this;
        }

        if (Type.TryGenerateImplicitConversion(options, targetType, this, out code, out DataId newId))
        {
            code += $"\n{this.FreeIfTemporary(options)}";
            return newId;
        }

        throw new ArgumentException($"{Type.Name} cannot be converted to {targetType.Name}");
    }

    public static DataId FromType(HlfType type)
    {
        switch (type.Kind)
        {
            case ValueKind.Nbt:
                return new NbtDataId(type);
            case ValueKind.Block:
                return new BlockTypeDataId();
            case ValueKind.EntityTag:
                return new EntityDataId();
            case ValueKind.Constant:
                return new ConstDataId(type, "");
        }

        throw new NotImplementedException($"Creation of dataid object for type {type.Name} is not implemented.");
    }

    public abstract string Free(GeneratorOptions gen);

    public string FreeIfTemporary(GeneratorOptions gen) => IsVariable ? "" : Free(gen);
}