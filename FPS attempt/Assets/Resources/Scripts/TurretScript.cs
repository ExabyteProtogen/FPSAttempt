using System.Collections;
using UnityEngine;
using Mirror;

public class TurretScript : Interactable
{
    private PlayerController userController;
    [SyncVar]
    private float rotX;
    [SyncVar]
    private float rotY;
    [SyncVar]
    private float lastRotY;
    private GameObject b;
    private Vector3 angleModif;
    public GameObject hitparticles;
    private PlayerController targetScript;
    private float lastShot;
    private Animator animator;
    private bool spanUp;
    private bool spinningUp;
    private bool spinningDown;
    private float initalWait;
    public AudioClip fireSound;
    public AudioClip spinDownSound;
    public AudioClip spinUpSound;
    public AudioSource source;
    private bool fireSoundPlaying = false;
    public GameObject builder;
    //*
    void RpcPlaySound(string clipName)
    {
        source.clip = fireSound;
        source.Play();
        fireSoundPlaying = true;
    }

    [Command]
    void CmdPlaySound(string clipName)
    {
        RpcPlaySound(clipName);
    }
    //*/

    IEnumerator WaitForSpinup()
    {
        source.PlayOneShot(spinUpSound, 1f);
        yield return new WaitForSeconds(0.66f);
        spanUp = true;
        StartCoroutine("FireCoroutine");
    }
    IEnumerator WaitForSpindown()
    {
        source.PlayOneShot(spinDownSound, 1f);
        yield return new WaitForSeconds(1f);
        spinningDown = false;
    }
    IEnumerator FireCoroutine()
    {
        if (Time.time > lastShot + 0.03 & spanUp)
        {
            
            angleModif = new Vector3(Random.Range(-0.02f, 0.02f), Random.Range(-0.02f, 0.02f), 0);
		
        	RaycastHit hit;
			Physics.Raycast(gameObject.transform.GetChild(1).transform.GetChild(3).transform.position, gameObject.transform.GetChild(1).transform.GetChild(3).transform.TransformDirection(Vector3.forward + angleModif), out hit, 1000);
            if (!fireSoundPlaying)
            {
                fireSoundPlaying = true;
                //source.clip = fireSound;
                //source.Play();
                CmdPlaySound(fireSound.name);
            }
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
					GameObject target = hit.collider.gameObject;
					targetScript = target.GetComponent<PlayerController>();
					if (user.GetComponent<PlayerController>().team == Teams.None | (targetScript.team != user.GetComponent<PlayerController>().team))
					{
						CmdDamageTarget(target, 3);
						if (!(user.GetComponent<WeaponController>().hitSound == null))
						{
							userController.audioSource.PlayOneShot(user.GetComponent<WeaponController>().hitSound, user.GetComponent<WeaponController>().hitSoundVolume);
						}
					}
				}
			}
			catch
			{
				yield break;
			}
            lastShot = Time.time;
        }
    }
    void Start()
    {
        displayOnHover = "Mount Turret";
        animator = gameObject.transform.GetChild(1).gameObject.GetComponent<Animator>();
    }

    [Command(ignoreAuthority = true)]
    public void CmdDamageTarget(GameObject target, int damage)
	{
		targetScript = target.GetComponent<PlayerController>();
		targetScript.hit(damage);
	}

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(spanUp);
        if (!(user == null))
        {
            if (userController == null)
            {
                userController = user.GetComponent<PlayerController>();
            }
            userController.cameraLock = true;

            rotY += Input.GetAxis("Mouse X") * userController.sensitivity;
            rotX += Input.GetAxis("Mouse Y") * userController.sensitivity;
			rotX = Mathf.Clamp(rotX, -45, 45);
			userController.cam.transform.eulerAngles = new Vector3(-rotX, rotY, 0);

            gameObject.transform.GetChild(0).eulerAngles = new Vector3(0, rotY, 0);
            gameObject.transform.GetChild(1).eulerAngles = new Vector3(-rotX, rotY, 0);

            user.transform.RotateAround(gameObject.transform.position, Vector3.up, rotY - lastRotY);
            lastRotY = rotY;

            if (Input.GetAxis("Fire1") == 1 & hasAuthority)
            {
                if (!spanUp & !spinningUp & !spinningDown)
                {
                    animator.Play("Base Layer.TurretSpinup");
                    spinningUp = true;
                    //spanUp = true;
                    //initalWait = 0.66f;
                    StartCoroutine("WaitForSpinup");
                }
                else
                {
                    //initalWait = 0f;
                    StartCoroutine("FireCoroutine");
                }
            }
            else
            {
                if (spanUp)
                {
                    animator.Play("Base Layer.TurretSpindown");
                    source.Stop();
                    fireSoundPlaying = false;
                    spinningDown = true;
                    spanUp = false;
                    spinningUp = false;
                    StartCoroutine("WaitForSpindown");
                }
            }
            
        }
        if (Input.GetKey(KeyCode.Escape) & user != null)
        {
            UnsetUser(user);
        }
    }

    void OnUse()
    {
        //Debug.Log("Turret is being used");
    }
}
