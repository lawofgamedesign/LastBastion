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
			Services.Tutorials.PlayTutorial(TutorialManager.Tutorials.Full);
		}


		public void LoadGame(){
			LoadScene(GAME_SCENE);
		}


		/// <summary>
		/// If the player isn't currently viewing the credits, start the task that moves the camera to the rulebook
		/// and "flips the page" to the credits.
		/// 
		/// If the player is viewing the credits, still send out an event indicating that the player pressed the credits button.
		/// ViewCreditsTask uses this event to decide when to start the DoneViewingCreditsTask.
		/// 
		/// </summary>
		public void LoadCredits(){
			Services.Events.Fire(new CreditsButtonEvent());
			
			//if the player isn't currently viewing the credits, start the task that moves the camera so that they can do so
			//if the player is viewing the credits, 
			if (!Services.Tasks.CheckForTaskOfType<ViewCreditsTask>()) Services.Tasks.AddTask(new ViewCreditsTask());
		}


		public void SetButtonText(TitleMenuButtons button, string newText){
			transform.Find(button.ToString() + BUTTON_OBJ_LABEL).Find(TEXT_OBJ)
					.GetComponent<TextMeshProUGUI>().text = newText;
		}
	}
}
