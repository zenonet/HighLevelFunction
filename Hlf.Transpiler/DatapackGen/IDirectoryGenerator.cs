namespace Hlf.Transpiler.DatapackGen;

public interface IDirectoryGenerator
{
    void GenerateDirectoryStructure(string parentPath, Directory directory);
}