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
		private enum CameraState { Rotating, Reading_Credits };
		private CameraState camState;


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
			Services.Tasks = new TaskManager();
			camScript = new CameraRotator();
			camScript.Setup();
			camState = CameraState.Rotating;
			Services.Events.Register<PauseEvent>(HandlePausing);
			Services.ScriptableObjs = new ScriptableObjectSource();
			Services.ScriptableObjs.Setup();
			Services.Events.Register<ToggleCamRotEvent>(SetCameraRotation);
		}


		private void Update(){
			Services.Sound.Tick(); //music fades in and out even when the game is paused
			Services.Tasks.Tick();

			if (paused) return;

			if (camState == CameraState.Rotating) camScript.Tick();
			ListenForMenu();
		}


		/// <summary>
		/// Menu buttons use this to get ready to leave the title scene.
		/// </summary>
		public void CleanUp(){
			Services.Events.Unregister<PauseEvent>(HandlePausing);
			Services.Events.Unregister<ToggleCamRotEvent>(SetCameraRotation);
			Services.EscapeMenu.Cleanup();
		}


		/// <summary>
		/// Carries out all actions required when the player activates the quit-game menu.
		/// </summary>
		private void ListenForMenu(){
			if (Input.GetKeyDown(KeyCode.Escape)){
				Services.Events.Fire(new EscMenuEvent());
				Services.Events.Fire(new PauseEvent(PauseEvent.Pause.Pause));
			}
		}

		
		/// <summary>
		/// Starts or stops the camera's rotation.
		/// </summary>
		private void SetCameraRotation(global::Event e){
			Debug.Assert(e.GetType() == typeof(ToggleCamRotEvent), "Non-ToggleCamRotEvent in SetCameraRotation");

			if (camState == CameraState.Rotating) camState = CameraState.Reading_Credits;
			else camState = CameraState.Rotating;
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
