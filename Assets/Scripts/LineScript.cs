using UnityEngine;
using System.Collections;

public class LineScript : MonoBehaviour {

	LineRenderer line; //the LineRenderer this line uses
	BallControlScript controlScript; //the controlscript giving the line instructions

	// Use this for initialization
	void Start () {
		line = GetComponent<LineRenderer> ();
		controlScript = GetComponent<BallControlScript> ();
	}
	
	//////////////////////////////////////////////////////////// SHOW THE AIM LINE
	/// Show the default line from the mouse to a distance past the active ball
	public void ShowAimLine () {
		line.enabled = true;

		//get the mouse position in world co-ordinates
		Vector3 posInScreen = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, (float)10);
		Vector3 posInWorld = Camera.main.ScreenToWorldPoint (posInScreen);

		//get the vector for the direction from the mouse to the ball
		Vector3 vect = GetVect();

		//set the array positions for this line from the mouse to a distance past the ball
		// (then back to the mouse to fill up the rest of the space)
		Vector3[] lineArray = new Vector3[6] {
			posInWorld,
			new Vector3 (controlScript.activeBall.transform.position.x, controlScript.activeBall.transform.position.y, 0) - vect * 5,
			posInWorld,
			posInWorld,
			posInWorld,
			posInWorld,
		};

		//set the positions to the listed positions in the array.
		line.SetPositions (lineArray);
	}

	////////////////////////////////////////////////////////// SHOW THE HIT LINE
	/// Show the line that predicts the incoming hit, for a given strength
	public void ShowHitLine(float hitStrength){
		line.enabled = true;

		//get the vector for the direction from the mouse to the ball
		Vector3 vect = GetVect ();

		//get the mouse position in world co-ordinates
		Vector3 posInScreen = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, (float)10);
		Vector3 posInWorld = Camera.main.ScreenToWorldPoint (posInScreen);

		//get the strength of the hit as a point from the current position
		Vector3 aimPoint = controlScript.activeBall.transform.position - vect * hitStrength / 10;

		//calculate the deflection for the hit strength
		float deflectMaxAngle = GetMaxAngle (hitStrength);
		Vector3 maxDeflect, minDeflect;
		maxDeflect = Quaternion.AngleAxis (deflectMaxAngle, new Vector3 (0, 0, 1)) * vect * hitStrength / 30;
		minDeflect = Quaternion.AngleAxis (-deflectMaxAngle, new Vector3 (0, 0, 1)) * vect * hitStrength / 30;

		//get the maximum and minimum deflection position
		Vector3 maxDeflectPosition = controlScript.activeBall.transform.position - maxDeflect;
		Vector3 minDeflectPosition = controlScript.activeBall.transform.position - minDeflect;

		//setup array to draw line from pointer > aimpoint > ball > max deflection > ball > min deflection
		Vector3[] lineArray = new Vector3[6] {
			posInWorld,
			aimPoint,
			controlScript.activeBall.transform.position,
			maxDeflectPosition,
			controlScript.activeBall.transform.position,
			minDeflectPosition
		};

		//draw this line
		line.SetPositions (lineArray);
	}

	////////////////////////////////////////////////////////// SHOW THE LINE FOR PLACING THE BALL
	/// This line is used when placing the ball in a roquet
	public void ShowPlacementLine(){
		line.enabled = true;

		//get the mouse position in world co-ordinates
		Vector3 posInScreen = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, (float)10);
		Vector3 posInWorld = Camera.main.ScreenToWorldPoint (posInScreen);

		//get the vector for the direction from the mouse to the ball
		Vector3 vect = GetVect ();

		//as before, create an array of points from the mouse to the centre ball
		Vector3[] lineArray = new Vector3[6] {
			posInWorld,
			controlScript.activeBall.transform.position - vect,
			posInWorld,
			posInWorld,
			posInWorld,
			posInWorld,
		};
			
		//and set the line to use this array
		line.SetPositions (lineArray);
	}

	////////////////////////////////////////////////////////// GET VECTOR
	/// This gets a unit vector version of the line from the mouse to the active ball
	public Vector3 GetVect(){
		
		//get the mouse position in world co-ordinates
		Vector3 posInScreen = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, (float)10);
		Vector3 posInWorld = Camera.main.ScreenToWorldPoint (posInScreen);

		//subtract the co-ords of this point from the ball position
		float x = posInWorld.x - controlScript.activeBall.transform.position.x;
		float y = posInWorld.y - controlScript.activeBall.transform.position.y;

		//reduce this down to a unit vector
		Vector3 vect = new Vector3 (x, y, 0);
		vect = vect.normalized;

		return vect;
	}

	/////////////////////////////////////////////////////////////////////////////////////////GET MAX ANGLE
	//this function gets the maximum possible deflect angle depending on the hit strength
	//kept seperate so i can make it more complicated if i want
	public float GetMaxAngle(float strength){
		return (float)(strength / 100 * 7.5);
	}

	//////////////////////////////////////////////////////////// GET AND SET
	public LineRenderer Line {
		get { return line; }
		set { line = value; }
	}
}
