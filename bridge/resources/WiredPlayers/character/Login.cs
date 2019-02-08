using GTANetworkAPI;
using WiredPlayers.model;
using WiredPlayers.database;
using WiredPlayers.globals;
using WiredPlayers.messages.information;
using WiredPlayers.messages.general;
using System.Threading.Tasks;
using System;

namespace WiredPlayers.character
{
    public class Login : Script
    {     
        [ServerEvent(Event.PlayerConnected)]
        public void OnPlayerConnected(Client player)
        {
            // Set the default skin and transparency
            NAPI.Player.SetPlayerSkin(player, PedHash.Strperf01SMM);
            player.Transparency = 255;

            // Initialize the player data
            Character.InitializePlayerData(player);

            Task.Factory.StartNew(() =>
            {
                AccountModel account = Database.GetAccount(player.SocialClubName);

                switch (account.status)
                {
                    case -1:
                        player.SendChatMessage(Constants.COLOR_INFO + InfoRes.account_disabled);
                        player.Kick(InfoRes.account_disabled);
                        break;
                    case 0:
                        // Show the register window
                        player.TriggerEvent("showRegisterWindow");
                        break;
                    default:
                        // Welcome message
                        string welcomeMessage = string.Format(GenRes.welcome_message, player.SocialClubName);
                        player.SendChatMessage(welcomeMessage);
                        player.SendChatMessage(GenRes.welcome_hint);
                        player.SendChatMessage(GenRes.help_hint);
                        player.SendChatMessage(GenRes.ticket_hint);

                        if (account.lastCharacter > 0)
                        {
                            // Load selected character
                            PlayerModel character = Database.LoadCharacterInformationById(account.lastCharacter);
                            SkinModel skinModel = Database.GetCharacterSkin(account.lastCharacter);
                            
                            player.Name = character.realName;
                            player.SetData(EntityData.PLAYER_SKIN_MODEL, skinModel);
                            NAPI.Player.SetPlayerSkin(player, character.sex == 0 ? PedHash.FreemodeMale01 : PedHash.FreemodeFemale01);

                            Character.LoadCharacterData(player, character);
                            Customization.ApplyPlayerCustomization(player, skinModel, character.sex);
                            Customization.ApplyPlayerClothes(player);
                            Customization.ApplyPlayerTattoos(player);
                        }

                        // Activate the login window
                        player.SetSharedData(EntityData.SERVER_TIME, DateTime.Now.ToString("HH:mm:ss"));

                        break;
                }
            });
        }

        [RemoteEvent("loginAccount")]
        public void LoginAccountEvent(Client player, string password)
        {
            Task.Factory.StartNew(() =>
            {
                bool login = Database.LoginAccount(player.SocialClubName, password);
                player.TriggerEvent(login ? "clearLoginWindow" : "showLoginError");
            });
        }

        [RemoteEvent("registerAccount")]
        public void RegisterAccountEvent(Client player, string password)
        {
            Task.Factory.StartNew(() =>
            {
                Database.RegisterAccount(player.SocialClubName, password);
                player.TriggerEvent("clearRegisterWindow");
            });
        }

        [RemoteEvent("changeCharacterSex")]
        public void ChangeCharacterSexEvent(Client player, int sex)
        {
            // Set the model of the player
            NAPI.Player.SetPlayerSkin(player, sex == 0 ? PedHash.FreemodeMale01 : PedHash.FreemodeFemale01);

            // Remove player's clothes
            player.SetClothes(11, 15, 0);
            player.SetClothes(3, 15, 0);
            player.SetClothes(8, 15, 0);

            // Save sex entity shared data
            player.SetData(EntityData.PLAYER_SEX, sex);
        }

        [RemoteEvent("createCharacter")]
        public void CreateCharacterEvent(Client player, string playerName, int playerAge, int playerSex, string skinJson)
        {
            PlayerModel playerModel = new PlayerModel();
            SkinModel skinModel = NAPI.Util.FromJson<SkinModel>(skinJson);

            playerModel.realName = playerName;
            playerModel.age = playerAge;
            playerModel.sex = playerSex;

            // Apply the skin to the character
            player.SetData(EntityData.PLAYER_SKIN_MODEL, skinModel);
            Customization.ApplyPlayerCustomization(player, skinModel, playerSex);

            Task.Factory.StartNew(() =>
            {
                int playerId = Database.CreateCharacter(player, playerModel, skinModel);

                if (playerId > 0)
                {
                    Character.InitializePlayerData(player);
                    player.Transparency = 255;
                    player.SetData(EntityData.PLAYER_SQL_ID, playerId);
                    player.SetData(EntityData.PLAYER_NAME, playerName);
                    player.SetData(EntityData.PLAYER_AGE, playerAge);
                    player.SetData(EntityData.PLAYER_SEX, playerSex);
                    player.SetSharedData(EntityData.PLAYER_SPAWN_POS, new Vector3(200.6641f, -932.0939f, 30.6868f));
                    player.SetSharedData(EntityData.PLAYER_SPAWN_ROT, new Vector3(0.0f, 0.0f, 0.0f));

                    Database.UpdateLastCharacter(player.SocialClubName, playerId);

                    player.TriggerEvent("characterCreatedSuccessfully");
                }
            });
        }

        [RemoteEvent("setCharacterIntoCreator")]
        public void SetCharacterIntoCreatorEvent(Client player)
        {
            // Change player's skin
            NAPI.Player.SetPlayerSkin(player, PedHash.FreemodeMale01);

            // Remove clothes
            player.SetClothes(11, 15, 0);
            player.SetClothes(3, 15, 0);
            player.SetClothes(8, 15, 0);

            // Remove all the tattoos
            Customization.RemovePlayerTattoos(player);

            // Set player's position
            player.Transparency = 255;
            player.Rotation = new Vector3(0.0f, 0.0f, 180.0f);
            player.Position = new Vector3(152.3787f, -1000.644f, -99f);
        }

        [RemoteEvent("loadCharacter")]
        public void LoadCharacterEvent(Client player, string name)
        {
            Task.Factory.StartNew(() =>
            {
                PlayerModel playerModel = Database.LoadCharacterInformationByName(name);
                SkinModel skinModel = Database.GetCharacterSkin(playerModel.id);

                // Load player's model
                player.Name = playerModel.realName;
                player.SetData(EntityData.PLAYER_SKIN_MODEL, skinModel);
                NAPI.Player.SetPlayerSkin(player, playerModel.sex == 0 ? PedHash.FreemodeMale01 : PedHash.FreemodeFemale01);

                Character.LoadCharacterData(player, playerModel);
                Customization.ApplyPlayerCustomization(player, skinModel, playerModel.sex);
                Customization.ApplyPlayerClothes(player);
                Customization.ApplyPlayerTattoos(player);

                // Update last selected character
                Database.UpdateLastCharacter(player.SocialClubName, playerModel.id);
            });
        }
    }
}