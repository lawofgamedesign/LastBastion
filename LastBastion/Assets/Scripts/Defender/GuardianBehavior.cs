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
		"<b>Single Combat</b>\n\nWhen attacking a Warlord, gain +3 attack.",
		"<b>You're Mine!</b>\n\nWhen attacking a Warlord, gain +3 attack.\n\nWhen you defeat a Warlord, upgrade again immediately.",
		"<b>Challenger</b>\n\nWhen attacking a Warlord, gain +3 attack and +3 armor.\n\nWhen you defeat a Warlord, upgrade again immediately.",
		"<b>Champion</b>\n\nWhen attacking a Warlord, gain +3 attack and +3 armor.\n\nWhen you defeat a Leader, also defeat their retinue.",
		"<b>Master of single combat!</b>"
	};
	private SingleCombatTrack currentSingleCombat;
	private const string LEADER_TAG = "Leader";
	private const int SINGLE_COMBAT_ATTACK_BONUS = 3;
	private const int SINGLE_COMBAT_ARMOR_BONUS = 3;


	//Hold the Line track
	private enum HoldTrack { None, Hold_the_Line, Draw_Them_In, Bulwark, The_Last_Bastion };
	private List<string> holdDescriptions = new List<string>() {


	};
	private HoldTrack currentHold;


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

		currentSingleCombat = SingleCombatTrack.None;
	}


	/// <summary>
	/// The Guardian is especially effective against Warlords when on the Single Combat track.
	/// </summary>
	/// <param name="attacker">The attacker this defender is fighting.</param>
	public override void TryFight(AttackerSandbox attacker){
		if (!CheckIsNorth(attacker)) return; //don't fight if the attacker isn't directly to the north

		//if the Defender gets this far, a fight will actually occur; get a combat card for the attacker
		int attackerValue = Services.AttackDeck.GetAttackerCard().Value;

		if (ChosenCard.Value + DetermineAttackMod(attacker) > attackerValue + attacker.AttackMod){
			attacker.TakeDamage((ChosenCard.Value + DetermineAttackMod(attacker)) - (attackerValue + attacker.AttackMod + attacker.Armor));
			FinishWithCard();
			DefeatedSoFar = DetermineDefeatedSoFar(attacker);
			charSheet.ReviseNextLabel(defeatsToNextUpgrade - DefeatedSoFar);
			if (currentSingleCombat == SingleCombatTrack.Champion) DefeatRetinue(attacker);
			DoneFighting();
		} else {
			attacker.FailToDamage();
			FinishWithCard();
			DoneFighting();
		}
	}


	/// <summary>
	/// Determines this Guardian's attack mofidier, taking Single Combat track bonuses into account.
	/// </summary>
	/// <returns>The attack mod.</returns>
	/// <param name="attacker">The attacker the Guardian is fighting.</param>
	private int DetermineAttackMod(AttackerSandbox attacker){
		if (currentSingleCombat == SingleCombatTrack.None || attacker.tag != LEADER_TAG) return AttackMod;
		else return AttackMod + SINGLE_COMBAT_ATTACK_BONUS;
	}


	/// <summary>
	/// At certain points on the Single Combat track, the Guardian upgrades especially quickly
	/// </summary>
	/// <returns>The defeated so far.</returns>
	private int DetermineDefeatedSoFar(AttackerSandbox attacker){
		int temp = DefeatedSoFar;

		if ((currentSingleCombat == SingleCombatTrack.Youre_Mine || currentSingleCombat == SingleCombatTrack.Challenger) &&
			attacker.tag == LEADER_TAG){
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


	/// <summary>
	/// Use this defender's name when taking over the character sheet, and display its upgrade paths.
	/// </summary>
	public override void TakeOverCharSheet(){
		charSheet.RenameSheet(GUARDIAN_NAME);
		charSheet.ReviseStatBlock(Speed, AttackMod, Armor);
		charSheet.ReviseTrack2(singleCombatDescriptions[(int)currentSingleCombat + 1], singleCombatDescriptions[(int)currentSingleCombat]);
		charSheet.ReviseNextLabel(defeatsToNextUpgrade - DefeatedSoFar);
		if (!charSheet.gameObject.activeInHierarchy) charSheet.ChangeSheetState();
	}


	/// <summary>
	/// When the player clicks a button to power up, this function is called.
	/// </summary>
	/// <param>The upgrade tree the player wants to move along.</param>
	/// <param name="tree">The upgrade tree the player clicked. Left is 0, right is 1.</param>
	public override bool PowerUp(int tree){
		if (!base.PowerUp(tree)) return false; //has the Brawler defeated enough attackers to upgrade?

		switch (tree){
		case (int)UpgradeTracks.Single_Combat:
			if (currentSingleCombat != SingleCombatTrack.Champion) currentSingleCombat++;
			break;
		}

		charSheet.ReviseTrack2(singleCombatDescriptions[(int)currentSingleCombat + 1], singleCombatDescriptions[(int)currentSingleCombat]);

		return true;
	}
}
