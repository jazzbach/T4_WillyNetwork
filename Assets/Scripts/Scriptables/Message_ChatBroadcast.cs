using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LLNet;

[CreateAssetMenu(menuName = "LLNet/Messages/ChatBroadcast")]
public class Message_ChatBroadcast : NetMessage {

	public override void Client_ReceiveMessage(Byterizer data, LLClient client) {

		string userName = data.PopString();
		string message = data.PopString();

		Debug.Log($"Server broadcast > {userName} says : {message}");

		// Add mesage to queue in order to show the message to the client
		client.AddMessageToQueue($"BROADCAST: {userName} says: {message}");
	}

	public override void Server_ReceiveMessage(int connectionId, Byterizer data, LLServer server) {

		string userName = data.PopString();
		string message = data.PopString();

		Debug.Log($"Server broadcast > {userName} says : {message}");

		foreach (var item in server._NetUsers) {
			server.SendNetMessage(item.Value.ConnectionId, server._ReliableChannel, data.GetBuffer());
		}

	}
}
