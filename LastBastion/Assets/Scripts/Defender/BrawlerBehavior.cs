using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrawlerBehavior : DefenderSandbox {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//brawler's stats
	private int brawlerSpeed = 4;
	private int brawlerAttackMod = 0;
	private int brawlerArmor = 1;


	//upgrade tracks
	private enum UpgradeTracks { Rampage, Boasting };


	//rampage track
	private enum RampageTrack { None, Rampage, Wade_In, Berserk, The_Last_One_Standing };
	private List<string> rampageDescriptions = new List<string>() {
		"<b>Start rampaging</b>",
		"<b>Rampage</b>\n\nYou get two attacks: one north and one east or west.",
		"<b>Wade In</b>\n\n<size=10>You get two attacks: one north and one east or west.\n\nAfter defeating a Horde member, you may move into the space they occupied.</size>",
		"<b>Berserk</b>\n\n<size=10>You get three attacks: one north and two east or west.\n\nAfter defeating a Horde member, you may move into the space they occupied.</size>",
		"<b>The Last One Standing</b>\n\n<size=10>You get infinite attacks east and west, so long as each one defeats a Horde member. You get one attack north.\n\nAfter defeating a Horde member, you may move into the space they occupied.</size>",
		"<b>Maximum rampage!</b>"
	};
	private RampageTrack currentRampage;
	private enum Directions { North, South, West, East, Error };
	private bool defeatedLastTarget = false;
	private List<Directions> attacksSoFar = new List<Directions>();
	private BoardClickedEvent.Handler boardFunc;
	private TwoDLoc lastDefeatedLoc = null;
	private const int AVAILABLE_JUMPS_WADE_IN = 2;
	private const int AVAILABLE_JUMPS_BERSERK = 3;
	private const int AVAILABLE_JUMPS_LAST_STANDING = 9999; //effectively unlimited
	private int availableJumps;
	private int jumpsSoFar = 0;


	//character sheet information
	private const string BRAWLER_NAME = "Brawler";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	/// <summary>
	/// Perform normal setup for a defender, but assign the brawler's stats.
	/// </summary>
	public override void Setup(){
		base.Setup();

		Speed = brawlerSpeed;
		AttackMod = brawlerAttackMod;
		Armor = brawlerArmor;



		currentRampage = RampageTrack.None;
	}


	/// <summary>
	/// Make a combat hand for this defender.
	/// </summary>
	/// <returns>A list of cards in the hand.</returns>
	protected override List<Card> MakeCombatHand(){
		return new List<Card>() { new Card(4), new Card(6), new Card(7) };
	}


	/// <summary>
	/// The brawler starts each fight phase without having used any attacks or defeated anyone.
	/// 
	/// If the Brawler is a significant distance down the Rampage track, they also get ready to move into the spaces of enemies they defeat.
	/// </summary>
	public override void PrepareToFight(){
		attacksSoFar.Clear();
		lastDefeatedLoc = null;

		if (currentRampage == RampageTrack.Wade_In || currentRampage == RampageTrack.Berserk || currentRampage == RampageTrack.The_Last_One_Standing){
			boardFunc = JumpToSpace;
			Services.Events.Register<BoardClickedEvent>(boardFunc);
			jumpsSoFar = 0;
		}

		if (currentRampage == RampageTrack.Wade_In) availableJumps = AVAILABLE_JUMPS_WADE_IN;
		else if (currentRampage == RampageTrack.Berserk) availableJumps = AVAILABLE_JUMPS_BERSERK;
		else if (currentRampage == RampageTrack.The_Last_One_Standing) availableJumps = AVAILABLE_JUMPS_LAST_STANDING;
		defeatedLastTarget = true; //important for The Last One Standing
	}


	public override void TryFight(AttackerSandbox attacker){
		Directions dir = GetAttackerDir(attacker);

		if (dir == Directions.Error) return; //if the player clicked on an enemy they can't possibly fight, do nothing

		if (currentRampage == RampageTrack.None){
			base.TryFight(attacker);
			return;
		}

		if (!UseUpAttacks(dir)) return;

		if (currentRampage == RampageTrack.The_Last_One_Standing &&
			!defeatedLastTarget) return; //the Brawler stops in The LastOneStanding mode after missing a hit

		//if the Brawler gets this far, a fight will actually occur; get a card for the Attacker
		int attackerValue = Services.AttackDeck.GetAttackerCard().Value;
		extraText.text = DisplayCombatMath(attacker, attackerValue);

		if (ChosenCard.Value + AttackMod > attackerValue + attacker.AttackMod){ //successful attack
			int damage = (ChosenCard.Value + AttackMod) - (attackerValue + attacker.AttackMod + attacker.Armor);

			damage = damage < 0 ? 0 : damage; //don't allow damage to be negative, "healing" the attacker

			if (damage >= attacker.Health){
				DefeatedSoFar++;
				powerupReadyParticle.SetActive(CheckUpgradeStatus());
				Services.UI.ReviseNextLabel(defeatsToNextUpgrade, DefeatedSoFar);
				if (currentRampage == RampageTrack.Wade_In ||
					currentRampage == RampageTrack.Berserk ||
					currentRampage == RampageTrack.The_Last_One_Standing) lastDefeatedLoc = new TwoDLoc(attacker.XPos, attacker.ZPos);
				defeatedLastTarget = true; //only important for The Last One Standing
			}

			attacker.TakeDamage(damage);

			FinishWithCard();
		} else { //the Brawler's value was too low
			if (dir == Directions.East ||
				dir == Directions.West) defeatedLastTarget = false; //only important for The Last One Standing
			attacker.FailToDamage();
			FinishWithCard();
		}
	}


	/// <summary>
	/// Determine whether the time-to-upgrade particle should be displayed.
	/// </summary>
	/// <returns><c>true</c> if the defender has defeated enough attackers and has room to upgrade, <c>false</c> otherwise.</returns>
	protected override bool CheckUpgradeStatus(){
		if (DefeatedSoFar >= defeatsToNextUpgrade &&
			currentRampage != RampageTrack.The_Last_One_Standing) return true;

		return false;
	}



	/// <summary>
	/// Finds the direction of an attacker, relative to this defender.
	/// </summary>
	/// <returns>The attacker's direction.</returns>
	/// <param name="attacker">The attacker whose location is being checked.</param>
	private Directions GetAttackerDir(AttackerSandbox attacker){
		if (attacker.XPos == GridLoc.x && attacker.ZPos == GridLoc.z + 1) return Directions.North;
		else if (attacker.XPos + 1 == GridLoc.x && attacker.ZPos == GridLoc.z) return Directions.West;
		else if (attacker.XPos - 1 == GridLoc.x && attacker.ZPos == GridLoc.z) return Directions.East;
		else return Directions.Error;
	}


	/// <summary>
	/// When the Brawler is in Rampage mode, they have more--but still a limited number of--attacks.
	/// 
	/// This function tracks which attacks have been used, and reports whether the intended attack is validly trying
	/// to employ an unused attack.
	/// </summary>
	/// <returns><c>true</c> if the Brawler can attack in this direction, <c>false</c> if they have no more attacks in that direction.</returns>
	/// <param name="direction">The direction of the attack.</param>
	private bool UseUpAttacks(Directions direction){
		bool temp = true;

		//at the early Rampage levels, the player only gets one attack in each direction
		if (currentRampage == RampageTrack.Rampage || currentRampage == RampageTrack.Wade_In){
			if (attacksSoFar.Contains(direction)) temp = false;
		}

		//at the Berserk level things are more complex; the Brawler can make up to two lateral attacks, but they can be the same
		else if (currentRampage == RampageTrack.Berserk){
			if (direction == Directions.North && attacksSoFar.Contains(direction)) temp = false;
			else if (direction == Directions.East && GetNumLateralAttacks() >= 2 ||
					 direction == Directions.West && GetNumLateralAttacks() >= 2) temp = false;
		}


		//at The Last One Standing, things get simple again. Attacks to the north are limited, lateral attacks are limitless
		else if (currentRampage == RampageTrack.The_Last_One_Standing) if (direction == Directions.North && attacksSoFar.Contains(direction)) temp = false;


		attacksSoFar.Add(direction);


		//at the early Rampage levels, the player only gets one sideways attack, so any sideways attack uses up both
		if (currentRampage == RampageTrack.Rampage || currentRampage == RampageTrack.Wade_In){
			if (direction == Directions.East) attacksSoFar.Add(Directions.West);
			if (direction == Directions.West) attacksSoFar.Add(Directions.East);
		}

		return temp;
	}


	/// <summary>
	/// Has the Brawler used up all of their attacks while attacks are limited?
	/// </summary>
	/// <returns><c>true</c> if so, <c>false</c> otherwise.</returns>
	private bool CheckUsedAllLimitedAttacks(){
		if (currentRampage == RampageTrack.Rampage || currentRampage == RampageTrack.Wade_In){
			if (attacksSoFar.Contains(Directions.North) &&
				attacksSoFar.Contains(Directions.West) &&
				attacksSoFar.Contains(Directions.East)) return true;
			else return false;
		} else if (currentRampage == RampageTrack.Berserk){
			if (GetNumLateralAttacks() >= 2 && attacksSoFar.Contains(Directions.North)) return true;
			else return false;
		}

		//at The Last One Standing, the computer never decides all attacks are used up
		return false;
	}


	/// <summary>
	/// Call to report the total number of attacks the Brawler has made to the east and west this turn.
	/// </summary>
	/// <returns>The number of lateral attacks.</returns>
	private int GetNumLateralAttacks(){
		int temp = 0;

		foreach (Directions dir in attacksSoFar) if (dir == Directions.East || dir == Directions.West) temp++;

		return temp;
	}


	/// <summary>
	/// This function is enabled (as a delegate) when the Brawler is advancing down the Rampage track.
	/// 
	/// Each time the board is clicked, this checks whether the place clicked is the place where the Brawler last
	/// defeated an enemy. If so, it moves the Brawler there.
	/// </summary>
	private void JumpToSpace(Event e){
		if (jumpsSoFar >= availableJumps) return;

		BoardClickedEvent boardEvent = e as BoardClickedEvent;

		//if the clicked space wasn't the last place where the Brawler defeated an attacker, do nothing
		if (lastDefeatedLoc == null || boardEvent.coords.x != lastDefeatedLoc.x || boardEvent.coords.z != lastDefeatedLoc.z) return;

		//move on the screen
		Services.Tasks.AddTask(new MoveDefenderTask(rb, moveSpeed, new List<TwoDLoc>() { boardEvent.coords }));

		//move on the grid used for game logic
		Services.Board.TakeThingFromSpace(GridLoc.x, GridLoc.z);
		Services.Board.PutThingInSpace(gameObject, boardEvent.coords.x, boardEvent.coords.z, SpaceBehavior.ContentType.Defender);
		NewLoc(boardEvent.coords.x, boardEvent.coords.z);

		//record this jump
		jumpsSoFar++;
	}


	/// <summary>
	/// The Brawler doesn't shut off cards automatically when rampaging.
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
			if (currentRampage == RampageTrack.None) defenderCards.ShutCardsOff();//resetTask.Then(new ShutOffCardsTask());
		} else if (currentRampage == RampageTrack.None) defenderCards.ShutCardsOff();//putDownTask.Then(new ShutOffCardsTask());
		else if (CheckUsedAllLimitedAttacks()) defenderCards.ShutCardsOff();//putDownTask.Then(new ShutOffCardsTask());

		//Services.Tasks.AddTask(flipTask);
	}


	/// <summary>
	/// When the Brawler is done fighting, it must additionally stop listening for instructions to move into new spaces.
	/// </summary>
	public override void DoneFighting(){
		if (Services.Tasks.CheckForTaskOfType<PutDownCardTask>()) return; //the Brawler has to wait for cards to be down to stop fighting, for error prevention

		if (currentRampage == RampageTrack.Wade_In || currentRampage == RampageTrack.Berserk || currentRampage == RampageTrack.The_Last_One_Standing){
			Services.Events.Unregister<BoardClickedEvent>(boardFunc);
		}

		base.DoneFighting();
	}


	/// <summary>
	/// Use this defender's name when taking over the character sheet, and display its upgrade paths.
	/// </summary>
	public override void TakeOverCharSheet(){
		Services.UI.TakeOverCharSheet(BRAWLER_NAME,
									  Speed,
									  AttackMod,
									  Armor,
									  defeatsToNextUpgrade,
									  DefeatedSoFar,
									  rampageDescriptions[(int)currentRampage + 1],
									  rampageDescriptions[(int)currentRampage],
									  "",
									  "");
	}


	/// <summary>
	/// When the player clicks a button to power up, this function is called.
	/// </summary>
	/// <param>The upgrade tree the player wants to move along.</param>
	/// <param name="tree">The upgrade tree the player clicked. Left is 0, right is 1.</param>
	public override bool PowerUp(int tree){
		if (!base.PowerUp(tree)) return false; //has the Brawler defeated enough attackers to upgrade?

		switch (tree){
			case (int)UpgradeTracks.Rampage:
				if (currentRampage != RampageTrack.The_Last_One_Standing) currentRampage++;
				break;
		}

		Services.UI.ReviseTrack1(rampageDescriptions[(int)currentRampage + 1], rampageDescriptions[(int)currentRampage]);

		//shut off the particle telling the player to power up
		powerupReadyParticle.SetActive(false);

		return true;
	}
}
