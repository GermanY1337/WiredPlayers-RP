using RAGE;
using RAGE.NUI;
using RAGE.Elements;
using WiredPlayers_Client.globals;
using System;
using System.Drawing;

namespace WiredPlayers_Client.vehicles
{
    class Vehicles : Events.Script
    {
        private Blip vehicleLocationBlip = null;

        private static float kms = 0.0f;
        private static float gas = 0.0f;
        private static float distance = 0.0f;
        private static float consumed = 0.0f;
        public static Vector3 lastPosition = null;

        public Vehicles()
        {
            Events.Add("initializeSpeedometer", InitializeSpeedometerEvent);
            Events.Add("locateVehicle", LocateVehicleEvent);
            Events.Add("deleteVehicleLocation", DeleteVehicleLocationEvent);
            Events.OnPlayerLeaveVehicle += PlayerLeaveVehicleEvent;
        }

        public static void UpdateSpeedometer()
        {
            Vehicle vehicle = Player.LocalPlayer.Vehicle;
            Vector3 currentPosition = vehicle.Position;

            // Get speedometer's data
            Vector3 velocity = vehicle.GetVelocity();
            int health = vehicle.GetHealth();
            int maxHealth = vehicle.GetMaxHealth();
            int healthPercent = (int)Math.Floor((double)(health / maxHealth) * 100);
            int speed = (int)Math.Round(Math.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y + velocity.Z * velocity.Z) * 3.6f);

            // Get the distance and consume
            distance = Vector3.Distance(currentPosition, lastPosition);
            consumed = distance * Constants.CONSUME_PER_METER;
            lastPosition = currentPosition;

            if(gas - consumed <= 0.0f)
            {
                // The fuel tank is empty
                Events.CallRemote("stopPlayerCar");
                consumed = 0.0f;
            }

            // Get the total gas and kms
            string totalKms = Math.Round((double)(kms + distance) / 10) / 100 + " km";
            string totalGas = Math.Round((double)(gas - consumed) * 100) / 100 + " Liter";

            // Draw the speedometer
            RAGE.Game.UIText.Draw("Kraftstoff: ", new Point(1025, 560), 0.5f, Color.White, RAGE.Game.Font.ChaletComprimeCologne, false);
            RAGE.Game.UIText.Draw(totalGas, new Point(1175, 560), 0.5f, Color.White, RAGE.Game.Font.ChaletComprimeCologne, false);
            RAGE.Game.UIText.Draw("Kilometerstand: ", new Point(1025, 590), 0.5f, Color.White, RAGE.Game.Font.ChaletComprimeCologne, false);
            RAGE.Game.UIText.Draw(totalKms, new Point(1175, 590), 0.5f, Color.White, RAGE.Game.Font.ChaletComprimeCologne, false);
            RAGE.Game.UIText.Draw("km/h: ", new Point(1025, 650), 0.5f, Color.White, RAGE.Game.Font.ChaletComprimeCologne, false);
            RAGE.Game.UIText.Draw(speed.ToString(), new Point(1175, 650), 0.75f, Color.White, RAGE.Game.Font.ChaletComprimeCologne, false);
            RAGE.Game.UIText.Draw("Zustand: ", new Point(1025, 620), 0.5f, Color.White, RAGE.Game.Font.ChaletComprimeCologne, false);

            if (healthPercent < 30)
            {
                RAGE.Game.UIText.Draw(healthPercent + "%", new Point(1175, 620), 0.5f, Color.Red, RAGE.Game.Font.ChaletComprimeCologne, false);
            }
            else if (healthPercent < 60)
            {
                RAGE.Game.UIText.Draw(healthPercent + "%", new Point(1175, 620), 0.5f, Color.Yellow, RAGE.Game.Font.ChaletComprimeCologne, false);
            }
            else
            {
                RAGE.Game.UIText.Draw(healthPercent + "%", new Point(1175, 620), 0.5f, Color.White, RAGE.Game.Font.ChaletComprimeCologne, false);
            }

            // Update the vehicle's values
            kms += distance;
            gas -= consumed;

            // Reinitialize the variables
            distance = 0.0f;
            consumed = 0.0f;
        }

        private void InitializeSpeedometerEvent(object[] args)
        {
            // Initialize the kilometers and gas
            kms = (float)Convert.ToDouble(args[0]);
            gas = (float)Convert.ToDouble(args[1]);

            // Don't let the player turn the engine on if by default
            Player.LocalPlayer.Vehicle.SetEngineOn(Convert.ToBoolean(args[2]), true, true);

            // Initialize the counters
            distance = 0.0f;
            consumed = 0.0f;
            lastPosition = Player.LocalPlayer.Vehicle.Position;
        }

        private void LocateVehicleEvent(object[] args)
        {
            // Get the variables from the array
            Vector3 position = (Vector3)args[0];

            // Create the blip on the map
            vehicleLocationBlip = new Blip(1, position, string.Empty, 1, 1);
        }

        private void DeleteVehicleLocationEvent(object[] args)
        {
            // Destroy the blip on the map
            vehicleLocationBlip.Destroy();
            vehicleLocationBlip = null;
        }

        private void PlayerLeaveVehicleEvent()
        {
            if (lastPosition != null)
            {
                // Reset the vehicle's position
                lastPosition = null;

                // Save the kilometers and gas
                Events.CallRemote("saveVehicleConsumes", Player.LocalPlayer.Vehicle, kms, gas);
            }
        }
    }
}
