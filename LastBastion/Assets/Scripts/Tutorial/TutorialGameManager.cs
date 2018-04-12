namespace Tutorial
{
	using UnityEngine;
	using UnityEngine.SceneManagement;

	public class TutorialGameManager : MonoBehaviour {

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
		private const string TUTORIAL_SCENE = "Tutorial3";


		//initialize variables and establish the game's starting state
		private void Awake(){
			Services.Tasks = new TaskManager();
			Services.AttackDeck = new LinkedAttackerDeck();
			Services.Events = new EventManager();
			Services.UI = new ChatUI();
			Services.UI.Setup();
			Services.Board = new BoardBehavior();
			Services.Board.Setup();
			Services.Attackers = new AttackerManager();
			Services.Attackers.Setup();
			Services.Rulebook = new TutorialTurnManager();
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
			Services.Sound.Setup(AudioManager.Clips.Komiku_La_Ville_Aux_Ponts_Suspendus);
			Services.PlayerEyes = new CameraBehavior();
			Services.PlayerEyes.Setup();
			Services.Environment = new EnvironmentManager();
			Services.EscapeMenu = new TutorialEscMenuBehavior();
			Services.EscapeMenu.Setup();
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
				Services.Sound.PlaySceneStartMusic();
			}
		}


		public void Restart(){
			Services.Events.Unregister<PauseEvent>(HandlePausing);
			Services.EscapeMenu.Cleanup();
			SceneManager.LoadScene(TUTORIAL_SCENE);
		}


		/// <summary>
		/// Do everything that happens each frame. This is the only update loop in the game! Everything that happens
		/// frame-by-frame is controlled from here.
		/// </summary>
		private void Update(){
			Services.Sound.Tick(); //music fades in and out even if the game is paused

			if (paused) return;

			Services.Tasks.Tick();
			Services.Inputs.Tick();
			Services.Rulebook.Tick();
		}
	}
}
