using static MessHallAPI.Managers.KeybindManager;

namespace MessHallAPIExample
{
    [PowerUpDefinition]
    public class Test2 : CustomPower
    {
        public override string PowerName => "test2";
        public override string Keybind => base.Keybind;
        public override Sprite PowerIcon => ModStorage.ModStamp;
        public override void OnUse()
        {
            Logging.Log("PowerUsed");
            return;
        }
    }

    [PowerUpDefinition]
    public class Test3 : CustomTargetedPower
    {
        public override string PowerName => "test3";
        public override string Keybind => base.Keybind;
        public override Sprite PowerIcon => ModStorage.ModStamp;
        public override void OnUseTarget(PlayerState target)
        {
            Logging.Log("PowerUsed");
            return;
        }
    }
}
