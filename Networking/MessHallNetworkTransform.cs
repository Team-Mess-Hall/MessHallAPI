using UnityEngine;
using MessHallAPI.Debugger;
using MessHallAPI.Config;
using System.Collections.Generic;

namespace MessHallAPI.Networking
{
    public class MessHallNetworkTransform : MonoBehaviour
    {
        public string ObjectId { get; private set; }

        public float SyncInterval = 0.1f;
        public float PositionThreshold = 0.01f;
        public float LerpSpeed = 12f;

        private float _syncTimer;
        private bool _initialized;

        private Vector3 _lastSentPos;
        private Vector3 _targetPos;
        private bool _hasTarget;

        private const string MOD_ID = "MessHallAPI";
        private const string RPC_SYNC = "SyncTransform";

        private static readonly Dictionary<string, MessHallNetworkTransform> _registry = new();

        public static void NetworkSpawn(GameObject obj)
        {
            if (obj == null) return;

            if (obj.GetComponent<MessHallNetworkTransform>() != null)
                return;

            var comp = obj.AddComponent<MessHallNetworkTransform>();
            comp.Initialize(obj.name + "_" + UnityEngine.Random.Range(1000, 9999));
        }

        public void Initialize(string id)
        {
            if (_initialized) return;

            ObjectId = id;
            _registry[ObjectId] = this;

            _lastSentPos = transform.position;
            _targetPos = transform.position;

            _initialized = true;

            Logging.Log($"[MesshallNetworkTransform] Your object is being networked! {ObjectId}");
        }

        private void OnDestroy()
        {
            if (_initialized && ObjectId != null)
                _registry.Remove(ObjectId);
        }

        private void Update()
        {
            if (!_initialized) return;

            if (!Settings.IsHost)
            {
                if (_hasTarget)
                {
                    transform.position = Vector3.Lerp(transform.position, _targetPos, Time.deltaTime * LerpSpeed);
                }
                return;
            }

            _syncTimer += Time.deltaTime;
            if (_syncTimer < SyncInterval) return;

            _syncTimer = 0f;

            Vector3 pos = transform.position;

            if (Vector3.Distance(pos, _lastSentPos) < PositionThreshold)
                return;

            _lastSentPos = pos;

            NetworkManager.InvokeRPC(
                MOD_ID,
                RPC_SYNC,
                ObjectId,
                pos.x,
                pos.y,
                pos.z
            );
        }

        [MessHallRPC(RPCTarget.AllInclusive, RPCCaller.HostOnly)]
        public static void SyncTransform(string objectId, float x, float y, float z)
        {
            if (!_registry.TryGetValue(objectId, out var obj))
                return;

            obj._targetPos = new Vector3(x, y, z);
            obj._hasTarget = true;

            if (Settings.IsHost)
                obj.transform.position = obj._targetPos;
        }


    }
}