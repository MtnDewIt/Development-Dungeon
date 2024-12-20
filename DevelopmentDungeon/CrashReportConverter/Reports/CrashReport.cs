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

        public enum GameMode : int
        {
            none = 0,
            campaign = 1,
            multiplayer = 2,
            mainmenu = 3,
            theater = 4,
            shared = 5,
        }

        public class Session
        {
            public string life_cycle_state { get; set; }
            public bool host { get; set; }
            public int player_count { get; set; }
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