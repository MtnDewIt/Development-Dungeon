using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class TestClass
{
    public int Id;
    public string Name;
    public DateTime DateCreated;
    public double MyWillToLive;

    public string[] ArrayStrings;

    public TestObjectClass[] ArrayObjects;

    public TestObjectClass TestObject;
    public List<string> NestedStrings;
    public List<TestNestedClass> NestedObjects;
}

public class TestNestedClass
{
    public string Description;
    public int Number;

    public List<TestNestedNestedClass> NestedObjects;
}

public class TestNestedNestedClass
{
    public string Description;
    public int Number;
}

public class TestObjectClass
{
    public string Name;
    public List<TestNestedNestedClass> NestedObjects;
}

public class Program
{
    public static void Main()
    {
        var testClass = new TestClass
        {
            Id = 1,
            Name = "Testing1",
            DateCreated = DateTime.Now,
            MyWillToLive = 69.420,
            ArrayStrings = new string[] 
            { 
                "String1", 
                "String2", 
            },
            ArrayObjects = new TestObjectClass[]
            {
                new TestObjectClass
                {
                    Name = "TestObject1",
                    NestedObjects = new List<TestNestedNestedClass>
                    {
                        new TestNestedNestedClass
                        {
                            Description = "TestObject1.1",
                            Number = 11
                        },
                        new TestNestedNestedClass
                        {
                            Description = "TestObject1.2",
                            Number = 12
                        }
                    }
                },
                new TestObjectClass
                {
                    Name = "TestObject2",
                    NestedObjects = new List<TestNestedNestedClass>
                    {
                        new TestNestedNestedClass
                        {
                            Description = "TestObject2.1",
                            Number = 21
                        },
                        new TestNestedNestedClass
                        {
                            Description = "TestObject2.2",
                            Number = 22
                        }
                    }
                }
            },
            TestObject = new TestObjectClass
            {
                Name = "TestObject1",
                NestedObjects = new List<TestNestedNestedClass>
                {
                    new TestNestedNestedClass
                    {
                        Description = "TestObject1.1",
                        Number = 11
                    },
                    new TestNestedNestedClass
                    {
                        Description = "TestObject1.2",
                        Number = 12
                    }
                }
            },
            NestedStrings = new List<string> 
            { 
                "Nested1", 
                "Nested2", 
            },
            NestedObjects = new List<TestNestedClass>
            {
                new TestNestedClass 
                { 
                    Description = "Nested1", 
                    Number = 1,
                    NestedObjects = new List<TestNestedNestedClass>
                    {
                        new TestNestedNestedClass 
                        { 
                            Description = "Nested1.1", 
                            Number = 11,
                            
                        },
                        new TestNestedNestedClass 
                        { 
                            Description = "Nested1.2", 
                            Number = 12,
                        },
                    }
                },
                new TestNestedClass 
                { 
                    Description = "Nested2", 
                    Number = 2,
                    NestedObjects = new List<TestNestedNestedClass>
                    {
                        new TestNestedNestedClass 
                        { 
                            Description = "Nested2.1", 
                            Number = 21,
                            
                        },
                        new TestNestedNestedClass 
                        { 
                            Description = "Nested2.2", 
                            Number = 22,
                        },
                    }
                },
            }
        };

        var settings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> 
            { 
                new CustomJsonConverter(typeof(int))
            },
            Formatting = Formatting.Indented,
        };

        string json = JsonConvert.SerializeObject(testClass, settings);
        
        Console.WriteLine(json);
    }
}

public class CustomJsonConverter : JsonConverter
{
    private readonly Type _typeToExclude;

    public CustomJsonConverter(Type typeToExclude)
    {
        _typeToExclude = typeToExclude;
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
                if (item.GetType() != _typeToExclude)
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
            if (field.FieldType != _typeToExclude)
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
