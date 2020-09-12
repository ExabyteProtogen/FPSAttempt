using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class WeaponController : NetworkBehaviour
{
	[Header("Weapon Performance Variables")]
	public float rof;
	public int damage;
	public int range;
	public float inaccuracy;
	public int pellets;
	public int magCap;
	public int mag;

	[Header("Weapon Settings")]
	public bool isSemiAuto;
	public bool usesAmmo = true;
	public int maxSpareAmmo;
	public int spareAmmo;
	public float reloadTime;
	public float earlyReloadTime;
	public bool fullReload;
	public GameObject model;
	public string gunName;
	
	//Sounds
	[Header("Audio Settings")]
	public AudioSource audioSource;
	public AudioClip hitSound;
	public AudioClip fireSound;
	public AudioClip reloadSound;
	public AudioClip earlyReloadSound;
	public AudioClip finishReloadSound;
	
	//Sound Volumes
	public float hitSoundVolume = 1;
	public float fireSoundVolume = 1;
	public float reloadSoundVolume = 1;
	public float earlyReloadSoundVolume = 1;

	//Misc Stuff
	[Header("Misc Stuff")]
	public Camera cam;
	public GameObject hitparticles;
	
	private Animator anim;
	private GameObject target;
	private PlayerController targetScript;
	private PlayerController playerController;
	private float lastShot;
	private bool semiAutoHold;
	private GameObject b;
	private Vector3 angleModif;
	private bool isReloading;
	private int magDiff;
	private bool escHeld;
	private bool UIenabled;
	//*
	[ClientRpc]
	void RpcPlayFireSound(GameObject source, float volume)
	{
		AudioSource soundSrc = source.GetComponent<AudioSource>();
		soundSrc.PlayOneShot(fireSound, volume);
	}
	//*/
	
	//*
	[Command]
	void CmdPlayFireSound(GameObject source, float volume)
	{
		AudioSource soundSrc = source.GetComponent<AudioSource>();
		soundSrc.PlayOneShot(fireSound, volume);
		RpcPlayFireSound(source, volume);
	}
	//*/
	
	IEnumerator ReloadCoroutine()
	{
		if (spareAmmo == 0 | mag == magCap | Time.time < lastShot + rof & !UIenabled)
		{
			yield break;
		}
		isReloading = true;
		magDiff = magCap - mag;
		if (fullReload)
		{
			if (magDiff == magCap)
			{
				anim.Play("Base Layer." + gunName + "Reload");
				if (!(reloadSound == null))
				{
					audioSource.clip = reloadSound;
					audioSource.volume = reloadSoundVolume;
					audioSource.Play();
					audioSource.priority = 64;
				}
				yield return new WaitForSeconds(reloadTime);
			}
			else
			{
				anim.Play("Base Layer." + gunName + "ReloadEarly");
				if (!(earlyReloadSound == null))
				{
					audioSource.clip = earlyReloadSound;
					audioSource.volume = earlyReloadSoundVolume;
					audioSource.Play();
					audioSource.priority = 64;
				}
				yield return new WaitForSeconds(earlyReloadTime);
			}
			mag = System.Math.Min(magCap, spareAmmo);
			spareAmmo -= System.Math.Min(magDiff, spareAmmo);
		}
		else
		{
			anim.Play("Base Layer." + gunName + "StartReload");
			while(mag < magCap & spareAmmo > 0)
			{
				yield return new WaitForSeconds(reloadTime);
				anim.Play("Base Layer." + gunName + "Reload");
				if (!(reloadSound == null))
				{
					audioSource.clip = reloadSound;
					audioSource.volume = reloadSoundVolume;
					audioSource.Play();
				}
				mag += 1;
				spareAmmo -= 1;
			}
			yield return new WaitForSeconds(reloadTime);
			if (magDiff == magCap)
			{
				anim.Play("Base Layer." + gunName + "FinishReload");
				audioSource.PlayOneShot(finishReloadSound, reloadSoundVolume);
			}
			else
			{
				anim.Play("Base Layer." + gunName + "FinishReloadEarly");
			}
		}
		isReloading = false;
	}
	
	[Command]
	public void CmdDamageTarget(GameObject target, int damage)
	{
		targetScript = target.GetComponent<PlayerController>();
		targetScript.hit(damage);
	}
    // Start is called before the first frame update
    void Start()
    {
        anim = model.GetComponent<Animator>();
		Debug.Log(gunName);
		playerController = gameObject.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
		//model.transform.position += model.transform.up * Mathf.Sin(Time.time * 2f) * 10f;
		
		if (!hasAuthority)
		{
			return;
		}
		
        if (Input.GetAxis("Fire1") == 1 && Time.time > lastShot + rof && !semiAutoHold && mag != 0 && !isReloading & !UIenabled & !playerController.weaponLock)
		{
			lastShot = Time.time;
			
			if (isSemiAuto)
			{
				semiAutoHold = true;
			}
			
			if (usesAmmo)
			{
				mag -= 1;
			}
			
			//Animation
			anim.Play("Base Layer." + gunName + "Fire");
			//*
			if (!(fireSound == null))
			{
				CmdPlayFireSound(gameObject, fireSoundVolume);
			}
			//*/
			for (int i = 0; i < pellets; i++)
			{
				angleModif = new Vector3(Random.Range(-inaccuracy, inaccuracy), Random.Range(-inaccuracy, inaccuracy), 0);
				
				RaycastHit hit;
				Physics.Raycast(cam.transform.position, cam.transform.TransformDirection(Vector3.forward + angleModif), out hit, range);
				
				/*
				Instantiate(hitsphere, hit.point, Quaternion.identity);
				//*/
				try
				{
					b = Instantiate(hitparticles, hit.point + hit.normal / 10, Quaternion.identity);
					//Detect if Target is actually able to take damage...
					if (hit.collider.gameObject.tag == "Player")
					{
						//...and deal it if it can
						target = hit.collider.gameObject;
						targetScript = target.GetComponent<PlayerController>();
						if (gameObject.GetComponent<PlayerController>().team == Teams.None | (targetScript.team != gameObject.GetComponent<PlayerController>().team))
						{
							CmdDamageTarget(target, damage);
							if (!(hitSound == null))
							{
								audioSource.PlayOneShot(hitSound, hitSoundVolume);
							}
						}
					}
				}
				catch
				{
					return;
				}
			}
			
		}
		if (Input.GetAxis("Fire1") == 0)
		{
			semiAutoHold = false;
		}
		
		if (Input.GetKey("r"))
		{
			if (!isReloading & !playerController.weaponLock)
			{
				StartCoroutine(ReloadCoroutine());
			}
		}
		
		if (Input.GetKey(KeyCode.Escape) & escHeld == false)
		{
			if (!UIenabled)
			{
				UIenabled = true;
			}
			else
			{
				UIenabled = false;
			}
			escHeld = true;
		}
		if (!Input.GetKey(KeyCode.Escape))
		{
			escHeld = false;
		}
    }
}
