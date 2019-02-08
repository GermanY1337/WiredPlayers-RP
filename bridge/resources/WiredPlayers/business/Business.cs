using GTANetworkAPI;
using WiredPlayers.database;
using WiredPlayers.globals;
using WiredPlayers.model;
using WiredPlayers.character;
using WiredPlayers.messages.error;
using WiredPlayers.messages.information;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace WiredPlayers.business
{
    public class Business : Script
    {
        public static List<BusinessModel> businessList;

        public void LoadDatabaseBusiness()
        {
            businessList = Database.LoadAllBusiness();
            foreach (BusinessModel businessModel in businessList)
            {
                // We create the entrance TextLabel for each business
                businessModel.businessLabel = NAPI.TextLabel.CreateTextLabel(businessModel.name, businessModel.position, 30.0f, 0.75f, 4, new Color(255, 255, 255), false, businessModel.dimension);

                // We mark the blip in the map
                foreach (BusinessBlipModel blipModel in Constants.BUSINESS_BLIP_LIST)
                {
                    if (blipModel.id == businessModel.id)
                    {
                        Blip businessBlip = NAPI.Blip.CreateBlip(businessModel.position);
                        businessBlip.Name = businessModel.name;
                        businessBlip.Sprite = (uint)blipModel.blip;
                        businessBlip.ShortRange = true;
                        break;
                    }
                }
            }
        }

        public static BusinessModel GetBusinessById(int businessId)
        {
            // Get the business given an specific identifier
            return businessList.Where(business => business.id == businessId).FirstOrDefault();
        }

        public static BusinessModel GetClosestBusiness(Client player, float distance = 2.0f)
        {
            BusinessModel business = null;
            foreach (BusinessModel businessModel in businessList)
            {
                if (player.Position.DistanceTo(businessModel.position) < distance)
                {
                    business = businessModel;
                    distance = player.Position.DistanceTo(business.position);
                }
            }
            return business;
        }

        public static List<BusinessItemModel> GetBusinessSoldItems(int business)
        {
            // Get the items sold in a business
            return Constants.BUSINESS_ITEM_LIST.Where(businessItem => businessItem.business == business).ToList();
        }

        public static BusinessItemModel GetBusinessItemFromName(string itemName)
        {
            // Get the item from its name
            return Constants.BUSINESS_ITEM_LIST.Where(businessItem => businessItem.description == itemName).FirstOrDefault();
        }

        public static BusinessItemModel GetBusinessItemFromHash(string itemHash)
        {
            // Get the item from its hash
            return Constants.BUSINESS_ITEM_LIST.Where(businessItem => businessItem.hash == itemHash).FirstOrDefault();
        }

        public static List<BusinessClothesModel> GetBusinessClothesFromSlotType(int sex, int type, int slot)
        {
            // Get the clothes for a sex from their slot and type
            return Constants.BUSINESS_CLOTHES_LIST.Where(clothes => clothes.type == type && (clothes.sex == sex || Constants.SEX_NONE == clothes.sex) && clothes.bodyPart == slot).ToList();
        }

        public static int GetClothesProductsPrice(int id, int sex, int type, int slot)
        {
            // Get the products needed for the given clothes
            BusinessClothesModel clothesModel = Constants.BUSINESS_CLOTHES_LIST.Where(c => c.type == type && (c.sex == sex || Constants.SEX_NONE == c.sex) && c.bodyPart == slot && c.clothesId == id).FirstOrDefault();

            return clothesModel == null ? 0 : clothesModel.products;
        }

        public static string GetBusinessTypeIpl(int type)
        {
            // Get the IPL given the business type
            BusinessIplModel businessIpl = Constants.BUSINESS_IPL_LIST.Where(ipl => ipl.type == type).FirstOrDefault();

            return businessIpl == null ? string.Empty : businessIpl.ipl;
        }

        public static Vector3 GetBusinessExitPoint(string ipl)
        {
            // Get the exit point from the given IPL
            return Constants.BUSINESS_IPL_LIST.Where(iplModel => iplModel.ipl == ipl).FirstOrDefault()?.position;
        }

        public static bool HasPlayerBusinessKeys(Client player, BusinessModel business)
        {
            return player.Name == business.owner;
        }

        private List<BusinessTattooModel> GetBusinessZoneTattoos(int sex, int zone)
        {
            // Get the tattoos matching a body part
            return Constants.TATTOO_LIST.Where(tattoo => tattoo.slot == zone && ((tattoo.maleHash.Length > 0 && sex == Constants.SEX_MALE) || (tattoo.femaleHash.Length > 0 && sex == Constants.SEX_FEMALE))).ToList();
        }

        [RemoteEvent("businessPurchaseMade")]
        public void BusinessPurchaseMadeEvent(Client player, string itemName, int amount)
        {
            int businessId = player.GetData(EntityData.PLAYER_BUSINESS_ENTERED);
            BusinessModel business = GetBusinessById(businessId);
            BusinessItemModel businessItem = GetBusinessItemFromName(itemName);

            if (business.type == Constants.BUSINESS_TYPE_AMMUNATION && businessItem.type == Constants.ITEM_TYPE_WEAPON && player.GetData(EntityData.PLAYER_WEAPON_LICENSE) < Globals.GetTotalSeconds())
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.weapon_license_expired);
            }
            else
            {
                int hash = 0;
                int price = (int)Math.Round(businessItem.products * business.multiplier) * amount;
                int money = player.GetSharedData(EntityData.PLAYER_MONEY);

                if(money < price)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_enough_money);
                }
                else
                {
                    string purchaseMessage = string.Format(InfoRes.business_item_purchased, price);
                    int playerId = player.GetData(EntityData.PLAYER_SQL_ID);

                    // We look for the item in the inventory
                    ItemModel itemModel = Globals.GetPlayerItemModelFromHash(playerId, businessItem.hash);
                    if (itemModel == null)
                    {
                        // We create the purchased item
                        itemModel = new ItemModel();
                        {
                            itemModel.hash = businessItem.hash;
                            itemModel.ownerIdentifier = player.GetData(EntityData.PLAYER_SQL_ID);
                            itemModel.amount = businessItem.uses * amount;
                            itemModel.position = new Vector3(0.0f, 0.0f, 0.0f);
                            itemModel.dimension = 0;
                        }

                        if (businessItem.type == Constants.ITEM_TYPE_WEAPON)
                        {
                            itemModel.ownerEntity = Constants.ITEM_ENTITY_WHEEL;
                        }
                        else
                        {
                            itemModel.ownerEntity = int.TryParse(itemModel.hash, out hash) ? Constants.ITEM_ENTITY_RIGHT_HAND : Constants.ITEM_ENTITY_PLAYER;
                        }

                        Task.Factory.StartNew(() => {
                            // Adding the item to the list and database
                            itemModel.id = Database.AddNewItem(itemModel);
                            Globals.itemList.Add(itemModel);
                        });
                    }
                    else
                    {
                        itemModel.amount += businessItem.uses * amount;

                        if (int.TryParse(itemModel.hash, out hash) == true)
                        {
                            itemModel.ownerEntity = Constants.ITEM_ENTITY_RIGHT_HAND;
                        }

                        Task.Factory.StartNew(() => {
                            // Update the item's amount
                            Database.UpdateItem(itemModel);
                        });
                    }

                    // If the item has a valid hash, we give it in hand
                    if (itemModel.ownerEntity == Constants.ITEM_ENTITY_RIGHT_HAND)
                    {
                        itemModel.objectHandle = NAPI.Object.CreateObject(uint.Parse(itemModel.hash), itemModel.position, new Vector3(0.0f, 0.0f, 0.0f), (byte)player.Dimension);
                        itemModel.objectHandle.AttachTo(player, "PH_R_Hand", businessItem.position, businessItem.rotation);
                    }
                    else if (businessItem.type == Constants.ITEM_TYPE_WEAPON)
                    {
                        // We give the weapon to the player
                        player.GiveWeapon(NAPI.Util.WeaponNameToModel(itemModel.hash), itemModel.amount);

                        // Checking if it's been bought in the Ammu-Nation
                        if (business.type == Constants.BUSINESS_TYPE_AMMUNATION)
                        {
                            Task.Factory.StartNew(() => {
                                // Add a registered weapon
                                Database.AddLicensedWeapon(itemModel.id, player.Name);
                            });
                        }
                    }

                    // We set the item into the hand variable
                    player.SetData(EntityData.PLAYER_RIGHT_HAND, itemModel.id);

                    // If it's a phone, we create a new number
                    if (itemModel.hash == Constants.ITEM_HASH_TELEPHONE)
                    {
                        if (player.GetData(EntityData.PLAYER_PHONE) == 0)
                        {
                            Random random = new Random();
                            int phone = random.Next(100000, 999999);
                            player.SetData(EntityData.PLAYER_PHONE, phone);

                            // Sending the message with the new number to the player
                            string message = string.Format(InfoRes.player_phone, phone);
                            player.SendChatMessage(Constants.COLOR_INFO + message);
                        }
                    }

                    // We substract the product and add funds to the business
                    if (business.owner != string.Empty)
                    {
                        business.funds += price;
                        business.products -= businessItem.products;

                        Task.Factory.StartNew(() => {
                            // Update the business
                            Database.UpdateBusiness(business);
                        });
                    }

                    player.SetSharedData(EntityData.PLAYER_MONEY, money - price);
                    player.SendChatMessage(Constants.COLOR_INFO + purchaseMessage);
                }
            }
        }

        [RemoteEvent("getClothesByType")]
        public void GetClothesByTypeEvent(Client player, int type, int slot)
        {
            int sex = player.GetData(EntityData.PLAYER_SEX);
            List<BusinessClothesModel> clothesList = GetBusinessClothesFromSlotType(sex, type, slot);
            if (clothesList.Count > 0)
            {
                BusinessModel business = GetBusinessById(player.GetData(EntityData.PLAYER_BUSINESS_ENTERED));
                player.TriggerEvent("showTypeClothes", NAPI.Util.ToJson(clothesList));
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.business_clothes_not_available);
            }
        }

        [RemoteEvent("dressEquipedClothes")]
        public void DressEquipedClothesEvent(Client player, int type, int slot)
        {
            int playerId = player.GetData(EntityData.PLAYER_SQL_ID);
            ClothesModel clothes = Globals.GetDressedClothesInSlot(playerId, type, slot);
            
            if (type == 0)
            {
                player.SetClothes(slot, clothes == null ? 0 : clothes.drawable, clothes == null ? 0 : clothes.texture);
            }
            else
            {
                player.SetAccessories(slot, clothes == null ? 255 : clothes.drawable, clothes == null ? 255 : clothes.texture);
            }
        }

        [RemoteEvent("clothesItemSelected")]
        public void ClothesItemSelectedEvent(Client player, string clothesJson)
        {
            ClothesModel clothesModel = NAPI.Util.FromJson<ClothesModel>(clothesJson);
            int businessId = player.GetData(EntityData.PLAYER_BUSINESS_ENTERED);
            int sex = player.GetData(EntityData.PLAYER_SEX);
            int products = GetClothesProductsPrice(clothesModel.id, sex, clothesModel.type, clothesModel.slot);
            BusinessModel business = GetBusinessById(businessId);
            int price = (int)Math.Round(products * business.multiplier);

            // We check whether the player has enough money
            int playerMoney = player.GetSharedData(EntityData.PLAYER_MONEY);

            if (playerMoney >= price)
            {
                int playerId = player.GetData(EntityData.PLAYER_SQL_ID);

                // Substracting paid money
                player.SetSharedData(EntityData.PLAYER_MONEY, playerMoney - price);

                // We substract the product and add funds to the business
                if (business.owner != string.Empty)
                {
                    business.funds += price;
                    business.products -= products;

                    Task.Factory.StartNew(() => {
                        // Update the business
                        Database.UpdateBusiness(business);
                    });
                }

                // Undress previous clothes
                Globals.UndressClothes(playerId, clothesModel.type, clothesModel.slot);

                // Store the remaining data
                clothesModel.player = playerId;
                clothesModel.dressed = true;

                Task.Factory.StartNew(() => {
                    // Storing the clothes into database
                    clothesModel.id = Database.AddClothes(clothesModel);
                    Globals.clothesList.Add(clothesModel);

                    // Confirmation message sent to the player
                    string purchaseMessage = string.Format(InfoRes.business_item_purchased, price);
                    player.SendChatMessage(Constants.COLOR_INFO + purchaseMessage);
                });
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_enough_money);
            }
        }

        [RemoteEvent("changeHairStyle")]
        public void ChangeHairStyleEvent(Client player, string skinJson)
        {
            int playerMoney = player.GetSharedData(EntityData.PLAYER_MONEY);
            int businessId = player.GetData(EntityData.PLAYER_BUSINESS_ENTERED);
            BusinessModel business = GetBusinessById(businessId);
            int price = (int)Math.Round(business.multiplier * Constants.PRICE_BARBER_SHOP);

            if (playerMoney >= price)
            {
                int playerId = player.GetData(EntityData.PLAYER_SQL_ID);

                // Getting the new hairstyle from the JSON
                SkinModel skinModel = NAPI.Util.FromJson<SkinModel>(skinJson);

                // Saving new entity data
                player.SetData(EntityData.PLAYER_SKIN_MODEL, skinModel);

                // Substract money to the player
                player.SetSharedData(EntityData.PLAYER_MONEY, playerMoney - price);

                Task.Factory.StartNew(() => {
                    // We update values in the database
                    Database.UpdateCharacterHair(playerId, skinModel);

                    // We substract the product and add funds to the business
                    if (business.owner != string.Empty)
                    {
                        business.funds += price;
                        business.products -= Constants.PRICE_BARBER_SHOP;
                        Database.UpdateBusiness(business);
                    }

                    // Delete the browser
                    player.TriggerEvent("destroyBrowser");

                    // Confirmation message sent to the player
                    string playerMessage = string.Format(InfoRes.haircut_purchased, price);
                    player.SendChatMessage(Constants.COLOR_INFO + playerMessage);
                });
            }
            else
            {
                // The player has not the required money
                string message = string.Format(ErrRes.haircut_money, price);
                player.SendChatMessage(Constants.COLOR_ERROR + message);
            }
        }

        [RemoteEvent("loadZoneTattoos")]
        public void LoadZoneTattoosEvent(Client player, int zone)
        {
            int sex = player.GetData(EntityData.PLAYER_SEX);
            List<BusinessTattooModel> tattooList = GetBusinessZoneTattoos(sex, zone);

            // We update the menu with the tattoos
            player.TriggerEvent("showZoneTattoos", NAPI.Util.ToJson(tattooList));
        }

        [RemoteEvent("purchaseTattoo")]
        public void PurchaseTattooEvent(Client player, int tattooZone, int tattooIndex)
        {
            int sex = player.GetData(EntityData.PLAYER_SEX);
            int businessId = player.GetData(EntityData.PLAYER_BUSINESS_ENTERED);
            BusinessModel business = GetBusinessById(businessId);

            // Getting the tattoo and its price
            BusinessTattooModel businessTattoo = GetBusinessZoneTattoos(sex, tattooZone).ElementAt(tattooIndex);
            int playerMoney = player.GetSharedData(EntityData.PLAYER_MONEY);
            int price = (int)Math.Round(business.multiplier * businessTattoo.price);

            if (price <= playerMoney)
            {
                TattooModel tattoo = new TattooModel();
                {
                    tattoo.player = player.GetData(EntityData.PLAYER_SQL_ID);
                    tattoo.slot = tattooZone;
                    tattoo.library = businessTattoo.library;
                    tattoo.hash = sex == Constants.SEX_MALE ? businessTattoo.maleHash : businessTattoo.femaleHash;
                }

                Task.Factory.StartNew(() => {
                    if (Database.AddTattoo(tattoo) == true)
                    {
                        // We add the tattoo to the list
                        Globals.tattooList.Add(tattoo);

                        // Substract player money
                        player.SetSharedData(EntityData.PLAYER_MONEY, playerMoney - price);

                        // We substract the product and add funds to the business
                        if (business.owner != string.Empty)
                        {
                            business.funds += price;
                            business.products -= businessTattoo.price;
                            Database.UpdateBusiness(business);
                        }

                        // Confirmation message sent to the player
                        string playerMessage = string.Format(InfoRes.tattoo_purchased, price);
                        player.SendChatMessage(Constants.COLOR_INFO + playerMessage);

                        // Reload client tattoo list
                        player.TriggerEvent("addPurchasedTattoo", NAPI.Util.ToJson(tattoo));
                    }
                    else
                    {
                        // Player already had that tattoo
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.tattoo_duplicated);
                    }
                });
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_enough_money);
            }
        }

        [RemoteEvent("loadCharacterClothes")]
        public void LoadCharacterClothesEvent(Client player)
        {
            // Generate player's clothes
            Customization.ApplyPlayerClothes(player);
        }
    }
}
