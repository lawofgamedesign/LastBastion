/// <summary>
/// Base class for defenders. All defender "verbs" are contained here.
/// </summary>
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Rigidbody))]
public class DefenderSandbox : MonoBehaviour {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//defender stats
	public int Speed { get; set; } //in spaces/turn
	public int AttackMod { get; set; }
	public int Armor { get; set; }


	//generic stats for testing purposes
	protected int baseSpeed = 4;
	protected int baseAttackMod = 1;
	protected int baseArmor = 1;


	//is this attacker currently selected? Also includes related variables
	public bool Selected;
	protected GameObject selectedParticle;
	protected const string SELECT_PARTICLE_OBJ = "Selected particle";


	//how many spaces of movement does the defender have left? Also other fields relating to movement
	protected int remainingSpeed = 0;
	protected List<TwoDLoc> moves = new List<TwoDLoc>();
	protected LineRenderer lineRend;
	protected Button moveButton;
	protected Button undoButton;
	protected Transform moveCanvas;
	protected const string MOVE_BUTTON_OBJ = "Done moving button";
	protected const string UNDO_BUTTON_OBJ = "Undo move button";
	protected const string PRIVATE_UI_CANVAS = "Defender canvas";
	[SerializeField] protected float moveSpeed = 1.0f; //movement on screen, as opposed to spaces on the grid
	protected Rigidbody rb;
	protected const float LINE_OFFSET = 0.5f; //picks the movement line up off the board to avoid clipping


	//location in the grid
	protected TwoDLoc GridLoc { get; set; }


	//this defender's hand of cards, along with UI
	protected List<Card> combatHand;
	protected Button noFightButton;
	protected Transform uICanvas;
	protected DefenderUIBehavior defenderCards;
	protected const string CARD_UI_CANVAS = "Defender card canvas";
	protected const string TEXT_OBJ = "Text";
	protected const string NO_FIGHT_BUTTON = "Done fighting button";


	//combat
	public Card ChosenCard { get; set; }
	public const int NO_CARD_SELECTED = 999;


	//character sheet UI
	private const string GENERIC_NAME = "Generic defender";


	//extra UI for displaying combat math
	protected Text extraText;
	protected const string EXTRA_INFO_OBJ = "Extra info canvas";
	protected const string DEFENDER_VALUE = "Defender: ";
	protected const string ATTACKER_VALUE = "Horde: ";
	protected const string PLUS = " + ";
	protected const string EQUALS = " = ";
	protected const string HITS = "Hits: ";
	protected const string DAMAGE = "Damage: ";
	protected const string NEWLINE = "\n";


	//powering up
	protected int defeatedSoFar = 0;
	protected int DefeatedSoFar {
		get { return defeatedSoFar; }
		set {
			if (value > defeatedSoFar) xpParticle.Play();
			defeatedSoFar = value;
		}
	}
	protected const int START_DEFEATED = 0;
	protected int defeatsToNextUpgrade = 0;
	protected int upgradeInterval = 3;


	//feedback particle systems
	protected ParticleSystem xpParticle;
	protected GameObject powerupReadyParticle;
	protected const string XP_PARTICLE_OBJ = "XP gained particle";
	protected const string POWER_UP_PARTICLE_OBJ = "Powerup ready particle";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	#region setup


	//initialize variables
	public virtual void Setup(){
		Speed = baseSpeed;
		AttackMod = baseAttackMod;
		Armor = baseArmor;
		Selected = false;
		selectedParticle = transform.Find(SELECT_PARTICLE_OBJ).gameObject;
		lineRend = GetComponent<LineRenderer>();
		ClearLine();
		moveButton = transform.Find(PRIVATE_UI_CANVAS).Find(MOVE_BUTTON_OBJ).GetComponent<Button>();
		undoButton = transform.Find(PRIVATE_UI_CANVAS).Find(UNDO_BUTTON_OBJ).GetComponent<Button>();
		moveCanvas = transform.Find(PRIVATE_UI_CANVAS);
		rb = GetComponent<Rigidbody>();
		GridLoc = new TwoDLoc(0, 0); //default initialization
		combatHand = MakeCombatHand();
		uICanvas = GameObject.Find(CARD_UI_CANVAS).transform;
		defenderCards = uICanvas.GetComponent<DefenderUIBehavior>();
		noFightButton = transform.Find(PRIVATE_UI_CANVAS).Find(NO_FIGHT_BUTTON).GetComponent<Button>();
		extraText = GameObject.Find(EXTRA_INFO_OBJ).transform.Find(TEXT_OBJ).GetComponent<Text>();
		DefeatedSoFar = START_DEFEATED;
		xpParticle = transform.Find(XP_PARTICLE_OBJ).GetComponent<ParticleSystem>();
		powerupReadyParticle = transform.Find(POWER_UP_PARTICLE_OBJ).gameObject;
	}


