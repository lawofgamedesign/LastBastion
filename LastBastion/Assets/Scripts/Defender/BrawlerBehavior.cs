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
		"<b>Rampage</b>\n\nYou can make an extra attack to the left or right each Defenders Fight phase.",
		"<b>Wade In</b>\n\nYou can make an extra attack to the left or right each Defenders Fight phase.\n\nAfter defeating a Horde member, you may move into the space they occupied.",
		"<b>Berserk</b>\n\nYou can make two extra attacks to the left or right each Defenders Fight phase.\n\nAfter defeating a Horde member, you may move into the space they occupied.",
		"<b>The Last One Standing</b>\n\nYou can make any number of extra attacks to the left or right each Defenders Fight phase.\n\nAfter defeating a Horde member, you may move into the space they occupied.\"",
		"<b>Maximum rampage!</b>"
	};
	private RampageTrack currentRampage;
	private enum Directions { North, South, West, East, Error };
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
	}


	public override void TryFight(AttackerSandbox attacker){
		Directions dir = GetAttackerDir(attacker);

		if (dir == Directions.Error) return; //if the player clicked on an enemy they can't possibly fight, do nothing

		bool canAttackThisDir = true;
		int attackerValue = 0;

		switch (currentRampage){
			case RampageTrack.None:
				base.TryFight(attacker);
				break;
			case RampageTrack.Rampage:
				canAttackThisDir = UseUpAttacks(dir);


				//There are several cases in Rampage mode:

				//the player has already used up the attack in this direction
				if (!canAttackThisDir){
					return;
				}

				//if the Brawler gets this far, a fight will actually occur; get a combat card for the attacker
				attackerValue = Services.AttackDeck.GetAttackerCard().Value;
				extraText.text = DisplayCombatMath(attacker, attackerValue);

				//the player can attack this direction, and they have a good enough value to hit the enemy
				if (ChosenCard.Value > attackerValue){
					attacker.TakeDamage(ChosenCard.Value - attackerValue);
					DefeatedSoFar++;
					Services.UI.ReviseNextLabel(defeatsToNextUpgrade, DefeatedSoFar);
					FinishWithCard();

				//the player can attack this direction, but they don't have a good enough value to hit the enemy
				} else if (ChosenCard.Value <= attackerValue){
					attacker.FailToDamage();
					FinishWithCard();
				}


				//if the player has used up all available attacks, they're done
				if (CheckUsedAllLimitedAttacks()) DoneFighting();

				break;
			case RampageTrack.Wade_In:
				canAttackThisDir = UseUpAttacks(dir);


				//There are several cases in Wade In mode:

				//the player has already used up the attack in this direction
				if (!canAttackThisDir){
					return;
				}
				
				//if the Brawler gets this far, a fight will actually occur; get a combat card for the attacker
				attackerValue = Services.AttackDeck.GetAttackerCard().Value;
				extraText.text = DisplayCombatMath(attacker, attackerValue);

				//the player can attack this direction, and they have a good enough value to hit the enemy
				if (canAttackThisDir &&
						 ChosenCard.Value > attackerValue){
					attacker.TakeDamage(ChosenCard.Value - attackerValue);
					lastDefeatedLoc = new TwoDLoc(attacker.XPos, attacker.ZPos);
					DefeatedSoFar++;
					Services.UI.ReviseNextLabel(defeatsToNextUpgrade, DefeatedSoFar);
					FinishWithCard();
				}


				//the player can attack this direction, but they don't have a good enough value to hit the enemy
				else if (canAttackThisDir &&
					ChosenCard.Value <= attackerValue){
					attacker.FailToDamage();
					FinishWithCard();
				}


				//if the Brawler has used all of their attacks, and has made all possible jumps, the Brawler is finished
				if (CheckUsedAllLimitedAttacks() && jumpsSoFar >= AVAILABLE_JUMPS_WADE_IN) DoneFighting();


				break;
			case RampageTrack.Berserk:
				canAttackThisDir = UseUpAttacks(dir);


				//There are several cases in Berserk mode:

				//the player has already used up all available attacks in this direction
				if (!canAttackThisDir){
					return;
				}

				//if the Brawler gets this far, a fight will actually occur; get a combat card for the attacker
				attackerValue = Services.AttackDeck.GetAttackerCard().Value;
				extraText.text = DisplayCombatMath(attacker, attackerValue);

				//the player can attack this direction, and they have a good enough value to hit the enemy
				if (canAttackThisDir &&
					ChosenCard.Value > attackerValue){
					attacker.TakeDamage(ChosenCard.Value - attackerValue);
					lastDefeatedLoc = new TwoDLoc(attacker.XPos, attacker.ZPos);
					DefeatedSoFar++;
					Services.UI.ReviseNextLabel(defeatsToNextUpgrade, DefeatedSoFar);
					FinishWithCard();
				}


				//the player can attack this direction, but they don't have a good enough value to hit the enemy
				else if (canAttackThisDir &&
					ChosenCard.Value <= attackerValue){
					attacker.FailToDamage();
					FinishWithCard();
				}


				//if the Brawler has used all of their attacks, and has made all possible jumps, the Brawler is finished
				if (CheckUsedAllLimitedAttacks() && jumpsSoFar >= AVAILABLE_JUMPS_BERSERK) DoneFighting();

				break;
			case RampageTrack.The_Last_One_Standing:
				canAttackThisDir = UseUpAttacks(dir);

				//There are several cases in The Last One Standing mode:

				//the player has already used up all available attacks in this direction
				if (!canAttackThisDir){
					return;
				}

				//if the Brawler gets this far, a fight will actually occur; get a combat card for the attacker
				attackerValue = Services.AttackDeck.GetAttackerCard().Value;
				extraText.text = DisplayCombatMath(attacker, attackerValue);

				//the player can attack this direction, and they have a good enough value to hit the enemy
				if (canAttackThisDir &&
					ChosenCard.Value > attackerValue){
					attacker.TakeDamage(ChosenCard.Value - attackerValue);
					lastDefeatedLoc = new TwoDLoc(attacker.XPos, attacker.ZPos);
					DefeatedSoFar++;
					Services.UI.ReviseNextLabel(defeatsToNextUpgrade, DefeatedSoFar);
					FinishWithCard();
				}


				//the player can attack this direction, but they don't have a good enough value to hit the enemy
				else if (canAttackThisDir &&
					ChosenCard.Value <= attackerValue){
					attacker.FailToDamage();
					FinishWithCard();
				}


				//in The Last One Standing we don't check for whether the Brawler is done; the Brawler has lots and lots of attacks!

				break;
		}
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
			else if (GetNumLateralAttacks() >= 2) temp = false;
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

		//needed by the compiler; the code should never get here
		Debug.Log("Checked for use of limited attacks in an improper Rampage track state");
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
	protected virtual void FinishWithCard(){
		ChosenCard.Available = false;

		FlipCardTask flipTask = new FlipCardTask(uICanvas.GetChild(combatHand.IndexOf(ChosenCard)).GetComponent<RectTransform>(), FlipCardTask.UpOrDown.Down);
		PutDownCardTask putDownTask = new PutDownCardTask(uICanvas.GetChild(combatHand.IndexOf(ChosenCard)).GetComponent<RectTransform>());
		flipTask.Then(putDownTask);

		ChosenCard = null;

		if (!StillAvailableCards()){
			ResetCombatHand();
			ResetHandTask resetTask = new ResetHandTask(this);
			putDownTask.Then(resetTask);

			if (currentRampage == RampageTrack.None) resetTask.Then(new ShutOffCardsTask());
		} else if (currentRampage == RampageTrack.None) putDownTask.Then(new ShutOffCardsTask());
		else if (CheckUsedAllLimitedAttacks()) putDownTask.Then(new ShutOffCardsTask());

		Services.Tasks.AddTask(flipTask);
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

		return true;
	}
}
