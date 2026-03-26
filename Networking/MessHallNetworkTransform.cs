using UnityEngine;
using MessHallAPI.Debugger;
using MessHallAPI.Config;

namespace MessHallAPI.Networking
{
	public class MessHallNetworkTransform : MonoBehaviour
	{
		public string ObjectId { get; private set; }

		public float PositionThreshold = 0.01f;
		public float RotationThreshold = 0.1f;
		public float SyncInterval = 0.05f;

		private Vector3 _lastPosition;
		private Quaternion _lastRotation;
		private Vector3 _lastScale;
		private float _syncTimer;

		private bool _initialized = false;

		private const string MOD_ID = "MessHallAPI";
		private const string RPC_SYNC = "SyncTransform";

		private static readonly Dictionary<string, MessHallNetworkTransform> _registry = new();

		public void Initialize(string customname)
		{
			if (_initialized)
			{
				Logging.Warn($"Initialize called again on already initialized '{ObjectId}' — skipping.");
				return;
			}

			ObjectId = customname ?? gameObject.name;
			_registry[ObjectId] = this;

			_lastPosition = transform.position;
			_lastRotation = transform.rotation;
			_lastScale = transform.localScale;

			_initialized = true;
		}

		private void OnDestroy()
		{
			if (_initialized && ObjectId != null)
			{
				_registry.Remove(ObjectId);
			}
		}

		private void Update()
		{
			if (!_initialized)
				return;

			if (!Settings.IsHost)
				return;

			_syncTimer += Time.deltaTime;
			if (_syncTimer < SyncInterval)
				return;

			_syncTimer = 0f;

			bool posChanged = Vector3.Distance(transform.position, _lastPosition) > PositionThreshold;
			bool rotChanged = Quaternion.Angle(transform.rotation, _lastRotation) > RotationThreshold;
			bool scaleChanged = _lastScale != transform.localScale;

			if (!posChanged && !rotChanged && !scaleChanged)
				return;

			_lastPosition = transform.position;
			_lastRotation = transform.rotation;
			_lastScale = transform.localScale;

			NetworkManager.InvokeRPC(MOD_ID, RPC_SYNC,
				ObjectId,
				transform.position.x, transform.position.y, transform.position.z,
				transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w,
				transform.localScale.x, transform.localScale.y, transform.localScale.z);
		}

		[MessHallRPC(RPCTarget.AllInclusive, RPCCaller.Anyone, Description = "Syncs a NetworkTransform")]
		public void SyncTransform(string objectId, float px, float py, float pz, float rx, float ry, float rz, float rw, float sx, float sy, float sz)
		{

			if (!_registry.TryGetValue(objectId, out var instance))
			{
				Logging.Warn($"No object found for '{objectId}' — registry has: [{string.Join(", ", _registry.Keys)}]");
				return;
			}

			instance.transform.position = new Vector3(px, py, pz);
			instance.transform.rotation = new Quaternion(rx, ry, rz, rw);
			instance.transform.localScale = new Vector3(sx, sy, sz);
		}

		internal static void RegisterRPCs()
		{
			var dummy = new GameObject("__MessHallNetworkTransform_RPCHandler__");
			var handler = dummy.AddComponent<MessHallNetworkTransform>();
			handler.ObjectId = "__handler__";
			handler._initialized = true;
			RPCRegistry.Register(handler, MOD_ID);
			DontDestroyOnLoad(dummy);
		}
	}
}