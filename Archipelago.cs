using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CERandomizer
{
    public static class Archipelago
    {
        const string GAME_NAME = "Chained Echoes";
        public const string LOCATION_DATA_PREFIX = "ARCHIPELAGO_LOC_";
        public const string ITEM_DATA_PREFIX = "ARCHIPELAGO_ITEM_";

        public static bool Connected { get; private set; }

        static ArchipelagoSession session;

        public static void Connect()
        {
            session = ArchipelagoSessionFactory.CreateSession(RandomizerOptions.ArchipelagoServer);
            LoginResult result = session.TryConnectAndLogin(GAME_NAME, RandomizerOptions.ArchipelagoUsername, ItemsHandlingFlags.AllItems,
                password:RandomizerOptions.ArchipelagoPassword == "" ? null : RandomizerOptions.ArchipelagoPassword);
            Connected = result.Successful;
            session.Items.ItemReceived += ReceiveItem;

            // Scout all locations
            session.Locations.ScoutLocationsAsync(session.Locations.AllLocations.ToArray());
        }

        public static void GetAllItems()
        {
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
