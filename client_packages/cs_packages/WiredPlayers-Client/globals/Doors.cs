using RAGE;
using RAGE.Game;
using RAGE.Elements;
using static RAGE.Events;

namespace WiredPlayers_Client.globals
{
    class Doors : Events.Script
    {
        private static Colshape policeMainDoors = null;
        private static Colshape policeBackDoors = null;
        private static Colshape policeCellDoors = null;
        private static Colshape motorsportMain = null;
        private static Colshape motorsportParking = null;
        private static Colshape supermarketDoors = null;
        private static Colshape clubhouseDoor = null;

        public Doors()
        {
            OnGuiReady += OnGuiReadyEvent;
            OnPlayerEnterColshape += OnPlayerEnterColshapeEvent;
        }

        public static void OnGuiReadyEvent()
        {
            policeMainDoors = new SphereColshape(new Vector3(468.535f, -1014.098f, 26.386f), 5.0f, 0);
            policeBackDoors = new SphereColshape(new Vector3(435.131f, -981.9197f, 30.689f), 5.0f, 0);
            policeCellDoors = new SphereColshape(new Vector3(461.7501f, -998.361f, 24.915f), 5.0f, 0);
            motorsportMain = new SphereColshape(new Vector3(-59.893f, -1092.952f, 26.8836f), 5.0f, 0);
            motorsportParking = new SphereColshape(new Vector3(-39.134f, -1108.22f, 26.72f), 5.0f, 0);
            supermarketDoors = new SphereColshape(new Vector3(-711.545f, -915.54f, 19.216f), 5.0f, 0);
            clubhouseDoor = new SphereColshape(new Vector3(981.7533f, -102.7987f, 74.8487f), 5.0f, 0);
        }

        public static void OnPlayerEnterColshapeEvent(Colshape colshape, CancelEventArgs cancel)
        {
            if (colshape.Id == policeMainDoors.Id)
            {
                Object.SetStateOfClosestDoorOfType(Misc.GetHashKey("v_ilev_ph_door002"), 434.7479f, -983.2151f, 30.83926f, true, 0.0f, false);
                Object.SetStateOfClosestDoorOfType(Misc.GetHashKey("v_ilev_ph_door01"), 434.7479f, -980.6184f, 30.83926f, true, 0.0f, false);
                return;
            }

            if (colshape.Id == policeBackDoors.Id)
            {
                Object.SetStateOfClosestDoorOfType(Misc.GetHashKey("v_ilev_rc_door2"), 469.9679f, -1014.452f, 26.53623f, true, 0.0f, false);
                Object.SetStateOfClosestDoorOfType(Misc.GetHashKey("v_ilev_rc_door2"), 467.3716f, -1014.452f, 26.53623f, true, 0.0f, false);
                return;
            }

            if (colshape.Id == policeCellDoors.Id)
            {
                Object.SetStateOfClosestDoorOfType(Misc.GetHashKey("v_ilev_ph_cellgate"), 461.8065f, -994.4086f, 25.06443f, true, 0.0f, false);
                Object.SetStateOfClosestDoorOfType(Misc.GetHashKey("v_ilev_ph_cellgate"), 461.8065f, -997.6583f, 25.06443f, true, 0.0f, false);
                Object.SetStateOfClosestDoorOfType(Misc.GetHashKey("v_ilev_ph_cellgate"), 461.8065f, -1001.302f, 25.06443f, true, 0.0f, false);
                return;
            }

            if (colshape.Id == motorsportMain.Id)
            {
                Object.SetStateOfClosestDoorOfType(Misc.GetHashKey("v_ilev_csr_door_l"), -59.89302f, -1092.952f, 26.88362f, false, 0.0f, false);
                Object.SetStateOfClosestDoorOfType(Misc.GetHashKey("v_ilev_csr_door_r"), -60.54582f, -1094.749f, 26.88872f, false, 0.0f, false);
                return;
            }

            if (colshape.Id == motorsportParking.Id)
            {
                Object.SetStateOfClosestDoorOfType(Misc.GetHashKey("v_ilev_csr_door_l"), -39.13366f, -1108.218f, 26.7198f, false, 0.0f, false);
                Object.SetStateOfClosestDoorOfType(Misc.GetHashKey("v_ilev_csr_door_r"), -37.33113f, -1108.873f, 26.7198f, false, 0.0f, false);
                return;
            }

            if (colshape.Id == supermarketDoors.Id)
            {
                Object.SetStateOfClosestDoorOfType(Misc.GetHashKey("v_ilev_gasdoor"), -711.5449f, -915.5397f, 19.21559f, true, 0.0f, false);
                Object.SetStateOfClosestDoorOfType(Misc.GetHashKey("v_ilev_gasdoor_r"), -711.5449f, -915.5397f, 19.2156f, true, 0.0f, false);
                return;
            }

            if (colshape.Id == policeMainDoors.Id)
            {
                Object.SetStateOfClosestDoorOfType(Misc.GetHashKey("v_ilev_lostdoor"), 981.7533f, -102.7987f, 74.84873f, true, 0, false);
                return;
            }
        }
    }
}