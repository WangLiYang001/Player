using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitAPP : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if(Input.GetKeyUp(KeyCode.Escape)){
            Application.Quit();
        }
	}
	private void OnApplicationQuit()
	{
        print("App Exit!");
	}
}
