using DevelopmentDungeon.CrashReportConverter.Reports;
using DevelopmentDungeon.CrashReportConverter.Converter;
using Newtonsoft.Json;
using System.IO;

namespace DevelopmentDungeon.CrashReportConverter
{
    public class CrashReportHandler
    {
        public static void Execute(string args0, string args1)
        {
            var debugData = File.ReadAllText(args0);

            var imagePath = args1;

            var debugReport = JsonConvert.DeserializeObject<DebugCrashReport>(debugData);

            var releaseReport = ReportConveter.ConvertCrashReport(debugReport, imagePath);

            var outputData = JsonConvert.SerializeObject(releaseReport, Formatting.Indented);

            File.WriteAllText("crash_dump_converted.json", outputData);
        }
    }
}