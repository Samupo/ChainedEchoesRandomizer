using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CERandomizer
{
    [BepInPlugin("com.Samupo.CERandomizer", "CE Randomizer", "1.0.0")]
    public class CERandomizer : BasePlugin
    {
        public static CERandomizer Instance { get; private set; }
        public const string OPTIONS_FILE = "RandomizerOptions.txt";

        public override void Load()
        {
            Instance = this;

            RandomizerOptions.LoadConnectionSettings(OPTIONS_FILE);
            RandomGen.Seed = 0;

            HarmonyPatches.ApplyPatches();

            AddComponent<ActionQueue>();
            AddComponent<RandomizerBehavior>();
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
        private const float displayDuration = 10f;

        private GUIStyle messageStyle;
        private GUIStyle connectionTitleStyle;
        private GUIStyle connectionLabelStyle;
        private GUIStyle connectionButtonStyle;
        private GUIStyle restartPanelStyle;
        private bool guiStylesReady = false;
        private Vector2 messageBoxSize = new Vector2(400, 30);
        private Texture2D backgroundTexture;
        private Texture2D panelTexture;
        private Texture2D restartTexture;

        private string serverInput;
        private string portInput;
        private string usernameInput;
        private string passwordInput;
        private bool randomInitialization = false;
        private bool connectionOverlayOpen = true;
        private bool restartCountdownStarted = false;
        private float restartAt = -1f;

        private void Awake()
        {
            backgroundTexture = MakeTexture(1, 1, new Color(0, 0, 0, 0.5f));
            panelTexture = MakeTexture(1, 1, new Color(0, 0, 0, 0.85f));
            restartTexture = MakeTexture(1, 1, new Color(0.15f, 0.02f, 0.02f, 0.95f));

            serverInput = RandomizerOptions.ArchipelagoServer;
            portInput = RandomizerOptions.ArchipelagoPort.ToString();
            usernameInput = RandomizerOptions.ArchipelagoUsername;
            passwordInput = RandomizerOptions.ArchipelagoPassword;

            RandomizerDatabase.LoadLocations();
            RandomizerDatabase.LoadItems();
        }


        private void TryConnectFromOverlay()
        {
            int port;
            if (!int.TryParse(portInput, out port) || port <= 0 || port > 65535)
            {
                ArchipelagoStatusOverride("Port must be between 1 and 65535.");
                return;
            }

            bool connected = Archipelago.Connect(serverInput, port, usernameInput, passwordInput, CERandomizer.OPTIONS_FILE);
            if (connected && Archipelago.ShouldRestartForNewSlot)
            {
                StartRestartCountdown();
            }
        }

        private void ArchipelagoStatusOverride(string message)
        {
            Archipelago.SetStatus(message);
        }

        private void StartRestartCountdown()
        {
            if (restartCountdownStarted)
            {
                return;
            }

            restartCountdownStarted = true;
            restartAt = Time.realtimeSinceStartup + 10f;
        }

        void Initialization()
        {
            if (restartCountdownStarted || !Archipelago.Connected)
            {
                return;
            }

            if (!randomInitialization || !RandomizerDatabase.Randomized)
            {
                if (GameObject.FindWithTag("SaveData") == null)
                {
                    return;
                }

                if (!RandomizerDatabase.Randomized)
                {
                    RandomizerDatabase.Randomize();
                }

                if (RandomizerDatabase.Randomized && !randomInitialization)
                {
                    randomInitialization = true;

                    CoreRandomizer.UnlearnAllSkills();
                    CoreRandomizer.RemoveEquipmentDealRewards();
                    RandomizerUtils.AddItem(405, 20);

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
                    if (RandomizerOptions.RandomizeCharacterInitialStats > 0)
                    {
                        CoreRandomizer.AverageCharacterInitialStats();
                    }
                    if (RandomizerOptions.RandomizeCharacterStatProgression > 0)
                    {
                        CoreRandomizer.RandomizeCharacterStatProgression();
                    }
                    if (RandomizerOptions.RandomizeCharacterStatBoosts > 0)
                    {
                        CoreRandomizer.RandomizeCharacterStatBoosts();
                    }
                    if (RandomizerOptions.RandomizeMechSkills > 0)
                    {
                        MechRandomizer.RandomizeMechSkills();
                    }
                    if (RandomizerOptions.RandomizeMechStatBoosts > 0)
                    {
                        MechRandomizer.RandomizeMechStatBoosts();
                    }
                    if (RandomizerOptions.RandomizeEmblemStats > 0 || RandomizerOptions.RandomizeEmblemSkills > 0 || RandomizerOptions.RandomizeEmblemPassives > 0)
                    {
                        EmblemRandomizer.RandomizeEmblems();
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
            if (restartCountdownStarted && Time.realtimeSinceStartup >= restartAt)
            {
                Application.Quit();
                return;
            }

            if (Input.GetKeyDown(KeyCode.F7))
            {
                connectionOverlayOpen = !connectionOverlayOpen;
            }

            Initialization();

            if (messageQueue.Count > 0 && messageQueue.Peek().displayUntil <= Time.time)
            {
                messageQueue.Dequeue();
            }

            if (randomInitialization && RandomizerDatabase.Randomized)
            {
                foreach (PartyMember member in RandomizerUtils.GetPlayableCharacters(false))
                {
                    member.gs = 0.0f;
                }
            }
        }

        public static void PushMessage(string message)
        {
            messageQueue.Enqueue(new TimedMessage(message, displayDuration + 0.1f * messageQueue.Count));
        }

        private void EnsureGuiStyles()
        {
            if (guiStylesReady)
            {
                return;
            }

            messageStyle = new GUIStyle
            {
                fontSize = 16,
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            connectionTitleStyle = new GUIStyle
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            connectionLabelStyle = new GUIStyle
            {
                fontSize = 14,
                normal = { textColor = Color.white }
            };

            connectionButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            restartPanelStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { background = restartTexture, textColor = Color.white }
            };

            guiStylesReady = true;
        }

        void OnGUI()
        {
            EnsureGuiStyles();

            DrawMessages();

            if (restartCountdownStarted)
            {
                DrawRestartPrompt();
                return;
            }

            if (connectionOverlayOpen)
            {
                DrawConnectionOverlay();
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
            }
        }

        private void DrawMessages()
        {
            int index = 0;
            foreach (TimedMessage timedMessage in messageQueue)
            {
                Rect messageRect = new Rect(
                    Screen.width - messageBoxSize.x - 20,
                    20 + index * (messageBoxSize.y + 5),
                    messageBoxSize.x,
                    messageBoxSize.y
                );

                GUI.DrawTexture(messageRect, backgroundTexture);
                GUI.Label(messageRect, timedMessage.message, messageStyle);
                index++;
            }
        }

        private void DrawConnectionOverlay()
        {
            Rect overlayRect = new Rect(0, 0, Screen.width, Screen.height);
            GUI.DrawTexture(overlayRect, backgroundTexture);

            float panelWidth = 460f;
            float panelHeight = 350f;
            Rect panelRect = new Rect((Screen.width - panelWidth) / 2f, (Screen.height - panelHeight) / 2f, panelWidth, panelHeight);
            GUI.DrawTexture(panelRect, panelTexture);

            float x = panelRect.x + 18f;
            float y = panelRect.y + 16f;
            float width = panelRect.width - 36f;
            float labelHeight = 20f;
            float fieldHeight = 24f;
            float spacing = 8f;

            GUI.Label(new Rect(x, y, width, 30f), "Archipelago Connection", connectionTitleStyle);
            y += 38f;

            GUI.Label(new Rect(x, y, width, labelHeight), "Server", connectionLabelStyle);
            y += labelHeight;
            serverInput = GUI.TextField(new Rect(x, y, width, fieldHeight), serverInput ?? string.Empty);
            y += fieldHeight + spacing;

            GUI.Label(new Rect(x, y, width, labelHeight), "Port", connectionLabelStyle);
            y += labelHeight;
            portInput = GUI.TextField(new Rect(x, y, width, fieldHeight), portInput ?? string.Empty);
            y += fieldHeight + spacing;

            GUI.Label(new Rect(x, y, width, labelHeight), "Username", connectionLabelStyle);
            y += labelHeight;
            usernameInput = GUI.TextField(new Rect(x, y, width, fieldHeight), usernameInput ?? string.Empty);
            y += fieldHeight + spacing;

            GUI.Label(new Rect(x, y, width, labelHeight), "Password", connectionLabelStyle);
            y += labelHeight;
            passwordInput = GUI.PasswordField(new Rect(x, y, width, fieldHeight), passwordInput ?? string.Empty, '*');
            y += fieldHeight + 12f;

            bool previousEnabled = GUI.enabled;
            GUI.enabled = !Archipelago.Connected;
            string connectButtonText = Archipelago.Connected ? "Connected" : "Connect";
            if (GUI.Button(new Rect(x, y, width, 32f), connectButtonText, connectionButtonStyle))
            {
                TryConnectFromOverlay();
            }
            GUI.enabled = previousEnabled;
            y += 42f;

            GUI.Label(new Rect(x, y, width, 42f), "Status: " + Archipelago.Status, connectionLabelStyle);
            y += 48f;

            if (GUI.Button(new Rect(x, y, width, 28f), "Close Window (F7)", connectionButtonStyle))
            {
                connectionOverlayOpen = false;
            }
        }

        private void DrawRestartPrompt()
        {
            Rect overlayRect = new Rect(0, 0, Screen.width, Screen.height);
            GUI.DrawTexture(overlayRect, backgroundTexture);

            int secondsLeft = Mathf.Max(0, Mathf.CeilToInt(restartAt - Time.realtimeSinceStartup));
            float panelWidth = 520f;
            float panelHeight = 120f;
            Rect panelRect = new Rect((Screen.width - panelWidth) / 2f, (Screen.height - panelHeight) / 2f, panelWidth, panelHeight);
            GUI.Box(panelRect, "A new Archipelago server or slot was detected.\nThe game will close in " + secondsLeft + " seconds so the slot seed is set properly.", restartPanelStyle);
        }

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
