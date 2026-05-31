using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace CERandomizer
{
    public static class MechRandomizer
    {
        private static readonly Dictionary<int, int> mechSkillLinkage = new Dictionary<int, int>();
        private static bool mechSkillPatchesApplied = false;

        public static void RandomizeMechStatBoosts()
        {
            Console.WriteLine("Randomizer - Randomizing mech stat boosts...");

            Dictionary<string, int> statValues = new Dictionary<string, int>
            {
                { "Health Points", 100 },
                { "Tech Points", 10 },
                { "Attack", 20 },
                { "Magic", 20 },
                { "Defense", 20 },
                { "Mind", 20 },
                { "Critical %", 5 },
                { "Agility", 5 }
            };

            foreach (StatBoost statBoost in GetDatabase.GetStatBoosts().Where(s => s.mechs))
            {
                KeyValuePair<string, int> selectedStat = statValues
                    .OrderBy(_ => RandomGen.Range(-10000, 10000))
                    .First();

                statBoost.value = selectedStat.Value;
                statBoost.stat = selectedStat.Key;
            }
        }

        public static void RandomizeMechSkills()
        {
            Console.WriteLine("Randomizer - Randomizing mech skills...");

            mechSkillLinkage.Clear();

            List<Skill> mechSkills = GetDatabase.GetSkills()
                .Where(s => s.skillUser >= 100)
                .ToList();
            List<int> randomizedSkillIds = mechSkills.Select(s => s.skillID).ToList();

            foreach (Skill skill in mechSkills)
            {
                int replacementIndex = RandomGen.Range(0, randomizedSkillIds.Count);
                int replacementSkillId = randomizedSkillIds[replacementIndex];
                randomizedSkillIds.RemoveAt(replacementIndex);
                mechSkillLinkage.Add(skill.skillID, replacementSkillId);
            }

            ApplyMechSkillPatches();
        }

        private static void ApplyMechSkillPatches()
        {
            if (mechSkillPatchesApplied)
            {
                return;
            }

            Harmony harmony = new Harmony("com.Samupo.CERandomizer.MechSkills");
            PatchPrefix(harmony, typeof(EquipFunctions), "CheckMechWeaponSkills", nameof(CheckMechWeaponSkills_Randomized));
            PatchPrefix(harmony, typeof(GameFunctions), "CheckMechSkillsAfterLevel", nameof(CheckMechSkillsAfterLevel_Randomized));
            PatchPrefix(harmony, typeof(MainMenuMecha), "ReturnProfSkill", nameof(ReturnProfSkill_Randomized));

            mechSkillPatchesApplied = true;
        }

        private static void PatchPrefix(Harmony harmony, Type targetType, string targetMethodName, string prefixMethodName)
        {
            MethodInfo original = AccessTools.Method(targetType, targetMethodName);
            MethodInfo prefix = typeof(MechRandomizer).GetMethod(prefixMethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            if (original == null || prefix == null)
            {
                Console.WriteLine("Randomizer - Failed to patch " + targetType.Name + "." + targetMethodName);
                return;
            }

            harmony.Patch(original, prefix: new HarmonyMethod(prefix));
        }

        private static bool CheckMechSkillsAfterLevel_Randomized(int slot, int user, Equip equip)
        {
            int profession = equip.equipType - 17;
            int professionLevel = GameFunctions.ReturnProfLevel(user, profession);
            List<SkillItem> randomizedSkills = GetRandomizedMechProfessionSkills(profession, user);
            List<SkillItem> learnedSkills = GetData.GetSkills();

            foreach (SkillItem skillItem in randomizedSkills)
            {
                if (GameFunctions.SkillExists(skillItem.skillId, user) == null)
                {
                    learnedSkills.Add(skillItem);
                }
            }

            AssignMechSkillSlots(slot, user, randomizedSkills, professionLevel);
            return false;
        }

        public static bool CheckMechWeaponSkills_Randomized(int slot, int user, Equip equip, int previousOwner)
        {
            if (slot != 6 && slot != 7)
            {
                return false;
            }

            int profession = equip.equipType - 17;
            int professionLevel = GameFunctions.ReturnProfLevel(user, profession);
            List<SkillItem> randomizedSkills = GetRandomizedMechProfessionSkills(profession, user);
            List<SkillItem> learnedSkills = GetData.GetSkills();

            if (previousOwner != -1)
            {
                ClearMechSkillSlots(slot, previousOwner);
            }

            ClearMechSkillSlots(slot, user);

            foreach (SkillItem skillItem in randomizedSkills)
            {
                if (GameFunctions.SkillExists(skillItem.skillId, user) == null)
                {
                    learnedSkills.Add(skillItem);
                }
            }

            AssignMechSkillSlots(slot, user, randomizedSkills, professionLevel);
            return false;
        }

        public static bool ReturnProfSkill_Randomized(ref Skill __result, int prof, int lvl)
        {
            int originalSkillId = 300 + prof * 4 + (lvl - 1);
            int randomizedSkillId;

            if (!mechSkillLinkage.TryGetValue(originalSkillId, out randomizedSkillId))
            {
                return true;
            }

            __result = GetDatabase.GetSkills().Find(s => s.skillID == randomizedSkillId);
            return false;
        }

        private static List<SkillItem> GetRandomizedMechProfessionSkills(int profession, int user)
        {
            List<SkillItem> randomizedSkills = new List<SkillItem>();

            for (int i = 0; i < 4; i++)
            {
                int originalSkillId = 300 + i + 4 * profession;
                int randomizedSkillId;

                if (!mechSkillLinkage.TryGetValue(originalSkillId, out randomizedSkillId))
                {
                    randomizedSkillId = originalSkillId;
                }

                SkillItem existingSkill = GameFunctions.SkillExists(randomizedSkillId, user);
                randomizedSkills.Add(existingSkill ?? new SkillItem(randomizedSkillId, 0, user, 0, 1, 0, false, false));
            }

            return randomizedSkills;
        }

        private static void ClearMechSkillSlots(int slot, int user)
        {
            for (int skillId = 300; skillId < 400; skillId++)
            {
                SkillItem skillItem = GameFunctions.SkillExists(skillId, user);
                if (skillItem == null)
                {
                    continue;
                }

                if (slot == 7 && skillItem.skillSlot < 5)
                {
                    skillItem.skillSlot = 0;
                }
                if (slot == 6 && skillItem.skillSlot >= 5)
                {
                    skillItem.skillSlot = 0;
                }
            }
        }

        private static void AssignMechSkillSlots(int slot, int user, List<SkillItem> randomizedSkills, int professionLevel)
        {
            int slotOffset;

            if (slot == 7)
            {
                slotOffset = 1;
            }
            else if (slot == 6)
            {
                slotOffset = 5;
            }
            else
            {
                return;
            }

            int[] requiredLevels = { 1, 3, 5, 7 };
            for (int i = 0; i < randomizedSkills.Count; i++)
            {
                SkillItem skillItem = GameFunctions.SkillExists(randomizedSkills[i].skillId, user);
                if (skillItem != null && professionLevel >= requiredLevels[i])
                {
                    skillItem.skillSlot = slotOffset + i;
                }
            }
        }
    }
}
