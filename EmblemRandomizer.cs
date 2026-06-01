using System;
using System.Collections.Generic;

namespace CERandomizer
{
    public static class EmblemRandomizer
    {
        public static void RandomizeEmblems()
        {
            Console.WriteLine("Randomizer - Randomizing emblems...");

            List<Skill> availableSkills = new List<Skill>();
            foreach (Skill skill in GetDatabase.GetSkills())
            {
                if (skill.skillName != "??" && skill.skillName != "XXX")
                {
                    availableSkills.Add(skill);
                }
            }
            ShuffleList(availableSkills);

            List<int> availablePassives = new List<int>();
            for (int i = 9; i < 199; i++)
            {
                if (i != 42 && i != 43 && i != 100 && i != 108 && i != 112 && i != 113 && i != 157 && i != 165 && i != 181 && i != 185)
                {
                    availablePassives.Add(i);
                }
            }
            ShuffleList(availablePassives);

            foreach (ClassEmblem classEmblem in GetDatabase.GetClassEmblems())
            {
                if (RandomizerOptions.RandomizeEmblemSkills > 0 && availableSkills.Count >= 2)
                {
                    classEmblem.classAction1 = availableSkills[0].skillID;
                    availableSkills.RemoveAt(0);
                    classEmblem.classAction2 = availableSkills[0].skillID;
                    availableSkills.RemoveAt(0);
                }

                if (RandomizerOptions.RandomizeEmblemPassives > 0 && availablePassives.Count >= 2)
                {
                    classEmblem.classPassive1 = availablePassives[0];
                    availablePassives.RemoveAt(0);
                    classEmblem.classPassive2 = availablePassives[0];
                    availablePassives.RemoveAt(0);
                }

                if (RandomizerOptions.RandomizeEmblemStats > 0)
                {
                    RandomizeEmblemStats(classEmblem);
                }
            }
        }

        private static void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int swapIndex = RandomGen.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[swapIndex];
                list[swapIndex] = temp;
            }
        }

        private static void RandomizeEmblemStats(ClassEmblem classEmblem)
        {
            classEmblem.classHP = 0;
            classEmblem.classTP = 0;
            classEmblem.classAtk = 0;
            classEmblem.classMag = 0;
            classEmblem.classDef = 0;
            classEmblem.classMnd = 0;
            classEmblem.classAgi = 0;
            classEmblem.classCrit = 0;

            int remainingStatBudget = RandomGen.Range(4, 8) * 10;
            while (remainingStatBudget > 0)
            {
                int value = RandomGen.Range(1, 4) * 10;
                remainingStatBudget -= value;

                switch (RandomGen.Range(0, 8))
                {
                    case 0:
                        classEmblem.classHP += value;
                        break;
                    case 1:
                        classEmblem.classTP += value;
                        break;
                    case 2:
                        classEmblem.classAtk += value;
                        break;
                    case 3:
                        classEmblem.classMag += value;
                        break;
                    case 4:
                        classEmblem.classDef += value;
                        break;
                    case 5:
                        classEmblem.classMnd += value;
                        break;
                    case 6:
                        classEmblem.classAgi += value;
                        break;
                    case 7:
                        classEmblem.classCrit += value;
                        break;
                }
            }
        }
    }
}
