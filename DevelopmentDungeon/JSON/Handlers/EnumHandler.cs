using Newtonsoft.Json;

public class EnumHandler : JsonConverter<Enum>
{
    public EnumHandler()
    {
        // Maybe I'll add some input variables :/
    }

    public override void WriteJson(JsonWriter writer, Enum value, JsonSerializer serializer)
    {
        var enumerator = new InlineEnum(value.ToString());

        serializer.Serialize(writer, enumerator);
    }

    public override Enum ReadJson(JsonReader reader, Type objectType, Enum existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException("Don't really need to deserialize the data in any specific way yet :/");
    }
}

public class InlineEnum
{
    public string EnumValues;

    public InlineEnum(string enumValues)
    {
        EnumValues = enumValues;
    }
}