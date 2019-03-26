using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using LLNet;

public class ServerClientUIButton : MonoBehaviour {

	public LLServer Server;
	public LLClient Client;

	public Button ClientButton;
	public Button ServerButton;

	[Header("User Text Edit")]
	public Text ServerText;
	public Text UserNameText;
	public Text TeamText;

	[Header("User InputField")]
	public InputField ServerInputField;
	public InputField UserNameInputField;
	public InputField TeamInputField;

	[Header("Toggles")]
	public Toggle ServerToggle;
	public Toggle UserNameToggle;
	public Toggle TeamNumberToggle;

	[Header("Toggles Text")]
	public Text ServerToggleText;
	public Text UserNameToggleText;
	public Text TeamNumberToggleText;

	[Header("LLClient")]
	public LLClient client;

	private void Start() {

		// Set override toggle button text
		ServerToggleText.text = $"Override: {client._ServerAddress}";
		UserNameToggleText.text = $"Override: {client.UserName}";
		TeamNumberToggleText.text = $"Override: {client.TeamNumber}";

		// Set toggle at the beginning
		OnServerToggle();
		OnUserNameToggle();
		OnTeamToggle();
	}

	// Button OnClick method
	public void OnServerButtonClick() {
		DisableButtons();
		OverrideClientValuesIfToggles();

		// Enabling this GameObject has to be done AT THE END of the method
		Server.gameObject.SetActive(true);
	}

	// Button OnClick method
	public void OnClientButtonClick() {
		DisableButtons();
		OverrideClientValuesIfToggles();

		// Enabling this GameObject has to be done AT THE END of the method
		Client.gameObject.SetActive(true);
	}

	// Toggle OnValueChanged(boolean) method
	public void OnServerToggle() {
		ServerInputField.interactable = ServerToggle.isOn;
	}

	// Toggle OnValueChanged(boolean) method
	public void OnUserNameToggle() {
		UserNameInputField.interactable = UserNameToggle.isOn;
	}

	// Toggle OnValueChanged(boolean) method
	public void OnTeamToggle() {
		TeamInputField.interactable = TeamNumberToggle.isOn;
	}

	private void OverrideClientValuesIfToggles() {
		if (ServerToggle.isOn) { client._ServerAddress = ServerText.text.Trim(); }
		if (UserNameToggle.isOn) { client.UserName = UserNameText.text.Trim(); }
		if (TeamNumberToggle.isOn) { client.TeamNumber = int.Parse(TeamText.text.Trim()); }
	}

	private void DisableButtons() {
		ClientButton.interactable = false;
		ServerButton.interactable = false;
	}
}
