using Il2CppSG.Airlock;
using MessHallAPI.Debugger;
using MessHallAPI.Networking;
using UnityEngine;
using static MessHallAPI.Base.References;
using static MessHallAPI.Config.Settings;

namespace MessHallAPI.Managers.Cosmetic
{
    public class CustomHatManager
    {
        private const string HeadPath = "CrewmatePhysics/Visuals/Player_Crewmate/SK_Char_CrewmateHandless_01/WorldJoint/spine1_loResSpine1/spine1_loResSpine2/spine1_loResSpine3/head1_neck/head1_head";

        private static Dictionary<string, Dictionary<string, GameObject>> _hatRegistry = new();
        private static Dictionary<int, (string modId, string hatId)> _playerHats = new();
        private static Dictionary<int, GameObject> _activeHats = new();

        public static void RegisterHat(string modId, string hatId, GameObject prefab)
        {
            if (!_hatRegistry.ContainsKey(modId))
                _hatRegistry[modId] = new();
            _hatRegistry[modId][hatId] = prefab;
        }

        public static GameObject? GetRegisteredPrefab(string modId, string hatId)
        {
            if (!_hatRegistry.TryGetValue(modId, out var hats)) return null;
            hats.TryGetValue(hatId, out var prefab);
            return prefab;
        }

        public static List<KeyValuePair<(string, string), GameObject>> GetAllRegistered()
        {
            var result = new List<KeyValuePair<(string, string), GameObject>>();
            foreach (var mod in _hatRegistry)
                foreach (var hat in mod.Value)
                    result.Add(new KeyValuePair<(string, string), GameObject>((mod.Key, hat.Key), hat.Value));
            return result;
        }

        public static bool PlayerHasHat(int playerId) => _playerHats.ContainsKey(playerId);

        [MessHallRPC(RPCTarget.AllInclusive, RPCCaller.Anyone)]
        public static void RPC_SetHat(int playerId, string modId, string hatId)
        {
            if (_activeHats.TryGetValue(playerId, out var old))
            {
                GameObject.Destroy(old);
                _activeHats.Remove(playerId);
            }

            var prefab = GetRegisteredPrefab(modId, hatId);
            if (prefab == null) return;

            Transform? head = null;
            foreach (PlayerState player in Spawn.ActivePlayerStates)
            {
                if (player.PlayerId != playerId) continue;
                head = player.LocomotionPlayer.transform.Find(HeadPath);
                break;
            }

            if (head == null) return;

            var instance = GameObject.Instantiate(prefab);
            instance.transform.SetParent(head, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;

            _activeHats[playerId] = instance;
            _playerHats[playerId] = (modId, hatId);
        }

        [MessHallRPC(RPCTarget.AllInclusive, RPCCaller.Anyone)]
        public static void RPC_ClearHat(int playerId)
        {
            if (_activeHats.TryGetValue(playerId, out var old))
            {
                GameObject.Destroy(old);
                _activeHats.Remove(playerId);
            }
            _playerHats.Remove(playerId);
        }

        public static void OnPlayerLeft(int playerId)
        {
            if (_activeHats.TryGetValue(playerId, out var hat))
            {
                GameObject.Destroy(hat);
                _activeHats.Remove(playerId);
            }
            _playerHats.Remove(playerId);
        }

        public static GameObject FindOverScopedHat()
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == "AP_Hat_Engineer_01" && go.transform.parent == null)
                {
                    return go;
                }
            }
            return null;
        }
    }
}