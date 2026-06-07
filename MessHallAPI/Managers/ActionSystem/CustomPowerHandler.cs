using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Graphics;
using Il2CppSG.Airlock.Roles;
using MessHallAPI.Debugger;
using MessHallAPI.Networking;
using UnityEngine;
using static MessHallAPI.Base.References;
using static MessHallAPI.Config.Settings;

namespace MessHallAPI.Managers.ActionSystem
{
    public static class CustomPowerHandler
    {
        private const int VanillaPowerMax = 128;

        [MessHallRPC(RPCTarget.Host, RPCCaller.Anyone)]
        public static void RPC_UsePowerTarget(int power, int PlayerID, int TargetID)
        {
            if (!IsCustomPower(power))
            {
                Logging.Log("CustomPowerHandler: Power is not a custom one, ignoring.");
                return;
            }

            PowerRegistration.DispatchTarget((PowerUps)power, PlayerID, TargetID);
        }

        [MessHallRPC(RPCTarget.Host, RPCCaller.Anyone)]
        public static void RPC_UsePower(int power, int CallerID)
        {
            if (!IsCustomPower(power))
            {
                Logging.Log("CustomPowerHandler: Power is not a custom one, ignoring.");
                return;
            }

            PowerRegistration.Dispatch((PowerUps)power, CallerID);
        }

        public static bool IsCustomPower(int power) => power > VanillaPowerMax;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PowerUpDefinition : Attribute
    {
        public PowerUpDefinition() { }
    }

    public interface IPowerUpHandler
    {
        PowerUp Definition { get; }
        string PowerName { get; }
        void OnUse();
        void OnUpdate() { }
        Sprite PowerIcon { get; }
        string Keybind { get; }
    }
    public interface ITargetedPowerHandler
    {
        PowerUp Definition { get; }
        string PowerName { get; }
        void OnUpdate() { }
        void OnUseTarget(int Target);
        Sprite PowerIcon { get; }
        string Keybind { get; }
    }
}