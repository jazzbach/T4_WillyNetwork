using UnityEngine;

namespace LLNet {

	// Abstract means that no instances of this object may be created
	public abstract class NetMessage : ScriptableObject {
		public NetMessageType MessageType;

		public abstract void Server_ReceiveMessage(int connectionId, Byterizer data, LLServer server);
		public abstract void Client_ReceiveMessage(Byterizer data, LLClient client);

	}
}
