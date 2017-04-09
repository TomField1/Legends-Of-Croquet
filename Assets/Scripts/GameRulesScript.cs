using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameRulesScript : MonoBehaviour {

	// An enumerator for the state of the game
	public enum GameState{
		Start,
		Turn,
		EndTurn,
		PlaceBall,
		Roquet,
		Continuation,
		Pause,
		Foul,
		Win,
	}
	//an enumerator for the team.
	public enum Team{
		Red,
		Blue,
	}

	public GameState currentState; //what's the current state?
	public GameState previousState; // what was the last state?
	Team currentTeam; //what team's turn is it currently?

	BallControlScript ballController;
	public MenuScript pauseScript;

	bool ballBeenHit; //has any ball been hit?
	bool ballThroughHoop; //did the ball go through the hoop?
	GameObject hitBallToPlaceNextTo; //if a ball was hit last turn, which one?

	GameObject lastRedBall; //track the last red and blue balls so we can automatically choose them
	GameObject lastBlueBall; // when the player switches team.

	List<HoopScript> hoopScriptList; //the list of hoop scripts
	public TextScript mainUITextScript; //the text for the main UI
	public Text scoreRed1; //the text for red's score
	public Text scoreRed2;
	public Text scoreBlue1; //the text for blue's score
	public Text scoreBlue2;

	//Game rules of croquet: Take it in turn to make a shot with either ball.
	//Getting a hoop gives you a bonus shot (Continuation)
	//Hitting another ball gives you one shot where you move the ball to
	//the ball you hit, (Roquet) then a bonus shot (Continuation)
	//Get both balls through the hoops in the right direction and then
	//hit the peg to win.

	////////////////////////////////////////////////////////// AWAKE
	/// This is run when the game starts, and should be the first thing run in the entire scene
	void Awake(){
		//set up all the defaults
		currentState = GameState.Start;
		ballBeenHit = false;
		ballThroughHoop = false;
		currentTeam = Team.Blue;

		ballController = gameObject.GetComponent<BallControlScript> ();

		//Get a list of hoops, and sort them in alphabetical order
		hoopScriptList = new List<HoopScript>();
		hoopScriptList.AddRange(GetComponentsInChildren<HoopScript> ());
		hoopScriptList.Sort(((x, y) => x.gameObject.transform.parent.name.CompareTo(
			y.gameObject.transform.parent.name)));
	}

	////////////////////////////////////////////////////////// UPDATE
	// This starts the game. It's in Update since it needs to wait for everything to be set up.
	void Update () {
		if (currentState == GameState.Start) {
			NewRedTurn ();
		}
	}

	////////////////////////////////////////////////////////// GETTERS AND SETTERS
	public GameState CurrentState{
		get { return currentState; }
		set { currentState = value; }
	}
	public GameObject HitBallToPlaceNextTo{
		get { return hitBallToPlaceNextTo; }
		set { hitBallToPlaceNextTo = value; }
	}
	public GameObject LastRedBall{
		get { return lastRedBall; }
		set { lastRedBall = value; }
	}
	public GameObject LastBlueBall{
		get { return lastBlueBall; }
		set { lastBlueBall = value; }
	}
	public Team CurrentTeam{
		get { return currentTeam; }
		set { currentTeam = value; }
	}
	public bool BallThroughHoop{
		get { return ballThroughHoop; }
		set { ballThroughHoop = value; }
	}
	public bool BallBeenHit{
		get { return ballBeenHit; }
		set { ballBeenHit = value; }
	}
	public HoopScript GetHoop(int i){
		return hoopScriptList [i-1];
	}

	///////////////////////////////////////////////////////////// PICK THE NEXT STATE
	/// This is the big one - it takes the state the game was in last time, and chooses the next state.
	/// Order of operations is important here, since it defines how the game rules work.
	public void PickNextState(){

		//Most importantly, if you're in the GameWin state, you can't be knocked out of it when the balls stop
		if (previousState == GameState.Win) {
			if (currentTeam == Team.Red) {
				mainUITextScript.DisplayMessage ("Red Wins Forever!", 30);
			} else {
				mainUITextScript.DisplayMessage ("Blue Wins Forever!", 30);
			}
		}

		//if a foul has been triggered, start a new turn.
		else if (previousState == GameState.Foul) {
			if (currentTeam == Team.Red) {
				NewBlueTurn ();
				mainUITextScript.DisplayMessage ("Red Foul! Blue's Turn", 3);
			} else {
				NewRedTurn ();
				mainUITextScript.DisplayMessage ("Blue Foul! Red's Turn", 3);
			}
		}

		//when continuation ends, start a new turn for the other team
		//no matter what you do in a continuation, its the end of your turn.
		else if (previousState == GameState.Continuation) {
			if (currentTeam == Team.Blue) {
				NewRedTurn ();
			} else {
				NewBlueTurn();
			}
		}

		//if it's not a continuation and the ball has been hit, then go to placing a ball
		//placing the ball then moves on to roquet
		else if (ballBeenHit) {
			mainUITextScript.DisplayMessage("ROQUET!!!", 3);
			ballBeenHit = false;
			currentState = GameState.PlaceBall;
		}

		//if the ball has gone through a hoop but not been hit, get a continuation
		else if (ballThroughHoop) {
			mainUITextScript.DisplayMessage("CONTINUATION!!!", 3);
			ballThroughHoop = false;
			currentState = GameState.Continuation;
		}

		//otherwise, if you've had an uneventful roquet, go to continuation
		//(this means that a roquet leads to a continuation any time it doesn't lead to a collision)
		else if (previousState == GameState.Roquet) {
			mainUITextScript.DisplayMessage("CONTINUATION!!!", 3);
			currentState = GameState.Continuation;
		}

		//if none of the options above have happened, go to a new turn for the other player
		else {
			if (currentTeam == Team.Blue) {
				NewRedTurn ();
			} else {
				NewBlueTurn();
			}
		}
	}

	////////////////////////////////////////////////////////// END TURN
	/// End the turn, and prepare to choose what happens next
	public void EndTurn(){
		
		//mark the current controlled ball to return to it
		if (currentTeam == Team.Red) {
			lastRedBall = ballController.GetActiveBall();
		} else {
			lastBlueBall = ballController.GetActiveBall();
		}

		//has someone won the game? if so, set the state to "win"
		if (ballController.HasTeamWon(Team.Red) || ballController.HasTeamWon(Team.Blue)){
			currentState = GameRulesScript.GameState.Win;
		}

		//save the current state
		previousState = currentState;

		//set the current state to "endTurn" so the game pauses
		currentState = GameRulesScript.GameState.EndTurn;
	}

	////////////////////////////////////////////////////////// GET NEXT HOOP
	/// Get the next hoop, given the last hoop
	public HoopScript GetNextHoop(HoopScript h, GameObject b){
		
		int i;

		//for each hoop before the last one
		for(i = 0; i < (hoopScriptList.Count-1); i++) {

			//if that hoop is the current hoop
			if (hoopScriptList [i].Equals(h)) {

				//increment the score
				if (b == ballController.ballB1) {
					scoreBlue1.text = (i + 1).ToString ();
				} else if (b == ballController.ballB2) {
					scoreBlue2.text = (i + 1).ToString ();
				} else if (b == ballController.ballR1) {
					scoreRed1.text = (i + 1).ToString ();
				} else if (b == ballController.ballR2) {
					scoreRed2.text = (i + 1).ToString ();
				} else {
					print ("broke the scores somehow?");
				}

				//return the next hoop
				print ("setting hoop to" + (i+2));
				return hoopScriptList [i + 1];
			}
		}
		//now we're at the last hoop
		//if the current hoop is the last hoop in the list
		if(hoopScriptList [i].Equals(h)){
			
			if (b == ballController.ballB1) {
				scoreBlue1.text = (i + 1).ToString ();
			} else if (b == ballController.ballB2) {
				scoreBlue2.text = (i + 1).ToString ();
			} else if (b == ballController.ballR1) {
				scoreRed1.text = (i + 1).ToString ();
			} else if (b == ballController.ballR2) {
				scoreRed2.text = (i + 1).ToString ();
			} else {
				print ("broke the scores somehow?");
			}

			//then set that ball as having won
			print ("one ball has won");
			b.GetComponent<BallScript> ().HasWon = true;
			return hoopScriptList [0];
		}

		//if the current hoop is NOT any hoop before last OR the last hoop, something's wrong.
		print ("you dun goofed. trying to find a nonexistant hoop");
		return hoopScriptList [0];
	}

	////////////////////////////////////////////////////////// NEW TURN
	/// For both teams, start a new turn
	/// clear the "ballthroughhoop" and "ballbeenhit" settings in case something else hasnt
	/// Display the new turn message, set the last ball as active, and set the current team and state. 
	/// Wipe out the last "hitball"
	void NewRedTurn (){
		mainUITextScript.DisplayMessage("Red's Turn!", 3);

		ballController.SetActiveBall (lastRedBall);

		ballThroughHoop = false;
		ballBeenHit = false;

		currentTeam = Team.Red;
		currentState = GameState.Turn;
		hitBallToPlaceNextTo = null;
	}
	void NewBlueTurn (){
		mainUITextScript.DisplayMessage("Blue's Turn!", 3);

		ballThroughHoop = false;
		ballBeenHit = false;

		ballController.SetActiveBall (lastBlueBall);
		currentTeam = Team.Blue;
		currentState = GameState.Turn;
		hitBallToPlaceNextTo = null;
	}

	////////////////////////////////////////////////////////// GAME START CINEMATIC
	/// TODO
	void GameStartCinematic(){
		//display a "game start" message
		//zoom the camera out to show the whole board
		//hold it like that for, say, ten seconds
		//zoom in on the ball
		//start game
	}

	////////////////////////////////////////////////////////// PAUSE THE GAME
	public void Pause(){
		//save the previous state and mark the current state as paused
		previousState = currentState;
		currentState = GameState.Pause;
		//show menu
		pauseScript.Pause();

	}
	////////////////////////////////////////////////////////// UNPAUSE THE GAME
	public void Unpause(){
		//hide the menu
		pauseScript.Unpause();

		//return the state to the previous state, and mark the previous state as "paused"
		currentState = previousState;
		previousState = GameState.Pause;
	}
}
