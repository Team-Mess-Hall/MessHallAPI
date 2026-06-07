using System;
using Il2CppSG.Airlock.Roles;
using MessHallAPI.Debugger;
using MessHallAPI.Networking;
using UnityEngine;

namespace MessHallAPI.Managers.ActionSystem
{
    public static class TargetedActionHandler
    {
        private const int VanillaActionMax = 7;

        [MessHallRPC(RPCTarget.Host, RPCCaller.Anyone)]
        public static void RPC_UseAction(int action, int playerID, int targetID)
        {
            if (!IsCustomAction(action))
            {
                Logging.Log("[TargetedActionHandler] Action is not a custom one, ignoring.");
                return;
            }

            TargetedActionRegistration.Dispatch((ProximityTargetedAction)action, playerID, targetID);
        }

        public static bool IsCustomAction(int action) => action > VanillaActionMax;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TargetedActionDefinitionAttribiute : Attribute { }

    public interface ITargetedActionHandler
    {
        string ActionName { get; }
        Sprite ActionIcon { get; }
        string Keybind { get; }
        int Cooldown { get; }
        ProximityTargetedAction action { get; }
        bool isMeetingAction { get; }
        bool IsEnabled();
        void OnUpdate() { }
        void OnUseTarget(int target);
    }
}