namespace osu_progressCLI
{
    /// <summary>
    /// used to turn Mod Value into ModString
    /// </summary>
    public static class ModParser
    {
        public static string ParseEverything(int combinedFlags)
        {
            // Define a dictionary mapping mod names to their corresponding bit values
            var modValues = new Dictionary<string, int>
            {
                { "NoFail", 1 },
                { "Easy", 2 },
                { "NoVideo", 4 },
                { "Hidden", 8 },
                { "HardRock", 16 },
                { "SuddenDeath", 32 },
                { "DoubleTime", 64 },
                { "Relax", 128 },
                { "HalfTime", 256 },
                { "Nightcore", 512 },
                { "Flashlight", 1024 },
                { "Autoplay", 2048 },
                { "SpunOut", 4096 },
                { "Autopilot", 8192 },
                { "Perfect", 16384 },
                { "Key4", 32768 },
                { "Key5", 65536 },
                { "Key6", 131072 },
                { "Key7", 262144 },
                { "Key8", 524288 },
                { "FadeIn", 1048576 },
                { "Random", 2097152 },
                { "Cinema", 4194304 },
                { "Target", 8388608 },
                { "Key9", 16777216 },
                { "KeyCoop", 33554432 },
                { "Key1", 67108864 },
                { "Key3", 134217728 },
                { "Key2", 268435456 },
                { "ScoreV2", 536870912 },
                { "Mirror", 1073741824 }
            };

            var modList = new List<string>();

            foreach (var kvp in modValues)
            {
                if ((combinedFlags & kvp.Value) != 0)
                {
                    modList.Add(kvp.Key);
                }
            }

            string modsString = string.Join(" ", modList);

            return modsString;
        }

        public static string ParseMods(int combinedFlags)
        {
            // Define a dictionary mapping mod names to their corresponding bit values
            var modValues = new Dictionary<string, int>
            {
                { "NoFail", 1 },
                { "Easy", 2 },
                { "Hidden", 8 },
                { "HardRock", 16 },
                { "SuddenDeath", 32 },
                { "DoubleTime", 64 },
                { "Relax", 128 },
                { "HalfTime", 256 },
                { "Nightcore", 512 },
                { "Flashlight", 1024 },
                { "Autoplay", 2048 },
                { "SpunOut", 4096 },
                { "Autopilot", 8192 },
                { "Perfect", 16384 },
                { "ScoreV2", 536870912 },
            };

            var modList = new List<string>();

            foreach (var kvp in modValues)
            {
                if ((combinedFlags & kvp.Value) != 0)
                {
                    modList.Add(kvp.Key);
                }
            }

            string modsString = string.Join(", ", modList);

            return modsString;
        }

        public static string PPCalcMods(int combinedFlags)
        {
            // Define a dictionary mapping mod names to their corresponding bit values
            var modValues = new Dictionary<string, int>
            {
                { "nf", 1 },
                { "ez", 2 },
                { "hd", 8 },
                { "hr", 16 },
                { "dt", 64 },
                { "ht", 256 },
                { "nc", 512 },
                { "fl", 1024 },
                { "so", 4096 },
            };

            var modList = new List<string>();

            foreach (var kvp in modValues)
            {
                if ((combinedFlags & kvp.Value) != 0)
                {
                    modList.Add(kvp.Key);
                }
            }

            if (modList.Count > 0)
            {
                string modsString = string.Join(" -m ", modList);
                return "-m " + modsString;
            }

            return String.Empty;
        }
    }
}
