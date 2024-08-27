using Newtonsoft.Json;

public class CommandListTest
{
    public static void Execute(string inputFile)
    {
        var jsonData = File.ReadAllText(inputFile);

        var tagTable = JsonConvert.DeserializeObject<List<string>>(jsonData);

        var outputList = new List<string>();

        foreach (var tag in tagTable)
        {
            var tagData = tag.Split('.');
            var tagName = tagData[0];
            var tagType = tagData[1];

            if (tagName.EndsWith("_convert"))
            {
                tagName = tagName.Replace("_convert", "");
                outputList.Add($@"generatetagobject {tagName}.{tagType} convert");
            }
            else if(tagName.EndsWith("_generate"))
            {
                tagName = tagName.Replace("_generate", "");
                outputList.Add($@"generatetagobject {tagName}.{tagType} generate");
            }
            else
            {
                outputList.Add($@"generatetagobject {tagName}.{tagType}");
            }
        }

        File.WriteAllLines($@"output_commands.cmd", outputList);

        Console.WriteLine("Command List Generated Successfully");
    }
}