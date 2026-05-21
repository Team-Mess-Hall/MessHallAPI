using Il2CppSG.Airlock;
using static MessHallAPI.Managers.KeybindManager;
using UnityEngine;
using static MessHallAPI.Base.References;
using Il2CppSG.Airlock.Roles;
using MessHallAPI.Debugger;

namespace MessHallAPIExample
{
    [TargetedActionDefinitionAttribiute]
    public class Test : CustomTargetedAction
    {
        public override string ActionName => "TestButton";
        public override void OnUseTarget(PlayerState Target) => Killing.KillPlayer(Peer, Client.PState, Client.PState.PlayerId, true);
        public override Sprite ActionIcon => null!;
        public override string Keybind => qKey;
        public override int Cooldown => 10;
        public override ProximityTargetedAction action => ProximityTargetedAction.Kill;
        public override bool isMeetingAction => false;
        public override bool IsEnabled()
        {
            return Client.PState.IsAlive;
        }
    }
}
