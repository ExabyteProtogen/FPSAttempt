using System;
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
	public Text interactionText;
	
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
	public bool cameraLock = false;
	public bool movementLock = false;
	public bool weaponLock = false;
	private GameObject[] spawns;
	public GameObject buildablePrefab;

	//Character Specific Things
	[Header("Character Specifics")]
	
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
	private bool qHeld;
	private PauseScript pauseScript;
	private Camera targetCam;
	private float lightIntensity = 0;
	private bool buildableSpawned;
	private GameObject turret;
	private RaycastHit rayHit;
	private Interactable targetScript;
	private bool eHeld = false;
	public string character;
	//public Material materialRed;
	//public Material materialBlue;

	[Command]
    void CmdGiveAuthority(GameObject _target)
    {
        _target.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
    }

	[ClientRpc]
	void RpcSyncTurretSpawn(GameObject obj)
	{
		turret = obj;
	}

	[Command]
	void CmdPlaceBuildable(Vector3 pos, Vector3 normal)
	{
		GameObject turretObj = Instantiate(buildablePrefab, pos, Quaternion.FromToRotation(Vector3.up, normal) * gameObject.transform.rotation);
		NetworkServer.Spawn(turretObj, connectionToClient);
		RpcSyncTurretSpawn(turretObj);
		return;
	}

	void despawnBuildable(GameObject _turret)
	{
		Destroy(_turret);
		return;
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
		
		gameObject.transform.GetChild(1).gameObject.SetActive(true);
		
        cam.fieldOfView = fov;
		lightcomp = flashlight.GetComponent<Light>();
		audioListener = cam.gameObject.GetComponent<AudioListener>();
		pauseScript = FindObjectOfType<PauseScript>();
		if (!(playerDecoObject == null))
		{
			playerDecoObject.layer = 9;
		}

		Debug.Log(team.ToString() + "Spawn");
		spawns = GameObject.FindGameObjectsWithTag(team.ToString() + "Spawn");

		Debug.Log(team);

		//CmdSetTeamColors(gameObject, team);
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
		Physics.Raycast(cam.transform.position, cam.transform.TransformDirection(Vector3.forward), out rayHit, 1.5f);
		//Debug.Log(rayHit.transform.gameObject);
		try
		{
			targetScript = rayHit.collider.gameObject.GetComponentInParent<Interactable>();
		}
		catch (NullReferenceException)
		{
			interactionText.text = "";
			interactionText.enabled = false;
			targetScript = null;
			//Debug.Log("No interaction script found.");
		}
		finally
		{
			if (!(targetScript == null))
			{
				interactionText.text = targetScript.displayOnHover;
				interactionText.enabled = true;
				//Debug.Log(targetScript.gameObject);
			}
			else
			{
				interactionText.text = "";
				interactionText.enabled = false;
			}
		}
		/*
		if (!(targetScript == null))
		{
			interactionText.text = targetScript.displayOnHover;
			interactionText.enabled = true;
			//Debug.Log(targetScript.gameObject);
		}
		else
		{
			interactionText.text = "";
			interactionText.enabled = false;
			//Debug.Log("No interaction script found.");
		}
		//*/

		//Check if ded
		if (health <= 0)
		{
			try
			{
				int spawnPointIndex = UnityEngine.Random.Range(0, spawns.Length);
				Vector3 spawnPoint = spawns[spawnPointIndex].transform.position;
				CmdRespawn(gameObject, spawnPoint);
				targetScript.UnsetUser(gameObject);
			}
			catch
			{
				return;
			}
			/*
			Debug.Log(spawns.Count);
			int spawnPointIndex = UnityEngine.Random.Range(0, spawns.Count);
			Vector3 spawnPoint = spawns[spawnPointIndex].position;
			CmdRespawn(gameObject, spawnPoint);
			Debug.Log("Respawning Player");
			//*/
			
		}
		
		//UI
		healthText.text = health.ToString();
		magText.text = gameObject.GetComponent<WeaponController>().mag + "/" + gameObject.GetComponent<WeaponController>().magCap;
		reserveText.text = gameObject.GetComponent<WeaponController>().spareAmmo.ToString();
		
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
		
			if (movementLock == false)
			{
				transform.Translate(horizontalMovement, 0, verticalMovement);
				transform.eulerAngles = new Vector3(0, rotY, 0);
			}
			
			if (canJump == true & Time.time > lastJump + jumpDelay)
			{
				float jumpMovement = Input.GetAxis("Jump") * jumpHight;
				RigidBody.AddForce(0, jumpMovement, 0, ForceMode.Impulse);
				lastJump = Time.time;
			}

			if (cameraLock == false)
			{
				rotX += Input.GetAxis("Mouse Y") * sensitivity;
				rotX = Mathf.Clamp(rotX, -90, 90);
				cam.transform.eulerAngles = new Vector3(-rotX, rotY, 0);
			}
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
		
		if (Input.GetKey("p") & pHeld == false)
		{
			{	
				//Todo: Try using a cooldown instead.
				int spawnPointIndex = UnityEngine.Random.Range(0, spawns.Length);
				Vector3 spawnPoint = spawns[spawnPointIndex].transform.position;
				CmdRespawn(gameObject, spawnPoint);
				try
				{
					targetScript.UnsetUser(gameObject);
				}
				catch
				{
					return;
				}
				pHeld = true;
			}
		}
		if (!Input.GetKey("p"))
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
		//*
		if (Input.GetKey("q") & qHeld == false)
		{
			qHeld = true;
			switch (character)
			{
				case "Turret":
				{
					if (buildableSpawned)
					{
						if (targetScript != null && targetScript.GetType().Name == "TurretScript")
						{
							buildableSpawned = false;
							despawnBuildable(turret);
						}
					}
					else
					{
						bool hitCollider = Physics.Raycast(cam.transform.position, cam.transform.TransformDirection(Vector3.forward), out rayHit, 5f);
						if (hitCollider)
						{
							if (Vector3.Angle(rayHit.normal, Vector3.up) == 0)
							{
								CmdPlaceBuildable(rayHit.point, rayHit.normal);
								buildableSpawned = true;
							}
						}
					}
					return;
				}

				case "Crate":
				{
					if (buildableSpawned)
					{
						if (targetScript != null && targetScript.GetType().Name == "CrateScript")
						{
							CrateScript targetScript = (CrateScript)gameObject.GetComponent<PlayerController>().targetScript;
							if (!targetScript.onCooldown)
							{
								buildableSpawned = false;
								despawnBuildable(turret);
							}
						}
					}
					else
					{
						bool hitCollider = Physics.Raycast(cam.transform.position, cam.transform.TransformDirection(Vector3.forward), out rayHit, 5f);
						if (hitCollider)
						{
							if (Vector3.Angle(rayHit.normal, Vector3.up) == 0)
							{
								CmdPlaceBuildable(rayHit.point, rayHit.normal);
								buildableSpawned = true;
							}
						}
					}
					return;
				}

				case "Medkit":
				{
					if (buildableSpawned)
					{

					}
					return;
				}

				default:
				{
					return;
				}
			}
		}
		if (!Input.GetKey("q"))
		{
			qHeld = false;
		}
		//*/
		if (Input.GetKey("e") & targetScript != null & eHeld == false)
		{
			eHeld = true;
			targetScript.OnUse(gameObject);
			//CmdGiveAuthority(targetScript.gameObject);
		}
		if (Input.GetKey("e") == false)
		{
			eHeld = false;
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
}
