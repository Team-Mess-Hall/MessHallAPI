// This is a rough example that will change at any time.
namespace MessHallAPI.Powers
{
    [PowerUpDefinition((PowerUps)256)]
    public class TestKillPower : ITargetedPowerHandler
    {
        public PowerUp Definition => new PowerUp()
        {
            Type = (PowerUps)256,
            Duration = 10,
            IsConsumable = false,
            PlayerFacingName = "test",
            PowerUpVFX = null,
            TargetedAction = ProximityTargetedAction.Kill
        };

        public string PowerName => "test";

        public Sprite PowerIcon => ModStorage.LoadSpriteFromResource("MessHallAPIExample.Assets.ModStamp.png");

        public void OnUseTarget(PlayerState Target) => Killing.KillPlayer(Peer, Target, Client.PState.PlayerId, true);
        public string Keybind => KeybindManager.rKey;
    }
}
