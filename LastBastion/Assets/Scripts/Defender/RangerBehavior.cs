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
	private Transform ammoOrganizer;
	private const string ATTACK_REMAINING_ORGANIZER = "Available attack icons";


	//the Lay Traps upgrade track
	public enum TrapTrack { None, Rockfall, Landslide, On_the_Lookout, The_Last_Chance };
	private List<string> trapDescriptions = new List<string>() {
		"<b>Lay your traps</b>",
		"<b>Rockfall</b>\n\n<size=11>When you choose this, select an empty space adjacent to you. The Horde must go around that space.\n\nGain 1 experience whenever your rockfall blocks the Horde.</size>",
		"<b>Landslide</b>\n\n<size=11>When you choose this, defeat every Horde member adjacent to your rockfall.\n\nGain 1 experience whenever your rockfall blocks the Horde.</size>",
		"<b>None shall pass!</b>"
	};
	private TrapTrack currentTrap;
	public TwoDLoc RockfallLoc { get; set; }
	private int landslideDamage = 999; //enough to defeat any attacker


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
		ammoOrganizer = transform.Find(PRIVATE_UI_CANVAS).Find(ATTACK_REMAINING_ORGANIZER);

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

		if (currentShowboat > ShowboatTrack.None) DisplayAvailableAttacks();
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


	private void DisplayAvailableAttacks(){
		if (currentShowboat == ShowboatTrack.None) return; //don't show any extra attack UI if the Ranger doesn't have extra attacks

		foreach (Transform child in ammoOrganizer){
			child.gameObject.SetActive(false);
		}

		for (int i = 0; i < currentAttacks; i++){
			ammoOrganizer.GetChild(i).gameObject.SetActive(true);
		}
	}


	/// <summary>
	/// The Ranger needs to show how many attacks they get when selected to fight while showboating.
	/// </summary>
	public override void BeSelectedForFight(){
		if (currentShowboat > ShowboatTrack.None && currentAttacks <= 0) return; //don't allow the Ranger to be selected when out of attacks
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


		Services.UI.ReviseCardsAvail(GetAvailableValues());
	}


	public override void WinFight(AttackerSandbox attacker){
		DefeatedSoFar++;
		powerupReadyParticle.SetActive(CheckUpgradeStatus());
		Services.UI.ReviseNextLabel(defeatsToNextUpgrade, DefeatedSoFar);

		//when the Ranger fights, they use up an attack. If they defeat the attacker, they get an extra attack for next turn.
		if (currentShowboat > ShowboatTrack.None){
			currentAttacks--;
			extraAttacks++;
			Services.UI.OpponentStatement(ReviseAttackText());
			DisplayAvailableAttacks();
		}

		FinishWithCard();
		if (currentAttacks <= 0 && currentShowboat > ShowboatTrack.None) DoneFighting();
	}


	public override void TieFight(AttackerSandbox attacker){
		Services.Events.Fire(new MissedFightEvent());

		if (currentShowboat > ShowboatTrack.None){
			currentAttacks--;
			Services.UI.OpponentStatement(ReviseAttackText());
			DisplayAvailableAttacks();
		}

		FinishWithCard();
		if (currentAttacks <= 0 && currentShowboat > ShowboatTrack.None) DoneFighting();
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

		int temp = 0;

		switch(currentShowboat){
			case ShowboatTrack.Effortless:
				temp = attacker.AttackMod - 1;
				break;
			case ShowboatTrack.Pull_Ahead:
			case ShowboatTrack.Set_the_Standard:
				temp = 0;
				break;
			default:
				temp = attacker.AttackMod;
				break;
		}

		return temp;
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


	public override void DoneFighting(){
		currentAttacks = 0;
		DisplayAvailableAttacks();

		base.DoneFighting();
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
						
						Services.Tasks.InsertTask(Services.Tasks.GetCurrentTaskOfType<UpgradeTask>(), new RockfallTask(this));
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
