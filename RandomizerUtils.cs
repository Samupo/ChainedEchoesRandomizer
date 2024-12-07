using CERandomizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sony.NP.Matching;


public static class RandomizerUtils
{
    const string LOCATION_PREFIX = "RAND_";

    public static IEnumerable<PartyMember> GetPlayableCharacters(bool database)
    {
        if (database)
        {
            foreach (PartyMember member in GetDatabase.GetPartyMember())
            {
                if (member.memberID <= 12) yield return member;
            }
        }
        else
        {
            foreach (PartyMember member in GetData.GetPartyMember())
            {
                if (member.memberID <= 12) yield return member;
            }
        }
    }

    public static void OptimizeCharacterEquipment(int memberID)
    {
        PartyMember member = null;
        foreach (PartyMember savedMember in GetData.GetPartyMember())
        {
            if (savedMember.memberID == memberID)
            {
                member = savedMember;
            }
        }

        if (member == null) return;


        // Copy to save
        foreach (PartyMember dbMember in RandomizerUtils.GetPlayableCharacters(true))
        {
            if (dbMember.memberID == member.memberID)
            {
                member.allowedEquipTypes = dbMember.allowedEquipTypes;
            }
        }

        EquipFunctions.UnEquip(member.weapon, false);
        EquipFunctions.UnEquip(member.body, false);
        EquipItem mostPowerfulEquip = EquipFunctions.GetMostPowerfulEquip(EquipFunctions.GetEquipForSlot(0, memberID, 999), false);
        EquipItem mostPowerfulEquip2 = EquipFunctions.GetMostPowerfulEquip(EquipFunctions.GetEquipForSlot(1, memberID, 999), false);
        if (mostPowerfulEquip != null)
        {
            EquipFunctions.EquipItem(memberID, mostPowerfulEquip.id);
        }
        if (mostPowerfulEquip2 != null)
        {
            EquipFunctions.EquipItem(memberID, mostPowerfulEquip2.id);
        }
    }

    public static void ResetPartyMemberLevel(int memberID)
    {
        int level = 0;
        foreach (SkillItem skill in GetData.GetSkills())
        {
            if (skill.skillUser == memberID)
            {
                level++;
            }
        }
        foreach (StatBoost boost in GetData.GetStatBoosts())
        {
            if (boost.user == memberID && boost.learned)
            {
                level++;
            }
        }
        GameFunctions.SetPartyMemberLevel(memberID, level);
    }

    public static bool UnlockItem(RandomizerDatabase.Item item, string sender = "")
    {
        Console.WriteLine("Unlocking item " + item.InternalName + " [" + item.Type + "]");
        switch (item.Type)
        {
            case RandomizerDatabase.Item.ItemType.CharacterSkill:
                {
                    int current = 1;
                    int memberID = -1;

                    foreach (PartyMember member in GetDatabase.GetPartyMember())
                    {
                        if (item.InternalName.Contains(member.memberName))
                        {
                            memberID = member.memberID;
                            break;
                        }
                    }

                    if (memberID == -1)
                    {
                        Console.WriteLine("Member not found for item " + item.InternalName);
                        return false;
                    }
                    while (HasLocationBeenCompleted(item.InternalName + current)) current++;
                    AddSkill(memberID, current, sender);
                    MarkLocationAsCompleted(item.InternalName + current);
                    return true;
                }

            case RandomizerDatabase.Item.ItemType.CharacterPassive:
                {
                    int current = 1;
                    int memberID = -1;

                    foreach (PartyMember member in GetDatabase.GetPartyMember())
                    {
                        if (item.InternalName.Contains(member.memberName))
                        {
                            memberID = member.memberID;
                            break;
                        }
                    }

                    if (memberID == -1)
                    {
                        Console.WriteLine("Member not found for item " + item.InternalName);
                        return false;
                    }
                    while (HasLocationBeenCompleted(item.InternalName + current)) current++;
                    AddPassive(memberID, current, sender);
                    MarkLocationAsCompleted(item.InternalName + current);
                    return true;
                }

            case RandomizerDatabase.Item.ItemType.CharacterStatBoost:
                {
                    int current = 1;
                    int memberID = -1;

                    foreach (PartyMember member in GetDatabase.GetPartyMember())
                    {
                        if (item.InternalName.Contains(member.memberName))
                        {
                            memberID = member.memberID;
                            break;
                        }
                    }

                    if (memberID == -1)
                    {
                        Console.WriteLine("Member not found for item " + item.InternalName);
                        return false;
                    }
                    while (HasLocationBeenCompleted(item.InternalName + current)) current++;
                    AddStatBoost(memberID, current, sender);
                    MarkLocationAsCompleted(item.InternalName + current);
                    return true;
                }

            case RandomizerDatabase.Item.ItemType.Equipment:
            case RandomizerDatabase.Item.ItemType.MechEquipment:
                {
                    if (RandomizerDatabase.ItemIDs.ContainsKey(item.InternalName)) // Use progressive
                    {
                        int current = 0;
                        while (HasLocationBeenCompleted(item.InternalName + current)) current++;
                        try
                        {
                            AddEquipment(RandomizerDatabase.ItemIDs[item.InternalName][current], 1, sender);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Item was not found " + item.InternalName + " " + current);
                            return false;
                        }
                        MarkLocationAsCompleted(item.InternalName + current);
                    }
                    else
                    {
                        Equip equipToGet = null;
                        foreach (Equip equip in GetDatabase.GetEquipment())
                        {
                            if (equip.equipName == item.InternalName)
                            {
                                equipToGet = equip;
                                break;
                            }
                        }
                        if (equipToGet == null)
                        {
                            Console.WriteLine("Item was not found " + item.InternalName);
                            return false;
                        }
                        else
                        {
                            AddEquipment(equipToGet.equipID, 1, sender);
                        }
                    }
                    return true;
                }
            case RandomizerDatabase.Item.ItemType.Item:
                Item itemToGet = null;
                foreach (Item dbItem in GetDatabase.GetInventory())
                {
                    if (dbItem.itemName == item.InternalName)
                    {
                        itemToGet = dbItem;
                        break;
                    }
                }
                if (itemToGet == null)
                {
                    Console.WriteLine("Item was not found " + item.InternalName);
                    return false;
                }
                else
                {
                    AddItem(itemToGet.itemId, 1, sender);
                }
                return true;
            case RandomizerDatabase.Item.ItemType.ClassEmblem:
                try
                {
                    AddEmblem(RandomizerDatabase.ItemIDs[item.InternalName][0], sender);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Emblem was not found " + item.InternalName);
                    return false;
                }
                return true;
        }

        return false;
    }

