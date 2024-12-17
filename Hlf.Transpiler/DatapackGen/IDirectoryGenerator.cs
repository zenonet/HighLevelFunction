namespace Hlf.Transpiler.DatapackGen;

public interface IDirectoryGenerator
{
    void GenerateDirectoryStructure(string parentPath, List<File> files);
}