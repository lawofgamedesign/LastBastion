using UnityEngine;
using UnityEngine.UI;

public class CharacterSheetBehavior : MonoBehaviour {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the name button
	private Text nameButton;
	private const string NAME_OBJ = "Sheet button";
	private const string TEXT_OBJ = "Text";


	//the stat block
	private Text statBlock;
	private const string STAT_OBJ = "Stat block";
	private const string SPEED_LABEL = "Speed: ";
	private const string ATTACK_MOD_LABEL = "Attack: +";
	private const string ARMOR_LABEL = "Armor: -";
	private const string NEW_LINE = "\n";


	//the label for next abilities
	private Text nextLabel;
	private const string NEXT_LABEL_OBJ = "Next label";
	private const string NEXT_IN = "Next upgrade in ";
	private const string DEFEATS = " defeats";
	private const string DEFEAT = " defeat";
	private const string CHOOSE = "Choose an upgrade";
	private const int UPGRADE_READY = 0;


	//upgrade track 1
	private Text track1Next;
	private Text track1Current;
	private const string TRACK_1_NEXT_OBJ = "Track 1 next";
	private const string TRACK_1_CURRENT_OBJ = "Track 1 current";


	//is the character sheet currently hidden or displayed?
	private enum SheetStatus { Displayed, Hidden };
	private SheetStatus currentStatus = SheetStatus.Hidden;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables and turn the sheet off
	public void Setup(){
		nameButton = transform.Find(NAME_OBJ).Find(TEXT_OBJ).GetComponent<Text>();
		statBlock = transform.Find(STAT_OBJ).GetComponent<Text>();
		nextLabel = transform.Find(NEXT_LABEL_OBJ).GetComponent<Text>();
		track1Next = transform.Find(TRACK_1_NEXT_OBJ).Find(TEXT_OBJ).GetComponent<Text>();
		track1Current = transform.Find(TRACK_1_CURRENT_OBJ).Find(TEXT_OBJ).GetComponent<Text>();
		ChangeSheetState();
	}


	/// <summary>
	/// If the character sheet is off, switch it on; if on, switch it off.
	/// </summary>
	public void ChangeSheetState(){
		gameObject.SetActive(!gameObject.activeInHierarchy);
	}


	/// <summary>
	/// Change the name at the top of the character sheet.
	/// </summary>
	/// <param name="newName">The new name.</param>
	public void RenameSheet(string newName){
		nameButton.text = newName;
	}


	/// <summary>
	/// Change the stat block in the upper-right of the character sheet.
	/// </summary>
	/// <param name="speed">The defender's speed.</param>
	/// <param name="attackMod">The defender's attack modifier.</param>
	/// <param name="armor">The defender's armor.</param>
	public void ReviseStatBlock(int speed, int attackMod, int armor){
		statBlock.text = SPEED_LABEL + speed.ToString() + NEW_LINE +
						 ATTACK_MOD_LABEL + attackMod.ToString() + NEW_LINE +
						 ARMOR_LABEL + armor.ToString() + NEW_LINE;
	}


	/// <summary>
	/// Change the label for the abilities players can choose from based on the number of attackers defeated.
	/// </summary>
	/// <param name="defeats">The number of attackers the defender must defeat before choosing another upgrade.</param>
	public void ReviseNextLabel(int defeats){
		if (defeats == UPGRADE_READY){
			nextLabel.text = CHOOSE;
		} else if (defeats != 1){
			nextLabel.text = NEXT_IN + defeats.ToString() + DEFEATS;
		} else {
			nextLabel.text = NEXT_IN + defeats.ToString() + DEFEAT;
		}
	}


	/// <summary>
	/// Change the current and next ability text on the track on the left.
	/// </summary>
	/// <param name="next">The text of the next ability.</param>
	/// <param name="current">The text of the ability the player currently has.</param>
	public void ReviseTrack1(string next, string current){
		track1Next.text = next;
		track1Current.text = current;
	}


	/// <summary>
	/// If the character sheet is currently hidden, bring it to the center of the screen; if it's displayed, return it to the corner.
	/// </summary>
	public void DisplayCharSheet(){
		if (Services.Tasks.CheckForTaskOfType<MoveCharSheetTask>()) return; //reject attempts to move the character sheet while it's moving

		switch (currentStatus){
			case SheetStatus.Hidden:
				Services.Tasks.AddTask(new MoveCharSheetTask(MoveCharSheetTask.Move.Pick_up));
				currentStatus = SheetStatus.Displayed;
				break;
			case SheetStatus.Displayed:
				Services.Tasks.AddTask(new MoveCharSheetTask(MoveCharSheetTask.Move.Put_down));
				currentStatus = SheetStatus.Hidden;
				break;
		}
	}


	/// <summary>
	/// Called when the player clicks on an upgrade tree button.
	/// </summary>
	/// <param name="tree">The upgrade tree the player clicked on. Left is 0, right is 1.</param>
	public void PowerUpButton(int tree){
		if (Services.Defenders.IsAnyoneSelected()) Services.Defenders.GetSelectedDefender().PowerUp(tree);
	}
}
