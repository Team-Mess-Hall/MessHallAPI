using SG.Airlock;
using SG.Airlock.Roles;
using SG.Airlock.UI;
using MessHallAPI.Base;
using MessHallAPI.Debugger;
using UnityEngine;
using static MessHallAPI.Base.References;

namespace MessHallAPI.Managers.ActionSystem
{
    public class TargetOutlineManager
    {
        public static void OutlineTarget(ProximityTargetedAction powerAction, ProximityTargetedAction action, float Cooldown, UIInteractButton button)
        {
            if (Killing._targetPlayers == null || Killing._targetPlayers.Count == 0)
            {
                return;
            }
            var target = Killing._targetPlayers[0];

            Killing._hasKillTarget = true;
            Killing._drawnTargetID = target.PlayerId;

            try
            {
                Killing.HandleValidTarget(new Il2CppSystem.Nullable<bool>(true), target, powerAction, action, button, Cooldown, Vector3.zero);
            }
            catch (Exception e)
            {
                Logging.Error(e.ToString());
            }
        }
    }
}