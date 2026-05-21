using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using MessHallAPI.Debugger;
using MessHallAPI.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MessHallAPI.Managers.ActionSystem
{
    public class TargetedActionHandler
    {
        private const int VanillaActionMax = 7;

        [MessHallRPC(RPCTarget.Host, RPCCaller.Anyone)]
        public static void RPC_UseAction(int action, int PlayerID, int TargetID)
        {
            if (!IsCustomAction(action))
            {
                Logging.Log("CustomActionHandler: Action is not a custom one, ignoring.");
                return;
            }

            TargetedActionRegistration.Dispatch((ProximityTargetedAction)action, PlayerID, TargetID);
        }

        public static bool IsCustomAction(int action) => action > VanillaActionMax;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TargetedActionDefinitionAttribiute : Attribute
    {
        public TargetedActionDefinitionAttribiute() { }
    }

    public interface ITargetedActionHandler
    {
        string ActionName { get; }
        Sprite ActionIcon { get; }
        string Keybind { get; }
        int Cooldown { get; }
        ProximityTargetedAction action { get; }
        bool IsEnabled();
        bool isMeetingAction { get; }

        void OnUpdate() { }
        void OnUseTarget(PlayerState target);
    }
}
