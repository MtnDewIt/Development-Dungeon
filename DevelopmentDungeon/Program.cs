using DevelopmentDungeon.Functions;

namespace DevelopmentDungeon
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the Development Dungeon\n");
            Console.WriteLine("Choose your poison: \n");
            Console.WriteLine("1: Object Structure Test (Basic object serialization into JSON)\n");
            Console.WriteLine("2: String List Test (Converts a String List into JSON)\n");
            Console.WriteLine("3: Unicode String List Test (Converts String Tuples into a UnicodeStringData list, then converts it to JSON)\n");
            Console.WriteLine("4: Forge Palette Test (Converts a string list into forge palette commands for use in TagTool)\n");
            Console.WriteLine("5: Command List Test (Converts the specified tag list into a list of TagTool commands)\n");
    
            var poison = Console.ReadLine();
    
            switch (int.Parse(poison))
            {
                case 1:
                    ObjectStructureTest.Execute();
                    break;
                case 2:
                    StringListTest.Execute();
                    break;
                case 3:
                    UnicodeListTest.Execute();
                    break;
                case 4:
                    ForgePaletteTest.Execute();
                    break;
                case 5:
                    Console.WriteLine("Specify the file you would like to convert: \n");
                    var inputFile = Console.ReadLine();
                    CommandListTest.Execute(inputFile);
                    break;
                default:
                    Console.WriteLine("Wise Choice");
                    break;
            }
        }
    }
}