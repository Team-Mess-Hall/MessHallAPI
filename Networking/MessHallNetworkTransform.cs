using MessHallAPI.Config;
using MessHallAPI.Debugger;
using System.Collections.Generic;
using UnityEngine;
using static MessHallAPI.Base.References;

namespace MessHallAPI.Networking
{
    public class MessHallNetworkTransform : MonoBehaviour
    {
        public string? ObjectId { get; set; }
        public int OwnerId = -1;

        public bool SyncPosition = true;
        public bool SyncRotation = true;
        public bool SyncScale = true;
        public bool UseLocalSpace = false;

        public float SyncInterval = 0.1f;
        public float PositionThreshold = 0.01f;
        public float RotationThreshold = 0.5f;
        public float ScaleThreshold = 0.01f;
        public float LerpSpeed = 12f;
        public float SettleThreshold = 0.001f;

        private float _syncTimer;
        private bool _initialized;

        private Vector3 _lastSentPos;
        private Quaternion _lastSentRot;
        private Vector3 _lastSentScale;

        private Vector3 _targetPos;
        private Quaternion _targetRot;
        private Vector3 _targetScale;
        private bool _hasTarget;

        private const string MOD_ID = "MessHallAPI";
        private const string RPC_SYNC = "SyncTransform";
        private const string RPC_LATE = "LateSync";

        private static readonly Dictionary<string, MessHallNetworkTransform> _registry = new();

        public void Initialize(string id)
        {
            if (_initialized) return;
            ObjectId = id;
            _registry[ObjectId] = this;

            _lastSentPos = UseLocalSpace ? transform.localPosition : transform.position;
            _lastSentRot = UseLocalSpace ? transform.localRotation : transform.rotation;
            _lastSentScale = transform.localScale;

            _targetPos = _lastSentPos;
            _targetRot = _lastSentRot;
            _targetScale = _lastSentScale;

            _initialized = true;
        }

        private void OnDestroy()
        {
            if (_initialized && ObjectId != null)
                _registry.Remove(ObjectId);
        }

        public static void SendValuesToJoiningPlayer(int joiningPlayerId)
        {
            if (!Settings.IsHost) return;

            foreach (var kvp in _registry)
            {
                var obj = kvp.Value;
                if (obj == null || !obj._initialized) continue;

                Vector3 pos = obj.UseLocalSpace ? obj.transform.localPosition : obj.transform.position;
                Quaternion rot = obj.UseLocalSpace ? obj.transform.localRotation : obj.transform.rotation;
                Vector3 scale = obj.transform.localScale;

                NetworkManager.InvokeRPC(
                    MOD_ID,
                    RPC_LATE,
                    joiningPlayerId,
                    kvp.Key,
                    pos.x, pos.y, pos.z,
                    rot.x, rot.y, rot.z, rot.w,
                    scale.x, scale.y, scale.z
                );
            }
        }

        private void Update()
        {
            if (!_initialized) return;

            bool isOwner = Client.PState.PlayerId == OwnerId ||
                           (OwnerId == -1 && Settings.IsHost);

            if (!isOwner)
            {
                if (_hasTarget)
                {
                    float t = Time.deltaTime * LerpSpeed;

                    if (SyncPosition)
                    {
                        Vector3 cur = UseLocalSpace ? transform.localPosition : transform.position;
                        Vector3 next = Vector3.Distance(cur, _targetPos) > SettleThreshold
                            ? Vector3.Lerp(cur, _targetPos, t)
                            : _targetPos;
                        if (UseLocalSpace) transform.localPosition = next;
                        else transform.position = next;
                    }

                    if (SyncRotation)
                    {
                        Quaternion cur = UseLocalSpace ? transform.localRotation : transform.rotation;
                        Quaternion next = Quaternion.Angle(cur, _targetRot) > SettleThreshold
                            ? Quaternion.Slerp(cur, _targetRot, t)
                            : _targetRot;
                        if (UseLocalSpace) transform.localRotation = next;
                        else transform.rotation = next;
                    }

                    if (SyncScale)
                    {
                        transform.localScale = Vector3.Distance(transform.localScale, _targetScale) > SettleThreshold
                            ? Vector3.Lerp(transform.localScale, _targetScale, t)
                            : _targetScale;
                    }
                }
                return;
            }

            _syncTimer += Time.deltaTime;
            if (_syncTimer < SyncInterval) return;
            _syncTimer = 0f;

            Vector3 pos = UseLocalSpace ? transform.localPosition : transform.position;
            Quaternion rot = UseLocalSpace ? transform.localRotation : transform.rotation;
            Vector3 scale = transform.localScale;

            bool dirty = false;
            if (SyncPosition && Vector3.Distance(pos, _lastSentPos) >= PositionThreshold) dirty = true;
            if (SyncRotation && Quaternion.Angle(rot, _lastSentRot) >= RotationThreshold) dirty = true;
            if (SyncScale && Vector3.Distance(scale, _lastSentScale) >= ScaleThreshold) dirty = true;

            if (!dirty) return;

            _lastSentPos = pos;
            _lastSentRot = rot;
            _lastSentScale = scale;

            NetworkManager.InvokeRPC(
                MOD_ID,
                RPC_SYNC,
                ObjectId,
                pos.x, pos.y, pos.z,
                rot.x, rot.y, rot.z, rot.w,
                scale.x, scale.y, scale.z
            );
        }

        [MessHallRPC(RPCTarget.AllInclusive, RPCCaller.HostOnly)]
        public static void SyncTransform(string objectId, float px, float py, float pz, float rx, float ry, float rz, float rw, float sx, float sy, float sz)
        {
            if (!_registry.TryGetValue(objectId, out var obj)) return;

            if (obj.SyncPosition) obj._targetPos = new Vector3(px, py, pz);
            if (obj.SyncRotation) obj._targetRot = new Quaternion(rx, ry, rz, rw);
            if (obj.SyncScale) obj._targetScale = new Vector3(sx, sy, sz);
            obj._hasTarget = true;

            bool isOwner = Client.PState.PlayerId == obj.OwnerId ||
                           (obj.OwnerId == -1 && Settings.IsHost);

            if (isOwner)
            {
                if (obj.UseLocalSpace)
                {
                    obj.transform.localPosition = obj._targetPos;
                    obj.transform.localRotation = obj._targetRot;
                }
                else
                {
                    obj.transform.position = obj._targetPos;
                    obj.transform.rotation = obj._targetRot;
                }
                obj.transform.localScale = obj._targetScale;
            }
        }

        [MessHallRPC(RPCTarget.AllInclusive, RPCCaller.HostOnly)]
        public static void LateSync([RPCTarget] int targetPlayerId, string objectId, float px, float py, float pz, float rx, float ry, float rz, float rw, float sx, float sy, float sz)
        {
            if (!_registry.TryGetValue(objectId, out var obj)) return;

            if (obj.SyncPosition) obj._targetPos = new Vector3(px, py, pz);
            if (obj.SyncRotation) obj._targetRot = new Quaternion(rx, ry, rz, rw);
            if (obj.SyncScale) obj._targetScale = new Vector3(sx, sy, sz);
            obj._hasTarget = true;
        }
    }
}