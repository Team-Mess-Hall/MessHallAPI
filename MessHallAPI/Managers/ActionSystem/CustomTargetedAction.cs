using SG.Airlock.Roles;
using UnityEngine;
using static MessHallAPI.Managers.KeybindManager;

namespace MessHallAPI.Managers.ActionSystem
{
    public abstract class CustomTargetedAction : ITargetedActionHandler
    {
        public abstract string ActionName { get; }
        public abstract ProximityTargetedAction action { get; }
        public abstract void OnUseTarget(int target);

        public virtual Sprite ActionIcon => null;
        public virtual string Keybind => qKey;
        public virtual int Cooldown => 30;
        public virtual bool isMeetingAction => false;
        public virtual bool IsEnabled() => false;
        public virtual void OnUpdate() { }
    }
}