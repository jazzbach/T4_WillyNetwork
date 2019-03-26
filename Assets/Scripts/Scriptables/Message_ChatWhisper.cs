using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LLNet;

[CreateAssetMenu(menuName = "LLNet/Messages/ChatWhisper")]
public class Message_ChatWhisper : NetMessage {

	public override void Client_ReceiveMessage(Byterizer data, LLClient client) {

		int userKey = data.PopInt32();
		string userName = data.PopString();

		Debug.Log($"Server whisper > {userName}");
		
		client.AddMessageToQueue($"WHISPER FROM {userName}");
	}

	public override void Server_ReceiveMessage(int connectionId, Byterizer data, LLServer server) {

		int userKey = data.PopInt32();
		string userName = data.PopString();

		Debug.Log($"Server whisper > {userName}");
		
		server.SendNetMessage(userKey, server._ReliableChannel, data.GetBuffer());
	}
}
