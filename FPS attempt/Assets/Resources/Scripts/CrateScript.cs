using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class CrateScript : Interactable
{
    public GameObject crateTextObject;
    public int timerLength;
    private Text crateText;
    [SyncVar]
    public int cooldownTimer;
    [SyncVar]
    public bool onCooldown;

    IEnumerator CooldownCoroutine()
    {
        onCooldown = true;
        for (cooldownTimer = timerLength; cooldownTimer != 0; cooldownTimer--)
        {
            yield return new WaitForSeconds(1);
        }
        onCooldown = false;
    }
    /*
    void OnUse()
    {
        if (!onCooldown)
        {
            Debug.Log("Crate Used");
            WeaponController userWeaponScript = user.GetComponent<WeaponController>();
            userWeaponScript.spareAmmo = userWeaponScript.maxSpareAmmo;
            StartCoroutine("CooldownCoroutine");
            UnsetUser(user);
        }
        else
        {
            Debug.Log("Crate still on cooldown");
            UnsetUser(user);
            return;
        }
    }
    //*/

    void Start()
    {
        crateText = crateTextObject.GetComponent<Text>();
    }

    void Update()
    {
        if (onCooldown)
        {
            crateText.text = "On Cooldown: \n" + cooldownTimer;
        }
        else
        {
            crateText.text = "Ready!";
        }
        if (user == null)
        {
            return;
        }
        if (!onCooldown)
        {
            Debug.Log("Crate Used");
            WeaponController userWeaponScript = user.GetComponent<WeaponController>();
            userWeaponScript.spareAmmo = userWeaponScript.maxSpareAmmo;
            StartCoroutine("CooldownCoroutine");
            UnsetUser(user);
        }
        else
        {
            Debug.Log("Crate still on cooldown for another " + cooldownTimer + " seconds.");
            UnsetUser(user);
            return;
        }
    }
}
