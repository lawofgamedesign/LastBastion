using UnityEngine;
using UnityEngine.UI;

public class UIManager {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//UI elements under this script's control
	private Text extraText;
	private Text turnText;
	private CharacterSheetBehavior charSheet;
	private GameObject undoButton;
	private const string EXTRA_CANVAS = "Extra info canvas";
	private const string TURN_CANVAS = "Turn canvas";
	private const string TEXT_OBJ = "Text";
	private const string CHAR_SHEET_OBJ = "Defender sheet canvas";
	private const string UNDO_BUTTON_OBJ = "Undo button";


	//text to be written to various UI elements
	private const string TURN = "Turn ";
	private const string BACKSLASH = "/";
	private const string GAME_OVER = "Game over";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables
	public void Setup(){
		extraText = GameObject.Find(EXTRA_CANVAS).transform.Find(TEXT_OBJ).GetComponent<Text>();
		turnText = GameObject.Find(TURN_CANVAS).transform.Find(TEXT_OBJ).GetComponent<Text>();
		charSheet = GameObject.Find(CHAR_SHEET_OBJ).GetComponent<CharacterSheetBehavior>();
		undoButton = GameObject.Find(UNDO_BUTTON_OBJ);
		ToggleUndoButton();
	}


	/// <summary>
	/// Set the text that displays on the card in the middle-left of the screen.
	/// </summary>
	/// <param name="info">The text to display.</param>
	public void SetExtraText(string info){
		extraText.text = info;
	}


	/// <summary>
	/// Set the text on the card in the middle-right that tracks the current turn.
	/// </summary>
	/// <param name="turn">The current turn.</param>
	/// <param name="totalTurns">How many turns there are in this wave.</param>
	public void SetTurnText(int turn, int totalTurns){
		if (turn <= totalTurns){
			turnText.text = TURN + turn.ToString() + BACKSLASH + totalTurns.ToString();
		}
	}


	/// <summary>
	/// Switch the undo button on or off.
	/// </summary>
	public void ToggleUndoButton(){
		undoButton.SetActive(!undoButton.activeInHierarchy);
	}


	#region character sheet


	/// <summary>
	/// Change the character sheet to reflect a particular defender, and turn the character sheet on if necessary.
	/// </summary>
	/// <param name="name">The defender's name.</param>
	/// <param name="speed">The defender's speed.</param>
	/// <param name="attackMod">The defender's attack mod.</param>
	/// <param name="armor">The defender's armor.</param>
	/// <param name="defeatsToNextUpgrade">The number of attackers the defender must defeat to upgrade.</param>
	/// <param name="defeatsSoFar">The defender's current progress toward the next upgrade.</param>
	public void TakeOverCharSheet(string name, int speed, int attackMod, int armor, int defeatsToNextUpgrade, int defeatsSoFar){
		charSheet.RenameSheet(name);
		charSheet.ReviseStatBlock(speed, attackMod, armor);
		charSheet.ReviseNextLabel(defeatsToNextUpgrade - defeatsSoFar);
		if (!charSheet.gameObject.activeInHierarchy) charSheet.ChangeSheetState();
	}


	/// <summary>
	/// As above, but also updates the upgrade tracks
	/// </summary>
	/// <param name="track1Next">The next upgrade on the left-side track.</param>
	/// <param name="track1Current">The defender's current upgrade on the left-side track.</param>
	/// <param name="track2Next">The next upgrade on the right-side track.</param>
	/// <param name="track2Current">The defender's current upgrade on the right-side track.</param>
	public void TakeOverCharSheet(string name,
								  int speed,
								  int attackMod,
								  int armor,
								  int defeatsToNextUpgrade,
								  int defeatsSoFar,
								  string track1Next,
								  string track1Current,
								  string track2Next,
								  string track2Current){
		charSheet.ReviseTrack1(track1Next, track1Current);
		charSheet.ReviseTrack2(track2Next, track2Current);
		TakeOverCharSheet(name, speed, attackMod, armor, defeatsToNextUpgrade, defeatsSoFar);
	}


	/// <summary>
	/// The character sheet indicates how many attackers the defender must defeat in order to upgrade; this changes
	/// that number.
	/// </summary>
	/// <param name="defeatsToNextUpgrade">The number of attackers the defender must defeat to upgrade.</param>
	/// <param name="defeatsSoFar">The defender's current progress toward the next upgrade.</param>
	public void ReviseNextLabel(int defeatsToNextUpgrade, int defeatsSoFar){
		charSheet.ReviseNextLabel(defeatsToNextUpgrade - defeatsSoFar);
	}


	/// <summary>
	/// Switch the character sheet on or off.
	/// </summary>
	public void ChangeCharSheetState(){
		charSheet.ChangeSheetState();
	}


	/// <summary>
	/// Shut off the character sheet.
	/// </summary>
	public void ShutOffCharSheet(){
		charSheet.ShutOffCharSheet();
	}


	/// <summary>
	/// Change the text of the upgrade track on the left.
	/// </summary>
	/// <param name="next">The next upgrade on the left-side track.</param>
	/// <param name="current">The defender's current upgrade on the left-side track.</param>
	public void ReviseTrack1(string next, string current){
		charSheet.ReviseTrack1(next, current);
	}


	/// <summary>
	/// Change the text of the upgrade track on the right.
	/// </summary>
	/// <param name="next">The next upgrade on the right-side track.</param>
	/// <param name="current">The defender's current upgrade on the right-side track.</param>
	public void ReviseTrack2(string next, string current){
		charSheet.ReviseTrack2(next, current);
	}

	#endregion
}
