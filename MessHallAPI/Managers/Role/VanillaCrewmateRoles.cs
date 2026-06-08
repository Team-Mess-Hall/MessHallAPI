using MessHallAPI.Managers.Role;
using UnityEngine;

namespace MessHallAPI.Managers.Role.Vanilla.Crewmember
{
    [VanillaRole]
    public class Engineer : IVanillaRole
    {
        public string RoleName => "Engineer";
        public string RoleDesc => "Engineers can use the vents. They can't stay in there\nforever though.";
        public string RoleTeam => "Crewmate";
        public Sprite RoleIcon => ModStorage.EngineerIcon;
    }

    [VanillaRole]
    public class GuardianAngel : IVanillaRole
    {
        public string RoleName => "Guardian Angel";
        public string RoleDesc => "Protects living players with a temporary shield.\nThe first Crewmates to die become Guardian Angels.";
        public string RoleTeam => "Crewmate";
        public Sprite RoleIcon => ModStorage.GuardianAngelIcon;
    }

    [VanillaRole]
    public class Scanner : IVanillaRole
    {
        public string RoleName => "Scanner";
        public string RoleDesc => "Scans others to find Impostors.";
        public string RoleTeam => "Crewmate";
        public Sprite RoleIcon => ModStorage.ScannerIcon;
    }

    [VanillaRole]
    public class Tracker : IVanillaRole
    {
        public string RoleName => "Tracker";
        public string RoleDesc => "Plants trackers on players. Watches player movement\naround the map.";
        public string RoleTeam => "Crewmate";
        public Sprite RoleIcon => ModStorage.TrackerIcon;
    }

    [VanillaRole]
    public class Vigilante : IVanillaRole
    {
        public string RoleName => "Vigilante";
        public string RoleDesc => "Vigilantes can take justice into their own hands.\nKill whoever is suspicious.";
        public string RoleTeam => "Crewmate";
        public Sprite RoleIcon => ModStorage.VigilanteIcon;
    }
}