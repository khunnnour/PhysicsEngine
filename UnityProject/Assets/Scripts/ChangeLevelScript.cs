using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeLevelScript : MonoBehaviour
{
	public enum CheckWhat { KeyDown = 0, MouseDown = 1 }

	[Header("How to Change Level")]
	[Tooltip("What input you want the change level script to wait for")]
	public CheckWhat WhatToCheck = CheckWhat.MouseDown;
	public bool rightMouseButton = false;
	public KeyCode keyDown;

	[Header("When and where")]
	public bool waitForGameEnd = false;
	public string nextLevel;

	// Update is called once per frame
	void Update()
	{
		bool canChangeLevel = true;
		if (waitForGameEnd)
			canChangeLevel = !GameManager.instance.GetGameRunning();

		if (canChangeLevel)
			if (GetInput())
				SceneManager.LoadScene(nextLevel);
	}

	bool GetInput()
	{
		if (WhatToCheck == CheckWhat.MouseDown)
		{
			if (rightMouseButton)
				return Input.GetMouseButtonDown(1);
			else
				return Input.GetMouseButtonDown(0);
		}
		else if (WhatToCheck == CheckWhat.KeyDown)
		{
			return Input.GetKeyDown(keyDown);
		}
		else
		{
			Debug.LogError("ChangeLevelScript.cs | ERROR: Invalid input check value; cannot detect input");
		}

		// Shouldn't get this far here
		return false;
	}
}
