using System;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace Server
{
    public class Server : BaseScript
    {
        private static int _cooldownTime = 900;
        private static bool _cooldown = false;
        public Server()
        {
            EventHandlers["Technician.JobStarted"] += new Action(Timer);
            EventHandlers["Technician.GetCooldownBool"] += new Action(GetCooldownBool);
            EventHandlers["Technician.Remaining"] += new Action(GetRemaining);
        }

        private static async void Timer()
        {
            _cooldown = true;
            while (_cooldownTime > 0)
            {
                _cooldownTime -= 1;
                await Delay(1000);
                if (_cooldownTime == 0)
                {
                    _cooldown = false;
                    TriggerClientEvent("Technician.Cooldown", _cooldown);
                    _cooldownTime = 900;
                    break;
                }
            }
        }

        private static void GetRemaining()
        {
            TriggerClientEvent("Technician.Remaining", _cooldownTime);
        }

        private static void GetCooldownBool()
        {
            TriggerClientEvent("Technician.Cooldown", _cooldown);
        }
    }
}