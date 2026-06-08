using MessHallAPI.Config;
using MessHallAPI.Debugger;
using System.Collections.Generic;
using UnityEngine;
using static MessHallAPI.Base.References;

namespace MessHallAPI.Networking
{
    public class MessHallNetworkTransform : MonoBehaviour
    {
        public string ObjectId;
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

        float timer;
        bool init, registered, networking;

        Vector3 lastPos, lastScale, targetPos, targetScale;
        Quaternion lastRot, targetRot;
        bool hasTarget;

        const string MOD_ID = "MessHallAPI";
        const string RPC_SYNC = "SyncTransform";

        static Dictionary<string, MessHallNetworkTransform> registry = new Dictionary<string, MessHallNetworkTransform>();
        static HashSet<string> pending = new HashSet<string>();

        public static void RegisterObject(string id)
        {
            var obj = Find(id);
            if (obj != null)
            {
                obj.Init(id);
                obj.registered = true;
                Logging.DebugLog($"Registered {id}");
            }
            else
            {
                pending.Add(id);
                Logging.Warn($"Queued {id}");
            }
        }

        public static void StartNetworking(string id)
        {
            if (!registry.TryGetValue(id, out var obj))
            {
                Logging.Warn($"Start fail {id}");
                return;
            }

            obj.networking = true;
            Logging.DebugLog($"Networking {id}");
        }

        static MessHallNetworkTransform Find(string id)
        {
            var go = GameObject.Find(id);
            if (go != null)
            {
                MessHallNetworkTransform? comp = go.GetComponent<MessHallNetworkTransform>();
                if (comp != null) return comp;
            }

            foreach (var o in Resources.FindObjectsOfTypeAll<MessHallNetworkTransform>())
                if (o && (o.ObjectId == id || o.gameObject.name == id))
                    return o;

            return null;
        }

        void Awake()
        {
            if (!string.IsNullOrEmpty(ObjectId) && pending.Contains(ObjectId))
            {
                Init(ObjectId);
                registered = true;
                pending.Remove(ObjectId);
                Logging.DebugLog($"Auto {ObjectId}");
            }

            if (pending.Contains(gameObject.name))
            {
                ObjectId = gameObject.name;
                Init(ObjectId);
                registered = true;
                pending.Remove(ObjectId);
                Logging.DebugLog($"Auto name {ObjectId}");
            }
        }

        void Init(string id)
        {
            if (init) return;

            ObjectId = id;
            registry[id] = this;

            lastPos = GetPos();
            lastRot = GetRot();
            lastScale = transform.localScale;

            targetPos = lastPos;
            targetRot = lastRot;
            targetScale = lastScale;

            init = true;
            Logging.DebugLog($"Init {id}");
        }

        void Update()
        {
            if (!init || !registered) return;

            if (!Settings.IsHost)
            {
                if (!hasTarget) return;

                float t = Time.deltaTime * LerpSpeed;

                if (SyncPosition) SetPos(Vector3.Lerp(GetPos(), targetPos, t));
                if (SyncRotation) SetRot(Quaternion.Slerp(GetRot(), targetRot, t));
                if (SyncScale) transform.localScale = Vector3.Lerp(transform.localScale, targetScale, t);

                return;
            }

            if (!networking) return;

            timer += Time.deltaTime;
            if (timer < SyncInterval) return;
            timer = 0f;

            Vector3 pos = GetPos();
            Quaternion rot = GetRot();
            Vector3 scale = transform.localScale;

            bool dirty =
                (SyncPosition && Vector3.Distance(pos, lastPos) >= PositionThreshold) ||
                (SyncRotation && Quaternion.Angle(rot, lastRot) >= RotationThreshold) ||
                (SyncScale && Vector3.Distance(scale, lastScale) >= ScaleThreshold);

            if (!dirty) return;

            lastPos = pos;
            lastRot = rot;
            lastScale = scale;

            NetworkManager.InvokeRPC(
                MOD_ID,
                RPC_SYNC,
                ObjectId,
                pos.x, pos.y, pos.z,
                rot.x, rot.y, rot.z, rot.w,
                scale.x, scale.y, scale.z
            );

            Logging.DebugLog($"Sync {ObjectId}");
        }

        [MessHallRPC(RPCTarget.AllInclusive, RPCCaller.HostOnly)]
        public static void SyncTransform(string id, float px, float py, float pz, float rx, float ry, float rz, float rw, float sx, float sy, float sz)
        {
            if (!registry.TryGetValue(id, out var obj))
            {
                Logging.Warn($"RPC miss {id}");
                return;
            }

            obj.targetPos = new Vector3(px, py, pz);
            obj.targetRot = new Quaternion(rx, ry, rz, rw);
            obj.targetScale = new Vector3(sx, sy, sz);
            obj.hasTarget = true;
        }

        Vector3 GetPos() => UseLocalSpace ? transform.localPosition : transform.position;
        Quaternion GetRot() => UseLocalSpace ? transform.localRotation : transform.rotation;

        void SetPos(Vector3 vector)
        {
            if (UseLocalSpace) transform.localPosition = vector;
            else transform.position = vector;
        }

        void SetRot(Quaternion rotation)
        {
            if (UseLocalSpace) transform.localRotation = rotation;
            else transform.rotation = rotation;
        }

        void OnDestroy()
        {
            if (ObjectId != null && registry.ContainsKey(ObjectId))
            {
                registry.Remove(ObjectId);
                Logging.DebugLog($"Removed {ObjectId}");
            }
        }
    }
}