using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the canvas defenders use for their UI
	private const string DEFENDER_UI = "Defender card canvas";


	//the canvas for the character sheet
	private const string CHAR_SHEET_UI = "Defender sheet canvas";


	//is the game paused right now?
	private bool paused = false;


	//scenes this scene can load
	private const string GAME_SCENE = "Game";


	//initialize variables and establish the game's starting state
	private void Awake(){
		Services.Tasks = new TaskManager();
		Services.AttackDeck = new LinkedAttackerDeck();
		Services.Events = new EventManager();
	

		//make sure there's a cursor manager (e.g., because the game scene was loaded directly)
		if (Services.Cursor == null){
			Services.Cursor = new CursorManager();
			Services.Cursor.Setup();
		}
		
		Services.UI = new ChatUI();
		Services.UI.Setup();
		Services.Board = new BoardBehavior();
		Services.Board.Setup();
		Services.Attackers = new AttackerManager();
		Services.Attackers.Setup();
		Services.Rulebook = new TurnManager();
		Services.Rulebook.Setup();
		Services.Defenders = new DefenderManager();
		Services.Defenders.Setup();
		GameObject.Find(DEFENDER_UI).GetComponent<DefenderUIBehavior>().Setup();
		Services.Inputs = new InputManager();
		GameObject.Find(CHAR_SHEET_UI).GetComponent<CharacterSheetBehavior>().Setup();
		Services.Undo = new UndoData();
		Services.Undo.Setup();
		Services.Momentum = new MomentumManager();
		Services.Momentum.Setup();
		Services.Sound = new AudioManager();
		Services.Sound.Setup(EnvironmentManager.Place.Kitchen);
		Services.CamControl = new CameraBehavior();
		Services.CamControl.Setup();
		Services.Environment = new EnvironmentManager();
		Services.Environment.Setup();
		Services.EscapeMenu = new GameEscMenuBehavior();
		Services.EscapeMenu.Setup();
		Services.Tutorials = new TutorialManager();
		Services.Tutorials.Setup();
		Services.Events.Register<PauseEvent>(HandlePausing);
	}


	private void HandlePausing(global::Event e){
		Debug.Assert(e.GetType() == typeof(PauseEvent), "Non-PauseEvent in HandlePausing.");

		PauseEvent pauseEvent = e as PauseEvent;

		if (pauseEvent.action == PauseEvent.Pause.Pause){
			paused = true;
			Services.Sound.PlayPauseMusic();
		}
		else {
			paused = false;
			Services.Sound.PlayPlaceMusic(Services.Environment.GetCurrentPlace());
		}
	}


	public void Restart(){
		Services.Events.Unregister<PauseEvent>(HandlePausing);
		Services.EscapeMenu.Cleanup();
		SceneManager.LoadScene(GAME_SCENE);
	}


	/// <summary>
	/// Do everything that happens each frame. This is the only update loop in the game! Everything that happens
	/// frame-by-frame is controlled from here.
	/// </summary>
	private void Update(){
		Services.Sound.Tick(); //sound always fades in and out, even if the game is paused

		if (Input.GetKeyDown(KeyCode.Space)) paused = !paused;
		
		if (paused) return;

		Services.Tasks.Tick();
		Services.Inputs.Tick();
		Services.Rulebook.Tick();
	}
}
