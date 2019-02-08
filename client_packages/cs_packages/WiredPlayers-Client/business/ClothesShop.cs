using RAGE;
using RAGE.Elements;
using System.Collections.Generic;
using WiredPlayers_Client.globals;
using WiredPlayers_Client.model;
using Newtonsoft.Json;
using System;

namespace WiredPlayers_Client.business
{
    class ClothesShop : Events.Script
    {
        private int selectedIndex = -1;
        private List<ClothesModel> clothesTypes;

        public ClothesShop()
        {
            Events.Add("showClothesBusinessPurchaseMenu", ShowClothesBusinessPurchaseMenuEvent);
            Events.Add("getClothesByType", GetClothesByTypeEvent);
            Events.Add("showTypeClothes", ShowTypeClothesEvent);
            Events.Add("replacePlayerClothes", ReplacePlayerClothesEvent);
            Events.Add("purchaseClothes", PurchaseClothesEvent);
            Events.Add("clearClothes", ClearClothesEvent); 
        }

        private void ShowClothesBusinessPurchaseMenuEvent(object[] args)
        {
            // Initialize the clothes types
            clothesTypes = new List<ClothesModel>();

            // Get the variables from the arguments
            string business = args[0].ToString();
            float price = (float)Convert.ToDouble(args[1]);

            // Show clothes menu
            Browser.CreateBrowserEvent(new object[] { "package://statics/html/sideMenu.html", "populateClothesShopMenu", JsonConvert.SerializeObject(Constants.CLOTHES_TYPES), business, price });
        }

        private void GetClothesByTypeEvent(object[] args)
        {
            // Save selected index
            selectedIndex = Convert.ToInt32(args[0]);

            // Get clothes list
            Events.CallRemote("getClothesByType", Constants.CLOTHES_TYPES[selectedIndex].type, Constants.CLOTHES_TYPES[selectedIndex].slot);
        }

        private void ShowTypeClothesEvent(object[] args)
        {
            // Get the variables from the arguments
            string clothesJson = args[0].ToString();
            int type = Constants.CLOTHES_TYPES[selectedIndex].type;
            int slot = Constants.CLOTHES_TYPES[selectedIndex].slot;

            // Get clothes list for the type
            clothesTypes = JsonConvert.DeserializeObject<List<ClothesModel>>(clothesJson);

            foreach (ClothesModel clothes in clothesTypes)
            {
                // Get the number of available textures
                clothes.textures = type == 0 ? Player.LocalPlayer.GetNumberOfTextureVariations(slot, clothes.clothesId) : Player.LocalPlayer.GetNumberOfPropTextureVariations(slot, clothes.clothesId);
            }

            // Show all the clothes from the selected type
            Browser.ExecuteFunctionEvent(new object[] { "populateTypeClothes", Globals.EscapeJsonCharacters(JsonConvert.SerializeObject(clothesTypes)) });
        }

        private void ReplacePlayerClothesEvent(object[] args)
        {
            // Get the variables from the arguments
            int index = Convert.ToInt32(args[0]);
            int texture = Convert.ToInt32(args[1]);

            if (clothesTypes[index].type == 0)
            {
                // Change player's clothes
                Player.LocalPlayer.SetComponentVariation(clothesTypes[index].bodyPart, clothesTypes[index].clothesId, texture, 0);
            }
            else
            {
                // Change player's accessory
                Player.LocalPlayer.SetPropIndex(clothesTypes[index].bodyPart, clothesTypes[index].clothesId, texture, true);
            }
        }

        private void PurchaseClothesEvent(object[] args)
        {
            // Get the variables from the arguments
            int index = Convert.ToInt32(args[0]);

            // Create the clothes model
            ClothesModel clothesModel = new ClothesModel();
            {
                clothesModel.type = clothesTypes[index].type;
                clothesModel.slot = clothesTypes[index].bodyPart;
                clothesModel.drawable = clothesTypes[index].clothesId;
                clothesModel.texture = Convert.ToInt32(args[1]);
            }

            // Purchase the clothes
            Events.CallRemote("clothesItemSelected", JsonConvert.SerializeObject(clothesModel));
        }

        private void ClearClothesEvent(object[] args)
        {
            // Get the variables from the arguments
            int index = Convert.ToInt32(args[0]);

            // Get the type and slot
            int type = clothesTypes[index].type;
            int slot = clothesTypes[index].bodyPart;

            // Clear the not purchased clothes
            Events.CallRemote("dressEquipedClothes", type, slot);
        }
    }
}
