public class ObjectStructureTest
{
    public static void Execute()
    {
        var testClass = new TestClass
        {
            Id = 1,
            Name = "Testing1",
            DateCreated = DateTime.Now,
            MyWillToLive = 69.420,
            Enum = TestClass.TestEnum.Test3,
            EnumFlags = TestClass.TestEnumFlags.Test1 | TestClass.TestEnumFlags.Test2 | TestClass.TestEnumFlags.Test3 | TestClass.TestEnumFlags.Test4 | TestClass.TestEnumFlags.Test5,
            ArrayStrings = new string[] 
            { 
                "String1", 
                "String2", 
            },
            ArrayObjects = new TestClass.TestObjectClass[]
            {
                new TestClass.TestObjectClass
                {
                    Name = "TestObject1",
                    NestedObjects = new List<TestClass.TestNestedClass.TestNestedNestedClass>
                    {
                        new TestClass.TestNestedClass.TestNestedNestedClass
                        {
                            Description = "TestObject1.1",
                            Number = 11
                        },
                        new TestClass.TestNestedClass.TestNestedNestedClass
                        {
                            Description = "TestObject1.2",
                            Number = 12
                        }
                    }
                },
                new TestClass.TestObjectClass
                {
                    Name = "TestObject2",
                    NestedObjects = new List<TestClass.TestNestedClass.TestNestedNestedClass>
                    {
                        new TestClass.TestNestedClass.TestNestedNestedClass
                        {
                            Description = "TestObject2.1",
                            Number = 21
                        },
                        new TestClass.TestNestedClass.TestNestedNestedClass
                        {
                            Description = "TestObject2.2",
                            Number = 22
                        }
                    }
                }
            },
            TestObject = new TestClass.TestObjectClass
            {
                Name = "TestObject1",
                NestedObjects = new List<TestClass.TestNestedClass.TestNestedNestedClass>
                {
                    new TestClass.TestNestedClass.TestNestedNestedClass
                    {
                        Description = "TestObject1.1",
                        Number = 11
                    },
                    new TestClass.TestNestedClass.TestNestedNestedClass
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
            NestedObjects = new List<TestClass.TestNestedClass>
            {
                new TestClass.TestNestedClass 
                { 
                    Description = "Nested1", 
                    Number = 1,
                    NestedObjects = new List<TestClass.TestNestedClass.TestNestedNestedClass>
                    {
                        new TestClass.TestNestedClass.TestNestedNestedClass 
                        { 
                            Description = "Nested1.1", 
                            Number = 11,
                            
                        },
                        new TestClass.TestNestedClass.TestNestedNestedClass 
                        { 
                            Description = "Nested1.2", 
                            Number = 12,
                        },
                    }
                },
                new TestClass.TestNestedClass 
                { 
                    Description = "Nested2", 
                    Number = 2,
                    NestedObjects = new List<TestClass.TestNestedClass.TestNestedNestedClass>
                    {
                        new TestClass.TestNestedClass.TestNestedNestedClass 
                        { 
                            Description = "Nested2.1", 
                            Number = 21,
                            
                        },
                        new TestClass.TestNestedClass.TestNestedNestedClass 
                        { 
                            Description = "Nested2.2", 
                            Number = 22,
                        },
                    }
                },
            }
        };

        var handler = new ObjectHandler();
        var output = handler.Serialize(testClass);

        Console.WriteLine(output);
    }
}