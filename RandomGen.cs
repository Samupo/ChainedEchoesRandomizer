using System;

namespace CERandomizer
{
	public static class RandomGen
	{
        private static Random random;

		public static int Seed
		{
			set
			{
				random = new Random(value);
			}
		}

		public static int Range(int min, int max)
		{
			return random.Next(min, max);
		}

		public static float Range(float min, float max)
		{
			return (float)random.NextDouble() * (max - min) + min;
		}

		public static float Next()
		{
			return (float)random.NextDouble();
		}
	}
}