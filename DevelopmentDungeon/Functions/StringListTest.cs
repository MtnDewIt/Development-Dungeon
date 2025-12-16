using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace DevelopmentDungeon.Functions
{
    public class StringListTest
    {
        public static List<string> StringTable = new List<string>()
        {
            // Insert Data Here :/
        };
    
        public static void Execute()
        {
            var jsonData = JsonConvert.SerializeObject(StringTable, Formatting.Indented);
            File.WriteAllText("ms23_strings.json", jsonData);
        }
    }
}