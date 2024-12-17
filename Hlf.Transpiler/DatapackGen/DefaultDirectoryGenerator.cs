namespace Hlf.Transpiler.DatapackGen;

public class DefaultDirectoryGenerator : IDirectoryGenerator
{
    public void GenerateDirectoryStructure(string parentPath, Directory directory)
    {
        System.IO.Directory.CreateDirectory(Path.Join(parentPath, directory.Name));

        foreach (Node node in directory.Children)
        {
            if (node is File file)
            {
                System.IO.File.WriteAllText(Path.Join(parentPath, directory.Name, file.Name), file.Content);
            }else if (node is Directory dir)
            {
                GenerateDirectoryStructure(Path.Join(parentPath, directory.Name), dir);
            }
        }
    }
}