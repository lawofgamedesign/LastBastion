namespace Title
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public class DemoEnragedWarlordBehavior : WarlordSandbox {

		/////////////////////////////////////////////
		/// Fields
		/////////////////////////////////////////////


		//Enraged Warlord stats
		private int enragedSpeed = 1;
		private int enragedAttack = 1;
		private int enragedArmor = 0;
		private int enragedHealth = 5;
		private const string NAME = "Enraged Warlord";


		//animation
		private const string ATTACK_ANIM = "WK_heavy_infantry_07_attack_A";


		/////////////////////////////////////////////
		/// Functions
		/////////////////////////////////////////////


		//initialize variables, including the Enraged Warlord's stats
		public override void Setup (){
			speed = enragedSpeed;
			AttackMod = enragedAttack;
			Armor = enragedArmor;
			Health = enragedHealth;
			startHealth = enragedHealth;
			attackerName = NAME;
			base.Setup();


			//pose the Enraged Warlord
			transform.Find(MODEL_ORGANIZER).Find(MINI_OBJ).GetComponent<Animation>()[ATTACK_ANIM].normalizedTime = 0.5f;
			transform.Find(MODEL_ORGANIZER).Find(MINI_OBJ).GetComponent<Animation>()[ATTACK_ANIM].normalizedSpeed = 0.0f;
			transform.Find(MODEL_ORGANIZER).Find(MINI_OBJ).GetComponent<Animation>().Play();


			//the Demo Enraged Warlord doesn't do anything to the deck
		}
	}
}
