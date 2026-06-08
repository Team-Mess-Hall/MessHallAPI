using HarmonyLib;
using MessHallAPI.Base;
using SG.Airlock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessHallAPI.Patches
{
    [HarmonyPatch(typeof(AirlockBootstrap), nameof(AirlockBootstrap.Main))]
    public class BootstrapPatch
    {
        public static void Prefix(AirlockBootstrap __instance)
        {
            Core.OnInit();
        }
    }
}
