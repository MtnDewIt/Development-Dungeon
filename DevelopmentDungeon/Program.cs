using DevelopmentDungeon.CrashReportConverter;
using DevelopmentDungeon.Functions;
using System;

namespace DevelopmentDungeon
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the Development Dungeon\n");
            Console.WriteLine("Pick your poison: \n");
            Console.WriteLine("1: Object Structure Test (Basic object serialization into JSON)\n");
            Console.WriteLine("2: String List Test (Converts a String List into JSON)\n");
            Console.WriteLine("3: Unicode String List Test (Converts String Tuples into a UnicodeStringData list, then converts it to JSON)\n");
            Console.WriteLine("4: Forge Palette Test (Converts a string list into forge palette commands for use in TagTool)\n");
            Console.WriteLine("5: Command List Test (Converts the specified tag list into a list of TagTool commands)\n");
            Console.WriteLine("6: Crash Report Converter (Converts the specified debug crash report into a crash report compatible with release)\n");
            Console.WriteLine("7: Mass File Renamer (Remove the specified substring from the file name of each file in the specified path)\n");
    
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
                case 6:
                    Console.WriteLine("Specify the debug crash dump you would like to convert: \n");
                    var debugCrash = Console.ReadLine();
                    Console.WriteLine("Specify the image you would like to include with the report: \n");
                    var imageFile = Console.ReadLine();
                    CrashReportHandler.Execute(debugCrash, imageFile);
                    break;
                case 7:
                    RenameFileTest.Execute();
                    break;
                default:
                    Console.WriteLine("Wise Choice");
                    break;
            }
        }
    }
}