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
		
		
		//choose which menu to display--the normal one, or the one for a player's first game
		private Transform menuCanvas;
		private const string MENU_CANVAS_OBJ = "Title text canvas";
		private const string NORMAL_MENU = "Menu";
		private const string FIRST_GAME_MENU = "First game menu";
		private int numPlays = 0; //0 if game has never been played before
		private const string NUM_PLAYS_KEY = "NumPlays";
		private const int FIRST_PLAY_DEFAULT = 0;



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
			Services.Tutorials = new TutorialManager();
			Services.Tutorials.Setup();
			menuCanvas = GameObject.Find(MENU_CANVAS_OBJ).transform;
			
			ChooseMenu();
		}

		/// <summary>
		/// Activates either the normal start-of-game menu or, if this is the first game on this machine, a special menu
		/// from which the player can load an introductory message. 
		/// </summary>
		private void ChooseMenu(){	
			if (Application.isEditor) PlayerPrefs.SetInt(NUM_PLAYS_KEY, FIRST_PLAY_DEFAULT); //for testing purposes, always assume a new game
			numPlays = PlayerPrefs.GetInt(NUM_PLAYS_KEY, FIRST_PLAY_DEFAULT);
			
			if (numPlays == 0) menuCanvas.Find(NORMAL_MENU).gameObject.SetActive(false);
			else menuCanvas.Find(FIRST_GAME_MENU).gameObject.SetActive(false);

			numPlays++;
			
			PlayerPrefs.SetInt(NUM_PLAYS_KEY, numPlays);
		}


		/// <summary>
		/// Turns on the normal start-of-game menu and switches off the special first-game menu.
		/// </summary>
		public void SwitchMenus(){
			menuCanvas.Find(NORMAL_MENU).gameObject.SetActive(true);
			menuCanvas.Find(FIRST_GAME_MENU).gameObject.SetActive(false);
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