	/// <summary>
	/// Make a combat hand for this defender.
	/// </summary>
	/// <returns>A list of cards in the hand.</returns>
	protected virtual List<Card> MakeCombatHand(){
		return new List<Card>() { new Card(3), new Card(4), new Card(5) };
	}


	/// <summary>
	/// Sets this defender's location in the grid.
	/// </summary>
	/// <param name="x">The grid x coordinate (not world x-position!).</param>
	/// <param name="z">The grid z coordinate (not world z-position!).</param>
	public void NewLoc(int x, int z){
		GridLoc.x = x;
		GridLoc.z = z;
	}


	#endregion setup


	#region movement


	/// <summary>
	/// Reports whether this defender is in the middle of a move.
	/// </summary>
	/// <returns><c>true</c> if this instance is moving, <c>false</c> otherwise.</returns>
	public virtual bool IsMoving(){
		return remainingSpeed == Speed || remainingSpeed == 0 ? false : true;
	}


	/// <summary>
	/// Carries out all effects associated with being selected to move.
	/// </summary>
	public virtual void BeSelectedForMovement(){
		if (Services.Defenders.IsDone(this)){ //if this defender has already reported itself done with this phase, it can't be selected
			TakeOverCharSheet();
			return;
		}

		Selected = true;
		selectedParticle.SetActive(true);
		moveButton.gameObject.SetActive(true);
		undoButton.gameObject.SetActive(true);
		TakeOverCharSheet();
	}



	/// <summary>
	/// Does everything that needs to happen when another defender is selected.
	/// </summary>
	public virtual void BeUnselected(){
		Selected = false;
		selectedParticle.SetActive(false);
		ChosenCard = null; //relevant for the fight phase
		moveButton.gameObject.SetActive(false);
		undoButton.gameObject.SetActive(false);
		noFightButton.gameObject.SetActive(false);
		moveCanvas.position = Services.Board.GetWorldLocation(GridLoc.x, GridLoc.z) + new Vector3(0.0f, LINE_OFFSET, 0.0f);
		Services.Defenders.NoSelectedDefender();
	}


	/// <summary>
	/// Call this at the start of the defender movement phase.
	/// </summary>
	public virtual void PrepareToMove(){
		moves.Clear();
		moves.Add(new TwoDLoc(GridLoc.x, GridLoc.z));
		remainingSpeed = Speed;
		ClearLine();
		DrawLine(0, GridLoc.x, GridLoc.z);
		moveCanvas.position = Services.Board.GetWorldLocation(GridLoc.x, GridLoc.z) + new Vector3(0.0f, LINE_OFFSET, 0.0f);
	}


	public TwoDLoc ReportGridLoc(){
		return GridLoc;
	}


	private void ReportMovesContents(){
		for (int i = 0; i < moves.Count; i++) Debug.Log("moves[" + i + "] x == " + moves[i].x + ", z == " + moves[i].z);
	}


	/// <summary>
	/// Whenever the player tries to move a defender, TurnManager calls this function to determine whether the move is legal--
	/// the defender has the movement remaining, the space is legal to enter, etc.
	/// 
	/// A move is illegal if:
	/// 1. It is not adjacent to the defender (or to the last space the defender would move to), or
	/// 2. the space is occupied.
	/// </summary>
	/// <param name="loc">Location.</param>
	public virtual bool TryPlanMove(TwoDLoc loc){
		if (CheckAdjacent(moves[Speed - remainingSpeed], loc) &&
			remainingSpeed > 0 &&
			!CheckAlreadyThroughSpace(loc) &&
			Services.Board.GeneralSpaceQuery(loc.x, loc.z) == SpaceBehavior.ContentType.None){

			moves.Add(loc);
			remainingSpeed--;
			DrawLine(Speed - remainingSpeed, loc.x, loc.z);
			moveCanvas.position = Services.Board.GetWorldLocation(loc.x, loc.z) + new Vector3(0.0f, LINE_OFFSET, 0.0f);

			return true;
		}

		return false;
	}


