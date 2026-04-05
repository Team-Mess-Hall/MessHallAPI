using Il2CppSG.Airlock;
using MessHallAPI.Debugger;
using MessHallAPI.Networking;
using UnityEngine;
using static MessHallAPI.Base.References;
using static MessHallAPI.Config.Settings;

namespace MessHallAPI.Managers.Cosmetic
{
    public class CustomVisorManager
    {
        private const string VisorPath = "CrewmatePhysics/Visuals/Player_Crewmate/SK_Char_CrewmateHandless_01/WorldJoint/spine1_loResSpine1/spine1_loResSpine2/spine1_loResSpine3/head1_neck/head1_head/head1_visor";

        private static Dictionary<string, Dictionary<string, GameObject>> _visorRegistry = new();
        private static Dictionary<int, (string modId, string visorId)> _playerVisors = new();
        private static Dictionary<int, GameObject> _activeVisors = new();

        public static GameObject FindVRHat()
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == "SM_HatVR_Black_01" && go.transform.parent == null)
                {
                    return go;
                }
            }
            return null;
        }

        public static GameObject FindGasMask()
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == "SM_HazmatMask_01" && go.transform.parent == null)
                {
                    return go;
                }
            }
            return null;
        }

        public static GameObject FindDeluxeScanner()
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == "SM_ScanalyzerDeluxe_01" && go.transform.parent == null)
                {
                    return go;
                }
            }
            return null;
        }

        public static GameObject FindScientistGoggles()
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == "SM_ScientistGoggles_01" && go.transform.parent == null)
                {
                    return go;
                }
            }
            return null;
        }

        public static GameObject FindDumSticker()
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == "SM_Hat_Note_2_Self" && go.transform.parent == null)
                {
                    return go;
                }
            }
            return null;
        }

        public static GameObject FindGreatGoalie()
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == "SM_Hat_GreatGoalie_01" && go.transform.parent == null)
                {
                    return go;
                }
            }
            return null;
        }

        public static GameObject FindScanner()
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == "SM_Scanalyzer_01" && go.transform.parent == null)
                {
                    return go;
                }
            }
            return null;
        }

        public static void RegisterVisor(string modId, string visorId, GameObject prefab)
        {
            if (!_visorRegistry.ContainsKey(modId))
                _visorRegistry[modId] = new();
            _visorRegistry[modId][visorId] = prefab;
        }

        public static GameObject? GetRegisteredPrefab(string modId, string visorId)
        {
            if (!_visorRegistry.TryGetValue(modId, out var visors)) return null;
            visors.TryGetValue(visorId, out var prefab);
            return prefab;
        }

        public static List<KeyValuePair<(string, string), GameObject>> GetAllRegistered()
        {
            var result = new List<KeyValuePair<(string, string), GameObject>>();
            foreach (var mod in _visorRegistry)
                foreach (var visor in mod.Value)
                    result.Add(new KeyValuePair<(string, string), GameObject>((mod.Key, visor.Key), visor.Value));
            return result;
        }

        public static bool PlayerHasVisor(int playerId) => _playerVisors.ContainsKey(playerId);

        [MessHallRPC(RPCTarget.AllInclusive, RPCCaller.Anyone)]
        public static void RPC_SetVisor(int playerId, string modId, string visorId)
        {
            if (_activeVisors.TryGetValue(playerId, out var old))
            {
                GameObject.Destroy(old);
                _activeVisors.Remove(playerId);
            }

            var prefab = GetRegisteredPrefab(modId, visorId);
            if (prefab == null) return;

            Transform? visorBone = null;
            foreach (PlayerState player in Spawn.ActivePlayerStates)
            {
                if (player.PlayerId != playerId) continue;
                visorBone = player.LocomotionPlayer.transform.Find(VisorPath);
                break;
            }

            if (visorBone == null) return;

            var instance = GameObject.Instantiate(prefab);
            instance.transform.SetParent(visorBone, false);
            instance.transform.localPosition = new Vector3(0, 0.05f, -0.225f);
            instance.transform.localRotation = Quaternion.identity;

            _activeVisors[playerId] = instance;
            _playerVisors[playerId] = (modId, visorId);
        }

        [MessHallRPC(RPCTarget.AllInclusive, RPCCaller.Anyone)]
        public static void RPC_ClearVisor(int playerId)
        {
            if (_activeVisors.TryGetValue(playerId, out var old))
            {
                GameObject.Destroy(old);
                _activeVisors.Remove(playerId);
            }
            _playerVisors.Remove(playerId);
        }

        public static void OnPlayerLeft(int playerId)
        {
            if (_activeVisors.TryGetValue(playerId, out var visor))
            {
                GameObject.Destroy(visor);
                _activeVisors.Remove(playerId);
            }
            _playerVisors.Remove(playerId);
        }
    }
}