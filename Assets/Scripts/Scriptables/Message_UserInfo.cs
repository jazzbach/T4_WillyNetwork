using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LLNet;

[CreateAssetMenu(menuName = "LLNet/Messages/UserInfo")]
public class Message_UserInfo : NetMessage {

	public override void Client_ReceiveMessage(Byterizer data, LLClient client) {

		string name = data.PopString();
		int teamNumber = data.PopInt32();
		int connectionId = data.PopInt32();

		NetUser newUser = new NetUser {
			ConnectionId = connectionId,
			UserName = name,
			TeamNumber = teamNumber
		};

		client.NetUsers[connectionId] = newUser;

		Debug.Log($"{name} connected to the server");


	}

	public override void Server_ReceiveMessage(int connectionId, Byterizer data, LLServer server) {

		// Client's name
		server._NetUsers[connectionId].UserName = data.PopString();

		// Client's team number
		server._NetUsers[connectionId].TeamNumber = data.PopInt32();

		// Before resetting the index, we must add the connection id to the
		// data
		data.Push(connectionId);

		foreach (var item in server._NetUsers) {
			server.SendNetMessage(item.Value.ConnectionId, server._ReliableChannel, data.GetBuffer());
		}
	}
}
