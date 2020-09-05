using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KillfeedController : MonoBehaviour
{
	private Text text;
	public void AddKillToFeed(string killer, string killed)
	{
		text = gameObject.GetComponent<Text>();
		text.text = killer + " killed " + killed;
	}
}
