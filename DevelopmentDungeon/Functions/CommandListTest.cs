using Newtonsoft.Json;

public class CommandListTest
{
    public static List<string> CommandTagTable = new List<string>();

    public static void Execute(string[] args)
    {
        var jsonData = File.ReadAllText(args[0]);

        CommandTagTable = JsonConvert.DeserializeObject<List<string>>(jsonData);

        var outputList = new List<string>();

        foreach (var tag in CommandTagTable)
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
    }
}