namespace DevelopmentDungeon.Functions
{
    public class RenameFileTest
    {
        public static void Execute()
        {
            string directoryPath = @"";

            RemoveSubstringFromFilenames(directoryPath, "_generate");
        }

        static void RemoveSubstringFromFilenames(string directoryPath, string substringToRemove)
        {
            string[] files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

            foreach (string filePath in files)
            {
                string fileName = Path.GetFileName(filePath);
                string directory = Path.GetDirectoryName(filePath);

                if (fileName.Contains(substringToRemove))
                {
                    string newFileName = fileName.Replace(substringToRemove, "");
                    string newFilePath = Path.Combine(directory, newFileName);

                    File.Move(filePath, newFilePath);
                    Console.WriteLine($"Renamed: {fileName} -> {newFileName}");
                }
            }
        }
    }
}