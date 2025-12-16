using DevelopmentDungeon.CrashReportConverter.Reports;
using System;
using System.IO;

namespace DevelopmentDungeon.CrashReportConverter.Converter
{
    public class ReportConveter
    {
        public static CrashReport ConvertCrashReport(DebugCrashReport debugReport, string imagePath)
        {
            return new CrashReport()
            {
                timestamp = DateTime.UtcNow.ToFileTime(),
                build = debugReport.build.Split(" - ")[0],
                build_date = debugReport.build.Split(" - ")[1],
                player_uid = debugReport.player_uid.ToString("X"),
                player_name = "unknown_value",
                scenario = debugReport.scenario,
                dedicated = false,
                map_loading = false,
                game_mode = ((CrashReport.GameMode)debugReport.game_mode).ToString(),
                game_engine = debugReport.game_engine,
                game_time = 69420,
                game_variant = debugReport.game_variant,
                game_mod = debugReport.mod != null ? debugReport.mod.name : "",
                mainmenu_mod = debugReport.mod != null ? debugReport.mod.name : "",
                session = ConvertDebugSession(debugReport.session),
                info = debugReport.info,
                tag_history = debugReport.tag_history,
                stack = debugReport.stack,
                registers = debugReport.registers,
                additional_info = ConvertAdditionalInfo(debugReport.cache_tag_globals),
                screenshot = ConvertImageData(imagePath),
            };
        }

        public static CrashReport.Session ConvertDebugSession(DebugCrashReport.DebugSession debugSession)
        {
            return new CrashReport.Session
            {
                life_cycle_state = debugSession.life_cycle_state,
                host = debugSession.is_host,
                player_count = debugSession.num_players,
            };
        }

        public static CrashReport.AdditionalInfo ConvertAdditionalInfo(DebugCrashReport.CacheTagGlobals debugCacheTagGlobals)
        {
            return new CrashReport.AdditionalInfo
            {
                os = "Windows XP SP3",
                cpu = "AMD Athlon 64 X2 3800+",
                gpu = "NVIDIA GeForce 7900 GS",
                gpu_driver = "69.4.20.0410",
                mem_avail_physical = debugCacheTagGlobals.physical_memory_size - debugCacheTagGlobals.physical_memory_used,
                mem_total_physical = debugCacheTagGlobals.physical_memory_size,
                mem_avail_virtual = debugCacheTagGlobals.tag_memory_used_size - debugCacheTagGlobals.tag_memory_used,
                mem_total_virtual = debugCacheTagGlobals.tag_memory_used_size,
            };
        }

        public static string ConvertImageData(string filePath)
        {
            if (filePath != "")
            {
                var imageBuffer = File.ReadAllBytes(filePath);
                
                return Convert.ToBase64String(imageBuffer);
            }

            return "";
        }
    }
}