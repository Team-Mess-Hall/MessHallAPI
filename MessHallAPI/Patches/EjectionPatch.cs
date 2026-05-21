using HarmonyLib;
using Il2CppFusion;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
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
    [HarmonyPatch(typeof(GameStateManager),nameof(GameStateManager.StartKickAnimation))]
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

                Logging.Log($"PlayerRole: {playerEjectedRole}, EjectedPlayerID: {kickedPlayer}, AliveImpostors: {aliveImposters}, AliveCrewmates: {aliveCrewmates}, isVigilanteAlive: {isVigilanteAlive}");
                if (CustomRoleManager.TryGetCachedRole(kickedPlayer.PlayerId, out int resolvedRole))
                {
                    playerEjectedRole = (GameRole)resolvedRole;
                    Logging.Log($"[EjectionPatch] Overriding ejected role to: {resolvedRole} for player {kickedPlayer.PlayerId}");
                }
            }
        }
    }
}
