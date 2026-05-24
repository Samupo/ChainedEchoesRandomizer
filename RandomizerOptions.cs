using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace CERandomizer
{
    public static class RandomizerOptions
    {
        // Archipelago connection config. Randomizer gameplay options and seed are always loaded from AP slot data.
        public static string ArchipelagoServer { get; private set; } = "localhost";
        public static int ArchipelagoPort { get; private set; } = 38281;
        public static string ArchipelagoUsername { get; private set; } = "Player";
        public static string ArchipelagoPassword { get; private set; } = "";

        public static string LoadedArchipelagoServer { get; private set; } = "";
        public static int LoadedArchipelagoPort { get; private set; } = -1;
        public static string LoadedArchipelagoUsername { get; private set; } = "";
        public static bool HasLoadedConnection { get; private set; } = false;

        public static int RandomizerSeed { get; private set; } = -1;

        public static int RandomizeCharacterInitialStats { get; private set; } = 1;

        public static int RandomizeCharacterStatProgression { get; private set; } = 1;

        public static int CharacterSRankStats { get; private set; } = 0;

        public static int CharacterARankStats { get; private set; } = 3;

        public static int CharacterBRankStats { get; private set; } = 3;

        public static int CharacterCRankStats { get; private set; } = 2;

        public static int RandomizeCharacterStatBoosts { get; private set; } = 1;

        public static int HPStatBoostValueLevel1 { get; private set; } = 10;

        public static int HPStatBoostValueLevel2 { get; private set; } = 20;

        public static int HPStatBoostValueLevel3 { get; private set; } = 30;

        public static int HPStatBoostValueLevel4 { get; private set; } = 40;

        public static int TPStatBoostValueLevel1 { get; private set; } = 5;

        public static int TPStatBoostValueLevel2 { get; private set; } = 5;

        public static int TPStatBoostValueLevel3 { get; private set; } = 5;

        public static int TPStatBoostValueLevel4 { get; private set; } = 10;

        public static int CoreStatsBoostValueLevel1 { get; private set; } = 2;

        public static int CoreStatsBoostValueLevel2 { get; private set; } = 2;

        public static int CoreStatsBoostValueLevel3 { get; private set; } = 4;

        public static int CoreStatsBoostValueLevel4 { get; private set; } = 6;

        public static int AgiStatBoostValueLevel1 { get; private set; } = 1;

        public static int AgiStatBoostValueLevel2 { get; private set; } = 1;

        public static int AgiStatBoostValueLevel3 { get; private set; } = 1;

        public static int AgiStatBoostValueLevel4 { get; private set; } = 2;

        public static int CritStatBoostValueLevel1 { get; private set; } = 4;

        public static int CritStatBoostValueLevel2 { get; private set; } = 4;

        public static int CritStatBoostValueLevel3 { get; private set; } = 5;

        public static int CritStatBoostValueLevel4 { get; private set; } = 6;

        public static int RandomizeCharacterEquipment { get; private set; } = 1;

        public static int RandomizeCharacterPassives { get; private set; } = 1;

        public static int RandomizeCharacterSkills { get; private set; } = 1;

        public static int RandomizeMechSkills { get; private set; } = 1;

        public static int RandomizeMechStatBoosts { get; private set; } = 1;

        public static int RandomizeEmblemStats { get; private set; } = 1;

        public static int RandomizeEmblemSkills { get; private set; } = 1;

        public static int RandomizeEmblemPassives { get; private set; } = 1;

        public static int AddTier1Weapons { get; private set; } = 1;

        private static readonly HashSet<string> ConnectionOptionNames = new HashSet<string>
        {
            "ArchipelagoServer",
            "ArchipelagoPort",
            "ArchipelagoUsername",
            "ArchipelagoPassword"
        };

        private static readonly HashSet<string> ServerBackedOptionNames = new HashSet<string>
        {
            "RandomizerSeed",
            "RandomizeCharacterInitialStats",
            "RandomizeCharacterStatProgression",
            "CharacterSRankStats",
            "CharacterARankStats",
            "CharacterBRankStats",
            "CharacterCRankStats",
            "RandomizeCharacterStatBoosts",
            "HPStatBoostValueLevel1",
            "HPStatBoostValueLevel2",
            "HPStatBoostValueLevel3",
            "HPStatBoostValueLevel4",
            "TPStatBoostValueLevel1",
            "TPStatBoostValueLevel2",
            "TPStatBoostValueLevel3",
            "TPStatBoostValueLevel4",
            "CoreStatsBoostValueLevel1",
            "CoreStatsBoostValueLevel2",
            "CoreStatsBoostValueLevel3",
            "CoreStatsBoostValueLevel4",
            "AgiStatBoostValueLevel1",
            "AgiStatBoostValueLevel2",
            "AgiStatBoostValueLevel3",
            "AgiStatBoostValueLevel4",
            "CritStatBoostValueLevel1",
            "CritStatBoostValueLevel2",
            "CritStatBoostValueLevel3",
            "CritStatBoostValueLevel4",
            "RandomizeCharacterEquipment",
            "RandomizeCharacterPassives",
            "RandomizeCharacterSkills",
            "RandomizeMechSkills",
            "RandomizeMechStatBoosts",
            "RandomizeEmblemStats",
            "RandomizeEmblemSkills",
            "RandomizeEmblemPassives",
            "AddTier1Weapons"
        };

        public static string ConnectionAddress
        {
            get { return ArchipelagoServer + ":" + ArchipelagoPort; }
        }

        public static void SetConnectionSettings(string server, int port, string username, string password)
        {
            ArchipelagoServer = NormalizeServer(server);
            ArchipelagoPort = port;
            ApplyServerValue(ArchipelagoServer);
            if (ArchipelagoPort <= 0 || ArchipelagoPort > 65535)
            {
                ArchipelagoPort = port;
            }
            ArchipelagoUsername = string.IsNullOrEmpty(username) ? "Player" : username.Trim();
            ArchipelagoPassword = password ?? string.Empty;
        }

        public static bool CurrentConnectionDiffersFromLoaded()
        {
            if (!HasLoadedConnection)
            {
                return false;
            }

            return LoadedArchipelagoServer != ArchipelagoServer
                || LoadedArchipelagoPort != ArchipelagoPort
                || LoadedArchipelagoUsername != ArchipelagoUsername;
        }

        public static void LoadConnectionSettings(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            string[] lines = File.ReadAllLines(path);
            ApplyLines(lines, true);

            LoadedArchipelagoServer = ArchipelagoServer;
            LoadedArchipelagoPort = ArchipelagoPort;
            LoadedArchipelagoUsername = ArchipelagoUsername;
            HasLoadedConnection = true;
        }

        public static void LoadArchipelagoSlotData(IDictionary slotData)
        {
            PropertyInfo[] properties = typeof(RandomizerOptions).GetProperties(BindingFlags.Static | BindingFlags.Public);
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (!ServerBackedOptionNames.Contains(propertyInfo.Name) || !propertyInfo.CanWrite)
                {
                    continue;
                }

                if (!slotData.Contains(propertyInfo.Name))
                {
                    continue;
                }

                SetPropertyValue(propertyInfo, slotData[propertyInfo.Name]);
            }
        }

        public static List<string> GetMissingSlotDataOptionNames(IDictionary slotData)
        {
            List<string> missingNames = new List<string>();
            PropertyInfo[] properties = typeof(RandomizerOptions).GetProperties(BindingFlags.Static | BindingFlags.Public);
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (!ServerBackedOptionNames.Contains(propertyInfo.Name) || !propertyInfo.CanWrite)
                {
                    continue;
                }

                if (!slotData.Contains(propertyInfo.Name))
                {
                    missingNames.Add(propertyInfo.Name);
                }
            }

            return missingNames;
        }

        public static void Save(string path)
        {
            PropertyInfo[] properties = typeof(RandomizerOptions).GetProperties(BindingFlags.Static | BindingFlags.Public);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (!propertyInfo.CanRead || propertyInfo.Name.StartsWith("Loaded") || propertyInfo.Name == "HasLoadedConnection" || propertyInfo.Name == "ConnectionAddress")
                {
                    continue;
                }

                string name = propertyInfo.Name;
                object value = propertyInfo.GetValue(null);
                stringBuilder.AppendLine(name + "=" + (value != null ? value.ToString() : string.Empty));
            }

            File.WriteAllText(path, stringBuilder.ToString());

            LoadedArchipelagoServer = ArchipelagoServer;
            LoadedArchipelagoPort = ArchipelagoPort;
            LoadedArchipelagoUsername = ArchipelagoUsername;
            HasLoadedConnection = true;
        }

        private static void ApplyLines(string[] lines, bool connectionOnly)
        {
            PropertyInfo[] properties = typeof(RandomizerOptions).GetProperties(BindingFlags.Static | BindingFlags.Public);
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (!propertyInfo.CanWrite)
                {
                    continue;
                }

                if (connectionOnly && !ConnectionOptionNames.Contains(propertyInfo.Name))
                {
                    continue;
                }

                foreach (string line in lines)
                {
                    if (!line.StartsWith(propertyInfo.Name + "="))
                    {
                        continue;
                    }

                    string rawValue = line.Substring(propertyInfo.Name.Length + 1);
                    if (propertyInfo.Name == "ArchipelagoServer")
                    {
                        ApplyServerValue(rawValue);
                    }
                    else
                    {
                        SetPropertyValue(propertyInfo, rawValue);
                    }
                    break;
                }
            }
        }

        private static void ApplyServerValue(string rawValue)
        {
            string value = NormalizeServer(rawValue);
            int separatorIndex = value.LastIndexOf(':');
            if (separatorIndex > 0 && separatorIndex < value.Length - 1)
            {
                int parsedPort;
                if (int.TryParse(value.Substring(separatorIndex + 1), out parsedPort))
                {
                    ArchipelagoServer = NormalizeServer(value.Substring(0, separatorIndex));
                    ArchipelagoPort = parsedPort;
                    return;
                }
            }

            ArchipelagoServer = value;
        }

        private static string NormalizeServer(string server)
        {
            string normalized = string.IsNullOrEmpty(server) ? "localhost" : server.Trim();
            if (normalized.StartsWith("ws://"))
            {
                normalized = normalized.Substring(5);
            }
            else if (normalized.StartsWith("wss://"))
            {
                normalized = normalized.Substring(6);
            }

            return normalized.TrimEnd('/');
        }

        private static void SetPropertyValue(PropertyInfo propertyInfo, object value)
        {
            if (propertyInfo.PropertyType == typeof(int))
            {
                propertyInfo.SetValue(null, System.Convert.ToInt32(value, CultureInfo.InvariantCulture));
            }
            else if (propertyInfo.PropertyType == typeof(float))
            {
                propertyInfo.SetValue(null, System.Convert.ToSingle(value, CultureInfo.InvariantCulture));
            }
            else if (propertyInfo.PropertyType == typeof(string))
            {
                propertyInfo.SetValue(null, value != null ? value.ToString() : string.Empty);
            }
        }
    }
}
