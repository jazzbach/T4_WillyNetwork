using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LLNet {
	public class LLServer : MonoBehaviour {

		// ---------------------------------------------------------------------------------------------UNITY VARIABLES

		[SerializeField]
		private int _ServerPort = 27000;

		[SerializeField]
		private int _BufferSize = 1024;

		[SerializeField]
		private byte _ThreadPoolSize = 3;

		[Space, SerializeField]
		private NetMessagesContainer _NetMessageContainer;

		// --------------------------------------------------------------------------------------------------PROPERTIES

		private int _HostId = 0;
		public byte _ReliableChannel = 0;
		public byte _UnreliableChannel = 0;
		public Dictionary<int, NetUser> _NetUsers = new Dictionary<int, NetUser>();

		// -----------------------------------------------------------------------------------------------UNITY METHODS

		// ------------------------------------------------------------------------------Start
		private void Start() => StartServer();

		// ------------------------------------------------------------------------------OnGUI
		private void OnGUI() {
			GUILayout.BeginHorizontal();
			{
				GUILayout.BeginVertical();
				{
					GUILayout.Label("Users Connected");
					GUILayout.Space(16);
					foreach (var item in _NetUsers) {
						GUILayout.Button($"{item.Value.ConnectionId} - {item.Value.UserName}");
					}
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();
		}

		// ----------------------------------------------------------------------------------------------PRIVATE MEHODS

		// ------------------------------------------------------------------------StartServer

		private void StartServer() {
			GlobalConfig globalConf = new GlobalConfig() {
				ThreadPoolSize = _ThreadPoolSize
			};

			NetworkTransport.Init(globalConf);

			ConnectionConfig connectionConf = new ConnectionConfig() {
				SendDelay = 0,
				MinUpdateTimeout = 1
			};
			_ReliableChannel = connectionConf.AddChannel(QosType.Reliable);
			_UnreliableChannel = connectionConf.AddChannel(QosType.Unreliable);

			HostTopology hostTopology = new HostTopology(connectionConf, 3);
			_HostId = NetworkTransport.AddHost(hostTopology, _ServerPort);

			StartCoroutine(Receiver());

			Debug.Log($"@StartServer -> {_HostId} {_ReliableChannel} {_UnreliableChannel}");
		}

		// ---------------------------------------------------------------------------Receiver

		private IEnumerator Receiver() {
			int recSocketId, recConnectionId, recChannelId, recDataSize;
			byte error;
			byte[] recBuffer = new byte[_BufferSize];

			while (true) {
				NetworkEventType netEventType = NetworkTransport.Receive
				(
					out recSocketId,
					out recConnectionId,
					out recChannelId,
					recBuffer,
					_BufferSize,
					out recDataSize,
					out error
				);

				switch (netEventType) {
					case NetworkEventType.Nothing:
						yield return null;
						break;

					case NetworkEventType.ConnectEvent:
						OnConnectedEvent(recConnectionId);
						break;

					case NetworkEventType.DataEvent:
						OnDataEvent(recConnectionId, _ReliableChannel, recBuffer, recDataSize);
						break;

					case NetworkEventType.DisconnectEvent:
						OnDisconnectedEvent(recConnectionId);
						break;


					default:
						Debug.LogWarning($"@Recevier -> Unrecognized Net Message type " +
								$"[{netEventType.ToString()}]", this);
						break;

				}// switch
			} // while
		}// Receiver






		// ---------------------------------------------------------------------PRIVATE METHODS (Called from Coroutine)






		// -------------------------------------------------------------------OnConnectedEvent
		private void OnConnectedEvent(int connectionId) {
			if (_NetUsers.ContainsKey(connectionId)) {
				Debug.Log($"UserId [{connectionId}] Re-Connected");
				return;
			} else {
				var newUser = new NetUser() { ConnectionId = connectionId };
				_NetUsers[connectionId] = newUser;
			}

			Byterizer data = new Byterizer();
			data.Push((byte)NetMessageType.CONNECTION_ACK);
			data.Push(connectionId);

			SendNetMessage(connectionId, _ReliableChannel, data.GetBuffer());
			Debug.Log($"@Server -> User [{connectionId}] Connected");
		}

		// ------------------------------------------------------------------------OnDataEvent
		private void OnDataEvent(int connectionId, int channelId, byte[] data, int dataSize) {
			if (_NetUsers.ContainsKey(connectionId) == false) {
				Debug.LogError($"User [{connectionId}] Not Registered");
				return;
			}

			Byterizer byterizerData = new Byterizer();
			byterizerData.LoadDeep(data, dataSize);

			// Pass a single byte
			NetMessageType messageType = (NetMessageType)byterizerData.PopByte();

			_NetMessageContainer.NetMessagesMap[messageType].Server_ReceiveMessage(
				connectionId,
				byterizerData,
				this);
		}

		// ----------------------------------------------------------------OnDisconnectedEvent
		private void OnDisconnectedEvent(int connectionId) {
			Debug.Log("disconnect");
		}


			// --------------------------------------------------------------------------------------SEND METHODS (Helpers)






			public void SendNetMessage(int targetId, int channelId, byte[] data) {
			NetworkTransport.Send(_HostId, targetId, channelId, data, data.Length, out var error);
			if (error != 0) {
				Debug.LogError($"@Server -> Error: [{error}] : Could not send message to [{targetId}]");
			}
		}


	} // Class
}// Namespace