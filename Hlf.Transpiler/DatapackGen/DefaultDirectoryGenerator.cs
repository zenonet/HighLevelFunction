namespace Hlf.Transpiler.DatapackGen;

public class DefaultDirectoryGenerator : IDirectoryGenerator
{
    public void GenerateDirectoryStructure(string parentPath, List<File> files)
    {
        foreach (File file in files)
        {
            string path = Path.Join(parentPath, file.Path);
            if (!Directory.Exists(Path.GetDirectoryName(path))) 
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            System.IO.File.WriteAllText(path, file.Content);
        }
    }
}