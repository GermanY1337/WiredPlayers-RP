using RAGE;
using RAGE.Elements;
using WiredPlayers_Client.globals;
using WiredPlayers_Client.model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace WiredPlayers_Client.house
{
    class House : Events.Script
    {
        private List<ClothesModel> clothes = null;

        public House()
        {
            Events.Add("showPlayerWardrobe", ShowPlayerWardrobeEvent);
            Events.Add("getPlayerPurchasedClothes", GetPlayerPurchasedClothesEvent);
            Events.Add("showPlayerClothes", ShowPlayerClothesEvent);
            Events.Add("previewPlayerClothes", PreviewPlayerClothesEvent);
            Events.Add("changePlayerClothes", ChangePlayerClothesEvent);
        }

        private void ShowPlayerWardrobeEvent(object[] args)
        {
            // Show wardrobe's menu
            Browser.CreateBrowserEvent(new object[] { "package://WiredPlayers/statics/html/sideMenu.html", "populateWardrobeMenu", JsonConvert.SerializeObject(Constants.CLOTHES_TYPES) });
        }

        private void GetPlayerPurchasedClothesEvent(object[] args)
        {
            // Get the variables from the array
            int index = Convert.ToInt32(args[0]);

            // Get the player's clothes
            Events.CallRemote("getPlayerPurchasedClothes", Constants.CLOTHES_TYPES[index].type, Constants.CLOTHES_TYPES[index].slot);
        }

        private void ShowPlayerClothesEvent(object[] args)
        {
            // Get the variables from the array
            List<string> clothesNames = JsonConvert.DeserializeObject<List<string>>(args[1].ToString());
            clothes = JsonConvert.DeserializeObject<List<ClothesModel>>(args[0].ToString());

            for(int i = 0; i < clothesNames.Count; i++)
            {
                // Add the name for each clothes
                clothes[i].description = clothesNames[i];
            }

            // Show clothes of the selected type
            Browser.ExecuteFunctionEvent(new object[] { "populateWardrobeClothes", args[0].ToString() });
        }

        private void PreviewPlayerClothesEvent(object[] args)
        {
            // Get the variables from the array
            int index = Convert.ToInt32(args[0]);

            if (clothes[index].type == 0)
            {
                // Change player's clothes
                Player.LocalPlayer.SetComponentVariation(clothes[index].slot, clothes[index].drawable, clothes[index].texture, 0);
            }
            else
            {
                // Change player's accessory
                Player.LocalPlayer.SetPropIndex(clothes[index].slot, clothes[index].drawable, clothes[index].texture, true);
            }
        }

        private void ChangePlayerClothesEvent(object[] args)
        {
            // Get the variables from the array
            int index = Convert.ToInt32(args[0]);

            // Equip the clothes
            Events.CallRemote("wardrobeClothesItemSelected", clothes[index].clothesId);
        }
    }
}
