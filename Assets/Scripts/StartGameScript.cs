using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class StartGameScript : MonoBehaviour {

	public Dropdown dropdown; //the dropdown to choose a scene

	//////////////////////////////////////////////////////////// LOAD THE SCENE
	//load the scene chosen in the dropdown
	public void LoadByIndex(){
		int sceneIndex = dropdown.value+1; //(add one since scene 0 is the menu)
		SceneManager.LoadScene (sceneIndex);
	}

	//////////////////////////////////////////////////////////// QUIT THE GAME
	/// Either Quit the application playing or quit the game
	public void QuitGame(){
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit ();
		#endif
	}
}
