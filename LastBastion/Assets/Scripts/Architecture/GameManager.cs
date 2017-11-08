using UnityEngine;

public class GameManager : MonoBehaviour {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the canvas defenders use for their UI
	private const string DEFENDER_UI = "Defender card canvas";


	//the canvas for the character sheet
	private const string CHAR_SHEET_UI = "Defender sheet canvas";


	//initialize variables and establish the game's starting state
	private void Awake(){
		Services.Board = new BoardBehavior();
		Services.Board.Setup();
		Services.Tasks = new TaskManager();
		Services.Events = new EventManager();
		Services.Attackers = new AttackerManager();
		Services.Attackers.Setup();
		Services.Rulebook = new TurnManager();
		Services.Rulebook.Setup();
		Services.AttackDeck = new AttackerDeck();
		Services.AttackDeck.Setup();
		Services.Defenders = new DefenderManager();
		Services.Defenders.Setup();
		GameObject.Find(DEFENDER_UI).GetComponent<DefenderUIBehavior>().Setup();
		Services.Inputs = new InputManager();
		Services.UI = new UIManager();
		Services.UI.Setup();
		GameObject.Find(CHAR_SHEET_UI).GetComponent<CharacterSheetBehavior>().Setup();
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
