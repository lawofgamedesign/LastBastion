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
	public FSM<TurnManager> TurnMachine { get { return turnMachine; } private set { turnMachine = value; } }


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


	//button for ending the current phase and moving to the next
	public GameObject nextPhaseButton;
	private const string NEXT_BUTTON_OBJ = "Next phase button";


	//what turn is it?
	public int CurrentTurn { get; private set; }
	public int TotalTurns { get; private set; }




	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables
	public void Setup(){
		turnMachine = new FSM<TurnManager>(this);
		TurnMachine = turnMachine;
		CurrentTurn = 0;
		TotalTurns = 10;
		turnMachine.TransitionTo<StartOfTurn>();
		phaseText = GameObject.Find(PHASE_OBJ).GetComponent<Text>();
		nextPhaseButton = GameObject.Find(NEXT_BUTTON_OBJ);
		ToggleNextPhaseButton();
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


	/// <summary>
	/// Handle all effects relating to keeping track of which turn it is.
	/// </summary>
	private void NewTurn(){
		CurrentTurn++;
		Services.UI.SetTurnText(CurrentTurn, TotalTurns);
	}


	/// <summary>
	/// Switch the next phase button on or off.
	/// </summary>
	private void ToggleNextPhaseButton(){
		nextPhaseButton.SetActive(!nextPhaseButton.activeInHierarchy);
	}


	/////////////////////////////////////////////
	/// States
	/////////////////////////////////////////////


	//update the turn counter
	private class StartOfTurn : FSM<TurnManager>.State {

		public override void OnEnter (){
			Context.NewTurn();
		}

		public override void Tick (){
			TransitionTo<AttackersAdvance>();
		}
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
			Context.phaseText.text = ATTACKER_MOVE;
			Context.TurnRulebookPage();
		}


		//wait while the attackers move
		public override void Tick(){
			timer += Time.deltaTime;
			if (timer >= Context.attackerAdvanceDuration) TransitionTo<PlayerMove>();;
		}
	}

	/// <summary>
	/// State for the defenders' movement.
	/// </summary>
	private class PlayerMove : FSM<TurnManager>.State {


		bool needToExit;

		private void HandleMoveInputs(Event e){
			InputEvent inputEvent = e as InputEvent;

			if (inputEvent.selected.tag == DEFENDER_TAG){
				Services.Defenders.SelectDefenderForMovement(inputEvent.selected.GetComponent<DefenderSandbox>());
			} else if (inputEvent.selected.tag == BOARD_TAG){
				if (Services.Defenders.IsAnyoneSelected()){
					Services.Defenders.GetSelectedDefender().TryPlanMove(inputEvent.selected.GetComponent<SpaceBehavior>().GridLocation);
				}
			} else if (inputEvent.selected.tag == ATTACKER_TAG || inputEvent.selected.tag == MINION_TAG || inputEvent.selected.tag == LEADER_TAG){
				Services.UI.SetExtraText(inputEvent.selected.GetComponent<AttackerSandbox>().GetUIInfo());
			}


			//each time the player clicks, ask if everyone is finished. If so, move on
			//if (Services.Defenders.IsEveryoneDone()) OnExit();
		}


		private void HandlePhaseEndInput(Event e){
			Debug.Assert(e.GetType() == typeof(EndPhaseEvent), "Non-EndPhaseEvent in HandlePhaseEndInput");

			TransitionTo<PlayerFight>();
		}


		public override void OnEnter(){
			Services.Defenders.PrepareDefenderMovePhase();
			Context.phaseText.text = PLAYER_MOVE;
			Context.TurnRulebookPage();
			Services.Events.Register<InputEvent>(HandleMoveInputs);
			Services.Events.Register<EndPhaseEvent>(HandlePhaseEndInput);
			needToExit = false;
			Context.ToggleNextPhaseButton();
		}


		public override void OnExit(){
			Services.Defenders.CompleteMovePhase();
			Services.Events.Unregister<InputEvent>(HandleMoveInputs);
			Services.Events.Unregister<EndPhaseEvent>(HandlePhaseEndInput);
			Context.ToggleNextPhaseButton();
		}
	}


	/// <summary>
	/// State for the defenders' combat. This is public so that the Ranger can determine whether it's the Defenders Fight phase, and display--or not display--
	/// the Showboating UI accordingly.
	/// </summary>
	public class PlayerFight : FSM<TurnManager>.State {


		private void HandleFightInputs(Event e){
			InputEvent inputEvent = e as InputEvent;

			if (inputEvent.selected.tag == DEFENDER_TAG){
				Services.Defenders.SelectDefenderForFight(inputEvent.selected.GetComponent<DefenderSandbox>());
			} else if ((inputEvent.selected.tag == ATTACKER_TAG || inputEvent.selected.tag == MINION_TAG || inputEvent.selected.tag == LEADER_TAG) &&
					   Services.Defenders.IsAnyoneSelected() &&
					   Services.Defenders.GetSelectedDefender().GetChosenCardValue() != DefenderSandbox.NO_CARD_SELECTED){
				Services.Defenders.GetSelectedDefender().TryFight(inputEvent.selected.GetComponent<AttackerSandbox>());
			} else if (inputEvent.selected.tag == ATTACKER_TAG || inputEvent.selected.tag == MINION_TAG || inputEvent.selected.tag == LEADER_TAG) {
				Services.UI.SetExtraText(inputEvent.selected.GetComponent<AttackerSandbox>().GetUIInfo());
			} else if (inputEvent.selected.tag == BOARD_TAG){
				Services.Events.Fire(new BoardClickedEvent(inputEvent.selected.GetComponent<SpaceBehavior>().GridLocation));
			}

			if (Services.Defenders.IsEveryoneDone()) TransitionTo<BesiegeWalls>();;
		}


		/// <summary>
		/// End the phase, but only if the cards aren't moving. This is a bodge to protect the Brawler, who otherwise might fail to
		/// unregister for InputEvents if they return from their DoneFighting() early.
		/// </summary>
		/// <param name="e">E.</param>
		private void HandlePhaseEndInput(Event e){
			Debug.Assert(e.GetType() == typeof(EndPhaseEvent), "Non-EndPhaseEvent in HandlePhaseEndInput");

			if (!Services.Tasks.CheckForTaskOfType<PickUpCardTask>() &&
				!Services.Tasks.CheckForTaskOfType<FlipCardTask>() &&
				!Services.Tasks.CheckForTaskOfType<PutDownCardTask>()) TransitionTo<BesiegeWalls>();
		}


		public override void OnEnter(){
			Services.Defenders.PrepareDefenderFightPhase();
			Context.phaseText.text = PLAYER_FIGHT;
			Context.TurnRulebookPage();
			Services.Events.Register<InputEvent>(HandleFightInputs);
			Services.Events.Register<EndPhaseEvent>(HandlePhaseEndInput);
			Context.ToggleNextPhaseButton();
		}


		public override void OnExit(){
			Services.Events.Unregister<InputEvent>(HandleFightInputs);
			Services.Events.Unregister<EndPhaseEvent>(HandlePhaseEndInput);
			Services.Defenders.CompleteFightPhase();
			Context.ToggleNextPhaseButton();
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
					TransitionTo<EndPhase>();
				}
			}
		}
	}


	/// <summary>
	/// Nothing has to happen during the end phase, but if there's some end-of-turn cleanup to be done, this state sends
	/// out an event to tell others to do it.
	/// </summary>
	public class EndPhase : FSM<TurnManager>.State {


		public override void OnEnter(){
			Services.Events.Fire(new EndPhaseEvent());
		}


		public override void Tick (){
			if(CheckStartNextTurn()) TransitionTo<StartOfTurn>();
		}


		/// <summary>
		/// Don't go on to the start of the next turn if this is the last turn of the wave.
		/// </summary>
		private bool CheckStartNextTurn(){
			if (Context.CurrentTurn < Context.TotalTurns) return true;
			return false;
		}
	}
}
