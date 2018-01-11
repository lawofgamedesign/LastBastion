namespace Title
{
	using UnityEngine;
	using UnityEngine.SceneManagement;

	public class TitleManager : MonoBehaviour {


		/////////////////////////////////////////////
		/// Fields
		/////////////////////////////////////////////


		//rotates the camera
		private CameraRotator camScript;


		//scenes to load
		private const string GAME_SCENE = "Game";
		private const string TUTORIAL_SCENE = "Tutorial";


		/////////////////////////////////////////////
		/// Functions
		/////////////////////////////////////////////


		private void Awake(){
			Services.Board = new BoardBehavior();
			Services.Board.Setup();
			Services.Events = new EventManager();
			Services.Attackers = new DemoAttackerCreator();
			Services.Attackers.Setup();
			Services.Defenders = new DefenderManager();
			Services.Defenders.Setup();
			Services.Sound = new AudioManager();
			Services.Sound.Setup();
			Services.Cursor = new CursorManager();
			Services.Cursor.Setup();
			camScript = new CameraRotator();
			camScript.Setup();
		}


		private void Update(){
			camScript.Tick();
			ListenForClick();
		}


		private void ListenForClick(){
			if (Input.GetMouseButtonDown(0)) SceneManager.LoadScene(TUTORIAL_SCENE);
			else if (Input.GetMouseButtonDown(1)) SceneManager.LoadScene(GAME_SCENE);
		}
	}
}
