using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LLNet {
	public class LLClient : MonoBehaviour {

		// ---------------------------------------------------------------------------------------------UNITY VARIABLES

		[SerializeField]
		public string _ServerAddress = "127.0.0.1";

		[SerializeField]
		private int _ServerPort = 27000;

		[SerializeField]
		private int _BufferSize = 1024;

		[SerializeField]
		private byte _ThreadPoolSize = 3;

		[Space, SerializeField]
		private NetMessagesContainer _NetMessageContainer;

		private Queue<string> _MessageQueue = new Queue<string>();

		public string UserName;
		public int TeamNumber;
		internal int MyConnectionId;
		private int _HostId = 0;
		private int _ServerConnectionId = 0;

		// --------------------------------------------------------------------------------------------------PROPERTIES
		public byte ReliableChannel { get; private set; } = 0;
		public byte UnreliableChannel { get; private set; } = 0;
		public Dictionary<int, NetUser> NetUsers { get; private set; } = new Dictionary<int, NetUser>();


		// ------------------------------------------------------------------------StartClient

		private void Start() => ConnectServer();




		
		private string BroadcastString;
		private string TeamString;
		//private string WhisperString;
		private void OnGUI() {
			GUILayout.BeginHorizontal();
			{
				GUILayout.BeginVertical();

				// USER INFO

				GUILayout.Label("USER INFO:");
				GUILayout.Label($"Server IP     : {_ServerAddress}");
				GUILayout.Label($"User Name     : {UserName}");
				GUILayout.Label($"Team Number   : {TeamNumber}");
				GUILayout.Label($"Connection Id : {MyConnectionId}");
				
				// BROADCAST

				// Broadcast to all clients
				GUILayout.Space(15);
				BroadcastString = GUILayout.TextField(BroadcastString);
				if (GUILayout.Button("BROADCAST TO ALL USERS")) {

					Byterizer byterizer = new Byterizer();
					byterizer.Push((byte)NetMessageType.CHAT_BROADCAST);
					byterizer.Push(UserName);
					byterizer.Push(BroadcastString);
					SendNetMessage(_ServerConnectionId, byterizer.GetBuffer());
					BroadcastString = "";
				}

				// BROADCAST TEAM

				GUILayout.Space(15);
				TeamString = GUILayout.TextField(TeamString);
				if (GUILayout.Button("BROADCAST TO TEAM")) {

					Byterizer byterizer = new Byterizer();
					byterizer.Push((byte)NetMessageType.CHAT_TEAM);
					byterizer.Push(UserName);
					byterizer.Push(TeamNumber);
					byterizer.Push(TeamString);
					SendNetMessage(_ServerConnectionId, byterizer.GetBuffer());
					TeamString = "";
				}

				// WHISPER

				GUILayout.Space(15);
				GUILayout.Label("USER CONNECTED (WHISPER)");
				foreach (var item in NetUsers) {
					if (GUILayout.Button($"{item.Key} - {item.Value.UserName}")) {


						Byterizer byterizer = new Byterizer();
						byterizer.Push((byte)NetMessageType.CHAT_WHISPER);
						byterizer.Push(item.Key);
						byterizer.Push(item.Value.UserName);
						SendNetMessage(_ServerConnectionId, byterizer.GetBuffer());
						
					}
				}





				GUILayout.EndVertical();

				GUILayout.Space(32);

				GUILayout.BeginVertical();
				{
					GUILayout.Label("Chat Messages");
					GUILayout.Space(16);
					foreach (var item in _MessageQueue) {
						GUILayout.Label(item);
					}
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();
		}

		// ----------------------------------------------------------------------------------------------PRIVATE MEHODS

		// ------------------------------------------------------------------------StartServer

		private void ConnectServer() {

			GlobalConfig globalConf = new GlobalConfig() {
				ThreadPoolSize = _ThreadPoolSize
			};

			NetworkTransport.Init(globalConf);

			ConnectionConfig connectionConf = new ConnectionConfig() {
				SendDelay = 0,
				MinUpdateTimeout = 1
			};
			ReliableChannel = connectionConf.AddChannel(QosType.Reliable);
			UnreliableChannel = connectionConf.AddChannel(QosType.Unreliable);

			HostTopology hostTopology = new HostTopology(connectionConf, 1);
			_HostId = NetworkTransport.AddHost(hostTopology, 0);

			byte error = 0;
			_ServerConnectionId = NetworkTransport.Connect(
					_HostId, _ServerAddress, _ServerPort, 0, out error);

			if (error == 0) {
				StartCoroutine(Receiver());
				Debug.Log($"@ConnectToServer -> {_HostId}, {_ServerConnectionId}");
			} else {
				Debug.LogError($"@ConnectToServer ERROR -> {error}", this);
			}
		}

		// ---------------------------------------------------------------------------Receiver

		private IEnumerator Receiver() {
			int recHostId, recConnectionId, recChannelId, recDataReceivedSize;
			byte error;
			byte[] recBuffer = new byte[_BufferSize];

			while (true) {
				NetworkEventType netEventType = NetworkTransport.Receive
				(
					out recHostId,
					out recConnectionId,
					out recChannelId,
					recBuffer, // Data to be sent
					recBuffer.Length, // Length of the data sent
					out recDataReceivedSize, // Length of the data received
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
						OnDataEvent(recChannelId, recBuffer, recDataReceivedSize);
						break;

					case NetworkEventType.DisconnectEvent:
						//OnDisconnectedEvent(recConnectionId);
						break;

					default:
						Debug.LogWarning($"@Recevier -> Unrecognized Net Message type " +
								$"[{netEventType.ToString()}]", this);
						break;

				}
			}
		}

		// ------------------------------------------------------------------AddWhisperToQueue
		public void AddMessageToQueue(string msg) {

			// Adds an object to the end
			_MessageQueue.Enqueue(msg);
			if (_MessageQueue.Count >= 20) {

				// Removes and returns the object at the beginning
				_MessageQueue.Dequeue();
			}
		}





		// ---------------------------------------------------------------------PRIVATE METHODS (Called from Coroutine)






		// -------------------------------------------------------------------OnConnectedEvent
		private void OnConnectedEvent(int connectionId) {

			Debug.Log($"@Client -> User [{connectionId}] Connected");
		}

		// ------------------------------------------------------------------------OnDataEvent
		private void OnDataEvent(int channelId, byte[] data, int dataSize) {
			Byterizer byterizerData = new Byterizer();
			byterizerData.LoadDeep(data, dataSize);

			// Pass a single byte
			NetMessageType messageType = (NetMessageType)byterizerData.PopByte();

			_NetMessageContainer.NetMessagesMap[messageType].Client_ReceiveMessage(
				byterizerData,
				this);

			Debug.Log($"@Client -> User OnDataEvent");

		}






		// ---------------------------------------------------------------------------------------------MESSAGE SENDING






		// ---------------------------------------------------------------------SendNetMessage
		public void SendNetMessage(int channelId, byte[] data) {
			NetworkTransport.Send(_HostId, _ServerConnectionId, channelId,
					data, data.Length, out var error);
			if (error != 0) {
				Debug.LogError($"@Client -> Error: [{error}] : " +
						$"Could not send message to Server");
			}
		}

	} // Class
}// Namespace