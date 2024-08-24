using Newtonsoft.Json;

public class UnicodeListTest
{
    public static Tuple<string, string>[] UnicodeStrings = 
    {
        // Tuple.Create("string_id_name", "string_id_content"),
    };

    public static void Execute()
    {
        List<UnicodeStringData> unicodeStringData = new List<UnicodeStringData>();

        foreach (var pair in UnicodeStrings)
        {
            unicodeStringData.Add(new UnicodeStringData(pair.Item1, pair.Item2));
        }

        var jsonData = JsonConvert.SerializeObject(unicodeStringData, Formatting.Indented);
        File.WriteAllText("unic_string_test.json", jsonData);
    }
}