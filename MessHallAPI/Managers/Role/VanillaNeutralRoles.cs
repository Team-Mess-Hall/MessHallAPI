using MessHallAPI.Managers.Role;
using UnityEngine;

namespace MessHallAPI.Managers.Role.Vanilla.Neutral
{
    [VanillaRole]
    public class Deputy : IVanillaRole
    {
        public string RoleName => "Deputy";
        public string RoleDesc => "Has the sole power to eject players.";
        public string RoleTeam => "Neutral";
        public Sprite RoleIcon => ModStorage.DeputyIcon;
    }

    [VanillaRole]
    public class Infected : IVanillaRole
    {
        public string RoleName => "Infected";
        public string RoleDesc => "Infect others.";
        public string RoleTeam => "Neutral";
        public Sprite RoleIcon => ModStorage.InfectedIcon;
    }
}