using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;

namespace Client
{
    public class Client : BaseScript
    {
        private static Vehicle _veh;
        private static bool _onDuty;
        private static bool _coolDown;
        private static bool _firstTick = true;
        private static int _coolDownTime;
        private static int _index = 0;
        private const float StartHeading = 343f;
        private static readonly Vector3 StartJob = new Vector3(2769.38f, 1402.09f, 23.55f);

        private static readonly List<Vector3> Locations = new List<Vector3>
        {
            new Vector3(2557.46f, 2578.59f, 36.98f),
            new Vector3(2295.41f, 2944.09f, 46.58f),
            new Vector3(2052.17f, 3688.94f, 34.59f),
            new Vector3(3438.59f, 3749.54f, 30.51f),
            new Vector3(2585.82f, 5066.15f, 44.92f),
            new Vector3(2232.99f, 6399.57f, 31.62f),
            new Vector3(-2542.85f, 2301.51f, 33.21f),
            new Vector3(750.23f, 1273.85f, 360.3f),
            new Vector3(1573.58f, 851.63f, 77.48f)
        };

        private static List<Blip> blipList = new List<Blip>();

        public Client()
        {
            var blip = World.CreateBlip(StartJob);
            blip.Name = "Power Technician Job";
            blip.IsShortRange = true;
            blip.Sprite = (BlipSprite) 544;
            blip.Color = BlipColor.TrevorOrange;
            EventHandlers["Technician.Cooldown"] += new Action<bool>(SetCooldown);
            EventHandlers["Technician.Remaining"] += new Action<int>(SetRemaining);
            Tick += OnTick;
            Tick += HandleJob;
        }

        private static void CreateMarker(MarkerType type, Vector3 location)
        {
            World.DrawMarker(type, location, Vector3.Zero,
                new Vector3(0f, 0.5f, 0f), Vector3.One * 5f, Color.FromArgb(200, 200, 200));
        }

        private static void SetCooldown(bool cooldown)
        {
            _coolDown = cooldown;
        }

        private static void SetRemaining(int cooldown)
        {
            _coolDownTime = cooldown;
        }

        private static async Task HandleSpawning()
        {
            TriggerServerEvent("Technician.GetCooldownBool");
            if (!_onDuty && !_coolDown)
            {
                var vehicle = await World.CreateVehicle(new Model(VehicleHash.UtilliTruck2), StartJob, StartHeading);
                vehicle.ToggleExtra(1, false);
                vehicle.ToggleExtra(2, false);
                vehicle.ToggleExtra(3, true);
                vehicle.ToggleExtra(4, true);
                vehicle.ToggleExtra(5, false);
                vehicle.ToggleExtra(6, true);
                Game.PlayerPed.Task.WarpIntoVehicle(vehicle, VehicleSeat.Driver);
                _veh = vehicle;
                _onDuty = true;
                TriggerServerEvent("Technician.JobStarted");
            }
            else if (_veh.Position.DistanceToSquared(StartJob) < 10f)
            {
                Game.PlayerPed.Task.LeaveVehicle(_veh, true);
                await Delay(2500);
                _veh.Delete();
                _onDuty = false;
            }
            else if (_coolDown)
            {
                TriggerServerEvent("Technician.Remaining");
                Screen.ShowNotification($"You must wait {_coolDownTime} seconds before going back on duty.");
            }
            else
            {
                Screen.ShowNotification($"Your vehicle must be close to the garage!");
            }
        }

        private static async Task HandleJob()
        {
            if (_onDuty)
            {
                TriggerEvent("chatMessage", "Now on duty");
                await Delay(0);
                if (_firstTick)
                {
                    TriggerEvent("chatMessage", "In first tick");
                    blipList.Clear();
                    foreach (var loc in Locations)
                    {
                        var blip = World.CreateBlip(loc);
                        blip.Color = BlipColor.White;
                        blip.IsShortRange = true;
                        blip.Sprite = (BlipSprite)544;
                        blipList.Add(blip);
                        await Delay(0);
                    }
                    TriggerEvent("chatMessage", "Created blips");
                    _firstTick = false;
                }

                var currentJob = blipList[_index].Position;
                await HandleOnSite(currentJob);
                _index++;
                if (_index > blipList.Count)
                {

                }
            }
        }

        private static async Task HandleOnSite(Vector3 currentJob)
        {
            TriggerEvent("chatMessage", "In handle on site");
            Function.Call(Hash.SET_NEW_WAYPOINT, currentJob.X, currentJob.Y);
            CreateMarker(MarkerType.ChevronUpx1, currentJob);
            while (true)
            {
                
                await Delay(0);
                var distance = Game.PlayerPed.Position.DistanceToSquared(currentJob);
                await Delay(0);
                if (distance < 5)
                {
                    TriggerEvent("chatMessage", "Within 2.5 meters");
                    await Delay(0);
                    Screen.DisplayHelpTextThisFrame("Press stuff to do stuff");
                    if (Game.IsControlJustPressed(0, Control.Context))
                    {
                        TriggerEvent("chatMessage", "Pressed E");
                        blipList.RemoveAt(_index);
                        await Delay(0);
                        break;
                    }
                }
            }
        }

        private static async Task OnTick()
        {
            try
            {
                var distance = Game.PlayerPed.Position.DistanceToSquared(StartJob);
                if (distance < 400)
                {
                    CreateMarker(MarkerType.HorizontalCircleFat, StartJob);
                    if (distance < 5)
                    {
                        Screen.DisplayHelpTextThisFrame($"Press ~INPUT_CONTEXT~ to {(_onDuty ? "store your" : "retrieve a")} ~g~Utility Truck~s~");
                        if (Game.IsControlJustPressed(0, Control.Context))
                        {
                            await HandleSpawning();
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
