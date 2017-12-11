using System.Collections.Generic;
using UnityEngine;

public class GuardianBehavior : DefenderSandbox {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//Guardian stats
	private int guardianSpeed = 4;
	private int guardianAttack = 0;
	private int guardianArmor = 2;


	//upgrade tracks
	private enum UpgradeTracks { Hold_the_Line, Single_Combat };


	//Single Combat track
	private enum SingleCombatTrack { None, Single_Combat, Youre_Mine, Challenger, Champion };
	private List<string> singleCombatDescriptions = new List<string>() {
		"<b>Prepare for single combat</b>",
		"<b>Single Combat</b>\n\nWhen you defeat a Warlord, upgrade immediately.",
		"<b>You're Mine!</b>\n\nWhen you defeat a Warlord, upgrade immediately.\n\nWhen attacking a Warlord, gain +5 attack.",
		"<b>Challenger</b>\n\nWhen you defeat a Warlord, upgrade immediately.\n\nWhen attacking a Warlord, gain +5 attack and +5 armor.",
		"<b>Champion</b>\n\nWhen attacking a Warlord, gain +5 attack and +5 armor.\n\nWhen you defeat a Leader, also defeat their retinue.",
		"<b>Master of single combat!</b>"
	};
	private SingleCombatTrack currentSingleCombat;
	private const string LEADER_TAG = "Leader";
	private const int SINGLE_COMBAT_ATTACK_BONUS = 5;
	private const int SINGLE_COMBAT_ARMOR_BONUS = 5;


	//Hold the Line track
	private enum HoldTrack { None, Hold_the_Line, Draw_Them_In, Bulwark, The_Last_Bastion };
	private List<string> holdDescriptions = new List<string>() {
		"Stand strong",
		"<b>Hold the Line</b>\n\n<size=12>After moving, choose the column to the left or right of the Guardian. The Horde does not advance there.\n\nWhen the Horde is defeated in that column, gain 1 experience.</size>",
		"<b>Face me!</b>\n\n<size=10>After moving, choose the column to the left or right of the Guardian. The Horde does not advance there.\n\nWhen there is an open space in the chosen column, the Horde will move into that space.\n\nWhen the Horde is defeated in that column, gain 1 experience.</size>",
		"<b>Bulwark</b>\n\n<size=10>After moving, choose the column to the left or right of the Guardian. The Horde does not advance there.\n\nWhen there is an open space in the chosen column, the Horde will move into that space.\n\nWhen the Horde is defeated in that column, gain 2 experience.</size>",
		"<b>The Last Bastion</b>\n\nEvery other turn, the Horde does not advance. Gain 2 experience when the Horde is defeated.",
		"<b>None shall pass!</b>"
	};
	private HoldTrack currentHold;
	private const string CHOOSE_TO_BLOCK = "Choose a column to hold the line";
	private const string INVALID_COLUMN = "Please choose an adjacent column";
	private const string COLUMN_CHOSEN = "The Guardian is holding the line";
	private const string BOARD_TAG = "Board";
	private const int BLANK_COLUMN = 999; //nonsense value
	private const int JUST_STARTED = 111; //nonsense value
	private int blockedColumn = BLANK_COLUMN;
	private int alternateTurn = 1;
	private const string ALL_BLOCKED = "The Last Bastion blocks all columns";
	private const string NONE_BLOCKED = "The Last Bastion inactive this turn";
	private const string LINE_MARKER_OBJ = "Line held marker";


	//character sheet information
	private const string GUARDIAN_NAME = "Guardian";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables with values for the Guardian
	public override void Setup(){
		base.Setup();

		Speed = guardianSpeed;
		AttackMod = guardianAttack;
		Armor = guardianArmor;

		currentHold = HoldTrack.None;
		currentSingleCombat = SingleCombatTrack.None;
	}


	#region movement


	/// <summary>
	/// When holding the line, the Guardian chooses one or more columns to block after moving.
	/// </summary>
	public override void Move(){
		base.Move();

		if (currentHold != HoldTrack.None &&
			currentHold != HoldTrack.The_Last_Bastion) extraText.text = CHOOSE_TO_BLOCK;


		/*
		 * 
		 * The following sequence handles the Guardian's post-movement Hold the Line choices.
		 * 1. If the Guardian is holding the line, but there's no chosen column, the Guardian didn't make a choice last turn. Don't re-register.
		 * 2. If the Guardian is holding the line, and there's a valid column chosen (or the column is set to the nonsense "just started" value),
		 * the Guardian made a choice last time. Unblock that column, and then re-register.
		 * 
		 * The above applies before the The Last Bastion level. At that level, the player stops making choices and the system automatically tracks
		 * whether the attackers can advance.
		 * 
		 */
		if (currentHold == HoldTrack.The_Last_Bastion){
			//do nothing; at The Last Bastion, blocking is handled by listening to EndPhaseEvents
		} else if (currentHold != HoldTrack.None && blockedColumn == BLANK_COLUMN) return;
		else if (currentHold != HoldTrack.None && currentHold != HoldTrack.The_Last_Bastion){
			if (blockedColumn != JUST_STARTED) MakeColumnLure(blockedColumn, false);
			UnholdLine();
			Services.Events.Register<InputEvent>(ChooseColumn);
			Services.Board.HighlightColumn(GridLoc.x - 1, BoardBehavior.OnOrOff.On);
			Services.Board.HighlightColumn(GridLoc.x + 1, BoardBehavior.OnOrOff.On);
		}
	}