	protected bool CheckAlreadyThroughSpace(TwoDLoc loc){
		foreach (TwoDLoc move in moves){
			if (move.x == loc.x &&
				move.z == loc.z) return true;
		}

		return false;
	}


	/// <summary>
	/// Draw the line that indicates to players where a defender is moving.
	/// </summary>
	/// <param name="index">The index of the new line position in the list of positions.</param>
	/// <param name="x">The x grid coordinate of the new position.</param>
	/// <param name="z">The z grid coordinate of the new position.</param>
	protected virtual void DrawLine(int index, int x, int z){
		lineRend.positionCount++;

		Vector3 lineEnd = Services.Board.GetWorldLocation(x, z);
		lineEnd.y += LINE_OFFSET;

		lineRend.SetPosition(index, lineEnd);
	}


	/// <summary>
	/// The undo button uses this to reset a defender's movement.
	/// </summary>
	public void UndoMove(){
		ClearLine();
		PrepareToMove();
		Services.Events.Fire(new UndoMoveEvent());
	}


	/// <summary>
	/// Called by the UI to move the defender.
	/// </summary>
	public virtual void Move(){
		Services.Tasks.AddTask(new MoveDefenderTask(rb, moveSpeed, moves));
		ClearLine();


		//move on the grid used for game logic
		Services.Board.TakeThingFromSpace(GridLoc.x, GridLoc.z);
		TwoDLoc destination = moves[moves.Count - 1];
		Services.Board.PutThingInSpace(gameObject, destination.x, destination.z, SpaceBehavior.ContentType.Defender);
		NewLoc(destination.x, destination.z);
		BeUnselected();
		Services.UI.ShutOffCharSheet();
		Services.Defenders.DeclareSelfDone(this);

		remainingSpeed = 0;
	}


	/// <summary>
	/// Reset the line players use to plan their movement.
	/// </summary>
	protected virtual void ClearLine(){
		lineRend.positionCount = 0;
	}


	/// <summary>
	/// Determine whether two grid spaces are orthogonally adjacent.
	/// </summary>
	/// <returns><c>true</c>, if so, <c>false</c> if not.</returns>
	/// <param name="next">the grid space being checked.</param>
	/// <param name="current">The space being checked against.</param>
	protected bool CheckAdjacent(TwoDLoc next, TwoDLoc current){
		return ((next.x == current.x && Mathf.Abs(next.z - current.z) == 1) ||
				(Mathf.Abs(next.x - current.x) == 1 && next.z == current.z)) ? true : false;
	}


	/// <summary>
	/// Reset this defender's position and available movement when a player undoes the Defenders Move phase.
	/// </summary>
	public void UndoMovePhase(){
		if (Services.Undo == null) return; //in the event that there's somehow no undo system in place, prevent null reference exceptions


		Services.Board.TakeThingFromSpace(GridLoc.x, GridLoc.z);
		GridLoc = new TwoDLoc(Services.Undo.GetDefenderLoc(this).x, Services.Undo.GetDefenderLoc(this).z);
		Services.Board.PutThingInSpace(gameObject, GridLoc.x, GridLoc.z, SpaceBehavior.ContentType.Defender);

		transform.position = Services.Board.GetWorldLocation(GridLoc.x, GridLoc.z);
	}


	/// <summary>
	/// Does this defender have any movement left?
	/// </summary>
	/// <returns><c>true</c> if this defender has movement left, <c>false</c> otherwise.</returns>
	public bool CheckForRemainingMovement(){
		if (remainingSpeed != 0) return true;
		else return false;
	}


	#endregion movement


	#region combat


	/// <summary>
	/// If this defender needs to do anything at the start of the Defender Fight phase, that happens here.
	/// </summary>
	public virtual void PrepareToFight(){
		//generic defenders don't need to do anything
	}


	/// <summary>
	/// Carries out all effects associated with being selected to move.
	/// </summary>
	public virtual void BeSelectedForFight(){
		if (Services.Defenders.IsDone(this)){ //if this defender has already reported itself done with this phase, it can't be selected
			TakeOverCharSheet();
			return;
		}

		Selected = true;
		selectedParticle.SetActive(true);
		uICanvas.GetComponent<DefenderUIBehavior>().ClearAllSelectedColor();

		Debug.Assert(combatHand.Count <= uICanvas.childCount, "Too many combat cards to display!");

		for (int i = 0; i < combatHand.Count; i++){
			uICanvas.GetChild(i).Find(TEXT_OBJ).GetComponent<Text>().text = combatHand[i].Value.ToString();
			uICanvas.GetChild(i).gameObject.SetActive(true);
		}

		ChosenCard = null;

		noFightButton.gameObject.SetActive(true);

		TurnOverAvailableCards();

		TakeOverCharSheet();
	}


