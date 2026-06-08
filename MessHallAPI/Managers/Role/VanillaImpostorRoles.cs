using MessHallAPI.Managers.Role;
using UnityEngine;

namespace MessHallAPI.Managers.Role.Vanilla.Impostor
{
    [VanillaRole]
    public class Impostor : IVanillaRole
    {
        public string RoleName => "Impostor";
        public string RoleDesc => "Kills Crewmates and sabotages the ship.\nTries not to get caught.";
        public string RoleTeam => "Impostor";
        public Sprite RoleIcon => ModStorage.ImpostorIcon;
    }

    [VanillaRole]
    public class Wraith : IVanillaRole
    {
        public string RoleName => "Wraith";
        public string RoleDesc => "Once dead, Wraiths haunt and kill Crewmates.";
        public string RoleTeam => "Impostor";
        public Sprite RoleIcon => ModStorage.WraithIcon;
    }
}