	/// <summary>
	/// Handle blocking and unblocking all columns when the Guardian has reached The Last Bastion on the Hold the Line track.
	/// </summary>
	private void AlternateBlockingColumns(Event e){
		Debug.Assert(e.GetType() == typeof(NewTurnEvent), "Non-NewTurnEvent in AlternateBlockingColumns");

		alternateTurn++;
		if (alternateTurn%2 == 0){
			BlockAllColumns();
			extraText.text = ALL_BLOCKED;
		} else {
			UnblockAllColumns();
			extraText.text = NONE_BLOCKED;
		}
	}


	/// <summary>
	/// If the Guardian is moving up the Hold the Line track, call this at the start of the Defenders Move phase to release the currently blocked column.
	/// </summary>
	private void UnholdLine(){
		if (blockedColumn != BLANK_COLUMN){
			Services.Events.Fire(new UnblockColumnEvent(blockedColumn));
			blockedColumn = BLANK_COLUMN;
		}
	}


	private void BlockAllColumns(){
		for (int i = 0; i < BoardBehavior.BOARD_WIDTH; i++) Services.Events.Fire(new BlockColumnEvent(i));
	}


	private void UnblockAllColumns(){
		for (int i = 0; i < BoardBehavior.BOARD_WIDTH; i++) Services.Events.Fire(new UnblockColumnEvent(i));
	}


	/// <summary>
	/// Used as an InputEvent handler to enable the Guardian to choose a column to block.
	/// </summary>
	/// <param name="e">The input event.</param>
	private void ChooseColumn(Event e){
		InputEvent inputEvent = e as InputEvent;

		if (inputEvent.selected.tag == BOARD_TAG){
			SpaceBehavior space = inputEvent.selected.GetComponent<SpaceBehavior>();

			if (space.GridLocation.x - 1 == GridLoc.x || space.GridLocation.x + 1 == GridLoc.x){
				blockedColumn = space.GridLocation.x;
				Services.Events.Fire(new BlockColumnEvent(blockedColumn));

				if (currentHold != HoldTrack.Hold_the_Line) MakeColumnLure(blockedColumn, true); //attackers move into blocked columns at higher Hold the Line levels

				Services.Events.Unregister<InputEvent>(ChooseColumn);
				extraText.text = COLUMN_CHOSEN;

				RemoveBlockFeedbackTask pickUpTask = new RemoveBlockFeedbackTask();
				pickUpTask.Then(new BlockFeedbackTask(blockedColumn, LINE_MARKER_OBJ));
				Services.Tasks.AddTask(pickUpTask);
				Services.Board.HighlightColumn(GridLoc.x - 1, BoardBehavior.OnOrOff.Off);
				Services.Board.HighlightColumn(GridLoc.x + 1, BoardBehavior.OnOrOff.Off);
				return;
			} else {
				extraText.text = INVALID_COLUMN;
			}
		}
	}


	/// <summary>
	/// Makes all spaces in a given column lure attackers into moving sideways into them, or undoes their lure state.
	/// </summary>
	private void MakeColumnLure(int column, bool newState){
		List<SpaceBehavior> spaces = Services.Board.GetAllSpacesInColumn(column);

		foreach (SpaceBehavior space in spaces) space.Lure = newState;
	}


	#endregion movement


	#region combat


