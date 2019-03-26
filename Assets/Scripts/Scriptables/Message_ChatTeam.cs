using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LLNet;

[CreateAssetMenu(menuName = "LLNet/Messages/ChatTeam")]
public class Message_ChatTeam : NetMessage {

	public override void Client_ReceiveMessage(Byterizer data, LLClient client) {

		string userName = data.PopString();
		int teamNumber = data.PopInt32();
		string message = data.PopString();

		Debug.Log($"Server broadcast team > {userName} at team {teamNumber.ToString()} says : {message}");

		// Add mesage to queue in order to show the message to the client
		client.AddMessageToQueue($"TEAM MESSAGE: {userName} says: {message}");
	}

	public override void Server_ReceiveMessage(int connectionId, Byterizer data, LLServer server) {

		string userName = data.PopString();
		int teamNumber = data.PopInt32();
		string message = data.PopString();

		Debug.Log($"Server broadcast team > {userName} at team {teamNumber.ToString()} says : {message}");

		foreach (var item in server._NetUsers) {

			if (teamNumber == item.Value.TeamNumber) {
				server.SendNetMessage(item.Value.ConnectionId, server._ReliableChannel, data.GetBuffer());
			}
		}
	}
}
