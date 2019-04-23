using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianBehaviorRevised : DefenderSandbox {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//Guardian stats
	private int guardianSpeed = 4;
	private int guardianAttack = 0;
	private int guardianArmor = 0;


	//upgrade tracks
	private enum UpgradeTracks { Hold_the_Line, Single_Combat };


	//Single Combat track
	private enum SingleCombatTrack { None, Single_Combat, Youre_Mine, Run_Fools, Champion };
	private List<string> singleCombatDescriptions = new List<string>() {
		"<b>Prepare for single combat</b>",
		"<b>Single Combat</b>\n\nWhen you defeat a Warlord, upgrade immediately.",
		"<b>You're Mine!</b>\n\nWhen you defeat a Warlord, upgrade immediately.\n\nWhen attacking a Warlord, gain +5 attack.",
		"<b>Run, Fools!</b>\n\nWhen you defeat a Warlord, upgrade immediately and reduce Momentum by 1.\n\nWhen attacking a Warlord, gain +5 attack.",
		"<b>Champion</b>\n\nWhen attacking a Warlord, gain +5 attack.\n\nWhen you defeat a Warlord, also defeat their retinue and reduce Momentum by 1.",
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
	private const string ALL_BLOCKED = "The Last Bastion blocks all columns";
	private const string NONE_BLOCKED = "The Last Bastion inactive this turn";
	private const string LINE_MARKER_OBJ = "Line held marker";
	private const string MARKER_ORGANIZER = "Markers and tokens";


	//character sheet information
	private const string GUARDIAN_NAME = "Guardian";


	//posing information
	private const string ATTACK_ANIM = "WK_heavy_infantry_08_attack_B";


	//icon
	private const string GUARDIAN_ICON_IMG = "Sprites/Guardian icon";


	//portrait
	private const string GUARDIAN_PORTRAIT_IMG = "Sprites/Portraits/Guardian portrait";


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


		//pose the Guardian
		transform.Find(MODEL_OBJ).Find(MINI_OBJ).GetComponent<Animation>()[ATTACK_ANIM].normalizedTime = 0.75f;
		transform.Find(MODEL_OBJ).Find(MINI_OBJ).GetComponent<Animation>()[ATTACK_ANIM].normalizedSpeed = 0.0f;
		transform.Find(MODEL_OBJ).Find(MINI_OBJ).GetComponent<Animation>().Play();

		//pose the unmoving model that stays behind when the Guardian moves
		transform.Find(UNMOVING_OBJ).Find(MINI_OBJ).GetComponent<Animation>()[ATTACK_ANIM].normalizedTime = 0.75f;
		transform.Find(UNMOVING_OBJ).Find(MINI_OBJ).GetComponent<Animation>()[ATTACK_ANIM].normalizedSpeed = 0.0f;
		transform.Find(UNMOVING_OBJ).Find(MINI_OBJ).GetComponent<Animation>().Play();


		//set icon
		icon = Resources.Load<Sprite>(GUARDIAN_ICON_IMG);


		//set portrait
		portrait = Resources.Load<Sprite>(GUARDIAN_PORTRAIT_IMG);
	}
	
	
	#region combat


	/// <summary>
	/// The Guardian is especially effective against Warlords when on the Single Combat track.
	/// </summary>
	/// <param name="attacker">The attacker this defender is fighting.</param>
	public override void TryFight(AttackerSandbox attacker){
		if (!CheckIsNorth(attacker)) return; //don't fight if the attacker isn't directly to the north

		//if the Defender gets this far, a fight will actually occur; get a combat card for the attacker
		int attackerValue = Services.AttackDeck.GetAttackerCard().Value;
		int damage = (ChosenCard.Value + DetermineAttackMod(attacker)) - (attackerValue + attacker.AttackMod + attacker.Armor);

		damage = damage < 0 ? 0 : damage; //don't let damage be negative, "healing" the attacker

		Services.Tasks.AddTask(new CombatExplanationTask(attacker,
														 this,
														 attackerValue,
														 ChosenCard.Value,
														 attacker.AttackMod,
														 DetermineAttackMod(attacker),
														 damage));

		Services.UI.ReviseCardsAvail(GetAvailableValues());
	}


	public override void WinFight(AttackerSandbox attacker){
		DefeatedSoFar = DetermineDefeatedSoFar(attacker);
		Services.UI.ReviseNextLabel(defeatsToNextUpgrade, DefeatedSoFar);
		if (currentSingleCombat >= SingleCombatTrack.Run_Fools) Services.Events.Fire(new ReduceMomentumEvent());
		if (currentSingleCombat == SingleCombatTrack.Champion) DefeatRetinue(attacker);
		if (Services.Rulebook.GetType() != typeof(Tutorial.TutorialTurnManager)) powerupReadyParticle.SetActive(CheckUpgradeStatus()); //don't play particle in tutorial; it's distracting

		FinishWithCard();
		DoneFighting();
	}


	public override void TieFight(AttackerSandbox attacker){
		Services.Events.Fire(new MissedFightEvent());
		FinishWithCard();
		DoneFighting();
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
			currentHold != HoldTrack.The_Last_Bastion &&
			currentSingleCombat!= SingleCombatTrack.Champion){

			Services.UI.ObjectStatement(transform.position, gameObject.name + POWER_UP_MSG);
			return true;
		} else {
			return false;
		}
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
			 currentSingleCombat == SingleCombatTrack.Run_Fools) && attacker.tag == LEADER_TAG){
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

		return true; //if you got this far, a retinue member was successfully defeated
	}


	#endregion combat
}
