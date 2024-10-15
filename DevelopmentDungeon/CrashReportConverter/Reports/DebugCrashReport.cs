namespace DevelopmentDungeon.CrashReportConverter.Reports
{
    public class DebugCrashReport
    {
        public string info;
        public string build { get; set; }
        public string scenario { get; set; }
        public int game_mode { get; set; }
        public string game_engine { get; set; }
        public string game_variant { get; set; }
        public string map_variant { get; set; }
        public DebugSession session { get; set; }
        public string exception_code { get; set; }
        public string exception_flags { get; set; }
        public string exception_record { get; set; }
        public string exception_address { get; set; }
        public string exception_number_parameters { get; set; }
        public int last_script_opcode { get; set; }
        public string tag_cache_sha1_hash { get; set; }
        public long player_uid { get; set; }
        public string[] tag_history { get; set; }
        public ModuleDump module { get; set; }
        public CacheTagGlobals cache_tag_globals { get; set; }
        public ModInfo mod { get; set; }
        public List<StackFrame> stack { get; set; }
        public RegisterDump registers { get; set; }
        
        public class DebugSession
        {
            public string local_state { get; set; }
            public string life_cycle_state { get; set; }
            public bool is_host { get; set; }
            public int num_players { get; set; }
        }
    
        public class ModuleDump
        {
            public string address { get; set; }
            public string range { get; set; }
            public string entry_point { get; set; }    
            public string path { get; set; }
        }
    
        public class CacheTagGlobals
        {
            public long loaded_instance_count { get; set; }
            public long total_instance_count { get; set; }
            public ulong tag_memory_used { get; set; }
            public ulong tag_memory_size { get; set; }
            public ulong tag_memory_used_size { get; set; }
            public ulong resource_memory_used_size { get; set; }
            public ulong physical_memory_used { get; set; }
            public ulong physical_memory_size { get; set; }
            public ulong data_memory_used_size { get; set; }
        }
    
        public class ModInfo
        {
            public string hash { get; set; }
            public string name { get; set; }
            public string author { get; set; }
        }
    }
}