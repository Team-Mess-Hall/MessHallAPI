using Il2CppSG.Airlock;
using MessHallAPI.Debugger;
using MessHallAPI.Networking;
using UnityEngine;
using static MessHallAPI.Base.References;
using static MessHallAPI.Config.Settings;

namespace MessHallAPI.Managers.Cosmetic
{
    public class CustomBackpackManager
    {
        private const string BackpackPath = "CrewmatePhysics/Visuals/Player_Crewmate/SK_Char_CrewmateHandless_01/WorldJoint/spine1_loResSpine1/spine1_loResSpine2/spine1_loResSpine3/spine1_backpack";

        private static Dictionary<string, Dictionary<string, GameObject>> _backpackRegistry = new();
        private static Dictionary<int, (string modId, string backpackId)> _playerBackpacks = new();
        private static Dictionary<int, GameObject> _activeBackpacks = new();

        public static void RegisterBackpack(string modId, string backpackId, GameObject prefab)
        {
            if (!_backpackRegistry.ContainsKey(modId))
                _backpackRegistry[modId] = new();
            _backpackRegistry[modId][backpackId] = prefab;
        }

        public static GameObject? GetRegisteredPrefab(string modId, string backpackId)
        {
            if (!_backpackRegistry.TryGetValue(modId, out var backpacks)) return null;
            backpacks.TryGetValue(backpackId, out var prefab);
            return prefab;
        }

        public static List<KeyValuePair<(string, string), GameObject>> GetAllRegistered()
        {
            var result = new List<KeyValuePair<(string, string), GameObject>>();
            foreach (var mod in _backpackRegistry)
                foreach (var backpack in mod.Value)
                    result.Add(new KeyValuePair<(string, string), GameObject>((mod.Key, backpack.Key), backpack.Value));
            return result;
        }

        public static bool PlayerHasBackpack(int playerId) => _playerBackpacks.ContainsKey(playerId);

        [MessHallRPC(RPCTarget.AllInclusive, RPCCaller.Anyone)]
        public static void RPC_SetBackpack(int playerId, string modId, string backpackId)
        {
            if (_activeBackpacks.TryGetValue(playerId, out var old))
            {
                GameObject.Destroy(old);
                _activeBackpacks.Remove(playerId);
            }

            var prefab = GetRegisteredPrefab(modId, backpackId);
            if (prefab == null) return;

            Transform? backpackBone = null;
            foreach (PlayerState player in Spawn.ActivePlayerStates)
            {
                if (player.PlayerId != playerId) continue;
                backpackBone = player.LocomotionPlayer.transform.Find(BackpackPath);
                break;
            }

            if (backpackBone == null) return;

            var instance = GameObject.Instantiate(prefab);
            instance.transform.SetParent(backpackBone, false);
            instance.transform.localPosition = new Vector3(0, -0.25f, 0.4f);
            instance.transform.localRotation = Quaternion.identity;

            _activeBackpacks[playerId] = instance;
            _playerBackpacks[playerId] = (modId, backpackId);
        }

        [MessHallRPC(RPCTarget.AllInclusive, RPCCaller.Anyone)]
        public static void RPC_ClearBackpack(int playerId)
        {
            if (_activeBackpacks.TryGetValue(playerId, out var old))
            {
                GameObject.Destroy(old);
                _activeBackpacks.Remove(playerId);
            }
            _playerBackpacks.Remove(playerId);
        }

        public static void OnPlayerLeft(int playerId)
        {
            if (_activeBackpacks.TryGetValue(playerId, out var backpack))
            {
                GameObject.Destroy(backpack);
                _activeBackpacks.Remove(playerId);
            }
            _playerBackpacks.Remove(playerId);
        }
        public static GameObject? FindRevengerScythe()
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == "SM_Char_RevengerScythe_01")
                    return go;
            }
            return null;
        }
    }
}