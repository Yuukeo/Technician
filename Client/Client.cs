using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.NaturalMotion;
using CitizenFX.Core.UI;

namespace Client
{
    public class Client : BaseScript
    {
        private static Vehicle _veh;
        private static bool _onDuty = false;
        private const float StartHeading = 343f;
        private static readonly Vector3 StartJob = new Vector3(2769.38f, 1402.09f, 23.55f);

        private static readonly List<Vector3> Locations = new List<Vector3>
        {
            new Vector3(2557.46f, 2578.59f, 37.95f),
            new Vector3(2295.41f, 2944.09f, 46.58f),
            new Vector3(2052.17f, 3688.94f, 34.59f),
            new Vector3(3438.59f, 3749.54f, 30.51f),
            new Vector3(2585.82f, 5066.15f, 44.92f),
            new Vector3(2232.99f, 6399.57f, 31.62f),
            new Vector3(-2542.85f, 2301.51f, 33.21f),
            new Vector3(750.23f, 1273.85f, 360.3f),
            new Vector3(1573.58f, 851.63f, 77.48f),
            new Vector3(2455.32f, 1603.83f, 32.73f),
        };

        private static List<Blip> blipList = new List<Blip>();

        public Client()
        {
            Tick += OnTick;
        }

        private static async Task HandleTechnician()
        {
            if (!_onDuty)
            {
                var vehicle = await World.CreateVehicle(new Model(VehicleHash.Boxville), StartJob, StartHeading);
                vehicle.ToggleExtra(2, false);
                vehicle.ToggleExtra(3, false);
                vehicle.ToggleExtra(4, true);
                Game.PlayerPed.Task.WarpIntoVehicle(vehicle, VehicleSeat.Driver);
                _veh = vehicle;
                _onDuty = !_onDuty;
                 await HandleWaypoints();
            }
            else
            {
                Game.PlayerPed.Task.LeaveVehicle(_veh, true);
                await Delay(2500);
                _veh.Delete();
                _onDuty = !_onDuty;
            }
        }

        private static async Task HandleWaypoints()
        {
            var player = Game.PlayerPed;
            foreach (var loc in Locations)
            {
                var blip = World.CreateBlip(loc);
                blip.Color = BlipColor.White;
                blip.IsShortRange = true;
                blip.Sprite = (BlipSprite) 544;
                blipList.Add(blip);
                await Delay(10);
            }

            int i = 0;
            while (_onDuty)
            {
                var currentLocation = Locations[i];
                API.SetNewWaypoint(currentLocation.X, currentLocation.Y);
                if (player.Position.DistanceToSquared(currentLocation) < 16f)
                {
                    await HandleJob(currentLocation);
                }
                i++;
            }
        }

        private static async Task HandleJob(Vector3 currentLocation)
        {
            World.DrawMarker(MarkerType.ChevronUpx3, currentLocation, Vector3.Zero, new Vector3(0f, 0.5f, 0f), Vector3.One * 5f, Color.FromArgb(255, 0, 100));
        }

        private static async Task OnTick()
        {
            try
            {
                var distance = Game.PlayerPed.Position.DistanceToSquared(StartJob);
                if (distance < 400)
                {
                    World.DrawMarker(MarkerType.HorizontalCircleFat, StartJob, Vector3.Zero, new Vector3(0f, 0.5f, 0f), Vector3.One * 5f, Color.FromArgb(0, 255, 255));
                    if (distance < 5f)
                    {
                        Screen.DisplayHelpTextThisFrame($"Press ~INPUT_CONTEXT~ to {(_onDuty ? "store your" : "retrieve a")} ~g~Utility Truck~s~");
                        if (Game.IsControlJustPressed(0, Control.Context))
                        {
                            await HandleTechnician();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
