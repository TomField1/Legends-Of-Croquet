using UnityEngine;
using System.Collections;

public class PoolScript : MonoBehaviour {

	private BallControlScript ballController; 
	private GameRulesScript gameRules;

	////////////////////////////////////////////////////////// START
	/// Setup the needed scripts
	void Start(){
		ballController = FindObjectOfType<BallControlScript> ();
		gameRules = FindObjectOfType<GameRulesScript> ();
	}

	////////////////////////////////////////////////////////// ON TRIGGER ENTER
	/// If the ball is more than halfway into the pool, it's a foul
	/// To determine Halfway, a tiny collider at the centre of the ball is used.
	void OnTriggerEnter2D(Collider2D other) {
		GameObject gObject = other.gameObject;

		//if the ballCentre is colliding with the object and the state is not "placing the ball"
		if (gObject.tag == "ballCentre" && gameRules.CurrentState != GameRulesScript.GameState.PlaceBall){
			GameObject parentBall = gObject.transform.parent.gameObject;

			//hide the ball
			parentBall.GetComponent<MeshRenderer>().enabled = false;

			//stop the ball
			parentBall.GetComponent<BallScript>().RBody.velocity = Vector2.zero;

			print (parentBall.ToString ());

			//mark the ball as foul
			ballController.OnFoulBall(parentBall);
		}
	}
}
