using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PauseScript : NetworkBehaviour
{
	public GameObject ui;
	private bool escHeld = false;
	private bool UIenabled = false;
	
	void Start()
	{
		ui.SetActive(false);
	}
	/*
    // Update is called once per frame
    void Update()
    {
		if (!isLocalPlayer)
		{
			return;
		}
        if (Input.GetKey(KeyCode.Escape) & escHeld == false)
		{
			if (!UIenabled)
			{
				Debug.Log("Pause Menu Opened");
				ui.SetActive(true);
				Cursor.lockState = CursorLockMode.None;
				UIenabled = true;
			}
			else
			{
				Debug.Log("Pause Menu Closed");
				ui.SetActive(false);
				Cursor.lockState = CursorLockMode.Locked;
				UIenabled = false;
			}
			escHeld = true;
		}
		if (!Input.GetKey(KeyCode.Escape))
		{
			escHeld = false;
		}
    }
	//*/
	
	public void openPauseMenu()
	{
		ui.SetActive(true);
		Cursor.lockState = CursorLockMode.None;
	}
	
	public void closePauseMenu()
	{
		ui.SetActive(false);
		Cursor.lockState = CursorLockMode.Locked;
	}
}
