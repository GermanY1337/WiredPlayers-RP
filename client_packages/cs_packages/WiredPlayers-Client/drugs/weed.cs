using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace WiredPlayers_Client.drugs
{
    class Weed : Events.Script
    {
        private static Colshape WeedFarm = null;

        public Weed()
        {
            Events.OnPlayerEnterColshape += OnPlayerEnterColshapeEvent;
            Events.OnGuiReady += OnGuiReadyEvent;
        }

        public static void OnGuiReadyEvent()
        {
            WeedFarm = new SphereColshape(new Vector3(2226.711f, 5576.737f, 53.86317f), 15.0f, 0);
        }

        private static void OnPlayerEnterColshapeEvent(Colshape colshape, Events.CancelEventArgs cancel)
        {
                RAGE.Chat.Output($"You entered Colshape ID:{colshape.Id}");
        }
    }
}
