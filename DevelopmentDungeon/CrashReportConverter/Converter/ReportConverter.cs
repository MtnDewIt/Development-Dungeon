using DevelopmentDungeon.CrashReportConverter.Reports;

namespace DevelopmentDungeon.CrashReportConverter.Converter
{
    public class ReportConveter
    {
        public static CrashReport ConvertCrashReport(DebugCrashReport debugReport, string imagePath)
        {
            var report = new CrashReport();

            report.timestamp = DateTime.UtcNow.ToFileTime();
            report.build = debugReport.build.Split(" - ")[0];
            report.build_date = debugReport.build.Split(" - ")[1];
            report.player_uid = debugReport.player_uid.ToString("X");
            report.player_name = "Dequarious Jerabell"; 
            report.scenario = debugReport.scenario;
            report.dedicated = false;
            report.map_loading = false;
            report.game_mode = ((CrashReport.GameMode)debugReport.game_mode).ToString();
            report.game_engine = debugReport.game_engine;
            report.game_time = 69420;
            report.game_variant = debugReport.game_variant;
            report.game_mod = GetModName(debugReport.mod);
            report.mainmenu_mod = GetModName(debugReport.mod);
            report.session = ConvertDebugSession(debugReport.session);
            report.info = debugReport.info;
            report.tag_history = debugReport.tag_history;
            report.stack = ConvertDebugStackFrames(debugReport.stack);
            report.registers = ConvertDebugRegisters(debugReport.registers);
            report.additional_info = ConvertAdditionalInfo(debugReport.cache_tag_globals);
            report.screenshot = ConvertImageData(imagePath);

            return report;
        }

        public static string GetModName(DebugCrashReport.DebugModInfo modInfo)
        {
            return modInfo != null ? modInfo.name : "";
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

        public static List<CrashReport.StackFrame> ConvertDebugStackFrames(List<DebugCrashReport.DebugStackFrame> debugStackFrames)
        {
            var stackFrames = new List<CrashReport.StackFrame>();

            foreach (var debugFrame in debugStackFrames)
            {
                var frame = new CrashReport.StackFrame
                {
                    address = debugFrame.address,
                    name = debugFrame.name,
                    line = debugFrame.line,
                };

                stackFrames.Add(frame);
            }

            return stackFrames;
        }

        public static CrashReport.RegisterDump ConvertDebugRegisters(DebugCrashReport.DebugRegisterDump debugRegisters)
        {
            return new CrashReport.RegisterDump
            {
                EAX = debugRegisters.EAX,
                EBX = debugRegisters.EBX,
                ECX = debugRegisters.ECX,
                EDX = debugRegisters.EDX,
                ESI = debugRegisters.ESI,
                EDI = debugRegisters.EDI,
                EIP = debugRegisters.EIP,
                ESP = debugRegisters.ESP,
                EBP = debugRegisters.EBP,
                EFL = debugRegisters.EFL,
            };
        }

        public static CrashReport.AdditionalInfo ConvertAdditionalInfo(DebugCrashReport.DebugCacheTagGlobals debugCacheTagGlobals)
        {
            return new CrashReport.AdditionalInfo
            {
                os = "Windows XP SP3",
                cpu = "AMD Athlon Silver 3050U",
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
            else
            {
                return "";
            }
        }
    }
}