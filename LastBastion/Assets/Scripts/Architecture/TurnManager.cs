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
		List<AttackerSandbox> besiegers;

		//how long the system waits between resolving besieging attacks
		private float besiegeWallDuration = 0.75f;


		//are any enemies besieging the wall? Get a list of them
		public override void OnEnter (){
			timer = 0.0f;
			besiegers = Services.Board.GetBesiegingAttackers();
		}


		/// <summary>
		/// If anyone is besieging, wait while the animations play. Otherwise, wait a brief period.
		/// </summary>
		public override void Tick(){
			timer += Time.deltaTime;

			if (timer >= besiegeWallDuration){
				if (besiegers.Count > 0){
					int combatValue = Services.AttackDeck.GetAttackerCard().Value;

					if (combatValue > Services.Board.GetWallStrength(besiegers[0].GetColumn())){
						Services.Board.ChangeWallDurability(besiegers[0].GetColumn(), -besiegers[0].SiegeStrength);
					} else {
						Services.Board.FailToDamageWall(besiegers[0].GetColumn());
					}

					besiegers.RemoveAt(0);
					timer = 0.0f;
				} else {
					OnExit();
				}
			}
		}


		public override void OnExit (){
			TransitionTo<AttackersAdvance>();
		}
	}
}
