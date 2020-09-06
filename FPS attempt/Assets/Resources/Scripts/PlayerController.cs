﻿using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public enum Teams
{
	Red,
	Blue,
    None
}

public class PlayerController : NetworkBehaviour
{
	//Movement Variables
	[Header("Movement Variables")]
	public float moveSpeed;
	public float jumpHight;
	public float jumpDelay;
	public Rigidbody RigidBody;
	public float sprintMultiplier;
	
	//Camera Variables
	[Header("Camera Variables")]
	public float sensitivity;
	[Range(10, 150)]
	public int fov;
	public Camera cam;
	public GameObject camObj;
	public GameObject flashlight;
	
	//Combat Variables
	[Header("Combat Variables")]
	[SyncVar]
	public int health;
	public int maxHealth;
	public GameObject weapon;
	[SyncVar]
	public Teams team = Teams.None;
	
	//UI Variables
	[Header("UI Variables")]
	public Text healthText;
	public Text magText;
	public Text reserveText;
	public GameObject pauseScriptObj;
	
	//Voice Variables
	[Header("Voice Variables")]
	public AudioSource audioSource;
	//public List<AudioClip> killLines;
	public AudioClip killLine;
	
	//Misc Variables
	[Header("Misc Stuff")]
	public GameObject wepField;
	public List<Vector3> startPositions;
	public GameObject playerDecoObject;
	[SyncVar]
	public string playerName;
	
	private InputField field;
	private AudioListener audioListener;
	private float speed;
	private Light lightcomp;
	private bool fHeld;
	private bool pHeld;
	private float lastJump;
	private bool canJump = false;
	private float rotY;
	[SyncVar]
	private float rotX;
	private bool UIenabled;
	private bool escHeld;
	private PauseScript pauseScript;
	private Camera targetCam;
	private float lightIntensity = 0;
	//public Material materialRed;
	//public Material materialBlue;

	[ClientRpc]
	void RpcSetTeamColors(GameObject obj, Teams team)
	{
		Debug.Log("(Rpc) Setting player color to " + team.ToString());
		Debug.Log(obj.name);
		Debug.Log(team.ToString());
		Debug.Log("Materials/Playermodel" + team.ToString());
		Renderer renderer = obj.GetComponent<Renderer>();
		Debug.Log(renderer);
		Debug.Log((Material)Resources.Load("Materials/Playermodel" + team.ToString()));
		renderer.material = (Material)Resources.Load("Materials/Playermodel" + team.ToString());
		/*
		foreach (Transform child in transform)
		{
			GameObject gameObj = child.gameObject;
			Renderer renderer = gameObj.GetComponent<Renderer>();
			//if (!(renderer == null))
			//{
				switch (team)
				{
					case Teams.Red:
						renderer.material = materialRed;
						break;
					case Teams.Blue:
						renderer.material = materialBlue;
						break;
					default:
						break;
				}
			//}
		}
		//*/
	}

	[Command]
	void CmdSetTeamColors(GameObject obj, Teams team)
	{
		Debug.Log("(Cmd) Setting player color to " + team.ToString());
		Debug.Log(obj.name);
		Debug.Log(team.ToString());
		Debug.Log("Materials/Playermodel" + team.ToString());
		Renderer renderer = obj.GetComponent<Renderer>();
		Debug.Log(renderer);
		Debug.Log((Material)Resources.Load("Materials/Playermodel" + team.ToString()));
		renderer.material = (Material)Resources.Load("Materials/Playermodel" + team.ToString());
		//Renderer renderer = obj.GetComponent<Renderer>();
		//renderer.material = (Material)Resources.Load("Materials/Playermodel" + team.ToString());
		/*
		foreach (Transform child in transform)
		{
			GameObject gameObj = child.gameObject;
			Renderer renderer = gameObj.GetComponent<Renderer>();
			//if (!(renderer == null))
			//{
				switch (team)
				{
					case Teams.Red:
						Debug.Log("Red team found, using Red Material");
						renderer.material = materialRed;
						break;
					case Teams.Blue:
						Debug.Log("Blue team found, using Blue Material");
						renderer.material = materialBlue;
						break;
					default:
						Debug.Log("No team found, using default Material");
						break;
				}
			//}
		}
		//*/
		RpcSetTeamColors(obj, team);
	}

	[ClientRpc]
	void RpcToggleFlashlight(GameObject target)
	{
		Transform lightObj = target.transform.GetChild(0).GetChild(0);
		Light targetLight = lightObj.GetComponent<Light>();
		if (targetLight.intensity == 0)
		{
			targetLight.intensity = 5;
		}
		else
		{
			targetLight.intensity = 0;
		}
	}
	
	[Command]
	void CmdToggleFlashlight(GameObject target)
	{
		RpcToggleFlashlight(target);
	}
	
	[ClientRpc]
	void RpcRespawn(GameObject target, Vector3 newPos)
	{
		PlayerController targetScript;
		targetScript = target.GetComponent<PlayerController>();
		target.transform.position = newPos;
		targetScript.health = targetScript.maxHealth;
		gameObject.GetComponent<WeaponController>().spareAmmo = gameObject.GetComponent<WeaponController>().maxSpareAmmo;
		gameObject.GetComponent<WeaponController>().mag = gameObject.GetComponent<WeaponController>().magCap;
	}
	
