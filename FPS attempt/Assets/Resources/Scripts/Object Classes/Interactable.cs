using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Interactable : NetworkBehaviour
{
    [SyncVar]
    public GameObject user;
    public string displayOnHover;
    [SyncVar]
    private bool inUse;

    public void OnUse(GameObject _user)
    {
        if (inUse)
        {
            if (_user == user)
            {
                UnsetUser(_user);
                return;
            }
        }
        inUse = true;
        user = _user;
        PlayerController ctrlr = _user.GetComponent<PlayerController>();
        ctrlr.cameraLock = true;
        ctrlr.movementLock = true;
        ctrlr.weaponLock = true;
    }

    public void UnsetUser(GameObject _user)
    {
        PlayerController ctrlr = _user.GetComponent<PlayerController>();
        ctrlr.cameraLock = false;
        ctrlr.movementLock = false;
        ctrlr.weaponLock = false;
        user = null;
        inUse = false;
    }

    public void UserUpdate()
    {

    }

    /*
    void Update()
    {
        
    }
    //*/
}
