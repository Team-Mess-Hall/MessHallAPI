using MelonLoader;
using MessHallAPI.Debugger;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MessHallAPI.Managers.Role
{
    public static class VanillaRoleManager
    {
        public static readonly List<IVanillaRole> _roles = new();

        public static void AutoRegisterRoles()
        {
            foreach (MelonMod mod in MelonTypeBase<MelonMod>.RegisteredMelons)
            {
                foreach (Type type in mod.GetType().Assembly.GetTypes())
                {
                    if (type.GetCustomAttribute<VanillaRoleAttribute>() == null) continue;

                    if (!typeof(IVanillaRole).IsAssignableFrom(type))
                    {
                        Logging.Warn($"{type.Name} has [VanillaRole] but does not implement IVanillaRole.");
                        continue;
                    }

                    var role = (IVanillaRole)Activator.CreateInstance(type);
                    _roles.Add(role);
                    Logging.Log($"VanillaRoleManager: Registered {role.RoleName}");
                }
            }
        }

        public static IReadOnlyList<IVanillaRole> GetRegisteredRoles() => _roles;
    }

    public interface IVanillaRole
    {
        string RoleName { get; }
        string RoleDesc { get; }
        string RoleTeam { get; }
        Sprite RoleIcon { get; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class VanillaRoleAttribute : Attribute { }

}