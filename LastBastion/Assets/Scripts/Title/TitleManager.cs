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
		private const string TUTORIAL_SCENE = "Tutorial3";


		//is the game paused for the menu?
		private bool paused = false;


		/////////////////////////////////////////////
		/// Functions
		/////////////////////////////////////////////


		private void Awake(){
			Services.Board = new BoardBehavior();
			Services.Board.Setup();
			Services.Events = new EventManager();
			Services.AttackDeck = null;
			Services.Attackers = new DemoAttackerCreator();
			Services.Attackers.Setup();
			Services.Defenders = new DefenderManager();
			Services.Defenders.Setup();
			Services.Sound = new AudioManager();
			Services.Sound.Setup(AudioManager.Clips.Komiku_Barque_sur_le_Lac);
			Services.Cursor = new CursorManager();
			Services.Cursor.Setup();
			Services.EscapeMenu = new TitleEscMenuBehavior();
			Services.EscapeMenu.Setup();
			camScript = new CameraRotator();
			camScript.Setup();
			Services.Events.Register<PauseEvent>(HandlePausing);
		}


		private void Update(){
			Services.Sound.Tick(); //music fades in and out even when the game is paused

			if (paused) return;

			camScript.Tick();
			ListenForClick();
			ListenForMenu();
		}


		private void ListenForClick(){
			if (Input.GetMouseButtonDown(0)){
				Services.Events.Unregister<PauseEvent>(HandlePausing);
				Services.EscapeMenu.Cleanup();
				SceneManager.LoadScene(TUTORIAL_SCENE);
			}
			else if (Input.GetMouseButtonDown(1)){
				Services.Events.Unregister<PauseEvent>(HandlePausing);
				Services.EscapeMenu.Cleanup();
				SceneManager.LoadScene(GAME_SCENE);
			}
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

			if (pauseEvent.action == PauseEvent.Pause.Pause){
				paused = true;
				Services.Sound.PlayPauseMusic();
			} else {
				paused = false;
				Services.Sound.PlaySceneStartMusic();
			}
		}
	}
}
