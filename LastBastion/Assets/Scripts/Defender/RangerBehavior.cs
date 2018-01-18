using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RangerBehavior : DefenderSandbox {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//ranger's stats
	private int rangerSpeed = 4;
	private int rangerAttack = 0;
	private int rangerArmor = 0;


	//character sheet
	private const string RANGER_NAME = "Ranger";
	private enum UpgradeTracks { Showboat, Trap };


	//the Showboat upgrade track
	private enum ShowboatTrack { None, Showboat, Effortless, Pull_Ahead, Set_the_Standard };
	private List<string> showboatDescriptions = new List<string>() {
		"<b>Start showboating</b>",
		"<size=14><b>Showboat</b></size><size=11>\n\nYou gain extra attacks equal to the number of Horde members you defeated last turn.\n\nYou may attack in any direction.</size>",
		"<size=14><b>Effortless</b></size><size=11>\n\nYou gain extra attacks equal to the number of Horde members you defeated last turn.\n\nYou may attack in any direction.\n\nIf you are behind the target, reduce their attack by 1.</size>",
		"<size=14><b>Pull Ahead</b></size><size=11>\n\nYou gain extra attacks equal to the number of Horde members you defeated last turn.\n\nYou may attack in any direction.\n\nIf you are behind the target, ignore their attack modifier.</size>",
		"<size=14><b>Set the Standard</b></size><size=11>\n\nYou gain extra attacks equal to the number of Horde members you defeated last turn.\n\nYou may attack in any direction.\n\nIf you are behind the target, ignore their attack modifier and armor.</size>",
		"<b>Maximum showboating!</b>"
	};
	private ShowboatTrack currentShowboat;
	private int currentAttacks = 0;
	private int extraAttacks = 0;
	private const int BASE_ATTACKS = 1;

	//UI for the Showboat track
	private const string ATTACKS_REMAINING = " attacks left";
	private const string RANGER_GETS = "Ranger gets ";
	private const string NEXT_ATTACKS = " extra attacks next turn";


	//the Lay Traps upgrade track
	public enum TrapTrack { None, Rockfall, Landslide, On_the_Lookout, The_Last_Chance };
	private List<string> trapDescriptions = new List<string>() {
		"<b>Lay your traps</b>",
		"<b>Rockfall</b>\n\n<size=11>When you choose this, select an empty space adjacent to you. The Horde must go around that space.\n\nGain 1 experience whenever your rockfall blocks the Horde.</size>",
		"<b>Landslide</b>\n\n<size=11>When you choose this, defeat every Horde member adjacent to your rockfall.\n\nGain 1 experience whenever your rockfall blocks the Horde.</size>",
		"<b>None shall pass!</b>"
	};
	private TrapTrack currentTrap;
	private const string BOARD_TAG = "Board";
	private const string BLOCK_MARKER_OBJ = "Space blocked marker";
	public TwoDLoc RockfallLoc { get; set; }
	private int landslideDamage = 999; //enough to defeat any attacker


	//UI for the Lay Traps track
	private const string ROCK_MSG = "Choose an adjacent, empty space to block.";
	private const string BLOCKED_MSG = "Space blocked!";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//do usual setup tasks; also initialize stats with the Ranger's stats, and initialize the upgrade tracks.
	public override void Setup(){
		base.Setup();

		Speed = rangerSpeed;
		AttackMod = rangerAttack;
		Armor = rangerArmor;

		currentShowboat = ShowboatTrack.None;
		currentTrap = TrapTrack.None;
	}


	/// <summary>
	/// Make a combat hand for this defender.
	/// </summary>
	/// <returns>A list of cards in the hand.</returns>
	protected override List<Card> MakeCombatHand(){
		return new List<Card>() { new Card(3), new Card(5), new Card(6) };
	}


	#region combat


	/// <summary>
	/// When a Ranger who has moved up the Showboat track gets ready to fight, they gain extra attacks.
	/// </summary>
	public override void PrepareToFight(){
		base.PrepareToFight();

		ResetCurrentAttacks();
	}


	/// <summary>
	/// Determine how many attacks the Ranger has right now. Reset the extra attacks.
	/// </summary>
	private void ResetCurrentAttacks(){
		if (currentShowboat != ShowboatTrack.None){
			currentAttacks = BASE_ATTACKS + extraAttacks;
			extraAttacks = 0; //reset the Ranger's extra attacks; these must be built up again.
		}
	}


	/// <summary>
	/// The Ranger needs to show how many attacks they get when selected to fight while showboating.
	/// </summary>
	public override void BeSelectedForFight(){
		base.BeSelectedForFight();

		if (currentShowboat != ShowboatTrack.None) Services.UI.OpponentStatement(ReviseAttackText());
	}


	/// <summary>
	/// The Ranger has special combat benefits if moving up the showboat track.
	/// </summary>
	/// <param name="attacker">The attacker this defender is fighting.</param>
	public override void TryFight(AttackerSandbox attacker){
		if (currentShowboat == ShowboatTrack.None){ //don't do anything special if the Ranger isn't showboating
			base.TryFight(attacker);
			return;
		}

		if (!CheckInRange(attacker)) return; //don't fight if the attacker is out of range

		//if the Ranger gets this far, a fight will actually occur; get a combat card for the attacker
		int attackerValue = Services.AttackDeck.GetAttackerCard().Value;
		int damage = (ChosenCard.Value + AttackMod) -
			(attackerValue + DetermineAttackerModifier(attacker) +
				DetermineAttackerArmor(attacker));

		damage = damage < 0 ? 0 : damage; //don't let damage be negative, "healing" the attacker

		Services.Tasks.AddTask(new CombatExplanationTask(attacker,
														 this,
														 attackerValue,
														 ChosenCard.Value,
														 DetermineAttackerModifier(attacker),
														 AttackMod,
														 damage));

		Services.UI.ExplainCombat(ChosenCard.Value, this, attacker, attackerValue, damage);


		//the Ranger can keep fighting until they run out of attacks
		if (currentAttacks <= 0) DoneFighting();
		Services.UI.ReviseCardsAvail(GetAvailableValues());
	}


	public override void WinFight(AttackerSandbox attacker){
		DefeatedSoFar++;
		powerupReadyParticle.SetActive(CheckUpgradeStatus());
		Services.UI.ReviseNextLabel(defeatsToNextUpgrade, DefeatedSoFar);

		//when the Ranger fights, they use up an attack. If they defeat the attacker, they get an extra attack for next turn.
		currentAttacks--;
		extraAttacks++;
		Services.UI.OpponentStatement(ReviseAttackText());

		FinishWithCard();
	}


	public override void TieFight(AttackerSandbox attacker){
		Services.Events.Fire(new MissedFightEvent());
		FinishWithCard();
		currentAttacks--;
		Services.UI.OpponentStatement(ReviseAttackText());
	}


	public override void LoseFight(AttackerSandbox attacker){
		TieFight(attacker);
	}


	/// <summary>
	/// Determine whether the time-to-upgrade particle should be displayed.
	/// </summary>
	/// <returns><c>true</c> if the defender has defeated enough attackers and has room to upgrade, <c>false</c> otherwise.</returns>
	protected override bool CheckUpgradeStatus(){
		if (DefeatedSoFar >= defeatsToNextUpgrade &&
			currentShowboat != ShowboatTrack.Set_the_Standard){

			Services.UI.ObjectStatement(transform.position, gameObject.name + POWER_UP_MSG);
			return true;
		} else {
			return false;
		}
	}


	/// <summary>
	/// The Ranger's combat math changes when showboating.
	/// </summary>
	/// <returns>A string explaining the math behind each combat.</returns>
	/// <param name="attacker">The attacker this defender is fighting.</param>
	/// <param name="attackerValue">The value of the attacker's card.</param>
	protected override string DisplayCombatMath(AttackerSandbox attacker, int attackerValue){
		int damage = (ChosenCard.Value + AttackMod) -
					 (attackerValue + DetermineAttackerModifier(attacker) +
					  DetermineAttackerArmor(attacker));

		damage = damage < 0 ? 0 : damage; //don't let damage be negative, "healing" the attacker

		return DEFENDER_VALUE + ChosenCard.Value + PLUS + AttackMod + NEWLINE +
			   ATTACKER_VALUE + attackerValue + PLUS + DetermineAttackerModifier(attacker) + NEWLINE +
			   HITS + ((ChosenCard.Value + AttackMod) - (attackerValue + DetermineAttackerModifier(attacker))).ToString() + NEWLINE +
			   DAMAGE + damage.ToString();;
	}


	/// <summary>
	/// Rangers who are moving up the Showboat track can attack in any direction, including diagonals.
	/// </summary>
	/// <returns><c>true</c> if the attacker is orthogonally or diagonally adjacent, <c>false</c> otherwise.</returns>
	/// <param name="attacker">The attacker being fought.</param>
	private bool CheckInRange(AttackerSandbox attacker){
		if (Mathf.Abs(GridLoc.x - attacker.XPos) <= 1 &&
			Mathf.Abs(GridLoc.z - attacker.ZPos) <= 1) return true;

		return false;
	}


	//the attacker's attack modifier can be altered, or nullified, depending on the Ranger's showboating status and their relative position
	private int DetermineAttackerModifier(AttackerSandbox attacker){
		if (attacker.ZPos >= GridLoc.z) return attacker.AttackMod; //the Ranger has to be behind the attacker (greater Z position) to get a benefit

		switch(currentShowboat){
			case ShowboatTrack.Effortless:
				return attacker.AttackMod - 1;
				break;
			case ShowboatTrack.Pull_Ahead:
			case ShowboatTrack.Set_the_Standard:
				return 0;
				break;
			default:
				return attacker.AttackMod;
				break;
		}
	}


	/// <summary>
	/// If the Ranger is behind the attacker and at maximum showboating, the Ranger nullifies their armor.
	/// </summary>
	/// <returns>The attacker's armor value.</returns>
	/// <param name="attacker">The attacker the Ranger is fighting.</param>
	private int DetermineAttackerArmor(AttackerSandbox attacker){
		if (attacker.ZPos >= GridLoc.z) return attacker.Armor;

		else if (currentShowboat == ShowboatTrack.Set_the_Standard) return 0;
		else return attacker.Armor;
	}


	/// <summary>
	/// Change UI text to provide feedback on how many attacks the Ranger gets.
	/// </summary>
	private string ReviseAttackText(){
		return currentAttacks.ToString() + ATTACKS_REMAINING + NEWLINE +
			   RANGER_GETS + extraAttacks.ToString() + NEXT_ATTACKS;
	}


	/// <summary>
	/// Blank the UI element that provides feedback on the number of attacks remaining.
	/// </summary>
	/// <returns>An empty string.</returns>
	private string BlankAttackText(){
		return "";
	}


	/// <summary>
	/// The Ranger's shutting off cards automatically is more complicated than for a generic defender.
	/// 
	/// Shut off the cards automatically after:
	/// 
	/// 1. The Ranger uses a card, and is not showboating.
	/// 2. The ranger is showboating, and runs out of attacks.
	/// 3. The ranger has used their last card, and the cards have reset.
	/// </summary>
	protected override void FinishWithCard(){
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

			if (currentShowboat == ShowboatTrack.None || currentAttacks <= 0) defenderCards.ShutCardsOff();//resetTask.Then(new ShutOffCardsTask());
			else if (currentShowboat == ShowboatTrack.None) defenderCards.ShutCardsOff();//putDownTask.Then(new ShutOffCardsTask());
			else if (currentAttacks <= 0) defenderCards.ShutCardsOff();//putDownTask.Then(new ShutOffCardsTask());
		} else if (currentShowboat == ShowboatTrack.None || currentAttacks <= 0) defenderCards.ShutCardsOff();//putDownTask.Then(new ShutOffCardsTask());

		//Services.Tasks.AddTask(flipTask);
	}


	#endregion combat


	/// <summary>
	/// Use this defender's name when taking over the character sheet, and display its upgrade paths.
	/// </summary>
	public override void TakeOverCharSheet(){
		Services.UI.TakeOverCharSheet(RANGER_NAME,
									  Speed,
									  AttackMod,
									  Armor,
									  defeatsToNextUpgrade,
									  DefeatedSoFar,
									  showboatDescriptions[(int)currentShowboat + 1],
									  showboatDescriptions[(int)currentShowboat],
									  trapDescriptions[(int)currentTrap + 1],
									  trapDescriptions[(int)currentTrap],
									  GetAvailableValues()
		);
	}


	/// <summary>
	/// When the player clicks a button to power up, this function is called.
	/// </summary>
	/// <param>The upgrade tree the player wants to move along.</param>
	/// <param name="tree">The upgrade tree the player clicked. Left is 0, right is 1.</param>
	public override bool PowerUp(int tree){
		if (!base.PowerUp(tree)) return false; //has the Brawler defeated enough attackers to upgrade?

		switch (tree){
			case (int)UpgradeTracks.Showboat:
				if (currentShowboat != ShowboatTrack.Set_the_Standard){
					currentShowboat++;
					Services.UI.ReviseTrack1(showboatDescriptions[(int)currentShowboat + 1], showboatDescriptions[(int)currentShowboat]);

					//just started showboating; need to make sure the UI is correct, and displayed if it's the appropriate phase
					if (currentShowboat == ShowboatTrack.Showboat){
						ResetCurrentAttacks();
						if (Services.Rulebook.TurnMachine.CurrentState.GetType() == typeof(TurnManager.PlayerFight)){
							Services.UI.OpponentStatement(ReviseAttackText());
						}
					}
				}

				break;
		case (int)UpgradeTracks.Trap:
			if (currentTrap != TrapTrack.Landslide){
				currentTrap++;

				//the Trap Track has a series of idiosyncratic abilities
				switch (currentTrap){
					case TrapTrack.Rockfall:
						//they gain the ability to gain XP when their blocks prevent movement
						Services.Events.Register<BlockedEvent>(GetXPFromBlock);

						//register for input and provide appropriate feedback
						Services.Events.Register<InputEvent>(PutDownBlock);
						Services.UI.OpponentStatement(ROCK_MSG);
						Services.Board.HighlightAllAroundSpace(GridLoc.x, GridLoc.z, BoardBehavior.OnOrOff.On, true);
						break;
					case TrapTrack.Landslide:
						DamageAtLoc(RockfallLoc.x, RockfallLoc.z + 1);
						DamageAtLoc(RockfallLoc.x, RockfallLoc.z - 1);
						DamageAtLoc(RockfallLoc.x - 1, RockfallLoc.z);
						DamageAtLoc(RockfallLoc.x + 1, RockfallLoc.z);
						break;
				}

				Services.UI.ReviseTrack2(trapDescriptions[(int)currentTrap + 1], trapDescriptions[(int)currentTrap]);
			}
			break;
		}

		//having powered up, shut off the particle telling the player to power up
		powerupReadyParticle.SetActive(false);

		return true;
	}


	public TrapTrack GetCurrentTrapTrack(){
		return currentTrap;
	}


	private void PutDownBlock(Event e){
		Debug.Assert(e.GetType() == typeof(InputEvent), "Non-InputEvent in PutDownBlock");

		//don't put down a block while the player is trying to hide the character sheet
		if (Services.UI.GetCharSheetStatus() == CharacterSheetBehavior.SheetStatus.Displayed) return;

		InputEvent inputEvent = e as InputEvent;

		if (inputEvent.selected.tag == BOARD_TAG){
			SpaceBehavior space = inputEvent.selected.GetComponent<SpaceBehavior>();


			//is this a block that can destroy an attacker?
			bool destroyingBlock = false;

			//try to put down the block
			if (CheckBlockable(space.GridLocation.x, space.GridLocation.z, destroyingBlock)){
				space.Block = true;
				Services.Tasks.AddTask(new BlockSpaceFeedbackTask(space.GridLocation.x, space.GridLocation.z, BLOCK_MARKER_OBJ));
				Services.UI.OpponentStatement(BLOCKED_MSG);

				if (currentTrap == TrapTrack.Rockfall) RockfallLoc = new TwoDLoc(space.GridLocation.x, space.GridLocation.z);

				Services.Events.Unregister<InputEvent>(PutDownBlock);
				Services.Board.HighlightAllAroundSpace(GridLoc.x, GridLoc.z, BoardBehavior.OnOrOff.Off, true);
			}
		}
	}


	private bool CheckBlockable(int x, int z, bool destroy){

		//is the space adjacent?
		if (!(Mathf.Abs(x - GridLoc.x) <= 1) ||
			!(Mathf.Abs(z - GridLoc.z) <= 1)) return false;

		//if this block can't destroy an attacker, return false if the space isn't empty
		if (!destroy){
			if (Services.Board.GeneralSpaceQuery(x, z) != SpaceBehavior.ContentType.None) return false;
		} 
		//if this block can destroy an attacker, return true if the space is empty or has an attacker in it
		else {
			if (Services.Board.GeneralSpaceQuery(x, z) == SpaceBehavior.ContentType.None ||
				Services.Board.GeneralSpaceQuery(x, z) == SpaceBehavior.ContentType.Attacker) return true;
		}

		//if the inquiry gets this far, the space can be blocked
		return true;
	}


	private void GetXPFromBlock(Event e){
		Debug.Assert(e.GetType() == typeof(BlockedEvent), "Non-BlockedEvent in GetXPFromBlock");

		DefeatedSoFar++;
		xpParticle.Play();
	}


	/// <summary>
	/// Inflicts damage on any attacker found at a given grid location, regardless of its armor, etc.
	/// </summary>
	/// <param name="x">The grid x coordinate.</param>
	/// <param name="z">The grid z coordinate.</param>
	private void DamageAtLoc(int x, int z){
		if (Services.Board.CheckValidSpace(x, z)){
			if (Services.Board.GeneralSpaceQuery(x, z) == SpaceBehavior.ContentType.Attacker){
				AttackerSandbox attacker = Services.Board.GetThingInSpace(x, z).GetComponent<AttackerSandbox>();
				if (attacker.Health <= landslideDamage) DefeatedSoFar++;
				attacker.TakeDamage(landslideDamage);
			}
		}
	}
}
