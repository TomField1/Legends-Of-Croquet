using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour {

	public GameObject menuCanvas;
	public GameObject gameCanvas;

	//////////////////////////////////////////////////////////// QUIT THE GAME
	/// Either Quit the application playing or quit the game
	public void QuitGame(){
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
		#else
			Application.Quit ();
		#endif
	}

	////////////////////////////////////////////////////////// GO TO MAIN MENU
	/// Load the Main Menu Scene
	public void ToMainMenu(){
		SceneManager.LoadScene (0);
	}

	////////////////////////////////////////////////////////// PAUSE/UNPAUSE THE GAME THE GAME
	/// Switch between the pause menu and the game UI
	public void Pause(){
		menuCanvas.SetActive(true);
		gameCanvas.SetActive (false);
	}
	public void Unpause(){
		gameCanvas.SetActive(true);
		menuCanvas.SetActive (false);
	}
}
