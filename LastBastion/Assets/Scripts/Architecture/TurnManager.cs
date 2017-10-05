/// <summary>
/// This class handles the overall turn structure.
/// 
/// Its heart is a state machine; each phase of the game is a state.
/// </summary>

using System.Collections.Generic;
using UnityEngine;

public class TurnManager {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the state machine that controls movement through phases of turns
	private FSM<TurnManager> turnMachine;


	//how long the system waits during each phase
	private float attackerAdvanceDuration = 10.0f;
	private float besiegeWallsDuration = 5.0f;




	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables
	public void Setup(){
		turnMachine = new FSM<TurnManager>(this);
		turnMachine.TransitionTo<AttackersAdvance>();
	}


	//go through one loop of the current state
	public void Tick(){
		turnMachine.Tick();
	}


	/// <summary>
	/// State for the attackers moving south at the start of each turn.
	/// </summary>
	private class AttackersAdvance : FSM<TurnManager>.State {
		float timer;

		//tell the attacker manager to move the attackers.
		//this is routed through the attacker manager to avoid spreading control over the attackers over multiple classes.
		public override void OnEnter(){
			timer = 0.0f;
			Services.Attackers.SpawnNewAttackers();
			Services.Attackers.MoveAttackers();
		}


		//wait while the attackers move
		public override void Tick(){
			timer += Time.deltaTime;
			if (timer >= Context.attackerAdvanceDuration) OnExit();
		}


		public override void OnExit(){
			TransitionTo<PlayerMove>();
		}
	}


	private class PlayerMove : FSM<TurnManager>.State {


		public override void Tick(){
			TransitionTo<BesiegeWalls>();
		}
	}


	private class BesiegeWalls : FSM<TurnManager>.State {
		float timer;
		bool besieging;
		List<AttackerSandbox> besiegers;

		public override void OnEnter (){
			timer = 0.0f;
			besieging = false;
			besiegers = Services.Board.GetBesiegingAttackers();

			if (besiegers.Count > 0){
				besieging = true;
				foreach (AttackerSandbox attacker in besiegers){
					int combatValue = Services.AttackDeck.GetAttackerCard().Value;

					if (combatValue > Services.Board.GetWallStrength(attacker.GetColumn())){
						Services.Board.ChangeWallDurability(attacker.GetColumn(), -attacker.SiegeStrength);
					} else {
						Debug.Log("Failed to damage the wall with a value of " + combatValue);
					}
				}
			}
		}


		/// <summary>
		/// If anyone is besieging, wait while the animations play
		/// </summary>
		public override void Tick(){
			if (besieging){
				timer += Time.deltaTime;

				if (timer >= Context.besiegeWallsDuration) OnExit();
			} else {
				OnExit();
			}
		}


		public override void OnExit (){
			TransitionTo<AttackersAdvance>();
		}
	}
}
