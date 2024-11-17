using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using System.Collections;
using System.IO;
using System;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using Rewired;

namespace CERandomizer
{
    [BepInPlugin("com.samupo.cerandomizer", "CE Randomizer", "1.0.0")]
    public class CERandomizer : BasePlugin
    {
        public static CERandomizer Instance { get; private set; }

        public override void Load()
        {
            // Set instance for global access
            Instance = this;

            // Initialize the GameObject and component only once after the scene loads
            AddComponent<ActionQuery>();
            AddComponent<RandomizerBehavior>();
            Console.WriteLine("Component added");

            if (File.Exists("RandomizerSeed.txt"))
            {
                RandomGen.Seed = int.Parse(File.ReadAllText("RandomizerSeed.txt"));
            }
            else
            {
                int seed = (int)DateTime.Now.Ticks;
                RandomGen.Seed = seed;
                File.WriteAllText("RandomizerSeed.txt", seed.ToString());
            }

            HarmonyPatches.ApplyPatches();
        }
    }

    public class RandomizerBehavior : MonoBehaviour
    {
        private struct TimedMessage
        {
            public string message;
            public float displayUntil;

            public TimedMessage(string message, float displayTime)
            {
                this.message = message;
                this.displayUntil = Time.time + displayTime;
            }
        }

        private static Queue<TimedMessage> messageQueue = new Queue<TimedMessage>();
        private const float displayDuration = 10f; // Duration to display each message

        private GUIStyle messageStyle;
        private Vector2 messageBoxSize = new Vector2(400, 30); // Width and height of each message box
        private Texture2D backgroundTexture;

        bool randomInitialization = false;

        private void Awake()
        {
            // Initialize the message style
            messageStyle = new GUIStyle
            {
                fontSize = 16,
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            // Create the background texture (semi-transparent black)
            backgroundTexture = MakeTexture(1, 1, new Color(0, 0, 0, 0.5f));

            RandomizerDatabase.LoadLocations();
            RandomizerDatabase.LoadItems();
        }

        void Initialization()
        {
            if (!randomInitialization || !RandomizerDatabase.Randomized)
            {
                if (!RandomizerDatabase.Randomized)
                {
                    RandomizerDatabase.Randomize();
                }
                // Wait for DB randomization to have happened to keep seed useful
                if (RandomizerDatabase.Randomized && GameObject.FindWithTag("SaveData") != null && !randomInitialization)
                {
                    randomInitialization = true;
                    // Randomize database as needed
                    if (RandomGen.Seed != 0)
                    {
                        if (RandomizerOptions.AddTier1Weapons > 0)
                        {
                            CoreRandomizer.AddTier1Weapons();
                        }
                        if (RandomizerOptions.RandomizeCharacterEquipment > 0)
                        {
                            CoreRandomizer.RandomizeCharacterWeaponTypes();
                        }
                        if (RandomizerOptions.RandomizeCharacterSkills > 0)
                        {
                            CoreRandomizer.RandomizeCharacterSkills();
                        }
                        if (RandomizerOptions.RandomizeCharacterPassives > 0)
                        {
                            CoreRandomizer.RandomizeCharacterPassives();
                        }
                        if (RandomizerOptions.RandomizeCharacterStatBoosts > 0)
                        {
                            CoreRandomizer.RandomizeCharacterStatBoosts();
                        }
                    }
                }

                if (randomInitialization && RandomizerDatabase.Randomized)
                {
                    PushMessage("Randomizer Initialized");
                }
            }
        }

        void Update()
        {
            Initialization();

            if (messageQueue.Count > 0 && messageQueue.Peek().displayUntil <= Time.time)
            {
                messageQueue.Dequeue();
            }

        }

        public static void PushMessage(string message)
        {
            messageQueue.Enqueue(new TimedMessage(message, displayDuration));
        }

        void OnGUI()
        {
            int index = 0;

            // Display each active message with a background in the top-right corner
            foreach (var timedMessage in messageQueue)
            {
                // Calculate the position for each message box
                Rect messageRect = new Rect(
                    Screen.width - messageBoxSize.x - 20, // 20px padding from the right
                    20 + index * (messageBoxSize.y + 5),  // 20px padding from the top, with spacing
                    messageBoxSize.x,
                    messageBoxSize.y
                );

                // Draw the background texture
                //GUIContent content = new GUIContent(timedMessage.message, MakeTexture(400, 30, Color.black), "");
                GUI.Label(messageRect, timedMessage.message, messageStyle);

                // Draw the text on top of the background
                //GUI.Label(messageRect, timedMessage.message, messageStyle);

                index++;
            }
        }

        // Helper function to create a texture with a specific color
        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
    }

}
