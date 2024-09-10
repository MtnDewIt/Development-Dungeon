namespace DevelopmentDungeon.JSON.Objects
{
    public class TestClass
    {
        public int Id;
        public string Name;
        public DateTime DateCreated;
        public double MyWillToLive;
    
        public TestEnum Enum;
        public TestEnumFlags EnumFlags;
    
        public string[] ArrayStrings;
    
        public TestObjectClass[] ArrayObjects;
    
        public TestObjectClass TestObject;
        public List<string> NestedStrings;
        public List<TestNestedClass> NestedObjects;
    
        public class TestNestedClass
        {
            public string Description;
            public int Number;
            public List<TestNestedNestedClass> NestedObjects;
            
            public class TestNestedNestedClass
            {
                public string Description;
                public int Number;
            }
        }
    
        
        public class TestObjectClass
        {
            public string Name;
            public List<TestNestedClass.TestNestedNestedClass> NestedObjects;
        }
    
        
        public enum TestEnum : int
        {
            None,
            Test1,
            Test2,
            Test3,
            Test4,
            Test5,
        }
        
        [Flags]
        public enum TestEnumFlags : int
        {
            None = 0,
            Test1 = 1 << 0,
            Test2 = 1 << 1,
            Test3 = 1 << 2,
            Test4 = 1 << 3,
            Test5 = 1 << 5,
        }
    }
}