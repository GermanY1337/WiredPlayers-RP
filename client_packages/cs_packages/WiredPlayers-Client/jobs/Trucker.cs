using RAGE;
using RAGE.Elements;
using WiredPlayers_Client.globals;
using System.Collections.Generic;
using System.Linq;

namespace WiredPlayers_Client.jobs
{
    class Trucker : Events.Script
    {
        private static List<MapObject> crateList;

        public Trucker()
        {
            Events.Add("createTruckerCrates", CreateTruckerCratesEvent);
        }

        public static void CheckPlayerStoredCrate()
        {
            if(crateList != null && crateList.Count > 0 && RAGE.Game.Misc.GetHashKey("forklift") == Player.LocalPlayer.Vehicle.Model) {
                // Check if the player has any crate near
                if (GetCrateInRange(Player.LocalPlayer.Vehicle, 1.5f) == null) return;

                // Store the crate into the closest vehicle
                Vehicle truck = StoreCrateIntoVehicle("mule");

                if(truck != null)
                {
                    
                }
            }
            else
            {
                Player.LocalPlayer.Vehicle.SetDoorOpen(2, false, false);
                Player.LocalPlayer.Vehicle.SetDoorOpen(3, false, false);
            }
        }

        private void CreateTruckerCratesEvent(object[] args)
        {
            // Initialize the crates
            crateList = new List<MapObject>();

            foreach (Vector3 crate in Constants.TRUCKER_CRATES)
            {
                MapObject crateObject = new MapObject(RAGE.Game.Misc.GetHashKey("prop_boxpile_04a"), crate, new Vector3(0.0f, 0.0f, 0.0f));
                crateObject.SetPhysicsParams(17.5f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f);
                crateObject.SetActivatePhysicsAsSoonAsItIsUnfrozen(true);
                crateObject.FreezePosition(false);

                // Add the crate to the list
                crateList.Add(crateObject);
            }
        }

        private static Vehicle StoreCrateIntoVehicle(string model, float distance = 2.5f)
        {
            uint vehicleHash = RAGE.Game.Misc.GetHashKey(model);

            // Get the list of vehicles with selected model and trunk opened
            List<Vehicle> truckList = Entities.Vehicles.Streamed.Where(veh => veh.IsDoorFullyOpen(2) && veh.Model == vehicleHash).ToList();

            foreach(Vehicle truck in truckList)
            {
                // Get the closest crate
                MapObject crate = GetCrateInRange(truck, distance);

                if (crate != null)
                {
                    // Remove the crate from the game
                    crateList.Remove(crate);
                    crate.Destroy();

                    return truck;
                }
            }

            return null;
        }

        private static MapObject GetCrateInRange(Vehicle truck, float distance)
        {
            foreach(MapObject crate in crateList)
            {
                if(crate.Position.DistanceTo(truck.Position) <= distance)
                {
                    // We found the crate the player is storing
                    return crate;
                }
            }

            return null;
        }
    }
}
