using UnityEngine;
using LLNet;

[CreateAssetMenu(menuName = "LLNet/Messages/ConnectionAck")]
public class Message_ConnectionAck : NetMessage {
	public override void Client_ReceiveMessage(Byterizer data, LLClient client) {

		// Popping moves the index
		int connectionId = data.PopInt32();
		NetUser newUser = new NetUser() {
			ConnectionId = connectionId,
			UserName = client.UserName,
			TeamNumber = client.TeamNumber
		};

		client.NetUsers[connectionId] = newUser;
		client.MyConnectionId = connectionId;

		Byterizer byterizer = new Byterizer();
		byterizer.Push((byte)NetMessageType.USER_INFO);
		byterizer.Push(client.UserName);
		byterizer.Push(client.TeamNumber);

		client.SendNetMessage(client.ReliableChannel, byterizer.GetBuffer());
		Debug.Log($"@Client -> MyConnectionId [{connectionId}]");
	}

	public override void Server_ReceiveMessage(int connectionId, Byterizer data, LLServer server) {

	}
}