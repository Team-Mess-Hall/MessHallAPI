using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.PlayerLoop;
using static Custom3DPanelManager;
using static MessHallAPI.Base.References;

namespace MessHallAPIExample.Managers.CustomPanels3D
{
    [PanelDefinition]
    public class ExampleCustomPanel : CustomPanel
    {
        public override string PanelName => "Test";
        public override string Keybind => KeybindManager.rKey;
        public override PanelOpenTrigger OpenTrigger => PanelOpenTrigger.Keybind;
        public override void OnPlayerSelected(GameObject panel, PlayerState playerIndex)
        {
            playerIndex.RPC_ShowDeathAnim(playerIndex.PlayerId, playerIndex.PlayerId, false, false);
        }
    }
}
