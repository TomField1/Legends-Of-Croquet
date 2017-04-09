using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HoopScript : MonoBehaviour {

	private BallControlScript ballController; //the ball controller
	public bool isPegNotHoop; //is this hoop a peg (can be hit from any direction) or a hoop (pass through one way)
	private List<SpriteRenderer> hoopSpriteList; //the hoop's sprite glow

	void Start(){
		//get the ball controller
		ballController = FindObjectOfType<BallControlScript> ();

		//get all the sprites in the hoop
		hoopSpriteList = new List<SpriteRenderer>();
		hoopSpriteList.AddRange(gameObject.transform.parent.GetComponentsInChildren<SpriteRenderer> ());

		//turn them off by default
		SpriteOff ();
	}
		
	/// Turn all sprites for this hoop on.
	public void SpriteOn(){
		foreach (SpriteRenderer r in hoopSpriteList) {
			r.enabled = true;
		}
	}
		
	/// Turn all sprites for this hoop off.
	public void SpriteOff(){
		foreach (SpriteRenderer r in hoopSpriteList) {
			r.enabled = false;
		}
	}

	//This trigger only works if the hoop is a hoop, not a peg.
	void OnTriggerEnter2D(Collider2D other) {
		//if the script is attached to a hoop, not a peg
		if (!isPegNotHoop) {

			GameObject ball = other.gameObject;
			HoopScript hoop = this;

			//if going in the right direction
			if (RightDirectionCheck (other)) {

				//fire the OnHoopCollision event.
				ballController.OnHoopCollision (ball, hoop);
			}
		}
	}

	//This checks that the ball entering a hoop is going the right way, and returns true if it is
	bool RightDirectionCheck(Collider2D other){

		//this gets the angle that the arrow in the hoop prefab points, clockwise from the top
		Transform t = GetComponentInParent<Transform> ();
		float transformRotation = 360 - t.rotation.eulerAngles.z;

		//angle gets the dot product, so can't cope with negative angles, just the distance
		float movementRotation = Vector2.Angle(Vector2.up, other.attachedRigidbody.velocity);

		//we get the cross product of the two as vector3s, then determine if the angle should be negative or not
		Vector3 cross = Vector3.Cross(Vector2.up, other.attachedRigidbody.velocity);

		//if it should be negative, then invert it.
		if (cross.z > 0) {
			movementRotation = 360 - movementRotation;
		}

		//if there's less than 90deg difference between the central angle of the prefab and the ball movement, return true
		float angleBetween = transformRotation - movementRotation;
		if (-90 < angleBetween && 90 > angleBetween) {
			return true;
		}
		//else return false
		return false;
	}

	//only if this is a peg do we need to bother with a collision
	void OnCollisionEnter2D(Collision2D collision){
		if (isPegNotHoop) {
			//fire OnHoopCollision when it hits
			ballController.OnHoopCollision (collision.gameObject, this);
		}
	}
}
