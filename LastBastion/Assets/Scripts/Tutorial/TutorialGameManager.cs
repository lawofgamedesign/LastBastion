namespace Tutorial
{
	using UnityEngine;

	public class TutorialGameManager : MonoBehaviour {

		/////////////////////////////////////////////
		/// Fields
		/////////////////////////////////////////////


		//the canvas defenders use for their UI
		private const string DEFENDER_UI = "Defender card canvas";


		//the canvas for the character sheet
		private const string CHAR_SHEET_UI = "Defender sheet canvas";


		//initialize variables and establish the game's starting state
		private void Awake(){
			Services.Tasks = new TaskManager();
			Services.AttackDeck = new AttackerDeck();
			Services.AttackDeck.Setup();
			Services.Events = new EventManager();
			Services.UI = new TutorialChatUI();
			Services.UI.Setup();
			Services.Board = new BoardBehavior();
			Services.Board.Setup();
			Services.Attackers = new TutorialAttackerManager();
			Services.Attackers.Setup();
			Services.Rulebook = new TutorialTurnManager();
			Services.Rulebook.Setup();
			Services.Defenders = new TutorialDefenderManager();
			Services.Defenders.Setup();
			GameObject.Find(DEFENDER_UI).GetComponent<DefenderUIBehavior>().Setup();
			Services.Inputs = new InputManager();
			GameObject.Find(CHAR_SHEET_UI).GetComponent<CharacterSheetBehavior>().Setup();
			Services.Undo = new UndoData();
			Services.Undo.Setup();
			Services.Momentum = new MomentumManager();
			Services.Momentum.Setup();
			Services.Sound = new AudioManager();
			Services.Sound.Setup();
		}


		/// <summary>
		/// Do everything that happens each frame. This is the only update loop in the game! Everything that happens
		/// frame-by-frame is controlled from here.
		/// </summary>
		private void Update(){
			Services.Inputs.Tick();
			Services.Rulebook.Tick();
			Services.Tasks.Tick();
		}
	}
}
