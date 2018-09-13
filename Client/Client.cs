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
        private static bool onDuty = false;
        private const float StartHeading = 343f;
        private static readonly Vector3 StartJob = new Vector3(2769.38f, 1402.09f, 23.55f);

        private static readonly List<Vector3> Locations = new List<Vector3>
        {
            new Vector3(2455.32f, 1603.83f, 32.73f),
            new Vector3(2557.46f, 2578.59f, 37.95f),
            new Vector3(2295.41f, 2944.09f, 46.58f),
            new Vector3(2052.17f, 3688.94f, 34.59f),
            new Vector3(3438.59f, 3749.54f, 30.51f),
            new Vector3(2585.82f, 5066.15f, 44.92f),
            new Vector3(2232.99f, 6399.57f, 31.62f),
            new Vector3(-2542.85f, 2301.51f, 33.21f),
            new Vector3(750.23f, 1273.85f, 360.3f),
            new Vector3(1573.58f, 851.63f, 77.48f),
        };

        public Client()
        {
            Tick += OnTick;
        }

        private static async Task HandleTechnician()
        {
            if (!onDuty)
            {
                onDuty = true;
                var vehicle = await World.CreateVehicle(new Model(VehicleHash.Boxville), StartJob, StartHeading);
                //var vehicle = Function.Call<Vehicle>(Hash.CREATE_VEHICLE, 3078201489, StartJob.X, StartJob.Y + 5f, StartJob.Z, StartHeading, true, false);
                Game.PlayerPed.Task.EnterVehicle(vehicle, VehicleSeat.Driver, 5000);
            }
            else
            {
                Screen.DisplayHelpTextThisFrame($"You may only rent one ~g~Utility Truck~s~ at a time!");
            }
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
                        Screen.DisplayHelpTextThisFrame($"Press ~INPUT_CONTEXT~ to retrieve a ~g~Utility Truck~s~");
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