	/// <summary>
	/// The Guardian is especially effective against Warlords when on the Single Combat track.
	/// </summary>
	/// <param name="attacker">The attacker this defender is fighting.</param>
	public override void TryFight(AttackerSandbox attacker){
		if (!CheckIsNorth(attacker)) return; //don't fight if the attacker isn't directly to the north

		//if the Defender gets this far, a fight will actually occur; get a combat card for the attacker
		int attackerValue = Services.AttackDeck.GetAttackerCard().Value;
		extraText.text = DisplayCombatMath(attacker, attackerValue);

		if (ChosenCard.Value + DetermineAttackMod(attacker) > attackerValue + attacker.AttackMod){
			int damage = (ChosenCard.Value + DetermineAttackMod(attacker)) - (attackerValue + attacker.AttackMod + attacker.Armor);

			damage = damage < 0 ? 0 : damage; //don't let damage be negative, "healing" the attacker

			if (damage >= attacker.Health){
				DefeatedSoFar = DetermineDefeatedSoFar(attacker);
				Services.UI.ReviseNextLabel(defeatsToNextUpgrade, DefeatedSoFar);
				if (currentSingleCombat == SingleCombatTrack.Champion) DefeatRetinue(attacker);
				powerupReadyParticle.SetActive(CheckUpgradeStatus());
			}

			attacker.TakeDamage(damage);
				
			FinishWithCard();
			DoneFighting();
		} else {
			attacker.FailToDamage();
			Services.Events.Fire(new MissedFightEvent());
			FinishWithCard();
			DoneFighting();
		}
	}


	/// <summary>
	/// Determine whether the time-to-upgrade particle should be displayed.
	/// </summary>
	/// <returns><c>true</c> if the defender has defeated enough attackers and has room to upgrade, <c>false</c> otherwise.</returns>
	protected override bool CheckUpgradeStatus(){
		if (DefeatedSoFar >= defeatsToNextUpgrade &&
			currentHold != HoldTrack.The_Last_Bastion &&
			currentSingleCombat!= SingleCombatTrack.Champion) return true;

		return false;
	}



	/// <summary>
	/// The Guardian's combat math changes when mastering single combat.
	/// </summary>
	/// <returns>A string explaining the math behind each combat.</returns>
	/// <param name="attacker">The attacker this defender is fighting.</param>
	/// <param name="attackerValue">The value of the attacker's card.</param>
	protected override string DisplayCombatMath(AttackerSandbox attacker, int attackerValue){
		int damage = (ChosenCard.Value + DetermineAttackMod(attacker)) - (attackerValue + attacker.AttackMod + attacker.Armor);

		damage = damage < 0 ? 0 : damage;

		return DEFENDER_VALUE + ChosenCard.Value + PLUS + DetermineAttackMod(attacker) + NEWLINE +
			   ATTACKER_VALUE + attackerValue + PLUS + attacker.AttackMod + NEWLINE +
			   HITS + ((ChosenCard.Value + DetermineAttackMod(attacker)) - (attackerValue + attacker.AttackMod)).ToString() + NEWLINE + 
			   DAMAGE + damage.ToString();;
	}


	/// <summary>
	/// Determines this Guardian's attack mofidier, taking Single Combat track bonuses into account.
	/// </summary>
	/// <returns>The attack mod.</returns>
	/// <param name="attacker">The attacker the Guardian is fighting.</param>
	private int DetermineAttackMod(AttackerSandbox attacker){
		if (currentSingleCombat == SingleCombatTrack.None ||
			currentSingleCombat == SingleCombatTrack.Single_Combat ||
			attacker.tag != LEADER_TAG) return AttackMod;
		else return AttackMod + SINGLE_COMBAT_ATTACK_BONUS;
	}


	/// <summary>
	/// At certain points on the Single Combat track, the Guardian upgrades especially quickly
	/// </summary>
	/// <returns>The defeated so far.</returns>
	private int DetermineDefeatedSoFar(AttackerSandbox attacker){
		int temp = DefeatedSoFar;

		if ((currentSingleCombat == SingleCombatTrack.Single_Combat ||
			 currentSingleCombat == SingleCombatTrack.Youre_Mine ||
			 currentSingleCombat == SingleCombatTrack.Challenger) && attacker.tag == LEADER_TAG){
			temp = defeatsToNextUpgrade;
		} else temp++;

		return temp;
	}


	/// <summary>
	/// When at the end of the Single Combat track, the Guardian defeats a leader's retinue after defeating the leader.
	/// </summary>
	/// <param name="attacker">The attacker the Guardian is fighting.</param>
	private void DefeatRetinue(AttackerSandbox attacker){
		if (attacker.tag != LEADER_TAG) return;

		if (TryDefeatRetinueMember(attacker.XPos, attacker.ZPos + 1)) DefeatedSoFar++;
		if (TryDefeatRetinueMember(attacker.XPos, attacker.ZPos - 1)) DefeatedSoFar++;
		if (TryDefeatRetinueMember(attacker.XPos - 1, attacker.ZPos)) DefeatedSoFar++;
		if (TryDefeatRetinueMember(attacker.XPos + 1, attacker.ZPos)) DefeatedSoFar++;
	}


