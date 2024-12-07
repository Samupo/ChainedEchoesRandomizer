using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CERandomizer
{
    public struct CharacterStatBooster
    {
        public int level;

        public int value;

        public string stat;

        public CharacterStatBooster(int level, int value, string stat)
        {
            this.level = level;
            this.value = value;
            this.stat = stat;
        }
    }

    public static class CoreRandomizer
    {
        public static int Tier1SwordID { get; private set; } = 0;
        public static int Tier1SpearID { get; private set; } = 12;
        public static int Tier1BowID { get; private set; } = 24;
        public static int Tier1RapierID { get; private set; } = 36;
        public static int Tier1KatanaID { get; private set; } = 76;
        public static int Tier1GreatswordID { get; private set; } = 88;

        // It'll use lowest available tier in DB (tier 1 if AddTier1Weapons was called)
        public static int Tier1GunID { get; private set; } = 48;
        public static int Tier1AmuletID { get; private set; } = 57;
        public static int Tier1GunbladeID { get; private set; } = 69;
        public static int Tier1ClawID { get; private set; } = 97;
        public static int Tier1AnchorID { get; private set; } = 105;
        public static int Tier1CardsID { get; private set; } = 114;

        public static void UnlearnAllSkills()
        {
            GetData.GetSkills().Clear();
        }

        public static void AddTier1Weapons()
        {
            Console.WriteLine("Randomizer - Adding tier 1 weapons...");

            Tier1GunID = GetDatabase.GetEquipment().Count;
            GetDatabase.GetEquipment().Add(new Equip(Tier1GunID, "Pistol", "Basic", 4, 0, 48, 0, 0, 18, 18, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "0", "0", 200, 50, 1, "30,37", "3,1", 100, "30,37,58", "3,2,1", 200, 2));
            
            Tier1AmuletID = GetDatabase.GetEquipment().Count;
            GetDatabase.GetEquipment().Add(new Equip(Tier1AmuletID, "Stone Amulet", "Basic", 5, 0, 57, 0, 0, 18, 18, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "0", "0", 200, 50, 1, "30,37", "3,1", 100, "30,37,58", "3,2,1", 200, 2));
            
            Tier1GunbladeID = GetDatabase.GetEquipment().Count;
            GetDatabase.GetEquipment().Add(new Equip(Tier1GunbladeID, "Prototype Gunblade", "Basic", 6, 0, 69, 0, 0, 18, 18, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "0", "0", 200, 50, 1, "30,37", "3,1", 100, "30,37,58", "3,2,1", 200, 2));
            
            Tier1ClawID = GetDatabase.GetEquipment().Count;
            GetDatabase.GetEquipment().Add(new Equip(Tier1ClawID, "Nice Gloves", "Basic", 9, 0, 97, 0, 0, 30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "0", "0", 200, 50, 1, "30,37", "3,1", 100, "30,37,58", "3,2,1", 200, 2));

            Tier1AnchorID = GetDatabase.GetEquipment().Count;
            GetDatabase.GetEquipment().Add(new Equip(Tier1AnchorID, "Anvil", "Basic", 10, 0, 105, 0, 0, 18, 18, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "0", "0", 200, 50, 1, "30,37", "3,1", 100, "30,37,58", "3,2,1", 200, 2));

            Tier1CardsID = GetDatabase.GetEquipment().Count;
            GetDatabase.GetEquipment().Add(new Equip(Tier1CardsID, "Playing Cards", "Basic", 11, 0, 105, 0, 0, 0, 30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "0", "0", 200, 50, 1, "30,37", "3,1", 100, "30,37,58", "3,2,1", 200, 2));
        }

        public static void RandomizeCharacterWeaponTypes()
        {
            Console.WriteLine("Randomizer - Randomizing character weapon types...");

            // Alter database
            List<int> weaponTypes = new List<int>();
            for (int i = 0; i < 12; i++) weaponTypes.Add(i);
            foreach (PartyMember member in RandomizerUtils.GetPlayableCharacters(true))
            {
                if (member.memberID == 12)
                {
                    member.allowedEquipTypes[0] = RandomGen.Range(0, 12);
                }
                else
                {
                    member.allowedEquipTypes[0] = weaponTypes[RandomGen.Range(0, weaponTypes.Count)];
                    weaponTypes.Remove(member.allowedEquipTypes[0]);
                }
            }
            // Copy to save
            foreach (PartyMember member in RandomizerUtils.GetPlayableCharacters(false))
            {
                foreach (PartyMember dbMember in RandomizerUtils.GetPlayableCharacters(true))
                {
                    if (dbMember.memberID == member.memberID)
                    {
                        member.allowedEquipTypes = dbMember.allowedEquipTypes;
                    }
                }
            }

            // Unequip all characters' weapons
            foreach (PartyMember member in RandomizerUtils.GetPlayableCharacters(false))
            {
                EquipFunctions.UnEquip(member.weapon);
            }

            // Remove all weapons from inventory
            for (int i=0; i < GetData.GetEquipItems().Count; i++)
            {
                Equip equipDB = null;
                foreach (Equip equipInDB in GetDatabase.GetEquipment())
                {
                    if (equipInDB.equipID == GetData.GetEquipItems()[i].equipID)
                    {
                        equipDB = equipInDB;
                    }
                }

                // Weapons
                if (equipDB.equipType <= 11)
                {
                    GetData.GetEquipItems().RemoveAt(i);
                    i--;
                }
            }

            // Add lowest available tier to inventory for each character
            foreach (PartyMember member in RandomizerUtils.GetPlayableCharacters(true))
            {
                Equip lowestTierEquip = null;
                foreach (Equip dbEquip in GetDatabase.GetEquipment())
                {
                    if (dbEquip.equipType == member.allowedEquipTypes[0])
                    {
                        if (lowestTierEquip == null || (lowestTierEquip.equipTier >= dbEquip.equipTier && dbEquip.equipTier > 0)) // >= to take the item with the higher ID
                        {
                            lowestTierEquip = dbEquip;
                        }
                    }
                }
                EquipFunctions.AddEquip(lowestTierEquip.equipID);
            }

            // Optimize equipment
            for (int i = 0; i<13; i++)
            {
                RandomizerUtils.OptimizeCharacterEquipment(i);
            }
        }

        public static void AverageCharacterInitialStats()
        {
            Console.WriteLine("Randomizer - Averaging character stats...");
            foreach (PartyMember member in GetDatabase.GetPartyMember())
            {
                if (member.memberID == 13 || member.memberID == 14)
                {
                    member.baseHP = (member.currentHP = (member.maxHP = (member.startingHP = 85)));
                    member.baseTP = (member.currentTP = (member.maxTP = (member.startingTP = 110)));
                    member.baseAtk = (member.currentAtk = (member.maxAtk = (member.startingAtk = 30)));
                    member.baseMag = (member.currentMag = (member.maxMag = (member.startingMag = 30)));
                    member.baseDef = (member.currentDef = (member.maxDef = (member.startingDef = 30)));
                    member.baseMnd = (member.currentMnd = (member.maxMnd = (member.startingMnd = 30)));
                    member.baseAgi = (member.currentAgi = (member.maxAgi = (member.startingAgi = 30)));
                }
                if (member.memberID <= 12)
                {
                    member.baseHP = (member.currentHP = (member.maxHP = (member.startingHP = 75)));
                    member.baseTP = (member.currentTP = (member.maxTP = (member.startingTP = 100)));
                    member.baseAtk = (member.currentAtk = (member.maxAtk = (member.startingAtk = 17)));
                    member.baseMag = (member.currentMag = (member.maxMag = (member.startingMag = 17)));
                    member.baseDef = (member.currentDef = (member.maxDef = (member.startingDef = 17)));
                    member.baseMnd = (member.currentMnd = (member.maxMnd = (member.startingMnd = 17)));
                    member.baseAgi = (member.currentAgi = (member.maxAgi = (member.startingAgi = 17)));
                }
            }
        }

        public static void RandomizeCharacterStatProgression()
        {
            Console.WriteLine("Randomizer - Randomizing character stat progression...");
            List<string> availableStats = new List<string>();
            availableStats.Add("Health Points");
            availableStats.Add("Tech Points");
            availableStats.Add("Attack");
            availableStats.Add("Magic");
            availableStats.Add("Defense");
            availableStats.Add("Mind");
            availableStats.Add("Agility");
            availableStats.Add("Critical %");
            int memberID;
            for (memberID = 0; memberID < 13; memberID++)
            {
                List<CharacterStatBooster> list2 = new List<CharacterStatBooster>();
                List<string> shuffledList = new List<string>(availableStats).OrderBy((s)=>RandomGen.Range(-10000,10000)).ToList();
                PartyMember member = null;
                foreach (PartyMember dbMember in GetDatabase.GetPartyMember())
                {
                    if (dbMember.memberID == memberID)
                    {
                        member = dbMember;
                    }
                }
                if (member == null) continue;

                List<string> shuffledRanks = new List<string>();
                for (int i = 0; i < RandomizerOptions.CharacterSRankStats; i++)
                {
                    shuffledRanks.Add("S");
                }
                for (int j = 0; j < RandomizerOptions.CharacterARankStats; j++)
                {
                    shuffledRanks.Add("A");
                }
                for (int k = 0; k < RandomizerOptions.CharacterBRankStats; k++)
                {
                    shuffledRanks.Add("B");
                }
                for (int l = 0; l < RandomizerOptions.CharacterCRankStats; l++)
                {
                    shuffledRanks.Add("C");
                }
                while (shuffledRanks.Count < shuffledList.Count)
                {
                    shuffledRanks.Add("A");
                }
                shuffledRanks = shuffledRanks.OrderBy((string g) => RandomGen.Range(-10000, 10000)).ToList();
                for (int n = 0; n < shuffledList.Count; n++)
                {
                    switch (shuffledList[n])
                    {
                        case "Health Points":
                            member.UPHP = shuffledRanks[0];
                            break;
                        case "Tech Points":
                            member.UPTP = shuffledRanks[0];
                            break;
                        case "Attack":
                            member.UPAtk = shuffledRanks[0];
                            break;
                        case "Magic":
                            member.UPMag = shuffledRanks[0];
                            break;
                        case "Defense":
                            member.UPDef = shuffledRanks[0];
                            break;
                        case "Mind":
                            member.UPMnd = shuffledRanks[0];
                            break;
                        case "Agility":
                            member.UPAgi = shuffledRanks[0];
                            break;
                        case "Critical %":
                            member.UPCrit = shuffledRanks[0];
                            break;
                    }
                    shuffledRanks.RemoveAt(0);
                }
            }
        }

        public static void RandomizeCharacterStatBoosts()
        {
            Console.WriteLine("Randomizer - Randomizing character stat boosts...");
            List<string> list = new List<string>();
            list.Add("Health Points");
            list.Add("Tech Points");
            list.Add("Attack");
            list.Add("Magic");
            list.Add("Defense");
            list.Add("Mind");
            list.Add("Agility");
            list.Add("Critical %");
            List<CharacterStatBooster> possibleBoosters = new List<CharacterStatBooster>();
            possibleBoosters.Add(new CharacterStatBooster(0, RandomizerOptions.HPStatBoostValueLevel1, list[0]));
            possibleBoosters.Add(new CharacterStatBooster(1, RandomizerOptions.HPStatBoostValueLevel2, list[0]));
            possibleBoosters.Add(new CharacterStatBooster(2, RandomizerOptions.HPStatBoostValueLevel3, list[0]));
            possibleBoosters.Add(new CharacterStatBooster(3, RandomizerOptions.HPStatBoostValueLevel4, list[0]));
            possibleBoosters.Add(new CharacterStatBooster(0, RandomizerOptions.TPStatBoostValueLevel1, list[1]));
            possibleBoosters.Add(new CharacterStatBooster(1, RandomizerOptions.TPStatBoostValueLevel2, list[1]));
            possibleBoosters.Add(new CharacterStatBooster(2, RandomizerOptions.TPStatBoostValueLevel3, list[1]));
            possibleBoosters.Add(new CharacterStatBooster(3, RandomizerOptions.TPStatBoostValueLevel4, list[1]));
            possibleBoosters.Add(new CharacterStatBooster(0, RandomizerOptions.CoreStatsBoostValueLevel1, list[2]));
            possibleBoosters.Add(new CharacterStatBooster(1, RandomizerOptions.CoreStatsBoostValueLevel2, list[2]));
            possibleBoosters.Add(new CharacterStatBooster(2, RandomizerOptions.CoreStatsBoostValueLevel3, list[2]));
            possibleBoosters.Add(new CharacterStatBooster(3, RandomizerOptions.CoreStatsBoostValueLevel4, list[2]));
            possibleBoosters.Add(new CharacterStatBooster(0, RandomizerOptions.CoreStatsBoostValueLevel1, list[3]));
            possibleBoosters.Add(new CharacterStatBooster(1, RandomizerOptions.CoreStatsBoostValueLevel2, list[3]));
            possibleBoosters.Add(new CharacterStatBooster(2, RandomizerOptions.CoreStatsBoostValueLevel3, list[3]));
            possibleBoosters.Add(new CharacterStatBooster(3, RandomizerOptions.CoreStatsBoostValueLevel4, list[3]));
            possibleBoosters.Add(new CharacterStatBooster(0, RandomizerOptions.CoreStatsBoostValueLevel1, list[4]));
            possibleBoosters.Add(new CharacterStatBooster(1, RandomizerOptions.CoreStatsBoostValueLevel2, list[4]));
            possibleBoosters.Add(new CharacterStatBooster(2, RandomizerOptions.CoreStatsBoostValueLevel3, list[4]));
            possibleBoosters.Add(new CharacterStatBooster(3, RandomizerOptions.CoreStatsBoostValueLevel4, list[4]));
            possibleBoosters.Add(new CharacterStatBooster(0, RandomizerOptions.CoreStatsBoostValueLevel1, list[5]));
            possibleBoosters.Add(new CharacterStatBooster(1, RandomizerOptions.CoreStatsBoostValueLevel2, list[5]));
            possibleBoosters.Add(new CharacterStatBooster(2, RandomizerOptions.CoreStatsBoostValueLevel3, list[5]));
            possibleBoosters.Add(new CharacterStatBooster(3, RandomizerOptions.CoreStatsBoostValueLevel4, list[5]));
            possibleBoosters.Add(new CharacterStatBooster(0, RandomizerOptions.AgiStatBoostValueLevel1, list[6]));
            possibleBoosters.Add(new CharacterStatBooster(1, RandomizerOptions.AgiStatBoostValueLevel2, list[6]));
            possibleBoosters.Add(new CharacterStatBooster(2, RandomizerOptions.AgiStatBoostValueLevel3, list[6]));
            possibleBoosters.Add(new CharacterStatBooster(3, RandomizerOptions.AgiStatBoostValueLevel4, list[6]));
            possibleBoosters.Add(new CharacterStatBooster(0, RandomizerOptions.CritStatBoostValueLevel1, list[7]));
            possibleBoosters.Add(new CharacterStatBooster(1, RandomizerOptions.CritStatBoostValueLevel2, list[7]));
            possibleBoosters.Add(new CharacterStatBooster(2, RandomizerOptions.CritStatBoostValueLevel3, list[7]));
            possibleBoosters.Add(new CharacterStatBooster(3, RandomizerOptions.CritStatBoostValueLevel4, list[7]));

            for (int i=0; i<GetDatabase.GetStatBoosts().Count; i++)
            {
                if (!GetDatabase.GetStatBoosts()[i].mechs)
                {
                    GetDatabase.GetStatBoosts().RemoveAt(i);
                    i--;
                }
            }

            int u;
            foreach (PartyMember member in GetDatabase.GetPartyMember())
            {
                if (member.memberID > 12) continue;

                foreach (StatBoost boost in GetData.GetStatBoosts())
                {
                    boost.learned = false;
                }

                List<CharacterStatBooster> boosters = new List<CharacterStatBooster>();
                List<string> characterStats = new List<string>(list);
                characterStats = characterStats.OrderBy(_ => RandomGen.Next()).ToList();

                int i;
                for (i = 0; i < characterStats.Count; i++)
                {
                    string text = "A";
                    switch (characterStats[i])
                    {
                        case "Health Points":
                            text = member.UPHP;
                            break;
                        case "Tech Points":
                            text = member.UPTP;
                            break;
                        case "Attack":
                            text = member.UPAtk;
                            break;
                        case "Magic":
                            text = member.UPMag;
                            break;
                        case "Defense":
                            text = member.UPDef;
                            break;
                        case "Mind":
                            text = member.UPMnd;
                            break;
                        case "Agility":
                            text = member.UPAgi;
                            break;
                        case "Critical %":
                            text = member.UPCrit;
                            break;
                    }
                    int boostersPerStat = 0;
                    switch (text)
                    {
                        case "S":
                            boostersPerStat = 12;
                            break;
                        case "A":
                            boostersPerStat = 8;
                            break;
                        case "B":
                            boostersPerStat = 4;
                            break;
                        case "C":
                            boostersPerStat = 2;
                            break;
                    }
                    for (int k = 0; k < boostersPerStat; k++)
                    {
                        boosters.AddRange(possibleBoosters.Where((CharacterStatBooster b) => b.stat == characterStats[i]));
                    }
                }
                int position = 0;
                int j;
                for (j = 0; j < 4; j++)
                {
                    for (int l = 0; l < 4; l++)
                    {
                        CharacterStatBooster characterStatBooster = (from s in boosters
                                                                     where s.level == j
                                                                     select s into g
                                                                     orderby RandomGen.Range(-10000, 10000)
                                                                     select g).First();
                        GetDatabase.GetStatBoosts().Add(new StatBoost(position, characterStatBooster.stat, characterStatBooster.value, member.memberID, false, false));
                        position++;
                    }
                }
            }
        }

        public static void RandomizeCharacterSkills()
        {
            Console.WriteLine("Randomizer - Randomizing character skills...");
            List<Skill> list = new List<Skill>();
            list.AddRange(GetDatabase.GetSkills().ToArray());
            list = (from s in list
                                where s.skillUser < 13 && s.skillID < 202
                                select s into g
                                orderby RandomGen.Range(-10000, 1000)
                                select g).ToList();

            foreach (PartyMember member in GetDatabase.GetPartyMember())
            {
                if (member.memberID < 12)
                {
                    for (int i = 1; i < 17; i++)
                    {
                        list[0].skillUser = member.memberID;
                        list[0].learnOrder = i;
                        list.RemoveAt(0);
                    }
                }
                else if (member.memberID == 12)
                {
                    for (int j = 1; j < 9; j++)
                    {
                        list[0].skillUser = member.memberID;
                        list[0].learnOrder = j;
                        list.RemoveAt(0);
                    }
                }
            }
        }

        public static void RandomizeCharacterPassives()
        {
            Console.WriteLine("Randomizer - Randomizing character passives...");
            List<int> list = new List<int>();
            for (int i = 0; i < 199; i++)
            {
                // These passives are obsolete or don't contain data
                if (i != 42 && i != 43 && i != 100 && i != 108 && i != 112 && i != 113 && i != 157 && i != 165 && i != 181 && i != 185 && i != 175 && i != 176 && i != 177 && i != 178 && i != 179 && i != 180 && i != 181 && i != 182 && i != 183 && i != 185)
                {
                    list.Add(i);
                }
            }
            foreach (PartyMember member in GetDatabase.GetPartyMember())
            {
                if (member.memberID > 12) continue;

                List<int> list2 = new List<int>(list.OrderBy((int g) => RandomGen.Range(-10000, 10000)));
                list2.RemoveRange(16, list2.Count - 16);
                member.learnablePassives = list2.ToArray();
                member.passives.Clear();
            }
        }

        public static void RemoveEquipmentDealRewards()
        {
            foreach (BazaarItem item in GetDatabase.GetBazaarGoods())
            {
                item.resultEquip = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<int>(new int[] { });
            }
            foreach (BazaarItem item in GetData.GetBazaarItems())
            {
                item.resultEquip = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<int>(new int[] { });
            }
        }
    }
}
