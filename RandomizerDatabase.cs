using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CERandomizer
{
    public static class RandomizerDatabase
    {
        public static readonly Dictionary<string, List<int>> ItemIDs = new Dictionary<string, List<int>>
        {
            // Weapon Progression
            { "Sword", new List<int>{ 1, 2, 3, 4, 5, 6, 7, 8, 9 } }, // Tier 2 to 10
            { "Spear", new List<int>{ 13, 14, 15, 16, 17, 18, 19, 20, 21 } }, // Tier 2 to 10
            { "Bow", new List<int>{ 25, 26, 27, 28, 29, 30, 31, 32, 33 } }, // Tier 2 to 10
            { "Rapier", new List<int>{ 37, 38, 39, 40, 41, 42, 43, 44, 45 } }, // Tier 2 to 10
            { "Gun", new List<int>{ 48, 49, 50, 51, 52, 53, 54 } }, // Tier 4 to 10
            { "Amulet", new List<int>{ 57, 58, 59, 60, 61, 62, 63, 64  } }, // Tier 3 to 10
            { "Gunspear", new List<int>{ 69, 70, 71, 72} }, // Tier 7 to 10
            { "Katana", new List<int>{ 77, 78, 79, 80, 81, 82, 83, 84, 85 } }, // Tier 2 to 10
            { "Greatsword", new List<int>{ 89, 90, 91, 92, 93, 94} }, // Tiers 1-3 and 8-10
            { "Anchor", new List<int>{ 105, 106, 107, 108, 109, 110, 111} }, // Tiers 4 to 10
            { "Claw", new List<int>{ 97, 98, 99, 100, 101 } }, // Tiers 6 to 10
            { "Cards", new List<int>{ 114, 115, 116 } }, // Tiers 8 to 10

            // Armor progression
            { "Light Armor", new List<int>{121,122,123,124,125,126,127,128,129} }, // Tiers 2 to 10
            { "Heavy Armor", new List<int>{133,134,135,136,137,138,139,140,141} }, // Tiers 2 to 10
            { "Clothes", new List<int>{145,146,147,148,149,150,151,152,152,153} }, // Tiers 2 to 10
            { "Robe", new List<int>{157,158,159,160,161,162,163,164,165} }, // Tiers 2 to 10
            // Second armor set
            { "Light Armor 2", new List<int>{121,122,123,124,125,126,127,128,129} }, // Tiers 2 to 9
            { "Heavy Armor 2", new List<int>{133,134,135,136,137,138,139,140,141} }, // Tiers 2 to 9
            { "Clothes 2", new List<int>{145,146,147,148,149,150,151,152,153} }, // Tiers 2 to 9
            { "Robe 2", new List<int>{157,158,159,160,161,162,163,164,165} }, // Tiers 2 to 9

            // Mech bodies
            { "Paris", new List<int>{401,402,403,404,405} }, // Tiers 2 to 6
            { "Ovelia", new List<int>{409,410,411,412,413} }, // Tiers 2 to 6
            { "Agamemnon", new List<int>{417,418,419,420,421} }, // Tiers 2 to 6
            { "Merlin", new List<int>{425,426,427,428,429} }, // Tiers 2 to 6
            { "Kerberos", new List<int>{436,437,438,439,440} }, // Tiers 2 to 6

            // Mech weapons
            { "MechSword", new List<int>{251, 252, 253, 254, 255} }, // Tiers 2 to 6
            { "MechHammer", new List<int>{259, 260, 261, 262, 263} }, // Tiers 2 to 6
            { "MechGreatsword", new List<int>{267, 268, 269, 270, 271} }, // Tiers 2 to 6
            { "MechGlaive", new List<int>{275, 276, 277, 278, 279} }, // Tiers 2 to 6
            { "MechBowgun", new List<int>{283, 284, 285, 286, 287} }, // Tiers 2 to 6
            { "MechEther", new List<int>{291, 292, 293, 294, 295} }, // Tiers 2 to 6
            { "MechSupport", new List<int>{299, 300, 301, 302, 303} }, // Tiers 2 to 6
            { "MechElemental", new List<int>{307, 308, 309, 310, 311} }, // Tiers 2 to 6
            { "MechOffensive", new List<int>{315, 316, 317, 318, 319} }, // Tiers 2 to 6

            // Key items
            { "Sacred Water", new List<int>{405} },
            {"Key Card A", new List<int>{410} },
            {"Key Card B", new List<int>{411} },
            {"Key Card C", new List<int>{412} },
            {"Key Card D", new List<int>{413} },
            {"Bronze Key", new List<int>{414} },
            {"Silver Key", new List<int>{415} },
            {"Gold Key", new List<int>{416} },
            {"Charon's Coin Bag", new List<int>{421} },
            {"Elevator Key", new List<int>{422} },
            {"Miner's Key", new List<int>{423} },
            {"Church Key", new List<int>{425} },
            {"Norgant's Key", new List<int>{426} },
            {"Manor Key", new List<int>{429} },
            {"Water Handle", new List<int>{452} },

            // Class Emblem
            //{"EmblemArithmetician", new List<int>{ } },
            {"EmblemGambler", new List<int>{15 } },
            {"EmblemSummoner", new List<int>{16 } },
            {"EmblemCleric", new List<int>{17 } },
            //{"EmblemOccultist", new List<int>{ } },
            {"EmblemMonk", new List<int>{19 } },
            //{"EmblemShadowKnight", new List<int>{ } },
            {"EmblemBandit", new List<int>{21 } },
            {"EmblemMageWarrior", new List<int>{22 } },
            //{"EmblemHolyKnight", new List<int>{ } },
            {"EmblemVampire", new List<int>{24 } },
            {"EmblemWarrior", new List<int>{25 } },
            {"EmblemRuneKnight", new List<int>{26 } },
            {"EmblemShaman", new List<int>{27 } },
            {"EmblemPyromancer", new List<int>{28 } },
            //{"EmblemViking", new List<int>{ } },
            //{"EmblemDancer", new List<int>{ } },
            {"EmblemChemist", new List<int>{31 } },

        };

        public class Location
        {
            public enum LocationType { Chest, Boss, Emblem, Deal, Board, ChainBoard, Upgrade }

            public struct ItemRequirement
            {
                public string ItemName { get; private set; }
                public int ItemCount { get; private set; }

                public ItemRequirement(string itemName, int itemCount) => (ItemName, ItemCount) = (itemName, itemCount);
            }

            public long ID { get;private set; }
            public string UserFriendlyName { get; private set; }
            public string InternalName { get; private set; }
            public bool Missable { get; private set; }
            public List<ItemRequirement> RequiredItems { get; private set; } = new List<ItemRequirement>();
            public Item AssignedItem { get; set; }
            public LocationType Type { get; private set; }

            public Location(long id, string userFriendlyName, string internalName, bool missable, LocationType type)
            {
                ID = id;
                UserFriendlyName = userFriendlyName;
                InternalName = internalName;
                Missable = missable;
                Type = type;
            }

            public void AddRequirement(string itemName, int count)
            {
                var existingRequirement = RequiredItems.FirstOrDefault(req => req.ItemName == itemName);
                if (existingRequirement.ItemName != null)
                {
                    RequiredItems.Remove(existingRequirement);
                    RequiredItems.Add(new ItemRequirement(itemName, existingRequirement.ItemCount + count));
                }
                else
                {
                    RequiredItems.Add(new ItemRequirement(itemName, count));
                }
            }
        }

        public class Item
        {
            public enum ItemClassification { Progression, Useful, Filler }
            public enum ItemType { CharacterSkill, CharacterPassive, CharacterStatBoost, Item, Equipment, ClassEmblem, MechEquipment }

            public string UserFriendlyName { get; private set; }
            public string InternalName { get; private set; }
            public ItemClassification Classification { get; private set; }
            public ItemType Type { get; private set; }

            public Item(string userFriendlyName, string internalName, ItemClassification classification, ItemType type)
            {
                UserFriendlyName = userFriendlyName;
                InternalName = internalName;
                Classification = classification;
                Type = type;
            }
        }

        public const string LOCATIONS_FILE = "LocationsDB.txt";
        public const string ITEMS_FILE = "ItemsDB.txt";
        public static List<Location> locations = new List<Location>();
        public static List<Item> items = new List<Item>();
        public static List<Item> unassignedItems = new List<Item>();

        public static bool Randomized { get; private set; }

        private static int seedOffset = 0;

        public static void LoadLocations()
        {
            int glennStatBoostRequirement = 0;
            using (var reader = new StreamReader(LOCATIONS_FILE))
            {
                string line;
                long locationID = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("//")) continue;
                    var data = line.Split(',');
                    if (data.Length < 4) continue;

                    Enum.TryParse(data[0], out Location.LocationType locationType);
                    var location = new Location(locationID, data[1], data[2], data[3] == "Missable", locationType);
                    locationID++;

                    if (glennStatBoostRequirement > 0)
                    {
                        // location.AddRequirement("GlennStatBoost", glennStatBoostRequirement);
                    }

                    if (locationType == Location.LocationType.Boss && !location.Missable)
                    {
                        glennStatBoostRequirement++;
                    }

                    locations.Add(location);
                }
            }
        }

        public static void LoadItems()
        {
            using (var reader = new StreamReader(ITEMS_FILE))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("//")) continue;
                    var data = line.Split(',');
                    if (data.Length < 5) continue;

                    int progressiveCount = int.Parse(data[2]);
                    Enum.TryParse(data[3], out Item.ItemType itemType);
                    Enum.TryParse(data[4], out Item.ItemClassification classification);

                    var item = new Item(data[0], data[1], classification, itemType);
                    items.Add(item);
                }
            }
        }

        public static bool Randomize()
        {
            var shuffledLocations = locations.OrderBy(_ => RandomGen.Next()).ToList();

            var progressionItems = items
                    .Where(i => i.Classification == Item.ItemClassification.Progression)
                    .ToList();

            var usefulItems = items
                    .Where(i => i.Classification == Item.ItemClassification.Useful)
                    .OrderBy(_ => RandomGen.Next())
                    .ToList();

            var fillerItems = items
                    .Where(i => i.Classification == Item.ItemClassification.Filler)
                    .ToList();

            ResetAssignments();

            AssignItems(shuffledLocations, progressionItems, false);
            AssignItems(shuffledLocations, usefulItems, true);
            AssignFillerItems(shuffledLocations, fillerItems);

            if (ValidateProgression())
            {
                Randomized = true;
                return true;
            }
            else
            {
                unassignedItems.Clear();
                return false;
            }
        }

        private static void ResetAssignments()
        {
            foreach (var location in locations)
            {
                location.AssignedItem = null;
            }
            unassignedItems.Clear();
        }

        private static void AssignItems(List<Location> shuffledLocations, List<Item> itemList, bool includeMissables)
        {
            foreach (var item in itemList)
            {
                var location = shuffledLocations.FirstOrDefault(l => l.AssignedItem == null && (includeMissables || !l.Missable));
                if (location != null)
                {
                    location.AssignedItem = item;
                }
                else
                {
                    unassignedItems.Add(item);
                }
            }
        }

        private static void AssignFillerItems(List<Location> shuffledLocations, List<Item> fillerItems)
        {
            if (fillerItems.Count > 0)
            {
                foreach (var location in shuffledLocations.Where(l => l.AssignedItem == null))
                {
                    var fillerItem = fillerItems[RandomGen.Range(0, fillerItems.Count)];
                    location.AssignedItem = fillerItem;
                }
            }
        }

        private static bool ValidateProgression()
        {
            var gottenItems = new Dictionary<string, int>();
            var locationsToVisit = new HashSet<Location>(locations);
            int visitedLocations;

            do
            {
                visitedLocations = 0;

                foreach (var location in locationsToVisit.ToList())
                {
                    if (RequirementsMet(location, gottenItems))
                    {
                        // Add the location's item to gottenItems
                        if (location.AssignedItem != null)
                        {
                            if (gottenItems.ContainsKey(location.AssignedItem.InternalName))
                            {
                                gottenItems[location.AssignedItem.InternalName]++;
                            }
                            else
                            {
                                gottenItems[location.AssignedItem.InternalName] = 1;
                            }
                        }

                        locationsToVisit.Remove(location);
                        visitedLocations++;
                    }
                }
            }
            while (visitedLocations > 0);

            // If all locations are visited, progression is valid
            return locationsToVisit.Count == 0;
        }

        private static bool RequirementsMet(Location location, Dictionary<string, int> gottenItems)
        {
            foreach (var requirement in location.RequiredItems)
            {
                if (!gottenItems.ContainsKey(requirement.ItemName) || gottenItems[requirement.ItemName] < requirement.ItemCount)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
