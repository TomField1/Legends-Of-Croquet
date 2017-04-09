using UnityEngine;
using System.Collections;
/*
 * This script runs for the flowers, animating them when a ball comes close enough
 * 
 **/
public class AnimateFlowerScript : MonoBehaviour {

	Animator flowerAnimator; //the animator attached to the object.

	void Start () {
		flowerAnimator = GetComponent<Animator> ();

		//rotate the flower to a random angle (So this doesn't need to be done in-editor for all of a hundred or so flowers)
		gameObject.transform.RotateAround (transform.position, Vector3.forward, Random.value * 360);
	}
		
	void OnTriggerEnter2D(Collider2D other){

		//when a ball enters the trigger
		if (other.CompareTag ("ball")) {

			//randomly decide whether to shift horizontally or vertically
			//then fire the trigger to animate in that direction
			if (Random.value <= 0.5) {
				flowerAnimator.SetTrigger ("RustleVertical");
			} else {
				flowerAnimator.SetTrigger ("RustleHorizontal");
			}
		}
	}

	void OnTriggerStay2D(Collider2D other){

		//for each ball that is inside the trigger
		if (other.CompareTag ("ball")) {

			//get it's speed, and if the speed is nearly 0
			//then fire the trigger to returns to the non-animated state
			float ballSpeed = other.gameObject.GetComponent<BallScript> ().RBody.velocity.magnitude;
			if (Mathf.Approximately (ballSpeed, 0)) {
				flowerAnimator.SetTrigger ("UnRustle");
			}
		}
	}

	void OnTriggerExit2D(Collider2D other){

		//when a ball leaves
		//fire the trigger to stop animating
		//note that the "endtime of the animation is set so the flower completes a full cycle
		if (other.CompareTag ("ball")) {
			flowerAnimator.SetTrigger ("UnRustle");
		}
	}
}
