using RAGE;
using RAGE.Elements;
using System.Collections.Generic;
using System;

namespace WiredPlayers_Client.globals
{
    class Keys : Events.Script
    {
        private static readonly int KEY_PRESS_TIME = 350000;
        private static Dictionary<int, long> pressedKeys;
        private static List<int> consoleKeys;

        public Keys()
        {
            // Initialize the dictionary
            pressedKeys = new Dictionary<int, long>();

            // Bind the required Keys
            BindConsoleKeys();
        }

        public static int DetectPressedKey(long currentTicks)
        {
            // Check the first released key
            int releasedKey = -1;

            // Check if the keys are loaded and player has not opened a CEF instance
            if (consoleKeys == null || Browser.customBrowser != null) return releasedKey;

            foreach(int key in consoleKeys)
            {
                if(pressedKeys.TryGetValue(key, out long downTicks))
                {
                    // If there's already a key released we do nothing
                    if (releasedKey >= 0) continue;

                    // Check if the key is already up
                    if(!Input.IsDown(key) && (currentTicks - downTicks) > KEY_PRESS_TIME)
                    {
                        releasedKey = key;
                        pressedKeys.Remove(releasedKey);
                    }
                }
                else if(Input.IsDown(key))
                {
                    // Store the key into the dictionary
                    pressedKeys.Add(key, currentTicks);
                }
            }

            return releasedKey;
        }

        public static void FireKeyPressed(int key)
        {
            switch (key)
            {
                case (int)ConsoleKey.E:
                    if (Player.LocalPlayer.Vehicle == null)
                    {
                        // Reset the player's animation
                        Events.CallRemote("checkPlayerEventKeyStopAnim");
                    }
                    break;
                case (int)ConsoleKey.F:
                    if (Player.LocalPlayer.Vehicle == null)
                    {
                        // Check if player can enter any place
                        Events.CallRemote("checkPlayerEventKey");
                    }

                    return;
                case (int)ConsoleKey.K:
                    if (Player.LocalPlayer.Vehicle != null)
                    {
                        if (!Player.LocalPlayer.Vehicle.IsSeatFree(-1, 0) && Player.LocalPlayer.Vehicle.GetPedInSeat(-1, 0) == Player.LocalPlayer.Handle)
                        {
                            // Toggle vehicle's engine
                            Events.CallRemote("engineOnEventKey");
                        }
                    }
                    break;
                case (int)ConsoleKey.J:
                    if(Player.LocalPlayer.Vehicle == null)
                    {
                        Events.CallRemote("drugPickUp");
                    }
                    break;
            }
        }

        private void BindConsoleKeys()
        {
            // Initialize the list
            consoleKeys = new List<int>();

            consoleKeys.Add((int)ConsoleKey.E);
            consoleKeys.Add((int)ConsoleKey.F);
            consoleKeys.Add((int)ConsoleKey.K);
            consoleKeys.Add((int)ConsoleKey.J);
        }
    }
}
