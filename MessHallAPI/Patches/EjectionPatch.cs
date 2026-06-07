using HarmonyLib;
using Il2CppFusion;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Localization;
using Il2CppSG.Airlock.Roles;
using MessHallAPI.Base;
using MessHallAPI.Config;
using MessHallAPI.Debugger;
using MessHallAPI.Managers.Role;
using MessHallAPI.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessHallAPI.Patches
{
    [HarmonyPatch(typeof(GameStateManager), nameof(GameStateManager.StartKickAnimation))]
    public class EjectionPatch
    {
        public static void Prefix(GameStateManager __instance, ref GameRole playerEjectedRole, PlayerRef kickedPlayer, int aliveImposters, int aliveCrewmates, bool isVigilanteAlive)
        {
            if (playerEjectedRole == GameRole.NotSet)
            {
                if (Settings.IsHost)
                {
                    if (playerEjectedRole == GameRole.NotSet)
                    {
                        CustomRoleManager.GetTrueRole(kickedPlayer.PlayerId);
                    }
                }

                Logging.DebugLog($"PlayerRole: {playerEjectedRole}, EjectedPlayerID: {kickedPlayer}, AliveImpostors: {aliveImposters}, AliveCrewmates: {aliveCrewmates}, isVigilanteAlive: {isVigilanteAlive}");
                if (CustomRoleManager.TryGetCachedRole(kickedPlayer.PlayerId, out int resolvedRole))
                {
                    playerEjectedRole = (GameRole)resolvedRole;
                    Logging.Log($"[EjectionPatch] Overriding ejected role to: {resolvedRole} for player {kickedPlayer.PlayerId}");
                }
            }
        }

        [HarmonyPatch(typeof(GameStateManager), nameof(GameStateManager.StartVotingResult))]
        public class EjectionPatch2
        {
            public static void Prefix(GameStateManager __instance, ref GameRole playerRole, int bootedPlayer)
            {
                if (playerRole == GameRole.NotSet)
                {
                    if (Settings.IsHost)
                    {
                        CustomRoleManager.GetTrueRole(bootedPlayer);
                    }

                    Logging.DebugLog($"PlayerRole: {playerRole}, EjectedPlayerID: {bootedPlayer}");
                    if (CustomRoleManager.TryGetCachedRole(bootedPlayer, out int resolvedRole))
                    {
                        playerRole = (GameRole)resolvedRole;
                        Logging.Log($"[EjectionPatch] Overriding ejected role to: {resolvedRole} for player {bootedPlayer}");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(VoteManager),nameof(VoteManager.EndVote))]
        public class EjectionPatch3
        {
            public static void Prefix(VoteManager __instance)
            {
                EjectionPatch.EndVoteHandler.EndVote(__instance);
            }
        }

        public class EndVoteHandler
        {
            public static void EndVote(VoteManager voteManager)
            {
                bool flag = !Settings.IsHost;
                if (!flag)
                {
                    foreach (PlayerState playerState in References.Spawn.ActivePlayerStates)
                    {
                        GameRole trueRoleHost = CustomRoleManager.GetTrueRoleHost(playerState.PlayerId);
                        NetworkManager.InvokeRPC("MessHallAPI", "RPC_EndVote", playerState.PlayerId, (int)trueRoleHost);
                    }
                }
            }

            [MessHallRPC(RPCTarget.All, RPCCaller.HostOnly)]
            public static void RPC_EndVote([RPCTarget] int target, int gameRole)
            {
                ValueTuple<RoleData, string, string, string, ICustomRole, TextKey> valueTuple;
                bool flag = !CustomRoleManager._roles.TryGetValue((GameRole)gameRole, out valueTuple);
                if (!flag)
                {
                    valueTuple.Item5.OnEndVote();
                }
            }
        }
    }
}
