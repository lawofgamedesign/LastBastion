using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the speech balloons used to transmit information to the player
	private GameObject speechBalloon;
	private const string BALLOON_OBJ = "Speech Balloon";
	private const string TEXT_OBJ = "Text";


	//the scroll rect where the chat text appears
	private Transform chatContent;
	private const string CHAT_OBJ = "Chat window";
	private const string VIEWPORT_OBJ = "Viewport";
	private const string CONTENT_OBJ = "Content";


	//buttons the player can press
	private GameObject phaseOverButton;
	private GameObject undoButton;
	private TextMeshProUGUI phaseText;
	private const string PHASE_BUTTON_OBJ = "Phase over button";
	private const string UNDO_BUTTON_OBJ = "Phase undo button";


	//common statements
	private const string MOVE_DONE_MSG = "I'm done moving. My defenders will fight now";
	private const string FIGHT_DONE_MSG = "My defenders are done fighting.";


	//the character sheet 
	private CharacterSheetBehavior charSheet;
	private const string CHAR_SHEET_OBJ = "Defender sheet canvas";


	//the attacker's combat cards
	private Transform deckOrganizer;
	private Transform discardOrganizer;
	private List<RectTransform> combatDeck = new List<RectTransform>();
	private const string COMBAT_CARD_OBJ = "Combat card";
	private const string COMBAT_CARD_ORGANIZER = "Draw deck";
	private const string DISCARD_ORGANIZER = "Discard pile";
	private const string ADDED_CARD = " added with value of ";
	private const string VALUE_OBJ = "Value";
	private const float CARD_VERTICAL_SPACE = 0.2f;
	private const float Y_AXIS_MESSINESS = 45.0f;


	//turn UI
	private Text turnText;
	private const string TURN_CANVAS = "Turn canvas";
	private const string TURN = "Turn ";
	private const string BACKSLASH = "/";


	//phase reminder
	//feedback for the player to help track which phase they're in
	protected TextMeshProUGUI phaseReminder;
	protected const string PHASE_OBJ = "Phase";
	protected const string CURRENT_MSG = "We're in the ";
	protected const string PHASE_MSG = " phase.";
	protected const string ATTACKER_MOVE = "Horde moves";
	protected const string PLAYER_MOVE = "Defenders move";
	protected const string PLAYER_FIGHT = "Defenders fight";
	protected const string BESIEGE = "Horde besieges";


	//balloon sizing
	protected const int SIZE_PER_ROW = 15; //used to determine how many rows a message needs to be divided into
	protected const int TEXT_ROW_SIZE = 17; //the amount of space each line should have, measured--roughly--in font size.
	protected const int BALLOON_PADDING = 5;


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//initialize variables and listen for phase-end events, so that the phase-end button behaves properly.
	public void Setup(){
		speechBalloon = Resources.Load<GameObject>(BALLOON_OBJ);
		chatContent = GameObject.Find(CHAT_OBJ).transform.Find(VIEWPORT_OBJ).Find(CONTENT_OBJ);
		phaseOverButton = GameObject.Find(PHASE_BUTTON_OBJ);
		phaseText = phaseOverButton.transform.Find(TEXT_OBJ).GetComponent<TextMeshProUGUI>();
		undoButton = GameObject.Find(UNDO_BUTTON_OBJ);
		undoButton.SetActive(false);
		Services.Events.Register<PhaseStartEvent>(PhaseStartHandling);


		//turn UI setup
		turnText = GameObject.Find(TURN_CANVAS).transform.Find(TEXT_OBJ).GetComponent<Text>();


		//phase reminder setup
		phaseReminder = GameObject.Find(PHASE_OBJ).GetComponent<TextMeshProUGUI>();


		//character sheet setup
		charSheet = GameObject.Find(CHAR_SHEET_OBJ).GetComponent<CharacterSheetBehavior>();


		//combat deck setup
		deckOrganizer = GameObject.Find(COMBAT_CARD_ORGANIZER).transform;
		discardOrganizer = GameObject.Find(DISCARD_ORGANIZER).transform;
		combatDeck.Clear(); //sanity check
		combatDeck = CreateCombatDeck();
	}


	#region statements


	/// <summary>
	/// For generic statements of unpredictable formatting--how many attacks the Ranger has left, etc.
	/// </summary>
	public void MakeStatement(string statement){
		TextMeshProUGUI balloon = AddBalloon(statement);
		balloon.text = statement;

		WaitTask waitTask = new WaitTask();
		WaitTask waitTask2 = new WaitTask(); //an ugly, irksome bodge! the scrollbar needs to be fully resized before the scrolling starts
		waitTask.Then(waitTask2);
		waitTask2.Then(new ScrollChatTask());

		Services.Tasks.AddTask(waitTask);
	}


	/// <summary>
	/// Say something when the attackers gain momentum.
	/// </summary>
	public void MomentumWarning(){
		string warning = "You lost, so I gain momentum. My attackers move " + (Services.Momentum.Momentum + 1).ToString() + " next turn.";

		MakeStatement(warning);
	}


	public void RemindPhase(FSM<TurnManager>.State phase){
		string temp = CURRENT_MSG;

		if (phase.GetType() == typeof(TurnManager.AttackersAdvance)){
			temp += ATTACKER_MOVE;
		} else if (phase.GetType() == typeof(TurnManager.PlayerMove)){
			temp += PLAYER_MOVE;
		} else if (phase.GetType() == typeof(TurnManager.PlayerFight)){
			temp += PLAYER_FIGHT;
		} else if (phase.GetType() == typeof(TurnManager.BesiegeWalls)){
			temp += BESIEGE;
		}

		temp += PHASE_MSG;

		phaseReminder.text = temp;
	}


	/// <summary>
	/// Defenders call this when they fight to explain the result of the combat.
	/// </summary>
	/// <param name="playerValue">The value of the player's card.</param>
	/// <param name="defender">The defender's script.</param>
	/// <param name="attacker">The attacker's script.</param>
	/// <param name="attackerValue">The value of the attacker's card.</param>
	/// <param name="damage">The damage inflicted. If none, any value is fine; this will be discarded.</param>
	public void ExplainCombat(int playerValue, DefenderSandbox defender, AttackerSandbox attacker, int attackerValue, int damage){
		string YOU_MSG = "You played a ";
		string BONUS_MSG = ", plus a bonus of ";
		string ATK_MSG = "I played a ";
		string YOU_WIN_MSG = "You beat me by ";
		string I_WIN_MSG = "You didn't beat me, so I won.";
		string ARMOR_MSG = "My armor of ";
		string REDUCE_MSG = " reduces the damage.";
		string TAKE_DAMAGE_MSG = "My piece takes ";
		string DAMAGE_MSG = " damage, and has ";
		string HEALTH_MSG = " health left.";
		string PERIOD = ".";
		string NEWLINE = "\n";

		string explanation = YOU_MSG + playerValue.ToString() + BONUS_MSG + defender.AttackMod.ToString() + PERIOD + NEWLINE +
							 ATK_MSG + attackerValue.ToString() + PERIOD + NEWLINE;

		if (playerValue > attackerValue){
			explanation += YOU_WIN_MSG + (playerValue - attackerValue).ToString() + PERIOD + NEWLINE;

			if (attacker.Armor > 0) explanation += ARMOR_MSG + attacker.Armor + REDUCE_MSG + NEWLINE;

			explanation += TAKE_DAMAGE_MSG + damage.ToString() + DAMAGE_MSG + attacker.Health + HEALTH_MSG;
		} else {
			explanation += I_WIN_MSG;
		}

		MakeStatement(explanation);
	}


	public void SetTurnText(int currentTurn, int totalTurns){
		turnText.text = TURN + currentTurn.ToString() + BACKSLASH + totalTurns.ToString();

		string turnMessage = "This is turn " + currentTurn.ToString() + " of " + totalTurns.ToString() + " this wave.";

		MakeStatement(turnMessage);
	}


	/// <summary>
	/// Handle the phase-end button, which displays when the player is in control and can end the phase, with
	/// appropriate text for each phase.
	/// 
	/// This triggers based on phase start, rather than phase end, because the button doesn't always end the phase.
	/// Frex., the first click on the button doesn't end the Defenders Move phase if all defenders have not yet moved.
	/// </summary>
	/// <param name="e">A PhaseStartEvent.</param>
	private void PhaseStartHandling(Event e){
		Debug.Assert(e.GetType() == typeof(PhaseStartEvent));

		PhaseStartEvent startEvent = e as PhaseStartEvent;

		if (startEvent.Phase.GetType() == typeof(TurnManager.PlayerMove)){
			phaseText.text = MOVE_DONE_MSG;
			phaseOverButton.SetActive(true);
			undoButton.SetActive(true);
		} else if (startEvent.Phase.GetType() == typeof(TurnManager.PlayerFight)){
			MakeStatement(MOVE_DONE_MSG);
			phaseText.text = FIGHT_DONE_MSG;
		} else if (startEvent.Phase.GetType() == typeof(TurnManager.BesiegeWalls)){
			MakeStatement(FIGHT_DONE_MSG);
			phaseOverButton.SetActive(false);
			undoButton.SetActive(false);
		}
	}


	/// <summary>
	/// Create a speech balloon, sized appropriately for its text.
	/// </summary>
	/// <returns>The balloon's TextMeshPro component, so that its text can be set.</returns>
	private TextMeshProUGUI AddBalloon(string message){
		TextMeshProUGUI balloon = MonoBehaviour.Instantiate<GameObject>(speechBalloon, chatContent).transform.Find(TEXT_OBJ).GetComponent<TextMeshProUGUI>();

		int rows = message.Length/SIZE_PER_ROW;

		int height = TEXT_ROW_SIZE * rows;

		balloon.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(balloon.rectTransform.rect.width, height + BALLOON_PADDING);

		foreach (Transform child in balloon.transform.parent){
			child.GetComponent<RectTransform>().sizeDelta = new Vector2(balloon.rectTransform.rect.width, height);
		}


		return balloon;
	}


	#endregion statements

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
	/// <param name="values">The values of the defender's currently available combat cards.</param>
	public void TakeOverCharSheet(string name, int speed, int attackMod, int armor, int defeatsToNextUpgrade, int defeatsSoFar, List<int> values){
		charSheet.RenameSheet(name);
		charSheet.ReviseStatBlock(speed, attackMod, armor);
		charSheet.ReviseNextLabel(defeatsToNextUpgrade - defeatsSoFar);
		charSheet.ReviseAvailCards(values);
		if (!charSheet.gameObject.activeInHierarchy) charSheet.ChangeSheetState();
	}


	/// <summary>
	/// As above, but also updates the upgrade tracks
	/// </summary>
	/// <param name="track1Next">The next upgrade on the left-side track.</param>
	/// <param name="track1Current">The defender's current upgrade on the left-side track.</param>
	/// <param name="track2Next">The next upgrade on the right-side track.</param>
	/// <param name="track2Current">The defender's current upgrade on the right-side track.</param>
	/// <param name="values">The values of the defender's currently available combat cards.</param>
	public void TakeOverCharSheet(string name,
		int speed,
		int attackMod,
		int armor,
		int defeatsToNextUpgrade,
		int defeatsSoFar,
		string track1Next,
		string track1Current,
		string track2Next,
		string track2Current,
		List<int> values){
		charSheet.ReviseTrack1(track1Next, track1Current);
		charSheet.ReviseTrack2(track2Next, track2Current);
		TakeOverCharSheet(name, speed, attackMod, armor, defeatsToNextUpgrade, defeatsSoFar, values);
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
	/// Update the character sheet's statement what combat cards are available.
	/// </summary>
	/// <param name="values">A list of the values of the defender's currently available cards.</param>
	public void ReviseCardsAvail(List<int> values){
		charSheet.ReviseAvailCards(values);
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
	/// Pick up or put down the character sheet.
	/// </summary>
	public void ShowOrHideSheet(){
		charSheet.DisplayCharSheet();
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


	/// <summary>
	/// Get whether the character sheet is displayed or hidden.
	/// </summary>
	/// <returns>The character sheet's status.</returns>
	public CharacterSheetBehavior.SheetStatus GetCharSheetStatus(){
		return charSheet.CurrentStatus;
	}

	#endregion

	#region combat deck


	/// <summary>
	/// Create a visible deck of cards for the attackers
	/// </summary>
	private List<RectTransform> CreateCombatDeck(){
		//get rid of the existing cards
		//foreach (Transform card in deckOrganizer) MonoBehaviour.Destroy(card.gameObject);
		foreach (Transform card in discardOrganizer) MonoBehaviour.Destroy(card.gameObject);


		//create a fresh deck
		List<RectTransform> temp = new List<RectTransform>();


		for (int i = 0; i < Services.AttackDeck.GetDeckCount(); i++){
			GameObject newCard = MonoBehaviour.Instantiate<GameObject>(Resources.Load<GameObject>(COMBAT_CARD_OBJ), deckOrganizer);
			newCard.name = COMBAT_CARD_OBJ + " " + i.ToString();
			newCard.transform.localPosition = new Vector3(0.0f, 0.0f, (i * CARD_VERTICAL_SPACE) * -1.0f); //because of the canvas' orientation, must * -1
			temp.Add(newCard.GetComponent<RectTransform>());
		}


		return temp;
	}


	public void RecreateCombatDeck(){
		combatDeck.Clear();
		combatDeck = CreateCombatDeck();
	}


	public void DrawCombatCard(int value){
		Debug.Assert(deckOrganizer.childCount > 0, "No visual card to draw.");

		Transform topCard = deckOrganizer.GetChild(deckOrganizer.childCount - 1);

		topCard.SetParent(discardOrganizer);
		topCard.localPosition = new Vector3(0.0f, 0.0f, ((discardOrganizer.childCount - 1) * CARD_VERTICAL_SPACE) * -1.0f); //-1 b/c don't include this card
		//make the discard a little sloppy, so that it's easier to recognize as distinct from the deck
		topCard.localRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, Random.Range(-Y_AXIS_MESSINESS, Y_AXIS_MESSINESS)));

		topCard.Find(VALUE_OBJ).GetComponent<Text>().text = value.ToString();
	}


	/// <summary>
	/// Puts a new card on the top of the deck.
	/// 
	/// Note that being on top doesn't mean that card will be drawn first--its value isn't game-relevant. The value is only used
	/// to give the card a descriptive name. Being on top only means that this representation of a card will be the next "drawn" visually.
	/// </summary>
	/// <param name="attacker">The attacker that's adding the card to the deck.</param>
	/// <param name="value">The new card's value.</param>
	public void AddCardToDeck(Transform attacker, int value){
		GameObject newCard = MonoBehaviour.Instantiate<GameObject>(Resources.Load<GameObject>(COMBAT_CARD_OBJ), deckOrganizer);
		newCard.name = COMBAT_CARD_OBJ + ADDED_CARD + value.ToString();
		newCard.transform.localPosition = new Vector3(0.0f, 0.0f, ((deckOrganizer.childCount - 1) * CARD_VERTICAL_SPACE) * -1.0f); //because of the canvas' orientation, must * -1
		combatDeck.Add(newCard.GetComponent<RectTransform>());


		//add a task that puts a card into the visibule deck
		if (!Services.Tasks.CheckForTaskOfType<PutInCardTask>()){ //this is the first/only such task: add the relevant task
			Services.Tasks.AddTask(new PutInCardTask(attacker, deckOrganizer, value));
		} 
		//this is the second or third such task. If it's the second, GetLastTaskOfType() will find the first one and make a new task to follow
		//if it's the third, GetLastTaskOfType() will NOT find the second yet (i.e., will return null).
		//DelayedPutInCardTask will handle waiting until the second is findable, and then add the third task to follow the second
		else {
			if (Services.Tasks.GetLastTaskOfType<PutInCardTask>() == null){
				Services.Tasks.AddTask(new DelayedPutInCardTask(attacker, deckOrganizer, value));
			} else {
				Services.Tasks.GetLastTaskOfType<PutInCardTask>().Then(new PutInCardTask(attacker, deckOrganizer, value));
			}
		}
	}


	/// <summary>
	/// Take a card out of the draw deck. This only affects visuals, not the game logic.
	/// </summary>
	/// <param name="attacker">The attacker that's removing the card from the deck.</param>
	/// <param name="value">The card's value.</param>
	public void RemoveCardFromDeck(Transform attacker, int value){
		//sanity check; if this is somehow trying to remove a card from an empty deck, stop and do nothing
		if (deckOrganizer.childCount == 0) return;

		MonoBehaviour.Destroy(deckOrganizer.GetChild(deckOrganizer.childCount - 1).gameObject);

		//if the card pulled out of the deck was the last card in the deck, reshuffle
		//note that this only affects the visuals; AttackerDeck is responsible for reshuffling the deck within the game's logic
		if (deckOrganizer.childCount == 0){
			combatDeck.Clear();
			combatDeck = CreateCombatDeck();
		}

		if (!Services.Tasks.CheckForTaskOfType<ThrowAwayCardTask>()){
			Services.Tasks.AddTask(new ThrowAwayCardTask(deckOrganizer, attacker, value));
		} else {
			Services.Tasks.GetLastTaskOfType<ThrowAwayCardTask>().Then(new ThrowAwayCardTask(deckOrganizer, attacker, value));
		}
	}


	/// <summary>
	/// Take a card out of the discard pile. This only affects visuals, not the game logic.
	/// </summary>
	/// <param name="attacker">The attacker that's removing the card from the deck.</param>
	/// <param name="value">The card's value.</param>
	public void RemoveCardFromDiscard(Transform attacker, int value){
		//sanity check; if this is somehow trying to remove a card from an empty discard, stop and do nothing
		if (discardOrganizer.childCount == 0) return;

		MonoBehaviour.Destroy(discardOrganizer.GetChild(0).gameObject); //destroy the bottom card in the discard

		foreach (Transform card in discardOrganizer){
			card.localPosition += new Vector3(0.0f, 0.0f, CARD_VERTICAL_SPACE); //add to lower because of the canvas' orientation
		}

		if (!Services.Tasks.CheckForTaskOfType<ThrowAwayCardTask>()){
			Services.Tasks.AddTask(new ThrowAwayCardTask(discardOrganizer, attacker, value));
		} else {
			Services.Tasks.GetLastTaskOfType<ThrowAwayCardTask>().Then(new ThrowAwayCardTask(discardOrganizer, attacker, value));
		}
	}


	#endregion
}
