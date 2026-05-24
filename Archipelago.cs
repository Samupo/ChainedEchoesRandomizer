using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CERandomizer
{
    public static class Archipelago
    {
        const string GAME_NAME = "Chained Echoes";
        public const string LOCATION_DATA_PREFIX = "ARCHIPELAGO_LOC_";
        public const string ITEM_DATA_PREFIX = "ARCHIPELAGO_ITEM_";

        public static bool Connected { get; private set; }
        public static bool ConnectionAttempted { get; private set; }
        public static bool ShouldRestartForNewSlot { get; private set; }
        public static string Status { get; private set; } = "Not connected.";

        static ArchipelagoSession session;

        public static void SetStatus(string status)
        {
            Status = status;
        }

        public static bool Connect(string server, int port, string username, string password, string optionsPath)
        {
            if (Connected)
            {
                Status = "Connected.";
                return true;
            }

            ConnectionAttempted = true;
            ShouldRestartForNewSlot = false;
            RandomizerOptions.SetConnectionSettings(server, port, username, password);
            Status = "Connecting to " + RandomizerOptions.ConnectionAddress + "...";

            try
            {
                session = ArchipelagoSessionFactory.CreateSession(RandomizerOptions.ConnectionAddress);
                LoginResult result = session.TryConnectAndLogin(GAME_NAME, RandomizerOptions.ArchipelagoUsername, ItemsHandlingFlags.AllItems,
                    password: RandomizerOptions.ArchipelagoPassword == "" ? null : RandomizerOptions.ArchipelagoPassword);

                Connected = result.Successful;
                if (!Connected)
                {
                    Status = "Archipelago login failed.";
                    Console.WriteLine(Status);
                    return false;
                }

                LoginSuccessful loginSuccessful = result as LoginSuccessful;
                if (loginSuccessful == null)
                {
                    Connected = false;
                    Status = "Archipelago login did not return slot data.";
                    Console.WriteLine(Status);
                    return false;
                }

                List<string> missingSlotData = RandomizerOptions.GetMissingSlotDataOptionNames(loginSuccessful.SlotData);
                if (missingSlotData.Count > 0)
                {
                    Connected = false;
                    Status = "AP slot data is missing Randomizer options: " + string.Join(", ", missingSlotData.ToArray());
                    Console.WriteLine(Status);
                    return false;
                }

                bool connectionChanged = RandomizerOptions.CurrentConnectionDiffersFromLoaded();
                RandomizerOptions.LoadArchipelagoSlotData(loginSuccessful.SlotData);
                RandomGen.Seed = RandomizerOptions.RandomizerSeed;
                RandomizerOptions.Save(optionsPath);
                ShouldRestartForNewSlot = connectionChanged;

                session.Items.ItemReceived += ReceiveItem;

                // Scout all locations
                session.Locations.ScoutLocationsAsync(session.Locations.AllLocations.ToArray());

                Status = ShouldRestartForNewSlot
                    ? "Connected. New server or slot detected; the game will close soon."
                    : "Connected. Options and seed loaded from Archipelago.";
                Console.WriteLine(Status);
                return true;
            }
            catch (Exception ex)
            {
                Connected = false;
                Status = "Archipelago connection failed: " + ex.Message;
                Console.WriteLine(Status);
                return false;
            }
        }

        public static void GetAllItems()
        {
            if (!Connected)
            {
                return;
            }

            foreach (ItemInfo item in session.Items.AllItemsReceived)
            {
                GetItem(item.ItemName, item.ItemId, item.Player.Name);
            }
        }

        private static void GetItem(string itemName, long itemID, string playerName)
        {
            itemName = itemName.Replace("´", "'");
            ActionQueue.AddAction(() =>
            {
                Console.WriteLine("Received archipelago item: " + itemName + " [" + itemID + "]");
                string saveDataString = ITEM_DATA_PREFIX + itemID;
                if (GetData.GetChests().Contains(saveDataString))
                {
                    Console.WriteLine("Item already in inventory");
                }
                else
                {
                    GetData.GetChests().Add(saveDataString);
                    RandomizerDatabase.Item databaseItem = RandomizerDatabase.items.Find(i => i.UserFriendlyName == itemName);
                    if (databaseItem == null)
                    {
                        Console.WriteLine("Item not found: " + itemName);
                    }
                    else
                    {
                        RandomizerUtils.UnlockItem(databaseItem, playerName);
                    }
                }
            }, 0f);
        }

        private static void ReceiveItem(ReceivedItemsHelper helper)
        {
            while (helper.PeekItem() != null)
            {
                ItemInfo item = helper.DequeueItem();
                GetItem(item.ItemName, item.ItemId, item.Player.Name);
            }
        }

        public static void MarkLocationAsChecked(long locationID)
        {
            if (Connected)
            {
                session.Locations.CompleteLocationChecks(locationID);
                Task<Dictionary<long, ScoutedItemInfo>> task = session.Locations.ScoutLocationsAsync(locationID);
                task.ContinueWith(t =>
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        foreach (ScoutedItemInfo item in task.Result.Values)
                        {
                            RandomizerBehavior.PushMessage("Sent " + item.ItemName + " to " + item.Player.Name);
                        }
                    }
                });
            }
        }

        public static void MarkGameAsCompleted()
        {
            if (Connected)
            {
                StatusUpdatePacket statusUpdatePacket = new StatusUpdatePacket();
                statusUpdatePacket.Status = ArchipelagoClientState.ClientGoal;
                session.Socket.SendPacket(statusUpdatePacket);
            }
        }
    }
}
