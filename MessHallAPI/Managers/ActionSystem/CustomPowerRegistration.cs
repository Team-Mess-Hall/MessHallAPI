using SG.Airlock;
using SG.Airlock.Roles;
using MessHallAPI.Debugger;
using System;
using System.Collections.Generic;

namespace MessHallAPI.Managers.ActionSystem
{
    /// <summary>
    /// Lets callers retrieve the <see cref="PowerUps"/> enum value that was
    /// allocated for a given <see cref="CustomPower"/> or
    /// <see cref="CustomTargetedPower"/> subclass.
    ///
    /// Usage:
    ///   PowerUps p = CustomPowerRegistration.GetPower&lt;Notes&gt;();
    /// </summary>
    public static class CustomPowerRegistration
    {
        // Populated by PowerRegistration.AutoRegister() when it sets AllocatedType
        private static readonly Dictionary<Type, PowerUps> _typeTopower = new();

        /// <summary>Called by PowerRegistration after it allocates a power.</summary>
        internal static void Track(Type handlerType, PowerUps allocated)
        {
            _typeTopower[handlerType] = allocated;
        }

        /// <summary>Returns the allocated PowerUps value for <typeparamref name="T"/>.</summary>
        public static PowerUps GetPower<T>() where T : class
        {
            if (_typeTopower.TryGetValue(typeof(T), out var power))
                return power;

            Logging.Error($"[CustomPowerRegistration] No power registered for {typeof(T).Name}.");
            return PowerUps.None;
        }
    }
}