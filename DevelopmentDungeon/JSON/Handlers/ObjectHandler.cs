using Newtonsoft.Json;

public class ObjectHandler
{
    public ObjectHandler()
    {
        // Maybe I'll add some input variables :/
    }

    public string Serialize(object input)
    {
        var settings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> 
            { 
                new ObjectStructureHandler(typeof(int)),
                new EnumHandler(),
            },
            Formatting = Formatting.Indented,
        };

        return JsonConvert.SerializeObject(input, settings);
    }

    public object Deserialize(string input)
    {
        throw new NotImplementedException("Don't really need to deserialize the data in any specific way yet :/");
    }
}