	public virtual void TurnOverAvailableCards(){
		uICanvas.GetComponent<DefenderUIBehavior>().FlipAllCardsDown();

		for (int i = 0; i < combatHand.Count; i++){
			if (combatHand[i].Available){
//				PickUpCardTask pickUpTask = new PickUpCardTask(uICanvas.GetChild(i).GetComponent<RectTransform>());
//				FlipCardTask flipTask = new FlipCardTask(uICanvas.GetChild(i).GetComponent<RectTransform>(), FlipCardTask.UpOrDown.Up);
//				PutDownCardTask putDownTask = new PutDownCardTask(uICanvas.GetChild(i).GetComponent<RectTransform>());
//				putDownTask.Then(new ShutOffCardsTask()); //the cards will only shut off if the player immediately hits "done" and doesn't select anyone else
//
//				pickUpTask.Then(flipTask);
//				flipTask.Then(putDownTask);
//
//				Services.Tasks.AddTask(pickUpTask);
				defenderCards.FlipCardUp(i);

			}
		}
	}


	/// <summary>
	/// Note which combat card the player has chosen. Reject attempts to choose a card that isn't available.
	/// </summary>
	/// <param name="index">The card's number, zero-indexed.</param>
	public virtual void AssignChosenCard(int index){
		if (combatHand[index].Available && combatHand[index] != ChosenCard){
			ChosenCard = combatHand[index];
			//Services.Tasks.AddTask(new PickUpCardTask(uICanvas.GetChild(index).GetComponent<RectTransform>()));
			defenderCards.TurnSelectedColor(index);
		}
	}


	/// <summary>
	/// Returns the chosen card's value.
	/// </summary>
	/// <returns>The value; if no card is currently selected, this will be 999 (NO_CARD_SELECTED).</returns>
	public virtual int GetChosenCardValue(){
		if (ChosenCard == null) return NO_CARD_SELECTED;
		return ChosenCard.Value;
	}


	/// <summary>
	/// Damages an attacker if it is directly north and the player has chosen a stronger card than its value.
	/// </summary>
	/// <param name="attacker">The attacker this defender is fighting.</param>
	public virtual void TryFight(AttackerSandbox attacker){
		if (!CheckIsNorth(attacker)) return; //don't fight if the attacker isn't directly to the north

		//if the Defender gets this far, a fight will actually occur; get a combat card for the attacker
		int attackerValue = Services.AttackDeck.GetAttackerCard().Value;
		extraText.text = DisplayCombatMath(attacker, attackerValue);

		if (ChosenCard.Value + AttackMod > attackerValue + attacker.AttackMod){
			attacker.TakeDamage((ChosenCard.Value + AttackMod) - (attackerValue + attacker.AttackMod + attacker.Armor));
			FinishWithCard();
			DefeatedSoFar++;
			Services.UI.ReviseNextLabel(defeatsToNextUpgrade, DefeatedSoFar);
			DoneFighting();
		} else {
			attacker.FailToDamage();
			FinishWithCard();
			DoneFighting();
		}
	}


	/// <summary>
	/// Determine whether the time-to-upgrade particle should be displayed.
	/// 
	/// Note that this version does not take into account where the defender is on their upgrade tracks,
	/// and whether there are any upgrades available. It should always be overridden!
	/// </summary>
	/// <returns><c>true</c> if the particle should be switched on, <c>false</c> otherwise.</returns>
	protected virtual bool CheckUpgradeStatus(){
		if (DefeatedSoFar >= defeatsToNextUpgrade) return true;
		return false;
	}


	/// <summary>
	/// Show the math behind combats on the extra information UI.
	/// </summary>
	/// <returns>A string explaining the math behind each combat.</returns>
	/// <param name="attacker">The attacker this defender is fighting.</param>
	/// <param name="attackerValue">The value of the attacker's card.</param>
	protected virtual string DisplayCombatMath(AttackerSandbox attacker, int attackerValue){
		int damage = (ChosenCard.Value + AttackMod) - (attackerValue + attacker.AttackMod + attacker.Armor);

		damage = damage < 0 ? 0 : damage;

		return DEFENDER_VALUE + ChosenCard.Value + PLUS + AttackMod + NEWLINE +
			   ATTACKER_VALUE + attackerValue + PLUS + attacker.AttackMod + NEWLINE +
			   HITS + ((ChosenCard.Value + AttackMod) - (attackerValue + attacker.AttackMod)).ToString() + NEWLINE + 
			   DAMAGE + damage.ToString();
	}


