using System.IO;
using System.Reflection;
using System.Text;
using CERandomizer;

namespace CERandomizer
{
	public static class RandomizerOptions
	{
		// Archipelago config
		public static int UseArchipelago { get; private set; } = 1;
		public static string ArchipelagoServer { get; private set; } = "localhost:38281";
		public static string ArchipelagoUsername { get; private set; } = "Player";
		public static string ArchipelagoPassword { get; private set; } = "";

		public static int RandomizerSeed { get; set; } = (int)System.DateTime.Now.Ticks;

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


		public static void Load(string path)
		{
			string[] array = File.ReadAllLines(path);
			PropertyInfo[] properties = typeof(RandomizerOptions).GetProperties(BindingFlags.Static | BindingFlags.Public);
			PropertyInfo[] array2 = properties;
			foreach (PropertyInfo propertyInfo in array2)
			{
				string[] array3 = array;
				foreach (string text in array3)
				{
					if (text.StartsWith(propertyInfo.Name + "="))
					{
						if (propertyInfo.PropertyType == typeof(int))
						{
							propertyInfo.SetValue(null, int.Parse(text.Split('=')[1]));
						}
						else if (propertyInfo.PropertyType == typeof(float))
						{
							propertyInfo.SetValue(null, float.Parse(text.Split('=')[1]));
						}
						else if (propertyInfo.PropertyType == typeof(string))
                        {
                            propertyInfo.SetValue(null, text.Split('=')[1]);
                        }
					}
				}
			}
		}

		public static void Save(string path)
		{
			PropertyInfo[] properties = typeof(RandomizerOptions).GetProperties(BindingFlags.Static | BindingFlags.Public);
			StringBuilder stringBuilder = new StringBuilder();
			PropertyInfo[] array = properties;
			foreach (PropertyInfo propertyInfo in array)
			{
				string name = propertyInfo.Name;
				object value = propertyInfo.GetValue(null);
				stringBuilder.AppendLine(name + "=" + ((value != null) ? value.ToString() : null));
			}
			File.WriteAllText(path, stringBuilder.ToString());
		}
	}
}