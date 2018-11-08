namespace Title
{
	using TMPro;
	using UnityEngine;
	using UnityEngine.SceneManagement;

	public class TitleMenuBehavior : MonoBehaviour {

		/////////////////////////////////////////////
		/// Fields
		/////////////////////////////////////////////


		//scenes that can be loaded
		public const string GAME_SCENE = "Game";
		public const string TUTORIAL_SCENE = "Tutorial3";


		//the manager object
		private const string MANAGER_OBJ = "Game managers";
		
		
		//buttons, and associated variables to manage their text
		public enum TitleMenuButtons{ Tutorial, Game, Credits }
		private const string BUTTON_OBJ_LABEL = " button";
		private const string TEXT_OBJ = "TextMeshPro Text";

		/////////////////////////////////////////////
		/// Functions
		/////////////////////////////////////////////


		//loads a scene safely, having properly cleaned up the current scene
		private void LoadScene(string scene){
			GameObject.Find(MANAGER_OBJ).GetComponent<TitleManager>().CleanUp();
			SceneManager.LoadScene(scene);
		}


		public void LoadTutorial(){
			LoadScene(TUTORIAL_SCENE);
		}


		public void LoadGame(){
			LoadScene(GAME_SCENE);
		}


		public void LoadCredits(){
			Services.Events.Fire(new CreditsButtonEvent());
			Services.Tasks.AddTask(new ViewCreditsTask());
		}


		public void SetButtonText(TitleMenuButtons button, string newText){
			transform.Find(button.ToString() + BUTTON_OBJ_LABEL).Find(TEXT_OBJ)
					.GetComponent<TextMeshProUGUI>().text = newText;
		}
	}
}
