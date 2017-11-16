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


		//the next scene to load
		private const string NEXT_SCENE = "Game";


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
			Services.Inputs = new DetectAnyInput();
			camScript = new CameraRotator();
			camScript.Setup();
			Services.Events.Register<InputEvent>(GoToGame);
		}


		private void Update(){
			camScript.Tick();
			Services.Inputs.Tick();
		}


		private void GoToGame(global::Event e){
			Services.Events.Unregister<InputEvent>(GoToGame);
			SceneManager.LoadScene(NEXT_SCENE);
		}
	}
}
