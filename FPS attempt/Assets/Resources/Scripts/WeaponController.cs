using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class WeaponController : NetworkBehaviour
{
	public float rof;
	public int damage;
	public int range;
	
	public Camera cam;
	//public GameObject hitsphere;
	public GameObject hitparticles;
	public bool isSemiAuto;
	public bool usesAmmo = true;
	public int magCap;
	public int mag;
	public int maxSpareAmmo;
	public int spareAmmo;
	public float inaccuracy;
	public int pellets;
	public float reloadTime;
	public float earlyReloadTime;
	public bool fullReload;
	public GameObject model;
	public string gunName;
	public AudioSource audioSource;
	
	//Sounds
	public AudioClip hitSound;
	public AudioClip fireSound;
	public AudioClip reloadSound;
	public AudioClip earlyReloadSound;
	
	//Sound Volumes
	public float hitSoundVolume = 1;
	public float fireSoundVolume = 1;
	public float reloadSoundVolume = 1;
	public float earlyReloadSoundVolume = 1;
	
	private Animator anim;
	private GameObject target;
	private PlayerController targetScript;
	private float lastShot;
	private bool semiAutoHold;
	private GameObject b;
	private Vector3 angleModif;
	private bool isReloading;
	private int magDiff;
	private bool escHeld;
	private bool UIenabled;
	
	[ClientRpc]
	void RpcPlayFireSound(GameObject source, float volume)
	{
		AudioSource soundSrc = source.GetComponent<AudioSource>();
		soundSrc.PlayOneShot(fireSound, volume);
	}
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
					audioSource.PlayOneShot(reloadSound, reloadSoundVolume);
				}
				yield return new WaitForSeconds(reloadTime);
			}
			else
			{
				anim.Play("Base Layer." + gunName + "ReloadEarly");
				if (!(earlyReloadSound == null))
				{
					audioSource.PlayOneShot(earlyReloadSound, earlyReloadSoundVolume);
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
				mag += 1;
				spareAmmo -= 1;
			}
			if (magDiff == magCap)
			{
				anim.Play("Base Layer." + gunName + "FinishReload");
			}
			else
			{
				//yield return new WaitForSeconds(0.12f * (magDiff));
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
    }

    // Update is called once per frame
    void Update()
    {
		//model.transform.position += model.transform.up * Mathf.Sin(Time.time * 2f) * 10f;
		if (!hasAuthority)
		{
			return;
		}
		
        if (Input.GetAxis("Fire1") == 1 && Time.time > lastShot + rof && !semiAutoHold && mag != 0 && !isReloading & !UIenabled)
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
			if (!(fireSound == null))
			{
				CmdPlayFireSound(gameObject, fireSoundVolume);
			}
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
						CmdDamageTarget(target, damage);
						if (!(hitSound == null))
						{
							audioSource.PlayOneShot(hitSound, hitSoundVolume);
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
			if (!isReloading)
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