	/// <summary>
	/// Attempts to defeat a minion at given grid coordinates.
	/// </summary>
	/// <returns><c>true</c>if a minion was defeated, <c>false</c> otherwise.</returns>
	/// <param name="x">The x coordinate to be checked.</param>
	/// <param name="z">The z coordinate to be checked.</param>
	private bool TryDefeatRetinueMember(int x, int z){
		if (!Services.Board.CheckValidSpace(x, z)) return false; //don't try to defeat retinue members off the board
		if (Services.Board.GeneralSpaceQuery(x, z) != SpaceBehavior.ContentType.Attacker) return false; //if there's no attacker in the space, do nothing

		GameObject attacker = Services.Board.GetThingInSpace(x, z);

		Debug.Assert(attacker != null, "Tried to get an attacker from a space with no contents.");

		if (attacker.tag == LEADER_TAG) return false; //only defeat minions in this way

		attacker.GetComponent<AttackerSandbox>().TakeDamage(attacker.GetComponent<AttackerSandbox>().Health); //inflict enough damage to zero their health

		return true; //if you got this far, a retinue member was successfull defeated
	}


	#endregion combat


	/// <summary>
	/// Use this defender's name when taking over the character sheet, and display its upgrade paths.
	/// </summary>
	public override void TakeOverCharSheet(){
		Services.UI.TakeOverCharSheet(GUARDIAN_NAME,
									  Speed,
									  AttackMod,
									  Armor,
									  defeatsToNextUpgrade,
									  DefeatedSoFar,
									  holdDescriptions[(int)currentHold + 1],
									  holdDescriptions[(int)currentHold],
									  singleCombatDescriptions[(int)currentSingleCombat + 1],
									  singleCombatDescriptions[(int)currentSingleCombat]
		);
	}


	/// <summary>
	/// When the player clicks a button to power up, this function is called.
	/// </summary>
	/// <param>The upgrade tree the player wants to move along.</param>
	/// <param name="tree">The upgrade tree the player clicked. Left is 0, right is 1.</param>
	public override bool PowerUp(int tree){
		if (!base.PowerUp(tree)) return false; //has the Guardian defeated enough attackers to upgrade?

		switch (tree){
			case (int)UpgradeTracks.Hold_the_Line:
				if (currentHold != HoldTrack.The_Last_Bastion) currentHold++;
				if (currentHold == HoldTrack.Hold_the_Line){
					blockedColumn = JUST_STARTED;
					Services.Events.Register<AttackerDefeatedEvent>(GainExperienceFromElsewhere); //listen for chances to gain experience from a blocked column
				//when the player reaches The Last Bastion, check whether the player has chosen a column to block (blockedColumn != BLANK_COLUMN)
				//if so, release that column and get rid of the marker
				//if not, unregister for input events--the player isn't going to need to make that choice
				//either way, start listening for the event that the Guardian uses to decide whether to block columns
				} else if (currentHold == HoldTrack.The_Last_Bastion) {
					Services.Events.Register<NewTurnEvent>(AlternateBlockingColumns);
					if (blockedColumn != BLANK_COLUMN) {
						MakeColumnLure(blockedColumn, false);
						UnholdLine();
						Services.Tasks.AddTask(new RemoveBlockFeedbackTask());
					} else Services.Events.Unregister<InputEvent>(ChooseColumn);
				}
				break;
			case (int)UpgradeTracks.Single_Combat:
				if (currentSingleCombat != SingleCombatTrack.Champion) currentSingleCombat++;
				break;
		}

		Services.UI.ReviseTrack1(holdDescriptions[(int)currentHold + 1], holdDescriptions[(int)currentHold]);
		Services.UI.ReviseTrack2(singleCombatDescriptions[(int)currentSingleCombat + 1], singleCombatDescriptions[(int)currentSingleCombat]);

		//having powered up, shut off the particle indicating the player should power up
		powerupReadyParticle.SetActive(false);

		return true;
	}


	/// <summary>
	/// Allows the Guardian to gain experience when an attacker is defeated in a blocked column.
	/// </summary>
	/// <param name="e">An AttackerDefeatedEvent from when an attacker is defeated..</param>
	private void GainExperienceFromElsewhere(Event e){
		Debug.Assert(e.GetType() == typeof(AttackerDefeatedEvent), "Non-AttackerDefeatedEvent in GainExperienceFromElsewhere.");

		AttackerDefeatedEvent defeatEvent = e as AttackerDefeatedEvent;

		if (currentHold == HoldTrack.The_Last_Bastion){ //the Guardian gets 2 total experience when any attacker is defeated while at The Last Bastion
			if (defeatEvent.location.x == GridLoc.x &&
				defeatEvent.location.z == GridLoc.z + 1) DefeatedSoFar++;
			else {
				DefeatedSoFar += 2;
			}
		} else if (defeatEvent.location.x == blockedColumn) {
			DefeatedSoFar++;

			if (currentHold == HoldTrack.Bulwark) DefeatedSoFar++; //gain extra experience at higher levels of the Hold the Line track
		}
	}
}
