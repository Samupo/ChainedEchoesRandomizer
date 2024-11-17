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
            { "Sword", new List<int>{ 1, 2, 3, 4, 5, 6, 7, 8 } }, // Tier 2 to 9
            { "Spear", new List<int>{ 13, 14, 15, 16, 17, 18, 19, 20 } }, // Tier 2 to 9
            { "Bow", new List<int>{ 25, 26, 27, 28, 29, 30, 31, 32 } }, // Tier 2 to 9
            { "Rapier", new List<int>{ 37, 38, 39, 40, 41, 42, 43, 44 } }, // Tier 2 to 9
            { "Gun", new List<int>{ 48, 49, 50, 51, 52, 53 } }, // Tier 4 to 9
            { "Amulet", new List<int>{ 57, 58, 59, 60, 61, 62, 63  } }, // Tier 3 to 9
            { "Gunspear", new List<int>{ 69, 70, 71} }, // Tier 7 to 9
            { "Katana", new List<int>{ 77, 78, 79, 80, 81, 82, 83, 84 } }, // Tier 2 to 9
            { "Greatsword", new List<int>{ 89, 90, 91, 92, 93} }, // Tiers 1-3 and 8-9
            { "Anchor", new List<int>{ 105, 106, 107, 108, 109, 110} }, // Tiers 4 to 9
            { "Cestus", new List<int>{ 97, 98, 99, 100 } }, // Tiers 6 to 9
            { "Cards", new List<int>{ 114, 115 } }, // Tiers 8 to 9

            // Armor progression
            { "LightArmor", new List<int>{121,122,123,124,125,126,127,128} }, // Tiers 2 to 9
            { "HeavyArmor", new List<int>{133,134,135,136,137,138,139,140} }, // Tiers 2 to 9
            { "Clothes", new List<int>{145,146,147,148,149,150,151,152} }, // Tiers 2 to 9
            { "Robe", new List<int>{157,158,159,160,161,162,163,164} }, // Tiers 2 to 9
            // Second armor set
            { "LightArmor2", new List<int>{121,122,123,124,125,126,127,128} }, // Tiers 2 to 9
            { "HeavyArmor2", new List<int>{133,134,135,136,137,138,139,140} }, // Tiers 2 to 9
            { "Clothes2", new List<int>{145,146,147,148,149,150,151,152} }, // Tiers 2 to 9
            { "Robe2", new List<int>{157,158,159,160,161,162,163,164} }, // Tiers 2 to 9

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
        };

        public class Location
        {
            public enum LocationType { Chest, Boss, Emblem, Deal, Board, ChainBoard }

            public struct ItemRequirement
            {
                public string ItemName { get; private set; }
                public int ItemCount { get; private set; }

                public ItemRequirement(string itemName, int itemCount) => (ItemName, ItemCount) = (itemName, itemCount);
            }

            public string UserFriendlyName { get; private set; }
            public string InternalName { get; private set; }
            public bool Missable { get; private set; }
            public List<ItemRequirement> RequiredItems { get; private set; } = new List<ItemRequirement>();
            public Item AssignedItem { get; set; }

            public Location(string userFriendlyName, string internalName, bool missable)
            {
                UserFriendlyName = userFriendlyName;
                InternalName = internalName;
                Missable = missable;
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
            public int ProgressiveCount { get; private set; }
            public bool IsProgressive => ProgressiveCount > 1;

            public Item(string userFriendlyName, string internalName, int progressiveCount, ItemClassification classification, ItemType type)
            {
                UserFriendlyName = userFriendlyName;
                InternalName = internalName;
                ProgressiveCount = progressiveCount;
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
                while ((line = reader.ReadLine()) != null)
                {
                    var data = line.Split(',');
                    if (data.Length < 4) continue;

                    Enum.TryParse(data[0], out Location.LocationType locationType);
                    var location = new Location(data[1], data[2], data[3] == "Missable");

                    if (glennStatBoostRequirement > 0)
                    {
                        location.AddRequirement("GlennStatBoost", glennStatBoostRequirement);
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
                    var data = line.Split(',');
                    if (data.Length < 5) continue;

                    int progressiveCount = int.Parse(data[2]);
                    Enum.TryParse(data[3], out Item.ItemType itemType);
                    Enum.TryParse(data[4], out Item.ItemClassification classification);

                    var item = new Item(data[0], data[1], progressiveCount, classification, itemType);
                    items.Add(item);
                }
            }
        }

        public static bool Randomize()
        {
            UnityEngine.Debug.Log("Attempting randomization...");
            var shuffledLocations = locations.OrderBy(_ => RandomGen.Next()).ToList();

            var progressionItems = items
                    .Where(i => i.Classification == Item.ItemClassification.Progression)
                    .SelectMany(i => Enumerable.Repeat(i, i.ProgressiveCount > 1 ? i.ProgressiveCount : 1))
                    .ToList();

            var usefulItems = items
                    .Where(i => i.Classification == Item.ItemClassification.Useful)
                    .SelectMany(i => Enumerable.Repeat(i, i.ProgressiveCount > 1 ? i.ProgressiveCount : 1))
                    .OrderBy(_ => RandomGen.Next())
                    .ToList();

            var fillerItems = items
                    .Where(i => i.Classification == Item.ItemClassification.Filler)
                    .SelectMany(i => Enumerable.Repeat(i, i.ProgressiveCount > 1 ? i.ProgressiveCount : 1))
                    .ToList();

            ResetAssignments();

            AssignItems(shuffledLocations, progressionItems, false);
            AssignItems(shuffledLocations, usefulItems, true);
            AssignFillerItems(shuffledLocations, fillerItems);

            if (ValidateProgression())
            {
                Randomized = true;
                foreach (Location location in locations)
                {
                    if (location.AssignedItem != null)
                    {
                        UnityEngine.Debug.Log("[DEBUG RANDOMIZER] " + location.UserFriendlyName + " has " + location.AssignedItem.UserFriendlyName);
                    }
                    else
                    {
                        UnityEngine.Debug.Log("[DEBUG RANDOMIZER] " + location.UserFriendlyName + " doesn't have anything");
                    }
                }
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
