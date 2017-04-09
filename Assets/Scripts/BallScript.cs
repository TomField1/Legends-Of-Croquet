using UnityEngine;
using System.Collections;

public class BallScript : MonoBehaviour {

	public bool activeBall; //is this ball active?
	private Rigidbody2D rb; //the ball's rigidbody
	public SpriteRenderer sprite; //the ball's sprite glow

	private float lastHit; //the last time the ball was hit
	public float speed; //how fast is the ball moving? (public so we can see it in-editor)
	private bool isMoving; //is the ball moving?

	private bool hasWon; //has this ball been through all hoops?

	public bool foulBall; //is this ball a foul ball (and so needs resetting)
	private Vector2 startPosition; //the ball's position at the start of the turn

	public HoopScript activeHoop; //which hoop is the ball aiming at?

	private GameRulesScript gameRules; // the game rules for the game

	// Use this for initialization
	void Start () {
		//get the sprite and rigidbody components
		rb = GetComponent<Rigidbody2D>();
		sprite = GetComponentInChildren<SpriteRenderer> ();

		//get the gamerules
		gameRules = GetComponentInParent<GameRulesScript> ();

		//set various other defaults
		activeHoop = gameRules.GetHoop (1);
		isMoving = false;
		hasWon = false;
		foulBall = false;
	}

	////////////////////////////////////////////////////////////////// ON ACTIVE
	//when the ball is set to Active by the game rules, mark it as active and turn the sprite on
	public void OnActive(){
		activeBall = true;
		sprite.enabled = true;
	}

	////////////////////////////////////////////////////////////////// ON InACTIVE
	// when the ball is set to inactive by the game rules, mark it as inactive and turn the sprite off
	// also turn the sprite for the hoop off.
	public void OnInactive(){
		activeBall = false;
		sprite.enabled = false;
		activeHoop.SpriteOff ();
	}
	////////////////////////////////////////////////////////////////// HIT THE BALL
	//Hit the ball with the given force, and mark it as moving
	public void hitBall (Vector3 vect){
		rb.AddForce (vect);
		isMoving = true;
	}
	////////////////////////////////////////////////////////////////// GETTERS AND SETTERS
	public bool IsMoving {
		get { return isMoving; }
		set { isMoving = value; }
	}
	public bool HasWon {
		get { return hasWon; }
		set { hasWon = value; }
	}
	public bool FoulBall {
		get { return foulBall; }
		set { foulBall = value; }
	}
	public Vector2 StartPosition {
		get { return startPosition; }
		set { startPosition = value; }
	}
	public Rigidbody2D RBody {
		get { return rb; }
		set { rb = value; }
	}
	public HoopScript ActiveHoop{
		get { return activeHoop; }
		set { activeHoop.SpriteOff (); //note: whenever we set the active hoop, we turn the old hoop's sprite off
			activeHoop = value; }
	}

	////////////////////////////////////////////////////////////////// UPDATE
	// Update is called once per frame
	void Update () {

		//if it's the active ball and it hasn't won, turn the hoop's sprite on.
		//(this is done every frame since there's several cases that make the ball active)
		if (activeBall && !hasWon) {
			activeHoop.SpriteOn ();
		}

		//get the magnitude of the velocity as the speed.
		speed = rb.velocity.magnitude;

		//if the ball's speed drops below a certain value, then stop
		if (speed <= (float)0.06 && isMoving) {
			isMoving = false;
			rb.velocity = new Vector3 (0, 0, 0);
		}
	}

	////////////////////////////////////////////////////////////////// ON COLLISION ENTER
	/// When the ball collides with another ball
	void OnCollisionEnter2D(Collision2D collision){
		// if the ball has collided with another ball, but not while being placed
		if (collision.collider.gameObject.CompareTag("ball") 
			&& gameRules.CurrentState != GameRulesScript.GameState.PlaceBall){

			//mark as moving (remember this will fire for both balls)
			isMoving = true;

			//if it's the active ball (so it only fires once)
			//and didnt hit the same ball as was hit last time (so you cant just chain them forever)
			if (activeBall && collision.collider.gameObject != gameRules.HitBallToPlaceNextTo) {
				
				//note in the rules that a ball was hit, and which one
				gameRules.BallBeenHit = true;
				gameRules.HitBallToPlaceNextTo = collision.collider.gameObject;

				//note that this means that if the ball hits multiple others, the last one is the only one saved
			}
		}
	}
		

	////////////////////////////////////////////////////////////////// SET THE DRAG
	/// Set the drag for this ball.
	public void SetDrag(float newDrag){
		rb.drag = newDrag;
	}
}
