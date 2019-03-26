using System;
using System.Collections.Generic;
using UnityEngine;

namespace LLNet {
	[CreateAssetMenu(menuName = "LLNet/NetMessageContainer")]
	public class NetMessagesContainer : ScriptableObject {
		[SerializeField]
		private NetMessage[] _NetMessages;

		public Dictionary<NetMessageType, NetMessage> NetMessagesMap { get; private set; }

		private void OnEnable() {
			MapMessages();
		}

		public void MapMessages() {
			NetMessagesMap = new Dictionary<NetMessageType, NetMessage>();

			foreach (var item in _NetMessages) {
				if (NetMessagesMap.ContainsKey(item.MessageType)) {
					Debug.LogWarning($"Duplicate Message [{item.MessageType.ToString()}]", item);
				} else {
					NetMessagesMap[item.MessageType] = item;
				}
			}

			Debug.Log($"Mapping Net Messages Done! -> Added{NetMessagesMap.Count} messages...");
		}
	}
}
