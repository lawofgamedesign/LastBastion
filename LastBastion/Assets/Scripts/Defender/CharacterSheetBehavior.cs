using System.Collections.Generic;
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
	private const string DEFEATS = " Inspiration";
	private const string DEFEAT = " Inspiration";
	private const string CHOOSE = "Choose an upgrade";
	private const string WRONG_PHASE = "Upgrade at start of turn";
	private const int UPGRADE_READY = 0;


	//upgrade track 1
	private Text track1Next;
	private Text track1Current;
	private const string TRACK_1_NEXT_OBJ = "Track 1 next";
	private const string TRACK_1_CURRENT_OBJ = "Track 1 current";


	//upgrade track 2
	private Text track2Next;
	private Text track2Current;
	private const string TRACK_2_NEXT_OBJ = "Track 2 next";
	private const string TRACK_2_CURRENT_OBJ = "Track 2 current";


	//available cards
	private Text availCards;
	private const string AVAILABLE_CARDS_OBJ = "Available cards";
	private const string AVAIL_MSG = "Cards available: ";
	private const string COMMA = ", ";


	//is the character sheet currently hidden or displayed?
	public enum SheetStatus { Displayed, Hidden };
	public SheetStatus CurrentStatus { get; private set; }


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
		track2Next = transform.Find(TRACK_2_NEXT_OBJ).Find(TEXT_OBJ).GetComponent<Text>();
		track2Current = transform.Find(TRACK_2_CURRENT_OBJ).Find(TEXT_OBJ).GetComponent<Text>();
		availCards = transform.Find(AVAILABLE_CARDS_OBJ).GetComponent<Text>();
		Services.Events.Register<InputEvent>(HandleClicks);
		CurrentStatus = SheetStatus.Hidden;
		ChangeSheetState();
	}


	private void HandleClicks(Event e){
		Debug.Assert(e.GetType() == typeof(InputEvent), "Non-InputEvent in HandleClicks.");

		InputEvent inputEvent = e as InputEvent;

		if (CurrentStatus == SheetStatus.Hidden &&
			inputEvent.selected == gameObject) DisplayCharSheet();
		else if (CurrentStatus == SheetStatus.Displayed &&
				 inputEvent.selected != gameObject) DisplayCharSheet();
	}


	/// <summary>
	/// If the character sheet is off, switch it on; if on, switch it off.
	/// </summary>
	public void ChangeSheetState(){
		gameObject.SetActive(!gameObject.activeInHierarchy);
	}


	/// <summary>
	/// Switch the character sheet off. A convenient way to guarantee that the character sheet ends up off, for situations where
	/// ChangeSheetState() might not give the right result.
	/// </summary>
	public void ShutOffCharSheet(){
		gameObject.SetActive(false);
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
		if (defeats <= UPGRADE_READY){
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
	/// Change the current and next ability text on the track on the right.
	/// </summary>
	/// <param name="next">The text of the next ability.</param>
	/// <param name="current">The text of the ability the player currently has.</param>
	public void ReviseTrack2(string next, string current){
		track2Next.text = next;
		track2Current.text = current;
	}


	/// <summary>
	/// Show the cards this defender currrently has available.
	/// </summary>
	/// <param name="values">A list of the values of the defender's available cards.</param>
	public void ReviseAvailCards(List<int> values){
		string temp = "";

		for (int i = 0; i < values.Count; i++){
			temp += values[i].ToString();

			if (i != values.Count - 1) temp += COMMA;
		}

		availCards.text = AVAIL_MSG + temp;
	}


	/// <summary>
	/// If the character sheet is currently hidden, bring it to the center of the screen; if it's displayed, return it to the corner.
	/// </summary>
	public void DisplayCharSheet(){
		if (Services.Tasks.CheckForTaskOfType<MoveCharSheetTask>()) return; //reject attempts to move the character sheet while it's moving

		switch (CurrentStatus){
			case SheetStatus.Hidden:
				Services.Tasks.AddTask(new MoveCharSheetTask(MoveCharSheetTask.Move.Pick_up));
				break;
			case SheetStatus.Displayed:
				Services.Tasks.AddTask(new MoveCharSheetTask(MoveCharSheetTask.Move.Put_down));
				break;
		}
	}


	public void ChangeCurrentStatus(SheetStatus newStatus){
		CurrentStatus = newStatus;
	}


	/// <summary>
	/// Called when the player clicks on an upgrade tree button.
	/// </summary>
	/// <param name="tree">The upgrade tree the player clicked on. Left is 0, right is 1.</param>
	public void PowerUpButton(int tree){

		//if it's the upgrade phase, allow the player to upgrade
		if (Services.Rulebook.TurnMachine.CurrentState.GetType() == typeof(TurnManager.PlayerUpgrade)) {
			Services.Events.Fire(new PowerChoiceEvent(Services.Defenders.GetSelectedDefender(), tree));
		}

		//if it's not the upgrade phase, provide feedback
		else nextLabel.text = WRONG_PHASE;
	}
}
