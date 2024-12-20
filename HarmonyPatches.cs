﻿using System;
using HarmonyLib;
using PixelCrushers.DialogueSystem.SequencerCommands;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CERandomizer
{
    public static class HarmonyPatches
    {
        private static Harmony harmony;

        public static void ApplyPatches()
        {
            harmony = new Harmony("com.Samupo.CERandomizer"); // Unique identifier for your patch

            ApplyPrefixPatch(typeof(Chest), nameof(Chest.StartAction), nameof(StartActionPrefix));
            ApplyPrefixPatch(typeof(BattleFunctions), nameof(BattleFunctions.DistributeLoot), nameof(DistributeLootPrefix));
            ApplyPrefixPatch(typeof(SequencerCommandClassEmblemAdd), nameof(SequencerCommandClassEmblemAdd.Start), nameof(AddEmblemSequencePrefix));
            ApplyPrefixPatch(typeof(MainMenuRewardBoard), nameof(MainMenuRewardBoard.Claim), nameof(ClaimPrefix));
            ApplyPrefixPatch(typeof(MainMenuRewardBoard), nameof(MainMenuRewardBoard.ClaimChainReward), nameof(ClaimChainPrefix));
            ApplyPrefixPatch(typeof(ShopMenuBazaar), nameof(ShopMenuBazaar.ExecutePurchase), nameof(ExecutePurchasePrefix));
            // ApplyPrefixPatch(typeof(CraftMenuEquipment), nameof(CraftMenuEquipment.ExecutePurchase), nameof(ExecuteUpgradePrefix));
            ApplyPrefixPatch(typeof(CreditsRoll), nameof(CreditsRoll.Start), nameof(EndGame));

            // Fixes
            ApplyPrefixPatch(typeof(BattleActions), nameof(BattleActions.PlayAnimation), nameof(PlayAnimationFix));
            ApplyPrefixPatch(typeof(SkillFunctions), nameof(SkillFunctions.UseSkill), nameof(PreUseSkill));

            ApplyPostfixPatch(typeof(GameFunctions), nameof(GameFunctions.SwitchPMSlot), nameof(OptimizeAfterJoiningParty));
            ApplyPostfixPatch(typeof(MainMenuFunctions), nameof(MainMenuFunctions.ActivateLeftButtons), nameof(ActivateLeftButtons_AlwaysAllowSkills));
            ApplyPostfixPatch(typeof(SaveLoad), nameof(SaveLoad.Load), nameof(DoAfterLoad));
        }

        // Helper method to apply a prefix patch
        private static void ApplyPrefixPatch(Type targetClass, string targetMethodName, string prefixMethodName, Type[] paramTypes = null)
        {
            // Get the method to patch
            System.Reflection.MethodInfo? original = null;
            if (paramTypes == null)
            {
                original = AccessTools.Method(targetClass, targetMethodName);
            }
            else
            {
                original = AccessTools.Method(targetClass, targetMethodName, paramTypes);
            }

            // Get the prefix method in this class
            var prefix = typeof(HarmonyPatches).GetMethod(prefixMethodName);

            // Apply the prefix patch
            harmony.Patch(original, prefix: new HarmonyMethod(prefix));
        }

        private static void ApplyPostfixPatch(Type targetClass, string targetMethodName, string prefixMethodName)
        {
            // Get the method to patch
            var original = AccessTools.Method(targetClass, targetMethodName);

            // Get the prefix method in this class
            var prefix = typeof(HarmonyPatches).GetMethod(prefixMethodName);

            // Apply the prefix patch
            harmony.Patch(original, postfix: new HarmonyMethod(prefix));
        }

        private static void RemoveDuplicates(string inputFilePath, string outputFilePath)
        {
            if (string.IsNullOrWhiteSpace(inputFilePath) || string.IsNullOrWhiteSpace(outputFilePath))
            {
                throw new ArgumentException("File paths cannot be null or empty.");
            }

            // Read all lines from the file
            var lines = File.ReadAllLines(inputFilePath);

            // Use a HashSet to track unique entries
            HashSet<string> seenLines = new HashSet<string>();
            List<string> outputLines = new List<string>();

            foreach (var line in lines)
            {
                if (seenLines.Add(line)) // Adds to HashSet, returns false if already present
                {
                    outputLines.Add(line);
                }
            }

            // Write the filtered lines to the output file
            File.WriteAllLines(outputFilePath, outputLines);
        }

        private static bool UnlockLocation(string locationName, out RandomizerDatabase.Item itemGotten)
        {
            RandomizerDatabase.Location location = RandomizerDatabase.locations.Find(location => location.InternalName == locationName);
            itemGotten = null;

            if (RandomizerOptions.UseArchipelago > 0)
            {
                GetData.GetChests().Add(Archipelago.LOCATION_DATA_PREFIX + location.ID);
                Archipelago.MarkLocationAsChecked(location.ID);
                return true;
            }
            else
            {
                if (location != null)
                {
                    if (location.AssignedItem == null) return false; // Can't find associated item
                    if (RandomizerUtils.HasLocationBeenCompleted(locationName)) return true; // Location already checked, true to skip
                    if (RandomizerUtils.UnlockItem(location.AssignedItem))
                    {
                        itemGotten = location.AssignedItem;
                        RandomizerUtils.MarkLocationAsCompleted(locationName);
                        return true; // Item found and unlocked
                    }
                    else
                    {
                        // TODO: Go through unassigned items and unlock one at random
                        return false; // Could not spawn an item for location
                    }
                }
            }
            return false; // Can't find location
        }

        public static void OptimizeAfterJoiningParty(int id, int slot)
        {
            ActionQueue.AddAction(() => { RandomizerUtils.OptimizeCharacterEquipment(id); }, 0.5f);
        }

        public static bool ExecuteUpgradePrefix(GameObject go)
        {
            Il2CppSystem.Collections.Generic.List<EquipItem> list = GetData.GetEquipItems();
            Equip equip = null;
            int equippedBy = go.GetComponent<MenuButtonInformation>().equipItem.equippedBy;
            if (equippedBy != 99)
            {
                EquipFunctions.UnEquip(go.GetComponent<MenuButtonInformation>().equipItem.id);
            }
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].id == go.GetComponent<MenuButtonInformation>().equipItem.id)
                {
                    equip = EquipFunctions.GetEquip(list[i].equipID, byEquipItemId: false);

                    string locationName = "Upgrade" + equip.equipName + list[i].equipLvl;
                    Debug.Log("Upgraded: " + equip.equipName );
                    //File.AppendAllText("locations.txt", "Upgrade," + locationName + "," + locationName + ",Missable\r\n");
                    //RemoveDuplicates("locations.txt", "locations.txt");
                }
            }
            if (equippedBy != 99)
            {
                EquipFunctions.EquipItem(equippedBy, go.GetComponent<MenuButtonInformation>().equipItem.id);
            }

            return true;
        }

        // Used for deals
        public static bool ExecutePurchasePrefix(BazaarItem bItem)
        {
            Debug.Log("Claimed Deal: " + bItem.bazaarName);
            //File.AppendAllText("locations.txt", "Deal," + bItem.bazaarName + "," + bItem.bazaarName + ",Not Missable\r\n");
            //RemoveDuplicates("locations.txt", "locations.txt");
            
            UnlockLocation(bItem.bazaarName, out RandomizerDatabase.Item item);
            return true;
        }

        public static bool ClaimPrefix(int id)
        {
            Debug.Log("Claimed Board: " + id);
            //File.AppendAllText("locations.txt", "Board,Board" + id + ",Board" + id + ",Not Missable\r\n");
            //RemoveDuplicates("locations.txt", "locations.txt");

            UnlockLocation("Board" + id, out RandomizerDatabase.Item item);
            return true;
        }

        public static bool ClaimChainPrefix()
        {
            int longest = MainMenuRewardBoard.longestChain;
            if (longest >= 4)
            {
                Debug.Log("Claimed Chain Board: " + 4);
                //File.AppendAllText("locations.txt", "ChainBoard,Chain" + 4 + ",Chain" + 4 + ",Not Missable\r\n");
                //RemoveDuplicates("locations.txt", "locations.txt");

                UnlockLocation("Chain" + 4, out RandomizerDatabase.Item item);
            }
            for (int i = 1; i < 16; i++)
            {
                int step = i * 8;
                if (longest >= step)
                {
                    Debug.Log("Claimed Chain Board: " + step);
                    //File.AppendAllText("locations.txt", "ChainBoard,Chain" + step + ",Chain" + step + ",Not Missable\r\n");
                    //RemoveDuplicates("locations.txt", "locations.txt");

                    UnlockLocation("Chain" + step, out RandomizerDatabase.Item item);
                }
            }
            return true;
        }

        public static bool AddEmblemSequencePrefix(SequencerCommandClassEmblemAdd __instance)
        {
            foreach (ClassEmblem emblem in GetDatabase.GetClassEmblems())
            {
                if (emblem.classId == __instance.GetParameterAsInt(0))
                {
                    Debug.Log("Got Emblem: " + emblem.className);
                    //File.AppendAllText("locations.txt", "Emblem," + emblem.className + "," + emblem.className + ",Not Missable\r\n");
                    //RemoveDuplicates("locations.txt", "locations.txt");

                    if (UnlockLocation(emblem.className, out RandomizerDatabase.Item item))
                    {
                        // TODO: Show gotten item
                        return false;
                    }
                    return true;
                }
            }
            return true;
        }

        public static bool DistributeLootPrefix(ref float GS)
        {
            if (GS > 0)
            {
                Debug.Log("Defeated boss: " + Battle.enemyGO[0].name);
                //File.AppendAllText("locations.txt", "Boss," + Battle.enemyGO[0].name + "0," + Battle.enemyGO[0].name + "0,Not Missable\r\n");
                //File.AppendAllText("locations.txt", "Boss," + Battle.enemyGO[0].name + "1," + Battle.enemyGO[0].name + "1,Missable\r\n");
                //File.AppendAllText("locations.txt", "Boss," + Battle.enemyGO[0].name + "2," + Battle.enemyGO[0].name + "2,Missable\r\n");
                //RemoveDuplicates("locations.txt", "locations.txt");

                if (UnlockLocation(Battle.enemyGO[0].name + "0", out RandomizerDatabase.Item item))
                {
                    if (item != null)
                    {
                        ShowItemGotten(Battle.enemyGO[0].transform, item);
                    }
                    GS = 0;
                }
                if (UnlockLocation(Battle.enemyGO[0].name + "1", out RandomizerDatabase.Item item2))
                {
                    if (item2 != null)
                    {
                        ShowItemGotten(Battle.enemyGO[0].transform, item2);
                    }
                    GS = 0;
                }
                if (UnlockLocation(Battle.enemyGO[0].name + "2", out RandomizerDatabase.Item item3))
                {
                    if (item3 != null)
                    {
                        ShowItemGotten(Battle.enemyGO[0].transform, item3);
                    }
                    GS = 0;
                }
            }
            return true;
        }

        // Prefix method to run before StartAction()
        public static bool StartActionPrefix(Chest __instance)
        {
            Debug.Log("Opening chest: " + __instance.chestID);
            //File.AppendAllText("locations.txt", "Chest," + __instance.chestID + "," + __instance.chestID + ",Not Missable\r\n");
            //RemoveDuplicates("locations.txt", "locations.txt");

            if (__instance.taken) return false; // Skip if already taken

            if (!__instance.CheckLockConditions())
            {
                if (UnlockLocation(__instance.chestID, out RandomizerDatabase.Item item))
                {
                    if (item != null)
                    {
                        ShowItemGotten(__instance.transform, item);
                    }

                    __instance.taken = true;
                    if (__instance.buriedTreasure || __instance.sparklingLight)
                    {
                        UnityEngine.Object.Destroy(__instance.gameObject, 0.1f);
                    }
                    else
                    {
                        // Visuals and feedback
                        __instance.GetComponent<Animator>().SetBool("opening", true);
                        SoundManager.PlaySound("ChestOpen");

                        // Other checks made by chests
                        var mainQuest = GetData.GetMainQuest();
                        bool flag = false;
                        for (int i = 0; i < mainQuest.Count; i++)
                        {
                            if (mainQuest[i].nameState == "ChestsOpened")
                            {
                                mainQuest[i].intState++;
                                flag = true;
                            }
                        }
                        if (!flag)
                        {
                            mainQuest.Add(new WorldState(0, "ChestsOpened", "", newBool: true, 1, ""));
                        }
                    }

                    // More checks
                    var chests = GetData.GetChests();
                    chests.Add(__instance.chestID);
                    GetData.InsertChests(chests);
                    RewardManager.CheckTreasures(__instance.chestID);
                    RewardManager.CheckBuriedTreasures(__instance.chestID);
                    RewardManager.CheckOther();
                    return false; // Handled by randomizer
                }
                else
                {
                    return true; // Handled by game
                }
            }
            return true; // Handled by game
        }

        private static void ShowItemGotten(Transform parent, RandomizerDatabase.Item item)
        {
            try
            {
                GameObject messageInstance = GameObject.FindWithTag("LevelManager").GetComponent<AssetManager>().LoadGameObject("BattleMessage");
                float x = parent.position.x;
                float y = parent.position.y + 30f;
                messageInstance = UnityEngine.Object.Instantiate(messageInstance, new Vector3(x, y, 0f), Quaternion.identity);

                Sprite sprite = GameObject.FindWithTag("MainMenu").GetComponent<AssetManager>().LoadSprite(GameFunctions.ReturnTypeName(1, 1));
                if (item.Classification == RandomizerDatabase.Item.ItemClassification.Progression)
                {
                    sprite = GameObject.FindWithTag("MainMenu").GetComponent<AssetManager>().LoadSprite(GameFunctions.ReturnTypeName(3, 1));
                }
                else if (item.Classification == RandomizerDatabase.Item.ItemClassification.Useful)
                {
                    sprite = GameObject.FindWithTag("MainMenu").GetComponent<AssetManager>().LoadSprite(GameFunctions.ReturnTypeName(2, 1));
                }

                messageInstance.GetComponentInChildren<Animator>().SetBool("open", value: true);
                messageInstance.GetComponent<UIElementHolder>().uiElement[0].GetComponent<UnityEngine.UI.Image>().sprite = sprite;
                messageInstance.GetComponent<UIElementHolder>().uiElement[1].SetActive(value: true);

                TMPro.TMP_Text componentInChildren = messageInstance.GetComponentInChildren<TMPro.TMP_Text>(true);
                componentInChildren.text = item.UserFriendlyName;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        public static void ActivateLeftButtons_AlwaysAllowSkills(ref MainMenuFunctions __instance, bool active)
        {
            if (active)
            {
                ((Selectable)__instance.mm.leftMenuButtons[2].GetComponent<Button>()).interactable = true;
                ((Graphic)__instance.mm.leftMenuButtons[2].GetComponentInChildren<TextMeshProUGUI>()).color = new Color32(255,255,255,255);
            }
        }

        public static void EndGame()
        {
            Archipelago.MarkGameAsCompleted();
        }

        public static void DoAfterLoad()
        {
            foreach (string chest in GetData.GetChests())
            {
                if (chest.StartsWith(Archipelago.LOCATION_DATA_PREFIX))
                {
                    try
                    {
                        long locationName = long.Parse(chest.Substring(Archipelago.LOCATION_DATA_PREFIX.Length));
                        Archipelago.MarkLocationAsChecked(locationName);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to send location to Archipelago: " + chest);
                    }
                }
            }
            Archipelago.GetAllItems();
        }

        static bool shouldDoAnimationFix = true;
        public static bool PlayAnimationFix(ref BattleActions __instance, ref GameObject __result, string name, GameObject target, int offsetX, int offsetY, string layer = "", GameObject loadSource = null, int sourceAnimation = 0, bool switchDir = false, GameObject flipTarget = null, int sortingOrder = -1)
        {
            if (shouldDoAnimationFix)
            {
                shouldDoAnimationFix = false;
                try
                {
                    __result = __instance.PlayAnimation(name, target, offsetX, offsetY, layer, loadSource, sourceAnimation, switchDir, flipTarget, sortingOrder);
                    if (__result != null)
                    {
                        GameObject.Destroy(__result, 5.0f);
                    }
                }
                catch (Exception e)
                {
                    __result = null;
                }
            }
            else
            {
                return true;
            }
            shouldDoAnimationFix = true;
            return false;
        }

        static bool shouldDoSkilFix = true;
        public static bool PreUseSkill(SkillFunctions __instance, int user, int target, int skill)
        {
            if (skill != 153 && skill != 205) return true;
            if (!shouldDoSkilFix) return true;

            shouldDoSkilFix = false;

            // Pre, to get all variables stored properly
            Battle.destinationReached = false;
            List<GameObject> goList = new List<GameObject>();
            __instance.physicalDamage = false;
            __instance.magicalDamage = false;
            __instance.AOEEffect = false;
            GameObject targetGO = ((target >= 1000) ? Battle.triggerGO[target - 1000] : ((Targeting.currentTargetMode != 1 && Targeting.currentTargetMode != 2 && Targeting.currentTargetMode != 7) ? Battle.partyGO[target] : Battle.enemyGO[target]));
            if (Battle.mechBattle && skill >= 300 && skill < 316)
            {
                Battle.partyGO[user].GetComponent<Animator>().runtimeAnimatorController = Battle.partyGO[user].GetComponent<SingleEntity>().mechWeapon1Animator;
                Battle.partyGO[user].GetComponent<SingleEntity>().mechAnimator = Battle.partyGO[user].GetComponent<SingleEntity>().mechWeapon1Animator;
                Battle.meleeMode[user] = true;
            }
            if (Battle.mechBattle && skill >= 316)
            {
                Battle.partyGO[user].GetComponent<Animator>().runtimeAnimatorController = Battle.partyGO[user].GetComponent<SingleEntity>().mechWeapon2Animator;
                Battle.partyGO[user].GetComponent<SingleEntity>().mechAnimator = Battle.partyGO[user].GetComponent<SingleEntity>().mechWeapon2Animator;
                Battle.meleeMode[user] = false;
            }
            if (Battle.mechBattle)
            {
                Battle.partyGO[user].GetComponent<Animator>().SetBool("battleModus", value: true);
                Battle.partyGO[user].GetComponent<Animator>().SetBool("started", value: true);
            }
            State state = null;
            goList.Clear();
            int stateDMG = 0;
            Battle.partyGO[user].GetComponent<Animator>();
            if (skill < 246 || skill >= 300)
            {
                BattleFunctions.SpendTP(user, skill);
            }
            GameObject userGO = Battle.partyGO[user];
            CounterEvents ce = __instance.GetComponent<CounterEvents>();
            __instance.GetComponent<BattleEvents>();
            __instance.CameraZoomReset();
            __instance.FixAllDir(fix: true);
            bool kneeing = Battle.partyGO[user].GetComponent<Animator>().GetBool("knee");
            Battle.partyGO[user].GetComponent<Animator>().SetBool("knee", value: false);
            var skills = Battle.skills;
            for (int num = 0; num < skills.Count; num++)
            {
                if (skills[num].skillID == skill)
                {
                    __instance.ShowSkillName(LocalizationManager.Translate(skills[num].skillName, "SKILLS_NAME"), Battle.partyGO[user].GetComponent<CurrentStats>().name, 1, 2f, party: true);
                    __instance.cSkill = skills[num];
                }
            }
            if (userGO.GetComponent<CurrentStats>().id == 9 && skill != 151)
            {
                userGO.GetComponent<CurrentStats>().no1 = 0;
            }

            // Skill specific corrections
            // Falling dragon
            if (skill == 153)
            {
                __instance.physicalDamage = true;
                __instance.AOEEffect = true;
                for (int num42 = 0; num42 < Battle.enemyGO.Count; num42++)
                {
                    if (Battle.enemyGO[num42].GetComponent<CurrentStats>().alive)
                    {
                        if (__instance.CheckIfHit(num42, __instance.physicalDamage, __instance.magicalDamage))
                        {
                            int amount56 = BattleFunctions.PhysAttack(Battle.partyGO[user], Battle.enemyGO[num42], __instance.cSkill.value1a, 0);
                            __instance.Shake("strongShake");
                            __instance.ShowDamage(Battle.partyGO[user], Battle.enemyGO[num42], amount56, crit: false, 1);
                            BattleFunctions.Damage(Battle.enemyGO[num42], amount56);
                        }
                        else
                        {
                            __instance.ShowMessage("Missed", Battle.enemyGO[num42], pop: true);
                        }
                    }
                }
                __instance.AddState(145, Battle.partyGO[user], Battle.partyGO[user], 0, 2);
            }
            // Suspicious darts
            else if (skill == 205)
            {
                int skillLevel = 1;
                foreach (SkillItem skillItem in GetData.GetSkills())
                {
                    if (skillItem.skillUser == Battle.partyGO[user].GetComponent<CurrentStats>().id
                        && skillItem.skillId == skill)
                    {
                        skillLevel = skillItem.skillLvl;
                    }
                }
                float chance = 0.25f * (skillLevel + 1); // 0.5, 0.75 and 1.0
                int dartState = UnityEngine.Random.RandomRangeInt(0, 4);
                if (UnityEngine.Random.Range(0f, 1f) <= chance)
                {
                    for (int num42 = 0; num42 < Battle.enemyGO.Count; num42++)
                    {
                        if (Battle.enemyGO[num42].GetComponent<CurrentStats>().alive)
                        {
                            if (dartState == 0) __instance.AddState(26, Battle.partyGO[user], Battle.enemyGO[num42], 0, 1);
                            else if (dartState == 1) __instance.AddState(27, Battle.partyGO[user], Battle.enemyGO[num42], 0, 1);
                            else if (dartState == 2) __instance.AddState(30, Battle.partyGO[user], Battle.enemyGO[num42], 0, 1);
                            else if (dartState == 3) __instance.AddState(31, Battle.partyGO[user], Battle.enemyGO[num42], 0, 1);
                        }
                    }
                }
            }

            // This should keep the integrity of the battle
            if (!Battle.mechBattle)
            {
                if (true)
                {
                    for (int num80 = 0; num80 < skills.Count; num80++)
                    {
                        if (skills[num80].skillID != skill)
                        {
                            continue;
                        }
                        if (skills[num80].skillType == BattleFunctions.ODInfluencer)
                        {
                            BattleFunctions.ChangeUltimate(5, userGO);
                            BattleFunctions.ChangeOverDrive(-12);
                            BattleFunctions.ODInfluencer = 0;
                            BattleFunctions.ODTurnsLeft = 0;
                            GameObject.FindWithTag("partyInfoBattle").GetComponent<UserInterfaceElements>().OverdriveNumberBG.GetComponent<Image>().sprite = Battle.overdriveSkillSprites[0];
                            GameObject.FindWithTag("partyInfoBattle").GetComponent<UserInterfaceElements>().OverdriveTurnsLeft.GetComponent<TMPro.TMP_Text>().text = "";
                            continue;
                        }
                        if (Battle.partyGO.Count >= 4)
                        {
                            BattleFunctions.ChangeOverDrive(7);
                        }
                        if (Battle.partyGO.Count == 3)
                        {
                            BattleFunctions.ChangeOverDrive(6);
                        }
                        if (Battle.partyGO.Count <= 2)
                        {
                            BattleFunctions.ChangeOverDrive(5);
                        }
                    }
                }
                else
                {
                    BattleFunctions.ChangeOverDrive(-15);
                    BattleFunctions.ODInfluencer = 0;
                    BattleFunctions.ODTurnsLeft = 0;
                    GameObject.FindWithTag("partyInfoBattle").GetComponent<UserInterfaceElements>().OverdriveNumberBG.GetComponent<Image>().sprite = Battle.overdriveSkillSprites[0];
                    GameObject.FindWithTag("partyInfoBattle").GetComponent<UserInterfaceElements>().OverdriveTurnsLeft.GetComponent<TMPro.TMP_Text>().text = "";
                }
            }
            else
            {
                BattleFunctions.ChangeOverDrive(7);
            }
            if (BattleFunctions.StateExists(userGO, 37) && __instance.cSkill.skillType == 6)
            {
                userGO.GetComponent<CurrentStats>().no1--;
                if (userGO.GetComponent<CurrentStats>().no1 <= 0)
                {
                    __instance.RemoveState(37, Battle.partyGO[user], 0);
                }
            }
            if (__instance.AOEEffect)
            {
                for (int rollElement7 = 0; rollElement7 < Battle.enemyGO.Count; rollElement7++)
                {
                    ce.CounterEventChecker(userGO, Battle.enemyGO[rollElement7], __instance.physicalDamage, __instance.magicalDamage, __instance.elementalDamage, __instance.buffEffect, __instance.debuffEffect, __instance.healEffect, __instance.AOEEffect);
                }
            }
            else
            {
                ce.CounterEventChecker(userGO, targetGO, __instance.physicalDamage, __instance.magicalDamage, __instance.elementalDamage, __instance.buffEffect, __instance.debuffEffect, __instance.healEffect, __instance.AOEEffect);
            }
            Battle.TurnTaken = false;
            Battle.GoCharacter = false;
            __instance.FixAllDir(fix: false);
            //yield return new WaitForSeconds(0.1f);
            GameFunctions.TurnToThere(Battle.partyGO[user], Battle.CameraPosition, ignoreVertical: true);
            Battle.partyInfoBattle.InsertInfos();
            if (kneeing)
            {
                Battle.partyGO[user].GetComponent<Animator>().SetBool("knee", value: true);
            }

            shouldDoSkilFix = true;

            return false;
        }
    }
}
