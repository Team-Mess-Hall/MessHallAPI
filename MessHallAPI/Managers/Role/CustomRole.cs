using Il2CppSG.Airlock.Roles;
using Il2CppFusion;
using MessHallAPI.Managers.Role;
using UnityEngine;

/// <summary>
/// Base class for all custom roles. Override <see cref="BuildRoleData"/> to
/// change role properties, and any of the event methods to react to game events.
/// </summary>
public abstract class CustomRole : ICustomRole
{
    public abstract string RoleName { get; }
    public abstract string RoleDesc { get; }
    public abstract string RoleRevealPrompt { get; }
    public virtual Sprite RoleIcon => null;

    public virtual RoleData BuildRoleData()
    {
        return new RoleData
        {
            ChanceOfRoleAssignment = null,
            VentIcon = VentIcon.Impostor,

            CanFreelyVent = false,
            CanCallSabotages = false,
            CanDoTasks = true,
            CanUsePowerups = true,

            IsAssignedAtStart = true,
            IsAssignedTasks = true,
            IsUniversalKnowledge = false,

            GestureHandColor = -1,
            MaxNumOfRole = null,
            IsGhostRole = false,

            PlayerSpeedMultiplier = 1f,
            VisionRange = 1f,
            TargetedActionRadius = 2,
            CameraHeight = 1.334f,

            Team = GameTeam.Crewmember,

            _targetedAction = ProximityTargetedAction.None,

            _otherTeamColor = Color.magenta,
            _crewmemberTeamColor = Color.cyan,
            _impostorTeamColor = Color.red,
            _infectedTeamColor = Color.green,

            _ghostTargetedAction = ProximityTargetedAction.None,
            _ghostRoleAssignedSFX = null,
            _ghostTargetedActionCooldown = null,

            _maxTimeInVents = null,
            _selfActions = new Il2CppSystem.Collections.Generic.List<ProximityTargetedAction>(),
            _targetedActionCooldown = null,
            _ventUseCooldown = null
        };
    }

    public virtual void OnEmergencyMeetingCalled(PlayerRef sourcePlayer, NetworkBool forceVote) { }
    public virtual void OnBodyCalled(int foundPlayer, PlayerRef sourcePlayer, NetworkBool forceVote) { }
    public virtual void OnRoleRevealed(GameRole role) { }
    public virtual void OnPlayerWasKilled(PlayerRef victim, PlayerRef killer, bool vigiKill, bool selfKill) { }
    public virtual void OnEndGame(GameTeam winningTeam) { }
    public virtual void OnEndVote() { }
    public virtual void OnTargetedAction() { }
}