    public static void MarkLocationAsCompleted(string locationName)
    {
        string chestLocation = LOCATION_PREFIX + locationName;
        if (!GetData.GetChests().Contains(chestLocation))
        {
            GetData.GetChests().Add(chestLocation);
        }
    }

    public static bool HasLocationBeenCompleted(string locationName)
    {
        string chestLocation = LOCATION_PREFIX + locationName;
        return GetData.GetChests().Contains(chestLocation);
    }

    public static void AddItem(int itemID, int amount, string sender = "")
    {
        string message = "Got " + amount + "x" + GameFunctions.GetItemName(0, itemID);
        if (sender != null && sender != "") message += " from " + sender;
        GameFunctions.AddItem(itemID, amount);
        RandomizerBehavior.PushMessage(message);
        SoundManager.PlaySound("Collectable");
    }

    public static void AddEquipment(int equipID, int amount, string sender = "")
    {
        string message = "Got " + amount + "x" + LocalizationManager.Translate(EquipFunctions.GetEquip(equipID, false).equipName, "EQUIPS_NAME");
        if (sender != null && sender != "") message += " from " + sender;
        for (int i = 0; i < amount; i++)
        {
            EquipFunctions.AddEquip(equipID);
        }
        RandomizerBehavior.PushMessage(message);
        SoundManager.PlaySound("Weapon Found");
    }

    public static void AddEmblem(int emblemID, string sender = "")
    {
        ClassEmblem classEmblem = EquipFunctions.AddClassEmblem(emblemID);
        string message = "Got Emblem: " + LocalizationManager.Translate(classEmblem.className, "CLASSEMBLEMS_NAME"); ;
        if (sender != null && sender != "") message += " from " + sender;
        RandomizerBehavior.PushMessage(message);
    }

    private static void LearnSkillInternal(int memberID, int skillID, bool passive)
    {
        GameFunctions.AddSkill(skillID, memberID, 0, passive ? 1 : 0);
        GameFunctions.ChangeSkillSlot(skillID, EquipFunctions.FindEmptySlot(memberID, passive ? 1 : 0), memberID, passive ? 1 : 0);
        ResetPartyMemberLevel(memberID);
    }

    public static void AddStatBoost(int memberID, int boostOrder, string sender = "")
    {
        Console.WriteLine("Adding stat boost " + boostOrder + " to " + memberID);
        string memberName = "????";
        foreach (var member in GetDatabase.GetPartyMember())
        {
            if (member.memberID == memberID)
            {
                memberName = member.memberName;
            }
        }

        int index = 0;
        foreach (StatBoost boostDB in GetDatabase.GetStatBoosts())
        {
            if (boostDB.user == memberID)
            {
                index++;
                if (index == boostOrder)
                {
                    string message = memberName + " got stronger";
                    if (sender != null && sender != "") message += " thanks to " + sender;
                    RandomizerBehavior.PushMessage(message);
                    GameFunctions.AddBoost(boostDB);
                    SoundManager.PlaySound("Rare Weapon Found");
                }
            }
        }

        ResetPartyMemberLevel(memberID);
    }

    public static void AddPassive(int memberID, int passiveOrder, string sender = "")
    {
        foreach (var member in GetDatabase.GetPartyMember())
        {
            if (member.memberID == memberID)
            {
                int passiveID = member.learnablePassives[passiveOrder - 1];

                Passive passive = null;
                foreach (Passive passiveDB in GetDatabase.GetPassives())
                {
                    if (passiveDB.passiveId == passiveID)
                    {
                        passive = passiveDB;
                    }
                }
                if (passive != null)
                {
                    string message = "Got Skill: " + member.memberName + "'s " + passive.passiveName;
                    if (sender != null && sender != "") message += " from " + sender;
                    RandomizerBehavior.PushMessage(message);
                    LearnSkillInternal(memberID, passiveID, true);
                    SoundManager.PlaySound("Rare Weapon Found");
                }
            }
        }
    }

    public static void AddSkill(int memberID, int skillOrder, string sender = "")
    {
        foreach (Skill dbSkill in GetDatabase.GetSkills())
        {
            if (dbSkill.skillUser == memberID && dbSkill.learnOrder == skillOrder)
            {
                string memberName = "????";
                foreach (var member in GetDatabase.GetPartyMember())
                {
                    if (member.memberID == memberID)
                    {
                        memberName = member.memberName;
                    }
                }
                string message = "Got Skill: " + memberName + "'s " + dbSkill.skillName;
                if (sender != null && sender != "") message += " from " + sender;
                RandomizerBehavior.PushMessage(message);
                LearnSkillInternal(memberID, dbSkill.skillID, false);
                SoundManager.PlaySound("Rare Weapon Found");
                break;
            }
        }
    }


}