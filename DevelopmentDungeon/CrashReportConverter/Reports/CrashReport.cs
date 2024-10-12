namespace DevelopmentDungeon.CrashReportConverter.Reports
{
    public class CrashReport
    {
        public long timestamp { get; set; }
        public string build { get; set; }
        public string build_date { get; set; }
        public string player_uid { get; set; }
        public string player_name { get; set; }
        public string scenario { get; set; }
        public bool dedicated { get; set; }
        public bool map_loading { get; set; }
        public string game_mode { get; set; }
        public string game_engine { get; set; }
        public int game_time { get; set; }
        public string game_variant { get; set; }
        public string map_variant { get; set; }
        public string game_mod { get; set; }
        public string mainmenu_mod { get; set; }
        public Session session { get; set; }
        public string info { get; set; }
        public string[] tag_history { get; set; }
        public List<StackFrame> stack { get; set; }
        public RegisterDump registers { get; set; }
        public AdditionalInfo additional_info { get; set; }
        public string screenshot { get; set; }

        public class Session
        {
            public string life_cycle_state { get; set; }
            public bool host { get; set; }
            public int player_count { get; set; }
        }
    
        public class StackFrame
        {
            public string address { get; set; }
            public string name { get; set; }
            public string line { get; set; }
        }
    
        public class RegisterDump
        {
            public string EAX { get; set; }
            public string EBX { get; set; }
            public string ECX { get; set; }
            public string EDX { get; set; }
            public string ESI { get; set; }
            public string EDI { get; set; }
            public string EIP { get; set; }
            public string ESP { get; set; }
            public string EBP { get; set; }
            public string EFL { get; set; }
        }
    
        public class AdditionalInfo
        {
            public string os { get; set; }
            public string cpu { get; set; }
            public string gpu { get; set; }
            public string gpu_driver { get; set; }
            public ulong mem_avail_physical { get; set; }
            public ulong mem_total_physical { get; set; }
            public ulong mem_avail_virtual { get; set; }
            public ulong mem_total_virtual { get; set; }
        }
    }
}