using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
	//Movement Variables
	public float moveSpeed;
	public float jumpHight;
	public float jumpDelay;
	public Rigidbody RigidBody;
	public float sprintMultiplier;
	
	//Camera Variables
	public float sensitivity;
	public int fov;
	public Camera cam;
	public GameObject flashlight;
	
	//Combat Variables
	[SyncVar]
	public int health;
	public int maxHealth;
	public GameObject weapon;
	
	//UI Variables
	public Text healthText;
	public Text magText;
	public Text reserveText;
	public GameObject pauseScriptObj;
	
	//Misc Variables
	public GameObject wepField;
	public List<Vector3> startPositions;
	public GameObject playerDecoObject;
	
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
		
        cam.fieldOfView = fov;
		lightcomp = flashlight.GetComponent<Light>();
		audioListener = cam.gameObject.GetComponent<AudioListener>();
		pauseScript = FindObjectOfType<PauseScript>();
		if (!(playerDecoObject == null))
		{
			playerDecoObject.layer = 9;
		}
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
					lightcomp.intensity = 5;
				}
				else
				{
					lightcomp.intensity = 0;
				}
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
}
