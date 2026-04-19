using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using MessHallAPI.Networking;
using UnityEngine;
using static MessHallAPI.Base.References;

namespace MessHallAPI.Managers.Cosmetic
{
    public class CustomPetManager : MonoBehaviour
    {
        public Transform followTarget;
        public float followSpeed = 5f;
        public float followDistance = 1.2f;
        public static GameObject? PetTest;

        private Vector3 _velocity;

        private static Dictionary<string, Dictionary<string, GameObject>> _petRegistry = new();

        private static Dictionary<int, (string modId, string petId)> _playerPets = new();

        private static Dictionary<int, GameObject> _activePets = new();

        private NetworkedLocomotionPlayer _ownerLocomotion;

        public void Init()
        {
            foreach (PlayerState player in Spawn.ActivePlayerStates)
            {
                if (player.PlayerId == OwnerPlayerId)
                {
                    followTarget = player.LocomotionPlayer.transform.Find("CrewmatePhysics");
                    _ownerLocomotion = player.LocomotionPlayer;
                    break;
                }
            }
            AutoAddNetworkTransform(gameObject);
        }

        private Vector3 _lastOwnerPos;
        private Vector3 _idleTargetPos;
        private float _idleTimer = 0f;
        private bool _isIdle = false;
        private const float IdleDelay = 1f;

        public void FollowPlayerScript()
        {
            if (_ownerLocomotion == null) return;

            Vector3 ownerPos = _ownerLocomotion.RigidbodyPosition;
            Quaternion ownerRot = _ownerLocomotion.RigidbodyRotation;

            float moved = Vector3.Distance(ownerPos, _lastOwnerPos);
            if (moved < 0.01f)
            {
                _idleTimer += Time.deltaTime;
                if (!_isIdle && _idleTimer >= IdleDelay)
                {
                    _isIdle = true;
                    _idleTargetPos = ownerPos + ownerRot * Vector3.right * followDistance;
                }
            }
            else
            {
                _idleTimer = 0f;
                _isIdle = false;
                _lastOwnerPos = ownerPos;
            }

            Vector3 targetPos = _isIdle
                ? _idleTargetPos
                : ownerPos + ownerRot * -Vector3.forward * followDistance;

            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, 1f / followSpeed);
            transform.rotation = ownerRot;
        }

        public static void RegisterPet(string modId, string petId, GameObject prefab)
        {
            if (!_petRegistry.ContainsKey(modId))
                _petRegistry[modId] = new();

            _petRegistry[modId][petId] = prefab;
        }

        public static void AssignPet(int playerId, string modId, string petId)
        {
            _playerPets[playerId] = (modId, petId);
        }

        public static GameObject? GetPetForPlayer(byte playerId)
        {
            if (!_playerPets.TryGetValue(playerId, out var entry)) return null;
            if (!_petRegistry.TryGetValue(entry.modId, out var pets)) return null;
            pets.TryGetValue(entry.petId, out var prefab);
            return prefab;
        }

        public static bool PlayerHasPet(int playerId) => _playerPets.ContainsKey(playerId);

        public static (string modId, string petId)? GetPlayerPetInfo(byte playerId)
        {
            if (_playerPets.TryGetValue(playerId, out var entry)) return entry;
            return null;
        }

        public static List<KeyValuePair<(string, string), GameObject>> GetAllRegistered()
        {
            var result = new List<KeyValuePair<(string, string), GameObject>>();
            foreach (var mod in _petRegistry)
                foreach (var pet in mod.Value)
                    result.Add(new KeyValuePair<(string, string), GameObject>((mod.Key, pet.Key), pet.Value));
            return result;
        }