	[Command]
	void CmdRespawn(GameObject target, Vector3 spawnpoint)
	{
		PlayerController targetScript;
		targetScript = target.GetComponent<PlayerController>();
		target.transform.position = spawnpoint;
		targetScript.health = targetScript.maxHealth;
		gameObject.GetComponent<WeaponController>().spareAmmo = gameObject.GetComponent<WeaponController>().maxSpareAmmo;
		gameObject.GetComponent<WeaponController>().mag = gameObject.GetComponent<WeaponController>().magCap;
		RpcRespawn(target, spawnpoint);
		return;
	}
	
	public void hit(int damage)
	{
		health -= damage;
	}
	
	public void rewardKill()
	{
		audioSource.PlayOneShot(killLine, 1f);
	}
	
    // Start is called before the first frame update
    void Start()
    {
		if (!isLocalPlayer)
		{
			return;
		}
		if (isClient)
		{
			Cursor.lockState = CursorLockMode.Locked;
		}

		//materialRed = (Material)Resources.Load("Materials/GunPolymerRed");
		//materialBlue = (Material)Resources.Load("Materials/GunPolymerBlue");
		
		
        cam.fieldOfView = fov;
		lightcomp = flashlight.GetComponent<Light>();
		audioListener = cam.gameObject.GetComponent<AudioListener>();
		pauseScript = FindObjectOfType<PauseScript>();
		if (!(playerDecoObject == null))
		{
			playerDecoObject.layer = 9;
		}

		Debug.Log(team);

		CmdSetTeamColors(gameObject, team);
    }

    // Update is called once per frame
    void Update()
    {
		if (!isLocalPlayer)
		{
			return;
		}
		else
		{
			if (cam.enabled == false)
			{
				cam.enabled = true;
			}
			if (audioListener.enabled == false)
			{
				audioListener.enabled = true;
			}
			lightcomp.intensity = lightIntensity;
		}
		
		//Check if ded
		if (health <= 0)
		{
			CmdRespawn(gameObject, new Vector3(0, 2, 95));
			Debug.Log("Respawning Player");
		}
		
		//UI
		healthText.text = "Health: " + health;
		magText.text = "Magazine: " + gameObject.GetComponent<WeaponController>().mag + "/" + gameObject.GetComponent<WeaponController>().magCap;
		reserveText.text = "Reserve: " + gameObject.GetComponent<WeaponController>().spareAmmo;
		
		//Movement
		if (Input.GetKey(KeyCode.RightShift) | Input.GetKey(KeyCode.LeftShift))
		{
			speed = moveSpeed * sprintMultiplier;
		}
		else
		{
			speed = moveSpeed;
		}

		if (!UIenabled)
		{
			rotY += Input.GetAxis("Mouse X") * sensitivity;
		
			float horizontalMovement = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
			float verticalMovement = Input.GetAxis("Vertical") * speed * Time.deltaTime;
		
			transform.Translate(horizontalMovement, 0, verticalMovement);
			transform.eulerAngles = new Vector3(0, rotY, 0);
			
			if (canJump == true & Time.time > lastJump + jumpDelay)
			{
				float jumpMovement = Input.GetAxis("Jump") * jumpHight;
				RigidBody.AddForce(0, jumpMovement, 0, ForceMode.Impulse);
				lastJump = Time.time;
			}
			
			rotX += Input.GetAxis("Mouse Y") * sensitivity;
			rotX = Mathf.Clamp(rotX, -90, 90);
			cam.transform.eulerAngles = new Vector3(-rotX, rotY, 0);
		}
		
		//cam.transform.position += cam.transform.up * Mathf.Sin(Time.time * 2f) * 0.001f;
		
		//Flashlight
		if (Input.GetKey("f"))
		{
			if (fHeld == false)
			{
				fHeld = true;
				if (lightcomp.intensity == 0)
				{
					lightIntensity = 5;
				}
				else
				{
					lightIntensity = 0;
				}
				CmdToggleFlashlight(gameObject);
			}
		}
		else
		{
			fHeld = false;
		}
		
		if (Input.GetKey("p"))
		{
			if (pHeld == false)
			{
				pHeld = true;
				CmdRespawn(gameObject, new Vector3(0, 2, 95));
			}
		}
		else
		{
			pHeld = false;
		}
		
		if (Input.GetKey(KeyCode.Escape) & escHeld == false)
		{
			if (!UIenabled)
			{
				pauseScript.openPauseMenu();
				UIenabled = true;
			}
			else
			{
				pauseScript.closePauseMenu();
				UIenabled = false;
			}
			escHeld = true;
		}
		if (!Input.GetKey(KeyCode.Escape))
		{
			escHeld = false;
		}
    }
	
	//Activate jump ability if touching the ground, disable otherwise.
	void OnCollisionEnter(Collision collided)
	{
		if (collided.gameObject.tag == "Ground")
		{
			canJump = true;
		}
	}
	
	void OnCollisionExit(Collision collided)
	{
		if (collided.gameObject.tag == "Ground")
		{
			canJump = false;
		}
	}
	
	public void PauseMenuClose()
	{
		UIenabled = false;
	}

	void OnStartLocalPlayer()
	{
		CmdSetTeamColors(gameObject, team);
	}
}
