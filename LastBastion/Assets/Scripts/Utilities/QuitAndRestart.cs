namespace Utilities
{
	using UnityEngine;
	using UnityEngine.SceneManagement;
	using System.Collections;

	public class QuitAndRestart : MonoBehaviour {

		public static QuitAndRestart instance;

		//make this a singleton
		void Awake()
		{
			if(instance == null){
				instance = this;
				DontDestroyOnLoad(gameObject);
			} else {
				Destroy(gameObject);
			}
		}

		//listen for key presses
		void Update()
		{
			QuitCheck();
			RestartCheck();
		}

		//quit the game if the player presses Escape
		void QuitCheck()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				Application.Quit();
			}
		}

		//restart the game if the player presses R
		//IMPORTANT: this assumes that the first item (index 0) in the build settings is what you want to restart to
		void RestartCheck()
		{
			if (Input.GetKeyDown(KeyCode.R))
			{
				SceneManager.LoadScene(0);
			}
		}
	}
}
