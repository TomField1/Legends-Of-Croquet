using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TextScript : MonoBehaviour {

	//This script exists so we can call messages for a specific length of time
	//Note that since only one message and end time is stored, you can overwrite the message at any point and
	//it will still display for the correct amount of time

	float messageEndTime; //The time the message should stop displaying
	Text messageField; //The text field that should display the message

	// Use this for initialization
	void Start () {
		messageField = GetComponent<Text> ();
	}
	
	// Update is called once per frame
	void Update () {
		//once the time has passed the end time, set the message text to blank.
		if ((float)Time.fixedTime > messageEndTime) {
			messageField.text = "";
		}
	}

	//Display a message
	public void DisplayMessage(string message, int time){
		//set the end time to the current time, plus the time to display for
		messageEndTime = (float)Time.fixedTime + time;

		//display the message
		messageField.text = message;
	}
}