	/// <summary>
	/// Is an attacker directly north of this defender?
	/// </summary>
	/// <returns><c>true</c> if the attacker is one space north, <c>false</c> otherwise.</returns>
	/// <param name="attacker">The attacker being checked.</param>
	protected bool CheckIsNorth(AttackerSandbox attacker){
		if (attacker.XPos == GridLoc.x && attacker.ZPos == GridLoc.z + 1) return true;
		return false;
	}


	/// <summary>
	/// Handle everything that needs to happen to the player's chosen card when a defender has finished a combat.
	/// </summary>
	protected virtual void FinishWithCard(){
		ChosenCard.Available = false;

//		FlipCardTask flipTask = new FlipCardTask(uICanvas.GetChild(combatHand.IndexOf(ChosenCard)).GetComponent<RectTransform>(), FlipCardTask.UpOrDown.Down);
//		PutDownCardTask putDownTask = new PutDownCardTask(uICanvas.GetChild(combatHand.IndexOf(ChosenCard)).GetComponent<RectTransform>());
//		flipTask.Then(putDownTask);
		defenderCards.FlipCardDown(combatHand.IndexOf(ChosenCard));

		ChosenCard = null;

		if (!StillAvailableCards()){
			ResetCombatHand();
//			ResetHandTask resetTask = new ResetHandTask(this);
//			putDownTask.Then(resetTask);
//			resetTask.Then(new ShutOffCardsTask());

		} else defenderCards.ShutCardsOff();
	}


	/// <summary>
	/// Are there any available cards in this defender's combat hand?
	/// </summary>
	/// <returns><c>true</c> if so, <c>false</c> if not.</returns>
	protected bool StillAvailableCards(){
		bool temp = false;

		foreach (Card card in combatHand){
			if (card.Available){
				temp = true;
				break;
			}
		}

		return temp;
	}


	/// <summary>
	/// Resets a defender's combat hand, making the cards available for use and providing relevant feedback.
	/// </summary>
	protected void ResetCombatHand(){
		foreach (Card card in combatHand){
			card.Available = true;
		}
		defenderCards.ClearAllSelectedColor();
		defenderCards.FlipAllCardsUp();
	}


	/// <summary>
	/// When this defender is done fighting, this carries out all associated effects.
	/// </summary>
	public virtual void DoneFighting(){
		BeUnselected();

		Services.UI.ShutOffCharSheet();

		Services.Defenders.DeclareSelfDone(this);

		//if the cards are on the table at this point, shut them off
		//the most common reason for that is if the player selects a defender, thinks about what to do, and decides to do nothing
		if (!Services.Tasks.CheckForTaskOfType<PickUpCardTask>() &&
			!Services.Tasks.CheckForTaskOfType<FlipCardTask>() &&
			!Services.Tasks.CheckForTaskOfType<PutDownCardTask>()) Services.Tasks.AddTask(new ShutOffCardsTask());
	}


	#endregion combat


	/// <summary>
	/// Each defender calls their own TakeOverCharSheet, which gives them the chance to substitute in their own text for the upgrade paths.
	/// </summary>
	public virtual void TakeOverCharSheet(){
		Services.UI.TakeOverCharSheet(name, Speed, AttackMod, Armor, defeatsToNextUpgrade, DefeatedSoFar);
	}


	/// <summary>
	/// Each defender is responsible for figuring out how they power up. This base function only handles making sure the defender
	/// meets the requirements to upgrade.
	/// 
	/// The pattern for upgrades is: the first one is free, the second costs 3, and each one thereafter costs one more than the one before it.
	/// </summary>
	/// <param>The upgrade tree the player wants to move along.</param>
	public virtual bool PowerUp(int tree){
		if (DefeatedSoFar < defeatsToNextUpgrade) return false; //make sure the defender has defeated enough attackers
		if (Services.UI.GetCharSheetStatus() != CharacterSheetBehavior.SheetStatus.Displayed) return false; //avoid misclicks on a moving character sheet

		defeatsToNextUpgrade += upgradeInterval;
		upgradeInterval++;
		Services.UI.ReviseNextLabel(defeatsToNextUpgrade, DefeatedSoFar);

		return true;
	}
}
