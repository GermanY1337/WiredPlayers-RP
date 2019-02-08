using RAGE;
using RAGE.Elements;
using System;
using System.Drawing;
using System.Collections.Generic;
using WiredPlayers_Client.account;
using WiredPlayers_Client.vehicles;

namespace WiredPlayers_Client.globals
{
    class Globals : Events.Script
    {
        private DateTime lastTimeChecked;
        private int playerMoney = 0;
        public static bool playerLogged;

        public Globals()
        {
            Events.Add("changePlayerWalkingStyle", ChangePlayerWalkingStyleEvent);
            Events.Add("resetPlayerWalkingStyle", ResetPlayerWalkingStyleEvent);
            Events.AddDataHandler("SERVER_TIME", PlayerConnectionStateChanged);
            Events.OnGuiReady += OnGuiReadyEvent;
            Events.Tick += TickEvent;
        }

        private void ChangePlayerWalkingStyleEvent(object[] args)
        {
            // Get the player
            Player player = (Player)args[0];
            string clipSet = args[1].ToString();

            player.SetMovementClipset(clipSet, 0.1f);
        }

        private void ResetPlayerWalkingStyleEvent(object[] args)
        {
            // Get the player
            Player player = (Player)args[0];

            player.ResetMovementClipset(0.0f);
        }

        public static void OnGuiReadyEvent()
        {
            // Remove health regeneration
            RAGE.Game.Player.SetPlayerHealthRechargeMultiplier(0.0f);

            // Remove weapons from the vehicles
            RAGE.Game.Player.DisablePlayerVehicleRewards();

            // Remove the fade out after player's death
            RAGE.Game.Misc.SetFadeOutAfterDeath(false);

            // Freeze the player until he logs in
            Player.LocalPlayer.FreezePosition(true);
        }

        private void PlayerConnectionStateChanged(Entity entity, object arg)
        {
            string[] serverTime = Player.LocalPlayer.GetSharedData("SERVER_TIME").ToString().Split(":");

            int hours = int.Parse(serverTime[0]);
            int minutes = int.Parse(serverTime[1]);
            int seconds = int.Parse(serverTime[2]);

            // Set the hour from the server
            RAGE.Game.Clock.SetClockTime(hours, minutes, seconds);

            // Get the current timestamp
            lastTimeChecked = DateTime.UtcNow;

            // Show the login window
            Login.AccountLoginFormEvent(null);
        }

        private void TickEvent(List<Events.TickNametagData> nametags)
        {
            // Check if the player is connected
            if (!playerLogged) return;

            DateTime dateTime = DateTime.UtcNow;

            if (Vehicles.lastPosition != null)
            {
                // Update the speedometer
                Vehicles.UpdateSpeedometer();
            }

            // Update the player's money each 375ms
            if (dateTime.Ticks - lastTimeChecked.Ticks >= 3750000)
            {
                // Check if the player is loaded
                object money = Player.LocalPlayer.GetSharedData("PLAYER_MONEY"); 

                if(money != null)
                {
                    playerMoney = Convert.ToInt32(money);
                    lastTimeChecked = dateTime;
                }
            }

            // Draw the money
            RAGE.NUI.UIResText.Draw(playerMoney + "$", 1900, 60, RAGE.Game.Font.Pricedown, 0.5f, Color.DarkOliveGreen, RAGE.NUI.UIResText.Alignment.Right, true, true, 0);

            // Detect if a key has been pressed
            int key = Keys.DetectPressedKey(dateTime.Ticks);

            if (key >= 0)
            {
                // Fire the event for the pressed key
                Keys.FireKeyPressed(key);
            }
        }
    }
}
