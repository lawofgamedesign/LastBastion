/// <summary>
/// This class handles the overall turn structure.
/// 
/// Its heart is a state machine; each phase of the game is a state.
/// </summary>

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the state machine that controls movement through phases of turns
	private FSM<TurnManager> turnMachine;


	//how long the system waits during each phase
	private float attackerAdvanceDuration = 1.0f;


	//tags for selecting and clicking on things
	private const string DEFENDER_TAG = "Defender";
	private const string BOARD_TAG = "Board";
	private const string ATTACKER_TAG = "Attacker";
	private const string LEADER_TAG = "Leader";
	private const string MINION_TAG = "Minion";


	//feedback for the player to help track which phase they're in
	private Text phaseText;
	private const string PHASE_OBJ = "Phase";
	private const string ATTACKER_MOVE = "Horde moves";
	private const string PLAYER_MOVE = "Defenders move";
	private const string PLAYER_FIGHT = "Defenders fight";
	private const string BESIEGE = "Horde besieges";




	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables
	public void Setup(){
		turnMachine = new FSM<TurnManager>(this);
		turnMachine.TransitionTo<AttackersAdvance>();
		phaseText = GameObject.Find(PHASE_OBJ).GetComponent<Text>();
	}


	//go through one loop of the current state
	public void Tick(){
		turnMachine.Tick();
	}


	/// <summary>
	/// Provide feedback for changing phases by turning the rulebook's pages
	/// </summary>
	private void TurnRulebookPage(){
		TurnPageTask turnPage = new TurnPageTask();
		if (!Services.Tasks.CheckForTaskOfType(turnPage)) Services.Tasks.AddTask(turnPage);
	}


	/////////////////////////////////////////////
	/// States
	/////////////////////////////////////////////


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
			Context.phaseText.text = ATTACKER_MOVE;
			Context.TurnRulebookPage();
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


		private void HandleMoveInputs(Event e){
			InputEvent inputEvent = e as InputEvent;

			if (inputEvent.selected.tag == DEFENDER_TAG){
				Services.Defenders.SelectDefenderForMovement(inputEvent.selected.GetComponent<DefenderSandbox>());
			} else if (inputEvent.selected.tag == BOARD_TAG){
				if (Services.Defenders.IsAnyoneSelected()){
					Services.Defenders.GetSelectedDefender().TryPlanMove(inputEvent.selected.GetComponent<SpaceBehavior>().GridLocation);
				}
			}


			//each time the player clicks, ask if everyone is finished. If so, move on
			if (Services.Defenders.IsEveryoneDone()) OnExit();
		}


		public override void OnEnter(){
			Services.Defenders.PrepareDefenderMovePhase();
			Context.phaseText.text = PLAYER_MOVE;
			Context.TurnRulebookPage();
			Services.Events.Register<InputEvent>(HandleMoveInputs);
		}


		public override void OnExit(){
			Services.Events.Unregister<InputEvent>(HandleMoveInputs);
			TransitionTo<PlayerFight>();
		}
	}


	private class PlayerFight : FSM<TurnManager>.State {


		private void HandleFightInputs(Event e){
			InputEvent inputEvent = e as InputEvent;

			if (inputEvent.selected.tag == DEFENDER_TAG){
				Services.Defenders.SelectDefenderForFight(inputEvent.selected.GetComponent<DefenderSandbox>());
			} else if ((inputEvent.selected.tag == ATTACKER_TAG || inputEvent.selected.tag == MINION_TAG || inputEvent.selected.tag == LEADER_TAG) &&
					   Services.Defenders.IsAnyoneSelected() &&
					   Services.Defenders.GetSelectedDefender().GetChosenCardValue() != DefenderSandbox.NO_CARD_SELECTED){
				Services.Defenders.GetSelectedDefender().TryFight(inputEvent.selected.GetComponent<AttackerSandbox>());
			} else if (inputEvent.selected.tag == BOARD_TAG){
				Services.Events.Fire(new BoardClickedEvent(inputEvent.selected.GetComponent<SpaceBehavior>().GridLocation));
			}

			if (Services.Defenders.IsEveryoneDone()) OnExit();
		}


		public override void OnEnter(){
			Services.Defenders.PrepareDefenderFightPhase();
			Context.phaseText.text = PLAYER_FIGHT;
			Context.TurnRulebookPage();
			Services.Events.Register<InputEvent>(HandleFightInputs);
		}


		public override void OnExit(){
			Services.Events.Unregister<InputEvent>(HandleFightInputs);
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
			Context.phaseText.text = BESIEGE;
			Context.TurnRulebookPage();
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
