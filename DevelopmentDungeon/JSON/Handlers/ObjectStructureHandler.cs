using System.Collections;
using System.Reflection;
using Newtonsoft.Json;

public class ObjectStructureHandler : JsonConverter
{
    private Type TypeToExclude;

    public ObjectStructureHandler(Type typeToExclude)
    {
        TypeToExclude = typeToExclude;
    }

    public override bool CanConvert(Type objectType)
    {
        return true;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        // Ideally what would happen here is that in TagTool, it would iterate through all fields in the tag structure, then it would cull fields based on thier
        // assigned tag attributes (ie: if its padding or unused) or based on the type (ie: cache specific data like resource data or resource offsets)

        // Probably gonna have to reference my existing TagTool GenerateTagObject code :/

        if (value == null)
        {
            writer.WriteNull();

            return;
        }

        Type objType = value.GetType();

        if (objType.IsPrimitive || objType == typeof(string) || objType.IsValueType)
        {
            writer.WriteValue(value);

            return;
        }

        if (typeof(IEnumerable).IsAssignableFrom(objType))
        {
            writer.WriteStartArray();

            foreach (var item in (IEnumerable)value)
            {
                if (item.GetType() != TypeToExclude)
                {
                    serializer.Serialize(writer, item);
                }
            }

            writer.WriteEndArray();

            return;
        }

        writer.WriteStartObject();

        FieldInfo[] fields = objType.GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (var field in fields)
        {
            if (field.FieldType != TypeToExclude)
            {
                writer.WritePropertyName(field.Name);

                var fieldValue = field.GetValue(value);

                serializer.Serialize(writer, fieldValue);
            }
        }

        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException("Don't really need to deserialize the data in any specific way yet :/");
    }
}