        [MessHallRPC(RPCTarget.AllInclusive, RPCCaller.Anyone)]
        public static void RPC_SetPet(int playerId, string modId, string petId)
        {
            if (_activePets.TryGetValue(playerId, out var old))
            {
                Destroy(old);
                _activePets.Remove(playerId);
            }

            var prefab = CustomPetManager.GetRegisteredPrefab(modId, petId);
            if (prefab == null) return;

            var instance = GameObject.Instantiate(prefab);
            var manager = instance.AddComponent<CustomPetManager>();
            manager.OwnerPlayerId = playerId;
            manager.Init();

            _activePets[playerId] = instance;
            _playerPets[playerId] = (modId, petId);
        }

        public static GameObject? GetRegisteredPrefab(string modId, string petId)
        {
            if (!_petRegistry.TryGetValue(modId, out var pets)) return null;
            pets.TryGetValue(petId, out var prefab);
            return prefab;
        }

        public static void OnPlayerLeft(int playerId)
        {
            if (_activePets.TryGetValue(playerId, out var pet))
            {
                Destroy(pet);
                _activePets.Remove(playerId);
            }
            _playerPets.Remove(playerId);
        }

        [MessHallRPC(RPCTarget.AllInclusive, RPCCaller.Anyone)]
        public static void RPC_ClearPet(int playerId)
        {
            if (_activePets.TryGetValue(playerId, out var old))
            {
                Destroy(old);
                _activePets.Remove(playerId);
            }
            _playerPets.Remove(playerId);
        }

        public int OwnerPlayerId;

        private void Awake()
        {

        }

        private void Update()
        {
            if (followTarget == null) return;

            foreach (PlayerState player in Spawn.ActivePlayerStates)
            {
                if (player.PlayerId != OwnerPlayerId) continue;
                if (!player.IsAlive) return;
                break;
            }

            FollowPlayerScript();
        }

        /*
        public static void AnimatePetOnDeath()
        {
            // TODO: hook to local player death event
            // Trigger death anim, then optionally hide or play idle
            // Example:
            // _animator.SetTrigger("Death");
            // yield return new WaitForSeconds(deathAnimLength);
            // gameObject.SetActive(false);
        }
        */

        public static GameObject? FindMiniCrewmate()
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == "SM_Hat_MiniCrewmate" && go.transform.parent == null)
                    return go;
            }
            return null;
        }

        public static GameObject? FindHeadslug()
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == "AP_Hat_Headslug" && go.transform.parent == null)
                    return go;
            }
            return null;
        }

        public static GameObject? FindChocolateScoop()
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == "SM_Hat_ChocolateScoop" && go.transform.parent == null)
                    return go;
            }
            return null;
        }

        public static GameObject? FindFlyTrap()
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == "SM_Hat_BeanusFlytrap" && go.transform.parent == null)
                    return go;
            }
            return null;
        }

        public static GameObject? FindBalloon()
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == "SM_Hat_Balloon" && go.transform.parent == null)
                    return go;
            }
            return null;
        }

        public static GameObject? FindSnowmate()
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == "SM_Snowmate_01" && go.transform.parent == null)
                    return go;
            }
            return null;
        }

        public static GameObject? FindIcemate()
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == "SM_IceCrewmate_01" && go.transform.parent == null)
                    return go;
            }
            return null;
        }

        public static GameObject? FindWhippedCream()
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == "SM_WhippedCream_01" && go.transform.parent == null)
                    return go;
            }
            return null;
        }

        public static GameObject? FindHeart()
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == "SM_Hat_Heart" && go.transform.parent == null)
                    return go;
            }
            return null;
        }

        public static GameObject? FindPenguin()
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == "SM_PengYinz_01" && go.transform.parent == null)
                    return go;
            }
            return null;
        }

        public static GameObject? FindToiletPaper()
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == "SM_Hat_TheLastWipe" && go.transform.parent == null)
                    return go;
            }
            return null;
        }

        public static void AutoAddNetworkTransform(GameObject obj)
        {
            if (obj.GetComponent<MessHallNetworkTransform>() != null) return;
            var nt = obj.AddComponent<MessHallNetworkTransform>();
            nt.ObjectId = "pet_" + obj.name + "_" + Client.PState.PlayerId;
        }
    }
}
