using UnityEngine;
using System.Collections;

public class FlowerbedScript : MonoBehaviour {

	////////////////////////////////////////////////////////////////// ON TRIGGER ENTER
	/// When a ball enters the flowerbed
	void OnTriggerEnter2D(Collider2D other){

		//if the other object is a ball
		GameObject ball = other.gameObject;
		if (ball.CompareTag("ball")){

			//set the Drag to 2
			ball.GetComponent<BallScript> ().SetDrag (2);
		}
	}

	////////////////////////////////////////////////////////////////// ON TRIGGER EXIT
	/// when a ball leaves the flowerbed
	void OnTriggerExit2D(Collider2D other){

		//if the other object is a ball
		GameObject ball = other.gameObject;
		if (ball.CompareTag("ball")){

			//set the Drag to 1
			ball.GetComponent<BallScript> ().SetDrag (1);
		}
	}
}
