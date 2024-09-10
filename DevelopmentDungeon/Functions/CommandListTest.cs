using Newtonsoft.Json;

namespace DevelopmentDungeon.Functions
{
    public class CommandListTest
    {
       public static void Execute(string inputFile)
       {
           var jsonData = File.ReadAllText(inputFile);
    
           var fileInfo = new FileInfo(inputFile);
    
           var objectTable = JsonConvert.DeserializeObject<List<string>>(jsonData);
    
           var outputList = new List<string>();
    
           switch (fileInfo.Name)
           {
               case "tags.json":
                   GenerateTagTable(outputList, objectTable);
                   break;
               case "maps.json":
                   GenerateMapTable(outputList, objectTable);
                   break;
               default:
                   Console.WriteLine("Well Shit :/");
                   break;
           }
    
           File.WriteAllLines($@"output_commands.cmd", outputList);
    
           Console.WriteLine("Command List Generated Successfully");
       }
       
       public static void GenerateTagTable(List<string> outputList, List<string> objectTable)
       {
           foreach (var tag in objectTable)
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
       }
    
       public static void GenerateMapTable(List<string> outputList, List<string> objectTable)
       {
           foreach (var map in objectTable)
           {
               if (map.EndsWith("_convert"))
               {
                   var mapName = map.Replace("_convert", "");
                   outputList.Add($@"generatemapobject {mapName}.map convert");
               }
               else if(map.EndsWith("_generate"))
               {
                   var mapName = map.Replace("_generate", "");
                   outputList.Add($@"generatemapobject {mapName}.map generate");
               }
               else
               {
                   outputList.Add($@"generatemapobject {map}.map");
               }
           }
       }
    }
}