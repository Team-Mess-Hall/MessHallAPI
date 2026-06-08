using HarmonyLib;
using SG.Airlock;
using SG.Airlock.Roles;
using MessHallAPI.Config;
using MessHallAPI.Debugger;
using MessHallAPI.Managers.Role;
using MessHallAPI.Networking;
using System.Collections;
using UnityEngine;
using static MessHallAPI.Base.References;

namespace MessHallAPI.Patches
{
    [HarmonyPatch(typeof(GameStateManager), nameof(GameStateManager.StartGame))]
    public class GameStartPatch
    {
        public static void Prefix()
        {
            if (Settings.IsHost)
            { 
                if (roleManager?.gameRoleToPlayerIds == null) return;

                foreach (var (role, _) in CustomRoleManager._roles)
                {
                    if (!roleManager.gameRoleToPlayerIds.ContainsKey(role))
                    {
                        roleManager.gameRoleToPlayerIds[role] = new Il2CppSystem.Collections.Generic.List<int>();
                        Logging.Log($"CustomRoleManager: Re-injected '{role}' into gameRoleToPlayerIds");
                    }
                }
            }
        }

        public static void Postfix()
        {
            if (Settings.IsHost)
            {
                SendRoles();
            }
        }

        private static void SendRoles()
        {
            foreach (var kvp in roleManager.gameRoleToPlayerIds)
            {
                GameRole role = kvp.Key;
                foreach (int playerId in kvp.Value)
                {
                    NetworkManager.InvokeRPC("MessHallAPI", "RPC_SendRole", playerId, (int)role);
                }
            }
        }
    }
}
