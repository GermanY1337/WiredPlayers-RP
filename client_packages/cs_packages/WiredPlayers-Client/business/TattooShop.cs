using RAGE;
using RAGE.Elements;
using System.Collections.Generic;
using WiredPlayers_Client.globals;
using WiredPlayers_Client.model;
using Newtonsoft.Json;
using System.Linq;
using System;

namespace WiredPlayers_Client.business
{
    class TattooShop : Events.Script
    {
        private List<Tattoo> playerTattoos = null;
        private List<Tattoo> tattooList = null;
        private List<Tattoo> zoneTattoos = null;
        private int playerSex;

        public TattooShop()
        {
            Events.Add("showTattooMenu", ShowTattooMenuEvent);
            Events.Add("getZoneTattoos", GetZoneTattoosEvent);
            Events.Add("addPlayerTattoo", AddPlayerTattooEvent);
            Events.Add("clearTattoos", ClearTattoosEvent);
            Events.Add("purchaseTattoo", PurchaseTattooEvent);
            Events.Add("exitTattooShop", ExitTattooShopEvent); 
        }

        private void ShowTattooMenuEvent(object[] args)
        {
            // Get the variables from the arguments
            string playerTattoosJson = args[1].ToString();
            string tattoosJson = args[2].ToString();
            string business = args[3].ToString();
            float price = (float)Convert.ToDouble(args[4]);
            playerSex = Convert.ToInt32(args[0]);


            // Initialize the player tattoos
            playerTattoos = JsonConvert.DeserializeObject<List<Tattoo>>(playerTattoosJson);
            tattooList = JsonConvert.DeserializeObject<List<Tattoo>>(tattoosJson);
            string tattooZoneJson = JsonConvert.SerializeObject(Constants.TATTOO_ZONES);

            // Show tattoos menu
            Browser.CreateBrowserEvent(new object[] { "package://statics/html/sideMenu.html", "populateTattooMenu", tattooZoneJson, business, price });
        }

        private void GetZoneTattoosEvent(object[] args)
        {
            // Get the variables from the arguments
            int zone = Convert.ToInt32(args[0]);

            // Get the tattoos from the zone
            zoneTattoos = tattooList.Where(tattoo => tattoo.slot == zone).ToList();

            // Show the tattoos for the selected zone
            Browser.ExecuteFunctionEvent(new object[] { "populateZoneTattoos", Globals.EscapeJsonCharacters(JsonConvert.SerializeObject(zoneTattoos)) });
        }

        private void AddPlayerTattooEvent(object[] args)
        {
            // Get the variables from the arguments
            int index = Convert.ToInt32(args[0]);

            // Load the player's tattoos
            ClearTattoosEvent(null);

            // Add the tattoo to the player
            uint tattooHash = playerSex == Constants.SEX_MALE ? RAGE.Game.Misc.GetHashKey(zoneTattoos[index].maleHash) : RAGE.Game.Misc.GetHashKey(zoneTattoos[index].femaleHash);
            Player.LocalPlayer.SetDecoration(RAGE.Game.Misc.GetHashKey(zoneTattoos[index].library), tattooHash);
        }

        private void ClearTattoosEvent(object[] args)
        {
            // Clear all the tattoos
            Player.LocalPlayer.ClearDecorations();

            foreach(Tattoo tattoo in playerTattoos)
            {
                // Add the tattoo to the player
                Player.LocalPlayer.SetDecoration(RAGE.Game.Misc.GetHashKey(tattoo.library), RAGE.Game.Misc.GetHashKey(tattoo.hash));
            }
        }

        private void PurchaseTattooEvent(object[] args)
        {
            // Get the variables from the arguments
            int slot = Convert.ToInt32(args[0]);
            int index = Convert.ToInt32(args[1]);

            // Add the new tattoo to the list
            Tattoo tattoo = new Tattoo();
            {
                tattoo.slot = slot;
                tattoo.library = zoneTattoos[index].library;
                tattoo.hash = playerSex == Constants.SEX_MALE ? zoneTattoos[index].maleHash : zoneTattoos[index].femaleHash;
            }
            playerTattoos.Add(tattoo);

            // Purchase the tattoo
            Events.CallRemote("purchaseTattoo", slot, index);
        }

        private void ExitTattooShopEvent(object[] args)
        {
            // Close the purchase menu
            Browser.DestroyBrowserEvent(null);

            // Dress the character
            Events.CallRemote("loadCharacterClothes");
        }
    }
}
