using Fusion;
using HarmonyLib;
using SG.Airlock.Network;
using MessHallAPI.Base;
using MessHallAPI.Config;
using MessHallAPI.Debugger;
using MessHallAPI.Managers.ActionSystem;
using MessHallAPI.Managers.Role;
using MessHallAPI.Managers.RoleSettings;
using MessHallAPI.Networking;
using SG.Airlock.Network;
using System.Collections;
using UnityEngine;

namespace MessHallAPI.Patches
{
    internal class OnPlayerJoinedPatch
    {
        public static readonly Dictionary<int, string> ReliableKeys = new();
        public static readonly HashSet<int> Confirmed = new();

        public static bool TryGetKey(int id, out string key)
        {
            return ReliableKeys.TryGetValue(id, out key);
        }

        [HarmonyPatch(typeof(NetworkedLocomotionPlayer), nameof(NetworkedLocomotionPlayer.RPC_SpawnInitialization))]
        private static class Patch
        {
            [HarmonyPostfix]
            private static void Postfix(NetworkedLocomotionPlayer __instance)
            {
                int playerId = __instance.PState.PlayerId;

                if (playerId == References.Client.PState.PlayerId)
                {
                    PowerRegistration.BuildIcons();
                    TargetedActionRegistration.BuildIcons();
                    CustomRoleManager.FlushRoles();
                    SettingsManager.BuildSettingsPages();
                    Custom3DPanelManager.FlushPanels();
                }

                if (!Settings.IsHost)
                    return;

                string key = Guid.NewGuid().ToString();

                ReliableKeys[playerId] = key;

                if (playerId == References.networkRunner.LocalPlayer)
                {
                    RPCRegistry.ReliableKey = key;
                    Confirmed.Add(playerId);
                    return;
                }

                CoreBehaviour.Instance.StartCoroutine(SendKeyLoop(playerId).ToString());
            }
        }

        private static IEnumerator SendKeyLoop(int playerId)
        {
            int attempts = 0;

            yield return new WaitForSeconds(0.5f);

            while (!Confirmed.Contains(playerId) && attempts < 15)
            {
                if (!ReliableKeys.TryGetValue(playerId, out var key))
                    yield break;

                Logging.DebugLog($"send key to {playerId}");

                NetworkManager.InvokeRPC("MessHallAPI", "RPC_ExchangeKeys", playerId, key);

                attempts++;

                yield return new WaitForSeconds(1.5f);
            }

            if (!Confirmed.Contains(playerId))
            {
                Logging.Warn($"key exchange failed for {playerId}");

                if (!NetworkManager.AllowUnregisteredPlayers)
                {
                    References.networkRunner.Disconnect((PlayerRef)playerId);
                }
            }
        }

        [MessHallRPC(RPCTarget.All, RPCCaller.HostOnly)]
        public static void RPC_ExchangeKeys([RPCTarget] int target, string key)
        {
            int localId = References.Client.PState.PlayerId;

            if (localId != target)
                return;

            ReliableKeys[localId] = key;
            RPCRegistry.ReliableKey = key;

            Logging.DebugLog("received key");

            NetworkManager.InvokeRPC("MessHallAPI", "RPC_KeyReceived");
        }

        [MessHallRPC(RPCTarget.Host, RPCCaller.Anyone)]
        public static void RPC_KeyReceived([RPCInfo] MessHallRpcInfo info)
        {
            if (!Settings.IsHost)
                return;

            if (!ReliableKeys.ContainsKey(info.Sender))
                return;

            Confirmed.Add(info.Sender);

            Logging.DebugLog($"key exchange confirmed from {info.Sender}");
        }
    }
}