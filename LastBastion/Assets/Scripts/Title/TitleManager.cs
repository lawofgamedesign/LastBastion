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
		private const string TUTORIAL_SCENE = "Tutorial2";


		//is the game paused for the menu?
		private bool paused = false;


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
			Services.EscapeMenu = new TitleEscMenuBehavior();
			Services.EscapeMenu.Setup();
			camScript = new CameraRotator();
			camScript.Setup();
			Services.Events.Register<PauseEvent>(HandlePausing);
		}


		private void Update(){
			if (paused) return;

			camScript.Tick();
			ListenForClick();
			ListenForMenu();
		}


		private void ListenForClick(){
			Services.Events.Unregister<PauseEvent>(HandlePausing);
			Services.EscapeMenu.Cleanup();

			if (Input.GetMouseButtonDown(0)) SceneManager.LoadScene(TUTORIAL_SCENE);
			else if (Input.GetMouseButtonDown(1)) SceneManager.LoadScene(GAME_SCENE);
		}


		private void ListenForMenu(){
			if (Input.GetKeyDown(KeyCode.Escape)){
				Services.Events.Fire(new EscMenuEvent());
				Services.Events.Fire(new PauseEvent(PauseEvent.Pause.Pause));
			}
		}


		private void HandlePausing(global::Event e){
			Debug.Assert(e.GetType() == typeof(PauseEvent), "Non-PauseEevent in TitleManager.");

			PauseEvent pauseEvent = e as PauseEvent;

			if (pauseEvent.action == PauseEvent.Pause.Pause) paused = true;
			else paused = false;
		}
	}
}
