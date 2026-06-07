using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using UnityEngine;
using static MessHallAPI.Managers.KeybindManager;

namespace MessHallAPI.Managers.ActionSystem
{
    public abstract class CustomPower : IPowerUpHandler
    {
        internal PowerUps AllocatedType { get; set; }

        public abstract string PowerName { get; }
        public abstract void OnUse();

        public virtual Sprite PowerIcon => null;
        public virtual string Keybind => eKey;
        public virtual void OnUpdate() { }

        public virtual PowerUp Definition => new PowerUp
        {
            Type = AllocatedType,
            Duration = 0,
            IsConsumable = true,
            PlayerFacingName = PowerName,
            PowerUpVFX = null,
            TargetedAction = ProximityTargetedAction.None
        };
    }

    public abstract class CustomTargetedPower : ITargetedPowerHandler
    {
        internal PowerUps AllocatedType { get; set; }

        public abstract string PowerName { get; }
        public abstract void OnUseTarget(int target);

        public virtual Sprite PowerIcon => null;
        public virtual string Keybind => eKey;
        public virtual void OnUpdate() { }

        public virtual PowerUp Definition => new PowerUp
        {
            Type = AllocatedType,
            Duration = 0,
            IsConsumable = true,
            PlayerFacingName = PowerName,
            PowerUpVFX = null,
            TargetedAction = ProximityTargetedAction.Kill
        };
    }
}