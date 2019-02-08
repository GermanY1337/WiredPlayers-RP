using RAGE;
using Newtonsoft.Json;
using System.Collections.Generic;
using WiredPlayers_Client.globals;
using System.Linq;
using System;

namespace WiredPlayers_Client.character
{
    class Inventory : Events.Script
    {
        private static int targetType;

        public Inventory()
        {
            Events.Add("showPlayerInventory", ShowPlayerInventoryEvent);
            Events.Add("getInventoryOptions", GetInventoryOptionsEvent);
            Events.Add("executeAction", ExecuteActionEvent); 
        }

        private void ShowPlayerInventoryEvent(object[] args)
        {
            // Store all the inventory data
            targetType = Convert.ToInt32(args[1]);

            // Show player's inventory
            Browser.CreateBrowserEvent(new object[] { "package://statics/html/inventory.html", "populateInventory", args[0].ToString(), "general.inventory" });
        }

        private void GetInventoryOptionsEvent(object[] args)
        {
            // Get the variables from the arguments
            int itemType = Convert.ToInt32(args[0]);
            string itemHash = args[1].ToString();

            List<string> optionsList = new List<string>();
            bool dropable = false;

            switch (targetType)
            {
                case 0:
                    // Player's inventory
                    if (itemType == 0)
                    {
                        // Consumable item
                        optionsList.Add("general.consume");
                    }
                    else if (itemType == 2)
                    {
                        // Container item
                        optionsList.Add("general.open");
                    }

                    if (itemHash.All(char.IsDigit))
                    {
                        // Equipable
                        optionsList.Add("general.equip");
                    }

                    dropable = true;
                    break;
                case 1:
                    // Player frisk
                    optionsList.Add("general.confiscate");
                    break;
                case 2:
                    // Vehicle trunk
                    optionsList.Add("general.withdraw");
                    break;
                case 3:
                    // Inventory store into the trunk
                    optionsList.Add("general.store");
                    break;
            }

            // Show the options into the inventory
            Browser.ExecuteFunctionEvent(new object[] { "showInventoryOptions", JsonConvert.SerializeObject(optionsList), dropable });
        }

        private void ExecuteActionEvent(object[] args)
        {
            // Get the variables from the arguments
            int item = Convert.ToInt32(args[0]);
            string option = args[1].ToString();

            // Execute the selected action
            Events.CallRemote("processMenuAction", item, option);
        }
    }
}
