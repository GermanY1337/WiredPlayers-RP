using RAGE;
using RAGE.Elements;
using WiredPlayers_Client.globals;
using WiredPlayers_Client.model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace WiredPlayers_Client.character
{
    class Character : Events.Script
    {
        private int camera;
        private string characters = null;
        private PlayerModel playerData = null;

        public Character()
        {
            Events.Add("showPlayerCharacters", ShowPlayerCharactersEvent);
            Events.Add("loadCharacter", LoadCharacterEvent);
            Events.Add("showCharacterCreationMenu", ShowCharacterCreationMenuEvent);
            Events.Add("changePlayerSex", ChangePlayerSexEvent);
            Events.Add("storePlayerData", StorePlayerDataEvent);
            Events.Add("cameraPointTo", CameraPointToEvent);
            Events.Add("rotateCharacter", RotateCharacterEvent);
            Events.Add("characterNameDuplicated", CharacterNameDuplicatedEvent);
            Events.Add("acceptCharacterCreation", AcceptCharacterCreationEvent);
            Events.Add("cancelCharacterCreation", CancelCharacterCreationEvent);
            Events.Add("characterCreatedSuccessfully", CharacterCreatedSuccessfullyEvent); 
        }

        private void ShowPlayerCharactersEvent(object[] args)
        {
            // Store account characters
            characters = args[0].ToString();

            // Show character list
            Browser.CreateBrowserEvent(new object[] { "package://statics/html/sideMenu.html", "populateCharacterList", characters });
        }

        private void LoadCharacterEvent(object[] args)
        {
            // Get the variables from the array
            string characterName = args[0].ToString();

            // Destroy the menu
            Browser.DestroyBrowserEvent(null);

            // Show character list
            Events.CallRemote("loadCharacter", characterName);
        }

        private void ShowCharacterCreationMenuEvent(object[] args)
        {
            // Destroy the menu
            Browser.DestroyBrowserEvent(null);

            // Initialize the character creation
            playerData = new PlayerModel();
            ApplyPlayerModelChanges();

            // Set the character into the creator menu
            Events.CallRemote("setCharacterIntoCreator");

            // Make the camera focus the player
            camera = RAGE.Game.Cam.CreateCameraWithParams(RAGE.Game.Misc.GetHashKey("DEFAULT_SCRIPTED_CAMERA"), 152.6008f, -1003.25f, -98f, -20.0f, 0.0f, 0.0f, 90.0f, true, 2);
            RAGE.Game.Cam.SetCamActive(camera, true);
            RAGE.Game.Cam.RenderScriptCams(true, false, 0, true, false, 0);

            // Disable the interface
            RAGE.Game.Ui.DisplayRadar(false);
            RAGE.Game.Ui.DisplayHud(false);
            Chat.Activate(false);
            Chat.Show(false);

            // Load the character creation menu
            Browser.CreateBrowserEvent(new object[] { "package://statics/html/characterCreator.html" });
        }

        private void ChangePlayerSexEvent(object[] args)
        {
            // Store the value into the object
            playerData.sex = Convert.ToInt32(args[0]);

            // Change the player's look
            Player.LocalPlayer.Model = playerData.sex == Constants.SEX_MALE ? RAGE.Game.Misc.GetHashKey("mp_m_freemode_01") : RAGE.Game.Misc.GetHashKey("mp_f_freemode_01");
            ApplyPlayerModelChanges();
        }

        private void StorePlayerDataEvent(object[] args)
        {
            // Get the object from the JSON string
            Dictionary<string, object> dataObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(args[0].ToString());

            foreach (KeyValuePair<string, object> keyValue in dataObject)
            {
                // Set the value into the player data object
                PropertyInfo property = playerData.GetType().GetProperty(keyValue.Key);
                property.SetValue(playerData, Convert.ChangeType(keyValue.Value, property.PropertyType));
            }

            // Update the model changes
            ApplyPlayerModelChanges();
        }

        private void CameraPointToEvent(object[] args)
        {
            // Get the variables from the array
            int bodyPart = Convert.ToInt32(args[0]);

            if(bodyPart == 0)
            {
                // Make the camera point to the body
                RAGE.Game.Cam.SetCamCoord(camera, 152.6008f, -1003.25f, -98f);
            }
            else
            {
                // Make the camera point to the face
                RAGE.Game.Cam.SetCamCoord(camera, 152.3708f, -1001.75f, -98.45f);
            }
        }

        private void RotateCharacterEvent(object[] args)
        {
            // Get the variables from the array
            int rotation = Convert.ToInt32(args[0]);

            // Rotate the character
            Player.LocalPlayer.SetHeading(rotation);
        }

        private void CharacterNameDuplicatedEvent(object[] args)
        {
            // Duplicated name
            Browser.ExecuteFunctionEvent(new object[] { "showPlayerDuplicatedWarn" });
        }

        private void AcceptCharacterCreationEvent(object[] args)
        {
            // Get the variables from the array
            string name = args[0].ToString();
            int age = Convert.ToInt32(args[1]);

            // Create the new character
            string skinJson = JsonConvert.SerializeObject(playerData);
            Events.CallRemote("createCharacter", name, age, playerData.sex, skinJson);
        }

        private void CancelCharacterCreationEvent(object[] args)
        {
            // Destroy the browser
            CharacterCreatedSuccessfullyEvent(null);

            // Add clothes and tattoos
            Events.CallRemote("loadCharacter", Player.LocalPlayer.Name);

            // Show the character list
            Browser.CreateBrowserEvent(new object[] { "package://statics/html/sideMenu.html", "populateCharacterList", characters });
        }

        private void CharacterCreatedSuccessfullyEvent(object[] args)
        {
            // Get the default camera
            RAGE.Game.Cam.DestroyCam(camera, true);
            RAGE.Game.Cam.RenderScriptCams(false, false, 0, true, false, 0);

            // Enable the interface
            RAGE.Game.Ui.DisplayRadar(true);
            RAGE.Game.Ui.DisplayHud(true);
            Chat.Activate(true);
            Chat.Show(true);

            // Destroy character creation menu
            Browser.DestroyBrowserEvent(null);
        }

        private void ApplyPlayerModelChanges()
        {
            // Apply the changes to the player
            Player.LocalPlayer.SetHeadBlendData(playerData.firstHeadShape, playerData.secondHeadShape, 0, playerData.firstSkinTone, playerData.secondSkinTone, 0, playerData.headMix, playerData.skinMix, 0, false);
            Player.LocalPlayer.SetComponentVariation(2, playerData.hairModel, 0, 0);
            Player.LocalPlayer.SetHairColor(playerData.firstHairColor, playerData.secondHairColor);
            Player.LocalPlayer.SetEyeColor(playerData.eyesColor);
            Player.LocalPlayer.SetHeadOverlay(1, playerData.beardModel, 1.0f);
            Player.LocalPlayer.SetHeadOverlayColor(1, 1, playerData.beardColor, 0);
            Player.LocalPlayer.SetHeadOverlay(10, playerData.chestModel, 1.0f);
            Player.LocalPlayer.SetHeadOverlayColor(10, 1, playerData.chestColor, 0);
            Player.LocalPlayer.SetHeadOverlay(2, playerData.eyebrowsModel, 1.0f);
            Player.LocalPlayer.SetHeadOverlayColor(2, 1, playerData.eyebrowsColor, 0);
            Player.LocalPlayer.SetHeadOverlay(5, playerData.blushModel, 1.0f);
            Player.LocalPlayer.SetHeadOverlayColor(5, 2, playerData.blushColor, 0);
            Player.LocalPlayer.SetHeadOverlay(8, playerData.lipstickModel, 1.0f);
            Player.LocalPlayer.SetHeadOverlayColor(8, 2, playerData.lipstickColor, 0);
            Player.LocalPlayer.SetHeadOverlay(0, playerData.blemishesModel, 1.0f);
            Player.LocalPlayer.SetHeadOverlay(3, playerData.ageingModel, 1.0f);
            Player.LocalPlayer.SetHeadOverlay(6, playerData.complexionModel, 1.0f);
            Player.LocalPlayer.SetHeadOverlay(7, playerData.sundamageModel, 1.0f);
            Player.LocalPlayer.SetHeadOverlay(9, playerData.frecklesModel, 1.0f);
            Player.LocalPlayer.SetHeadOverlay(4, playerData.makeupModel, 1.0f);
            Player.LocalPlayer.SetFaceFeature(0, playerData.noseWidth);
            Player.LocalPlayer.SetFaceFeature(1, playerData.noseHeight);
            Player.LocalPlayer.SetFaceFeature(2, playerData.noseLength);
            Player.LocalPlayer.SetFaceFeature(3, playerData.noseBridge);
            Player.LocalPlayer.SetFaceFeature(4, playerData.noseTip);
            Player.LocalPlayer.SetFaceFeature(5, playerData.noseShift);
            Player.LocalPlayer.SetFaceFeature(6, playerData.browHeight);
            Player.LocalPlayer.SetFaceFeature(7, playerData.browWidth);
            Player.LocalPlayer.SetFaceFeature(8, playerData.cheekboneHeight);
            Player.LocalPlayer.SetFaceFeature(9, playerData.cheekboneWidth);
            Player.LocalPlayer.SetFaceFeature(10, playerData.cheeksWidth);
            Player.LocalPlayer.SetFaceFeature(11, playerData.eyes);
            Player.LocalPlayer.SetFaceFeature(12, playerData.lips);
            Player.LocalPlayer.SetFaceFeature(13, playerData.jawWidth);
            Player.LocalPlayer.SetFaceFeature(14, playerData.jawHeight);
            Player.LocalPlayer.SetFaceFeature(15, playerData.chinLength);
            Player.LocalPlayer.SetFaceFeature(16, playerData.chinPosition);
            Player.LocalPlayer.SetFaceFeature(17, playerData.chinWidth);
            Player.LocalPlayer.SetFaceFeature(18, playerData.chinShape);
            Player.LocalPlayer.SetFaceFeature(19, playerData.neckWidth);
        }
    }
}
