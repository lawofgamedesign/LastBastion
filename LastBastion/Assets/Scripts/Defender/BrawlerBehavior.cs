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
	private int brawlerArmor = 0;


	//upgrade tracks
	private enum UpgradeTracks { Rampage, Drink };


	//rampage track
	private enum RampageTrack { None, Rampage, Wade_In, Berserk, The_Last_One_Standing };
	private List<string> rampageDescriptions = new List<string>() {
		"<b>Start rampaging</b>",
		"<b>Rampage</b>\n\nYou get two attacks: one north and one east or west.",
		"<b>Wade In</b>\n\n<size=10>You get two attacks: one north and one east or west.\n\nAfter defeating a Horde member, you may move into the space they occupied.</size>",
		"<b>Berserk</b>\n\n<size=10>You get three attacks: one north and two east or west.\n\nAfter defeating a Horde member, you may move into the space they occupied.</size>",
		"<b>The Last One Standing</b>\n\n<size=10>You get infinite attacks east and west, so long as each one inflicts damage. You get one attack north.\n\nAfter defeating a Horde member, you may move into the space they occupied.</size>",
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
	//UI for extra attacks
	private Transform dirFeedback;
	private const string DIR_FEEDBACK_ORGANIZER = "Available attacks";
	private const string SECOND_ATTACK = " 2";


	//drink track
	public enum DrinkTrack { None, Party_Foul, Liquid_Courage, Open_The_Tap, Buy_A_Round };
	private List<string> drinkDescriptions = new List<string>() {
		"<b>Start drinking</b>",
		"<b>Party Foul</b>\n\nPut a tankard on the field. When you stop on it, take a drink and then kick it.\n\nAny Horde member it hits takes 1 damage.",
		"<b>Liquid Courage</b>\n\nWhen you drink, gain 1 Inspiration and kick the tankard. Any Horde member it hits takes 1 damage.",
		"<b>Open the Tap</b>\n\nWhen you drink, gain 2 Inspiration and kick the tankard. Any Horde member it hits takes 2 damage.",
		"<b>Buy a Round</b>\n\nPut two more tankards on the field. All defenders can drink for 2 Inspiration. When you drink, kick the tankard. Any Horde member it hits takes 2 damage.",
		"<b>Here we go!</b>"
	};
	private DrinkTrack currentDrink;
	private const string BOARD_TAG = "Board";
	private const string KICK_DIRECTIONS = "Pick an adjacent space to kick the tankard to.";
	private const string KICK_MSG = "Gave it the boot!";
	private int drinkDamage = 0;
	private int drinkInspiration = 0;
	private const int START_DRINK_DAMAGE = 1;
	private const int BETTER_DRINK_DAMAGE = 2;
	private const int START_DRINK_INSP = 1;
	private const int BETTER_DRINK_INSP = 2;
	private int tankardsToDrop = 0;
	protected const string ATTACKER_TAG = "Attacker";
	protected const string LEADER_TAG = "Leader";
	protected const string MINION_TAG = "Minion";


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

		dirFeedback = transform.Find(PRIVATE_UI_CANVAS).Find(DIR_FEEDBACK_ORGANIZER);

		currentRampage = RampageTrack.None;
		currentDrink = DrinkTrack.None;
	}


	/// <summary>
	/// Make a combat hand for this defender.
	/// </summary>
	/// <returns>A list of cards in the hand.</returns>
	protected override List<Card> MakeCombatHand(){
		return new List<Card>() { new Card(4), new Card(6), new Card(7) };
	}


	public override void BeUnselected(){
		base.BeUnselected();

		dirFeedback.gameObject.SetActive(false);
	}


	/// <summary>
	/// This override protects against the situation where the player is in the middle of choosing where to kick a tankard
	/// when they undo the move phase, resulting in errors as the system tries to kick a non-existent tankard in the
	/// Brawler's original space. If the Brawler is in a space with a tankard, they are necessarily in the process of kicking
	/// it--the Brawler can never coexist with a tankard for any length of time. In that case, the undo doesn't work on the Brawler,
	/// preventing the error.
	/// </summary>
	public override void UndoMovePhase(){
		if (Services.Board.GetSpace(GridLoc.x, GridLoc.z).Tankard) return;

		base.UndoMovePhase();
	}


	#region combat


	/// <summary>
	/// The brawler starts each fight phase without having used any attacks or defeated anyone.
	/// 
	/// If the Brawler is a significant distance down the Rampage track, they also get ready to move into the spaces of enemies they defeat.
	/// </summary>
	public override void PrepareToFight(){
		base.PrepareToFight();

		attacksSoFar.Clear();
		lastDefeatedLoc = null;

		if ((int)currentRampage >= (int)RampageTrack.Wade_In){
			boardFunc = JumpToSpace;
			Services.Events.Register<BoardClickedEvent>(boardFunc);
			jumpsSoFar = 0;
		}

		if (currentRampage == RampageTrack.Wade_In) availableJumps = AVAILABLE_JUMPS_WADE_IN;
		else if (currentRampage == RampageTrack.Berserk) availableJumps = AVAILABLE_JUMPS_BERSERK;
		else if (currentRampage == RampageTrack.The_Last_One_Standing) availableJumps = AVAILABLE_JUMPS_LAST_STANDING;
		defeatedLastTarget = true; //important for The Last One Standing
	}


	public override void BeSelectedForFight(){
		base.BeSelectedForFight();

		if (currentRampage != RampageTrack.None){
			dirFeedback.gameObject.SetActive(true);
			SetAvailableFeedback();
		}
	}


	private void SetAvailableFeedback(){
		//in all cases, the Brawler gets one attack to the north
		if (!attacksSoFar.Contains(Directions.North)){
			dirFeedback.Find(Directions.North.ToString()).gameObject.SetActive(true);
		} else {
			dirFeedback.Find(Directions.North.ToString()).gameObject.SetActive(false);
		}

		//Rampage and Wade In give the Brawler one attack east or west
		if (currentRampage == RampageTrack.Rampage || currentRampage == RampageTrack.Wade_In){
			if (GetNumLateralAttacks() == 0){
				dirFeedback.Find(Directions.East.ToString()).gameObject.SetActive(true);
				dirFeedback.Find(Directions.West.ToString()).gameObject.SetActive(true);
			} else {
				dirFeedback.Find(Directions.East.ToString()).gameObject.SetActive(false);
				dirFeedback.Find(Directions.West.ToString()).gameObject.SetActive(false);
			}

		//Berserk gives the Brawler 2 total lateral attacks
		} else if (currentRampage == RampageTrack.Berserk){
			if (GetNumLateralAttacks() == 0){
				dirFeedback.Find(Directions.East.ToString()).gameObject.SetActive(true);
				dirFeedback.Find(Directions.East.ToString() + SECOND_ATTACK).gameObject.SetActive(true);
				dirFeedback.Find(Directions.West.ToString()).gameObject.SetActive(true);
				dirFeedback.Find(Directions.West.ToString() + SECOND_ATTACK).gameObject.SetActive(true);
			} else if (GetNumLateralAttacks() == 1){
				dirFeedback.Find(Directions.East.ToString()).gameObject.SetActive(true);
				dirFeedback.Find(Directions.East.ToString() + SECOND_ATTACK).gameObject.SetActive(false);
				dirFeedback.Find(Directions.West.ToString()).gameObject.SetActive(true);
				dirFeedback.Find(Directions.West.ToString() + SECOND_ATTACK).gameObject.SetActive(false);
			} else {
				dirFeedback.Find(Directions.East.ToString()).gameObject.SetActive(false);
				dirFeedback.Find(Directions.East.ToString() + SECOND_ATTACK).gameObject.SetActive(false);
				dirFeedback.Find(Directions.West.ToString()).gameObject.SetActive(false);
				dirFeedback.Find(Directions.West.ToString() + SECOND_ATTACK).gameObject.SetActive(false);
			}

		//The Last One Standing gives the Brawler infinite lateral attacks, so long as the Brawler keeps inflicting damage
		} else if (currentRampage == RampageTrack.The_Last_One_Standing){
			if (defeatedLastTarget){
				dirFeedback.Find(Directions.East.ToString()).gameObject.SetActive(true);
				dirFeedback.Find(Directions.East.ToString() + SECOND_ATTACK).gameObject.SetActive(true);
				dirFeedback.Find(Directions.West.ToString()).gameObject.SetActive(true);
				dirFeedback.Find(Directions.West.ToString() + SECOND_ATTACK).gameObject.SetActive(true);
			} else {
				dirFeedback.Find(Directions.East.ToString()).gameObject.SetActive(false);
				dirFeedback.Find(Directions.East.ToString() + SECOND_ATTACK).gameObject.SetActive(false);
				dirFeedback.Find(Directions.West.ToString()).gameObject.SetActive(false);
				dirFeedback.Find(Directions.West.ToString() + SECOND_ATTACK).gameObject.SetActive(false);
			}
		}
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
			dir != Directions.North &&
			!defeatedLastTarget) return; //the Brawler stops in The LastOneStanding mode after missing a hit

		//if the Brawler gets this far, a fight will actually occur; get a card for the Attacker
		int attackerValue = Services.AttackDeck.GetAttackerCard().Value;
		int damage = (ChosenCard.Value + AttackMod) - (attackerValue + attacker.AttackMod + attacker.Armor);

		damage = damage < 0 ? 0 : damage; //don't allow damage to be negative, "healing" the attacker

		Services.Tasks.AddTask(new CombatExplanationTask(attacker,
														 this,
														 attackerValue,
														 ChosenCard.Value,
														 attacker.AttackMod,
														 AttackMod,
														 damage));

		if ((ChosenCard.Value + AttackMod) > (attackerValue + attacker.AttackMod)){ //successful attack
			if (damage >= attacker.Health){
				
			}

			//inflicting damage is handled by the combat explanation task, if appropriate


		} else { //the Brawler's value was too low
			
		}

		if (currentRampage != RampageTrack.None) SetAvailableFeedback();
		Services.UI.ReviseCardsAvail(GetAvailableValues());
	}


	public override void WinFight(AttackerSandbox attacker){
		DefeatAttacker();
		Services.UI.ReviseNextLabel(defeatsToNextUpgrade, DefeatedSoFar);
		if (currentRampage == RampageTrack.Wade_In ||
			currentRampage == RampageTrack.Berserk ||
			currentRampage == RampageTrack.The_Last_One_Standing){

			if (lastDefeatedLoc != null) Services.Board.HighlightSpace(lastDefeatedLoc.x, lastDefeatedLoc.z, BoardBehavior.OnOrOff.Off);
			lastDefeatedLoc = new TwoDLoc(attacker.XPos, attacker.ZPos);
			Services.Board.HighlightSpace(attacker.XPos, attacker.ZPos, BoardBehavior.OnOrOff.On);
		}
		defeatedLastTarget = true; //only important for The Last One Standing

		FinishWithCard();
	}


	public override void TieFight(AttackerSandbox attacker){
		Directions dir = GetAttackerDir(attacker);

		if (dir == Directions.East ||
			dir == Directions.West) defeatedLastTarget = false; //only important for The Last One Standing
		Services.Events.Fire(new MissedFightEvent());
		FinishWithCard();
	}


	/// <summary>
	/// The Brawler does the same thing whether they lose or tie a fight.
	/// </summary>
	/// <param name="attacker">The attacker in the fight.</param>
	public override void LoseFight(AttackerSandbox attacker){
		TieFight(attacker);
	}


	/// <summary>
	/// Determine whether the time-to-upgrade particle should be displayed.
	/// </summary>
	/// <returns><c>true</c> if the defender has defeated enough attackers and has room to upgrade, <c>false</c> otherwise.</returns>
	protected override bool CheckUpgradeStatus(){
		if (DefeatedSoFar >= defeatsToNextUpgrade &&
			currentRampage != RampageTrack.The_Last_One_Standing){

			Services.UI.ObjectStatement(transform.position, gameObject.name + POWER_UP_MSG);
			return true;
		} else {
			return false;
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
		} else if (currentRampage == RampageTrack.The_Last_One_Standing){
			if (attacksSoFar.Contains(Directions.North) && !defeatedLastTarget) return true;
			else return false;
		} else {
			Debug.Log("Fell out of CheckUsedAllLimitedAttacks");
			return false;
		}
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
		Services.Board.HighlightSpace(lastDefeatedLoc.x, lastDefeatedLoc.z, BoardBehavior.OnOrOff.Off);
		lastDefeatedLoc = null;

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
		else if (CheckUsedAllLimitedAttacks()){
			defenderCards.ShutCardsOff();//putDownTask.Then(new ShutOffCardsTask());
			DoneFighting();
		}

		//Services.Tasks.AddTask(flipTask);
	}


	/// <summary>
	/// When the Brawler is done fighting, it must additionally stop listening for instructions to move into new spaces.
	/// </summary>
	public override void DoneFighting(){
		if (Services.Tasks.CheckForTaskOfType<PutDownCardTask>()) return; //the Brawler has to wait for cards to be down to stop fighting, for error prevention

		if (currentRampage == RampageTrack.Wade_In || currentRampage == RampageTrack.Berserk || currentRampage == RampageTrack.The_Last_One_Standing){
			Services.Events.Unregister<BoardClickedEvent>(boardFunc);

			//switch off any highlighting for where the Brawler can jump to
			if (lastDefeatedLoc != null){
				Services.Board.HighlightSpace(lastDefeatedLoc.x, lastDefeatedLoc.z, BoardBehavior.OnOrOff.Off);
			}
		}

		base.DoneFighting();
	}


	#endregion combat


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
									  drinkDescriptions[(int)currentDrink + 1],
									  drinkDescriptions[(int)currentDrink],
									  GetAvailableValues());
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
				if (currentRampage != RampageTrack.The_Last_One_Standing){
					currentRampage++;
					Services.Events.Fire(new UpgradeEvent(gameObject, currentRampage.ToString()));
				}
				break;
			case (int)UpgradeTracks.Drink:
				if (currentDrink != DrinkTrack.Buy_A_Round){
					currentDrink++;
					Services.Events.Fire(new UpgradeEvent(gameObject, currentDrink.ToString()));

					switch(currentDrink){
						case DrinkTrack.Party_Foul:
							foreach (TankardBehavior tankard in Services.Board.GetAllTankards()){
								tankard.GridLoc = new TwoDLoc(-999, -999); //nonsense value to indicate off the board
							}
							tankardsToDrop = 1;
							drinkDamage = START_DRINK_DAMAGE;
							Services.Tasks.InsertTask(Services.Tasks.GetCurrentTaskOfType<UpgradeTask>(), new TankardDropTask(this, tankardsToDrop));
							break;
						case DrinkTrack.Liquid_Courage:
							drinkInspiration = START_DRINK_INSP;
							break;
						case DrinkTrack.Open_The_Tap:
							drinkDamage = BETTER_DRINK_DAMAGE;
							drinkInspiration = BETTER_DRINK_INSP;
							break;
						case DrinkTrack.Buy_A_Round:
							Services.Events.Register<EndPhaseEvent>(GiveInspiration);
							tankardsToDrop = 2;
							Services.Tasks.InsertTask(Services.Tasks.GetCurrentTaskOfType<UpgradeTask>(), new TankardDropTask(this, tankardsToDrop));
							break;
					}
				}
				break;
		}

		Services.UI.ReviseTrack1(rampageDescriptions[(int)currentRampage + 1], rampageDescriptions[(int)currentRampage]);
		Services.UI.ReviseTrack2(drinkDescriptions[(int)currentDrink + 1], drinkDescriptions[(int)currentDrink]);

		//shut off the particle telling the player to power up
		powerupReadyParticle.SetActive(false);

		return true;
	}


	public DrinkTrack ReportCurrentDrink(){
		return currentDrink;
	}


	#region tankard


	/// <summary>
	/// This override handles the Tankard track, in which the Brawler kicks around a tankard and gains Inspiration for drinking at
	/// higher Tankard track levels.
	/// </summary>
	public override void Move(){
		base.Move();

		//try to kick the tankard
		if (Services.Board.CheckIfTankard(GridLoc.x, GridLoc.z)){
			Services.UI.OpponentStatement(KICK_DIRECTIONS);
			Services.Events.Register<InputEvent>(ChooseTankardKick);
			Services.Board.HighlightAllAroundSpace(GridLoc.x, GridLoc.z, BoardBehavior.OnOrOff.On);
			//gain inspiration for drinking, if at that point in the track
			if ((int)currentDrink > (int)DrinkTrack.Party_Foul){
				DefeatedSoFar += drinkInspiration;
				powerupReadyParticle.SetActive(CheckUpgradeStatus());
			}
		}
	}


	private void ChooseTankardKick(Event e){
		Debug.Assert(e.GetType() == typeof(InputEvent), "Non-InputEvent in ChooseTankardKick.");

		InputEvent inputEvent = e as InputEvent;

		if (inputEvent.selected.tag == BOARD_TAG){
			SpaceBehavior space = inputEvent.selected.GetComponent<SpaceBehavior>();

			if (CheckAllAdjacent(GridLoc, space.GridLocation)){
				KickTankard(space);	
			}
		} else if (inputEvent.selected.tag == ATTACKER_TAG || inputEvent.selected.tag == LEADER_TAG || inputEvent.selected.tag == MINION_TAG){
			AttackerSandbox attacker = inputEvent.selected.GetComponent<AttackerSandbox>();

			SpaceBehavior space = Services.Board.GetSpace(attacker.XPos, attacker.ZPos);

			if (CheckAllAdjacent(GridLoc, space.GridLocation)){
				KickTankard(space);
			}
		}
	}


	private void KickTankard(SpaceBehavior space){
		WaitToArriveTask waitTask = new WaitToArriveTask(transform, new TwoDLoc(GridLoc.x, GridLoc.z));

		Transform localTankard = Services.Board.GetTankardInSpace(GridLoc);

		Debug.Assert(localTankard != null, "Didn't find local tankard.");

		MoveObjectTask moveTask = new MoveObjectTask(localTankard,
			new TwoDLoc(GridLoc.x, GridLoc.z),
			new TwoDLoc(space.GridLocation.x, space.GridLocation.z));
		DamageRemotelyTask damageTask = new DamageRemotelyTask(new TwoDLoc(space.GridLocation.x, space.GridLocation.z),
			drinkDamage,
			this);
		waitTask.Then(moveTask);
		moveTask.Then(damageTask);
		space.Tankard = true;
		Services.Board.GetSpace(GridLoc.x, GridLoc.z).Tankard = false;
		localTankard.GetComponent<TankardBehavior>().GridLoc = new TwoDLoc(space.GridLocation.x, space.GridLocation.z);
		Services.Tasks.AddTask(waitTask);
		Services.UI.OpponentStatement(KICK_MSG);
		Services.Events.Unregister<InputEvent>(ChooseTankardKick);
		Services.Board.HighlightAllAroundSpace(GridLoc.x, GridLoc.z, BoardBehavior.OnOrOff.Off);
	}


	/// <summary>
	/// Checks adjacency of two spaces on the grid, including diagonal adjacency.
	/// </summary>
	/// <returns><c>true</c> if the spaces are adjacent, <c>false</c> otherwise.</returns>
	/// <param name="one">The first space to check.</param>
	/// <param name="two">The space the first is being checked against.</param>
	private bool CheckAllAdjacent(TwoDLoc one, TwoDLoc two){
		return (Mathf.Abs(one.x - two.x) <= 1 &&
				Mathf.Abs(one.z - two.z) <= 1) ? true : false;
	}


	/// <summary>
	/// Give each defender in a space with a tankard at the end of the Defenders Move phase inspiration.
	/// </summary>
	/// <param name="e">An EndPhaseEvent.</param>
	private void GiveInspiration(Event e){
		Debug.Assert(e.GetType() == typeof(EndPhaseEvent), "Non-EndPhaseEvent in GiveInspiration");

		EndPhaseEvent endEvent = e as EndPhaseEvent;

		if (endEvent.Phase.GetType() == typeof(TurnManager.PlayerMove)){
			foreach (DefenderSandbox defender in Services.Defenders.GetAllDefenders()){
				if (defender != this){
					TwoDLoc loc = defender.ReportGridLoc();

					if (Services.Board.CheckIfTankard(loc.x, loc.z)){
						for (int i = 0; i < BETTER_DRINK_INSP; i++){
							defender.DefeatAttacker();
						}
					}
				}
			}
		}
	}


	#endregion tankard
}
