using MessHallAPI.Managers.Roles;
using Il2CppSG.Airlock.Roles;

namespace MessHallAPIExample
{
    [RoleDefinition]
    public class TestRole : CustomRole
    {
        public override string RoleName => "RoleName";
        public override string RoleDesc => "Role Description";
        public override string RoleRevealPrompt =>  "Role Reveal Prompt";
        public override RoleData BuildRoleData()
        {
            var data = base.BuildRoleData();

            data.PlayerSpeedMultiplier = 1f;
            data.CanFreelyVent = true;
            data.Team = GameTeam.Other;

            return data;
        }
    }
}
