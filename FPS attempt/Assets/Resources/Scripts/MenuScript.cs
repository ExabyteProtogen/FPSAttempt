using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Discovery;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : NetworkBehaviour
{
	
	public GameObject networkObject;
	public GameObject JoinButton;
	public GameObject IPfield;
	public GameObject WeaponField;
	public string Weapon;
	//public PlayerController localPlayerController;
	
	private NewNetworkManager manager;
	private NetworkDiscovery discovery;
	private InputField field;
	
	void Start()
	{
		manager = FindObjectOfType<NewNetworkManager>();
		//localPlayerController = FindObjectOfType<PlayerController>();
	}
	
	/*
	public void Connect(ServerResponse info)
    {
        NetworkManager.singleton.StartClient(info.uri);
    }
	
	public void OnDiscoveredServer(ServerResponse info)
    {
        discoveredServers[info.serverId] = info;
    }
	*/
	
	public void Join()
	{
		manager = networkObject.GetComponent<NewNetworkManager>();
		field = IPfield.GetComponent<InputField>();
        manager.networkAddress = field.text;
		field = WeaponField.GetComponent<InputField>();
		manager.weaponChoice = field.text;
		//localPlayerController.wepField = WeaponField;
		manager.StartClient();
	}
	
	public void Host()
	{
		manager = networkObject.GetComponent<NewNetworkManager>();
		field = WeaponField.GetComponent<InputField>();
		manager.weaponChoice = field.text;
		manager.StartHost();
	}
	/*
	public void OnClientConnect()
	{
		field = WeaponField.GetComponent<InputField>();
		
		ChooseWeapon weaponMessage = new ChooseWeapon
		{
			wep = field.text
		};
		NetworkClient.Send(weaponMessage);
	}
	*/
	/*
	public void DiscoverServers()
	{
		discoveredServers.Clear();
		discovery.StartDiscovery();
	}
	*/

    // Update is called once per frame
    void Update()
    {
        
    }
}
