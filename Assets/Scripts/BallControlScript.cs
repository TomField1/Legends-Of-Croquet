using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class BallControlScript : MonoBehaviour {

	private GameRulesScript gameRules; //the game rules
	public GameObject ballR1, ballR2, ballB1, ballB2; //the four balls
	private BallScript ballScriptR1, ballScriptR2, ballScriptB1, ballScriptB2; //the scripts for the four balls
	private LineScript line; //the script for the line

	public GameObject activeBall; //the current active ball being controlled by the player
	private BallScript activeBallScript; //the BallScript for this ball

	private float hitStrength; //how strong is the incoming hit
	private bool hitBuilding; //is the hit strength increasing?
	private float lastHit; //the last time the ball was hit
	private bool ballsMoving; //are any balls moving?
	public float speed; //how fast is the ball moving?

	public Camera mainCamera; //the main camera
	private bool zoomedOut; //is the camera zoomed out?
	private Vector2 posInWorld; //where is the mouse cursor in the world?

	private float timeLastTurnEnded; //what time did the last turn end?

	// Use this for initialization
	void Start () {
		//get the GameRules and the LineScript
		gameRules = gameObject.GetComponent<GameRulesScript> ();
		line = gameObject.GetComponent<LineScript> ();

		//link the scripts
		ballScriptR1 = ballR1.GetComponent<BallScript> ();
		ballScriptR2 = ballR2.GetComponent<BallScript> ();
		ballScriptB1 = ballB1.GetComponent<BallScript> ();
		ballScriptB2 = ballB2.GetComponent<BallScript> ();

		//set default values for LastRed and LastBlue ball
		gameRules.LastRedBall = ballR1;
		gameRules.LastBlueBall = ballB1;

		//get a starting position for the mouse cursor
		Vector3 posInScreen = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10);
		posInWorld = Camera.main.ScreenToWorldPoint(posInScreen);

		//set up some other things
		ballsMoving = false;
		hitBuilding = true;
	}

	////////////////////////////////////////////////////////////////////////////// UPDATE
	// Update is called once per frame
	void Update () {

		/////////////////////////////////////////////////////////////////// SWITCH BALLS (TURN ONLY)
		//if space is pressed, toggle the active ball
		if (Input.GetKeyUp (KeyCode.Space) && gameRules.CurrentState == GameRulesScript.GameState.Turn) {
			toggleBall ();
		}

		/////////////////////////////////////////////////////////////////// WAIT AFTER UNPAUSING
		/// If the game was paused, wait one frame before unpausing it
		/// So clicking to unpause doesnt also click in the game
		if (gameRules.previousState == GameRulesScript.GameState.Pause) {
			print ("Unpausing game");
			gameRules.previousState = gameRules.CurrentState;
			return;
		}

		////////////////////////////////////////////////////////////////// ZOOM OUT/IN/CAMERA CONTROLS
		//if the mouse button is pressed, zoom out to see the whole board
		if (Input.GetMouseButtonDown (1)) {
			//set the position to the centre of the board, and zoom out
			mainCamera.transform.position = new Vector3 (0, 0, -10);
			mainCamera.orthographicSize = 15;

			//set the parent of the camera to null (so it doesnt follow the ball any more)
			mainCamera.transform.SetParent (null);

			//mark the camera as being zoomed out, so if the active ball changes it doesn't follow
			zoomedOut = true;
		}

		//when the mouse button is released, zoom back in
		else if (Input.GetMouseButtonUp (1)) {
			//set the parent of the camera to the active ball
			mainCamera.transform.SetParent (activeBall.transform);

			//set the ball's size to normal and the position to that of the active ball
			mainCamera.transform.position = 
				new Vector3 (mainCamera.transform.parent.position.x, mainCamera.transform.parent.position.y, -10);
			mainCamera.orthographicSize = 5;

			//mark the camera as no longer zoomed out
			zoomedOut = false;
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// TURN/ROQUET/CONTINUATION /////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// Only difference between turn and the bonus turns is whether you can switch balls or not.
		if (gameRules.CurrentState == GameRulesScript.GameState.Roquet || gameRules.CurrentState == GameRulesScript.GameState.Continuation
			|| gameRules.CurrentState == GameRulesScript.GameState.Turn) {

			/////////////////////////////////////////////////////////////// HITTING AND DRAWING (TURN/BONUSTURN ONLY)
			//Only do the following if no balls are moving
			if (!ballsMoving) {
					
				////////////////////////////////////////////////////////// BUILDING UP STRENGTH
				//if the mouse is held down, increase the strength of the incoming hit
				if (Input.GetMouseButton (0)) {

					//increase the hit strength
					incrementHitStrength ();

					//draw the line for the strength of the hit
					line.ShowHitLine (hitStrength);
				}

				////////////////////////////////////////////////////////// HIT ACTIVE BALL
				//When the mouse is released, hit the ball with the given strength
				else if (Input.GetMouseButtonUp (0)) {

					//save the start positions for all balls, in case this hit is a foul.
					MarkStartPositions ();

					//randomly adjust the vector angle
					float randAngle = Random.Range (-line.GetMaxAngle (hitStrength), line.GetMaxAngle (hitStrength));
					Vector2 deflectVect = Quaternion.AngleAxis (randAngle, new Vector3 (0, 0, 1)) * line.GetVect();
					deflectVect.Normalize ();

					//apply a force in the direction of the vector
					activeBallScript.hitBall (-deflectVect * hitStrength * 10);

					//reset the strength of the hit and the direction the hit builds
					hitStrength = 0;
					hitBuilding = true;
						
					//mark the ball as having been hit so we can't move it again
					ballsMoving = true;
					lastHit = Time.realtimeSinceStartup;

					//hide the line
					line.Line.enabled = false;

				}
				////////////////////////////////////////////////////////////////// DRAW DEFAULT LINE
				//if nothing is being held down, draw the default line
				else {
					line.ShowAimLine ();
				}
			}// end of "is the ball moving"
		}// end of "is it some kind of turn"

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// PLACE BALL////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// The "Placing Ball" mode
		else if (gameRules.CurrentState == GameRulesScript.GameState.PlaceBall && !ballsMoving) {

			//////////////////////////////////////////////////////////// PLACE BALL NEXT TO BALL
			/// Place the active ball next to the hit ball, and then when m1 is pressed, leave the ball there and switch state to BonusTurn

			//get the target ball's position
			Vector3 centrePos = gameRules.HitBallToPlaceNextTo.transform.position;

			//get the mouse position in world co-ordinates
			Vector3 posInScreen = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 10);
			posInWorld = Camera.main.ScreenToWorldPoint (posInScreen);

			//subtract the co-ords of this point from the ball position
			float x = posInWorld.x - centrePos.x;
			float y = posInWorld.y - centrePos.y;

			//reduce this down to a unit vector from the centre ball in the direction of the mouse
			Vector2 vect = new Vector2 (x, y);
			vect = vect.normalized;

			//now set the Active Ball's position to the centre ball's position, plus that normalised vector.
			activeBall.gameObject.transform.position = (Vector2)centrePos + 2*vect/3;

			//and just to look nice, draw a line from the mouse to the ball
			line.ShowPlacementLine();

			//Finally, when M1 is released, set the state as "Roquet"
			if (Input.GetMouseButtonUp (0)) {
				gameRules.CurrentState = GameRulesScript.GameState.Roquet;
			}
		}// end of "if placing the ball"

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// END TURN //////////////////////////////////////////////////////////////////////////////////////////////////////
		/// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// If the turn has ended, pause for a second before continuing
		if (gameRules.CurrentState == GameRulesScript.GameState.EndTurn){
			
			//three seconds after the end of the turn, pick the next state and move to it, starting the next turn
			if (Time.fixedTime >= timeLastTurnEnded + (float)3.0) {
				gameRules.PickNextState ();
			}

			//two seconds after the end of the turn, reset the positions of the foul balls
			else if (Time.fixedTime >= timeLastTurnEnded + (float)2.0) {
				FoulBallCleanup ();
				Debug.Log ("cleaning fouls");
			}
		}
		else{
			//if the turn has not ended, check if all balls have stopped
			if (CheckAllBallsStopped ()) {
				//if they have, save the time the turn ended so we can wait before the next turn
				timeLastTurnEnded = Time.fixedTime;
				ballsMoving = false;

				//change state to "EndTurn" for the wait
				gameRules.EndTurn ();
			}
		}


		//////////////////////////////////////////////////////////////////// PAUSE GAME
		/// if escape is pressed, pause the game
		if (Input.GetKeyDown (KeyCode.Escape)){
			gameRules.Pause();
		}

		
	}

	//////////////////////////////////////////////////////////////////	CHECK ALL BALLS STOPPED
	/// This function returns TRUE if all balls have stopped.
	/// It waits a second after the last hit (or it would fire automatically as the ball accelerates)
	/// and only returns if ballsMoving is true (I.E. it only fires once)
	bool CheckAllBallsStopped(){
		
		float dt = Time.realtimeSinceStartup;

		if (dt - lastHit > 1.0 && //if a second of time has elapsed since the last hit
			!ballScriptB1.IsMoving &&
			!ballScriptB2.IsMoving && //and none of the balls are moving
			!ballScriptR1.IsMoving &&
			!ballScriptR2.IsMoving &&
			ballsMoving) { //and we dont already know the balls have stopped

			//then return true: all balls have stopped
			return true;
		}
		return false;
	}


	///////////////////////////////////////////////////////////////////////INCREMENT HIT STRENGTH
	///Increment the strength of the hit by one frame's worth: increase to a maximum, then reduce to a minimum
	///Increases to 100%, then reduces to 10%, then oscillates between those values
	void incrementHitStrength(){

		float ratePerFrame = (float)0.75; //kept seperate for ease of changing/casting

		//if the hit strength is building
		if (hitBuilding) {
			//if the hit strength is less than 100%, then add the rate to it
			if (hitStrength < 100) {
				hitStrength = hitStrength + ratePerFrame;
			}
			//if the hitStrength reaches 100%, change the hit building direction so it starts decreasing
			else if (hitStrength >= 100) {
				hitBuilding = false;
			}
		}

		//if the hit strength is decreasing
		else {
			//if the hit strength is more than 10%, then reduce it by the rate
			if (hitStrength > 10) {
				hitStrength = hitStrength - ratePerFrame;
			}
			//if the hitStrength reaches 10%, change the hit building direction so it starts increasing
			else if (hitStrength <= 10) {
				hitBuilding = true;
			}
		}
	}
	


	///////////////////////////////////////////////////////////////////////// TOGGLE BALLS
	//swap the active ball between the two balls of the same color in a scene
	void toggleBall(){
		//if a red ball, swap to the other red ball
		if (activeBall == ballR1) {
			SetActiveBall (ballR2);
		}
		else if (activeBall == ballR2) {
			SetActiveBall (ballR1);
		}

		//if a blue ball, swap to the other blue ball
		else if (activeBall == ballB2) {
			SetActiveBall (ballB1);
		}
		else if (activeBall == ballB1) {
			SetActiveBall (ballB2);
		}
	}

	//////////////////////////////////////////////////////////////////////////// HAS A TEAM WON?
	/// Returns true if both balls from the given team have been through all hoops.
	public bool HasTeamWon(GameRulesScript.Team team){
		//if the team is blue, return true if both blue balls have won
		if (team == GameRulesScript.Team.Blue) {
			if (ballScriptB1.HasWon == true && ballScriptB2.HasWon == true) {
				return true;
			}

			//else return false
			return false;
		}

		//same thing but for red
		if (team == GameRulesScript.Team.Red) {
			if (ballScriptR1.HasWon == true && ballScriptR2.HasWon == true) {
				return true;
			}
			return false;
		}

		//if you get here, something has gone wrong.
		print ("Something wrong with HasWon");
		return false;
	}

	////////////////////////////////////////////////////////////////////////// SET ACTIVE BALL
	/// Set the given ball to the active ball, and unset all other balls
	public void SetActiveBall(GameObject ball){

		//set all balls inactive, and the current ball active
		ballScriptB1.OnInactive();
		ballScriptB2.OnInactive();
		ballScriptR1.OnInactive();
		ballScriptR2.OnInactive();

		ball.GetComponent<BallScript> ().OnActive ();

		activeBall = ball;
		activeBallScript = ball.GetComponent<BallScript> ();

		//if the camera isn't zoomed out, jump it to the ball's position and parent it
		if (!zoomedOut) {
			mainCamera.transform.SetParent(ball.transform);
			mainCamera.transform.position = 
				new Vector3 (mainCamera.transform.parent.position.x, mainCamera.transform.parent.position.y, -10);
		}
	}
	/////////////////////////////////////////////////////////////////////////// GET ACTIVE BALL
	/// Since setting the Active Ball is so complex, we need a seperate getter
	public GameObject GetActiveBall(){
		return activeBall;
	}

	//////////////////////////////////////////////////////////////////////// ON HOOP COLLISION	
	//this is thrown by each hoop trigger when the ball goes through the right way.
	public void OnHoopCollision(GameObject ball, HoopScript hScript){

		//if the ball is the active ball and it enters it's active hoop
		if(ball.GetComponent<BallScript> ().ActiveHoop == hScript){

			//set the ball's new target hoop
			SetTargetHoop (gameRules.GetNextHoop(hScript, ball), ball.GetComponent<BallScript> ());

			//mark the ball as having gone through the hoop for the gameRules
			gameRules.BallThroughHoop = true;
		}
	}
		
	/////////////////////////////////////////////////////////////////////////////////SET TARGET HOOP
	/// Set the target hoop for the ball.
	void SetTargetHoop(HoopScript hoop, BallScript ball){
		ball.ActiveHoop = hoop;
	}

	/////////////////////////////////////////////////////////////////////////////////ON FOUL BALL
	/// Mark a ball as having committed a foul
	public void OnFoulBall(GameObject ball){

		//set the ball as foul
		ball.GetComponent<BallScript>().FoulBall = true;

		//if the ball is the active ball, it's a foul: set the mode to "Foul" the next turn is a new turn
		if (ball == activeBall) {
			gameRules.CurrentState = GameRulesScript.GameState.Foul;
		}
	}

	////////////////////////////////////////////////////////////////////////////// MARK START POSITION
	/// Save the start position for each ball: if this ball is fouled, it will return to here.
	public void MarkStartPositions(){
		ballScriptB1.StartPosition = ballB1.transform.position;
		ballScriptB2.StartPosition = ballB2.transform.position;
		ballScriptR1.StartPosition = ballR1.transform.position;
		ballScriptR2.StartPosition = ballR2.transform.position;
	}

	//////////////////////////////////////////////////////////////////////////////// CLEANUP FOUL BALLS
	/// Return foul balls to their start position, and remove the other trappings of having been fouled
	public void FoulBallCleanup(){
		if (ballScriptB1.FoulBall){
			ballScriptB1.FoulBall = false;
			ballB1.transform.position = ballScriptB1.StartPosition;
			ballB1.GetComponent<MeshRenderer> ().enabled = true;
		}
		if (ballScriptB2.FoulBall){
			ballScriptB2.FoulBall = false;
			ballB2.transform.position = ballScriptB2.StartPosition;
			ballB2.GetComponent<MeshRenderer> ().enabled = true;
		}
		if (ballScriptR1.FoulBall){
			ballScriptR1.FoulBall = false;
			ballR1.transform.position = ballScriptR1.StartPosition;
			ballR1.GetComponent<MeshRenderer> ().enabled = true;
		}
		if (ballScriptR2.FoulBall){
			ballScriptR2.FoulBall = false;
			ballR2.transform.position = ballScriptR2.StartPosition;
			ballR2.GetComponent<MeshRenderer> ().enabled = true;
		}
	}
}
