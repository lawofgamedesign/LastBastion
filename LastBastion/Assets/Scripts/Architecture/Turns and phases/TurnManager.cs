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
	protected FSM<TurnManager> turnMachine;
	public FSM<TurnManager> TurnMachine { get { return turnMachine; } protected set { turnMachine = value; } }


	//how long the system waits during each phase
	protected float attackerAdvanceDuration = 1.0f;


	//tags for selecting and clicking on things
	protected const string DEFENDER_TAG = "Defender";
	protected const string BOARD_TAG = "Board";
	protected const string ATTACKER_TAG = "Attacker";
	protected const string LEADER_TAG = "Leader";
	protected const string MINION_TAG = "Minion";


	//feedback for the player to help track which phase they're in
	protected Text phaseText;
	protected const string PHASE_OBJ = "Phase";
	protected const string ATTACKER_MOVE = "Horde moves";
	protected const string PLAYER_MOVE = "Defenders move";
	protected const string PLAYER_FIGHT = "Defenders fight";
	protected const string BESIEGE = "Horde besieges";


	//button for ending the current phase and moving to the next
	protected GameObject nextPhaseButton;
	protected Text phaseButtonText;
	protected const string TEXT_OBJ = "Text";
	protected const string NEXT_BUTTON_OBJ = "Next phase button";
	protected const string STOP_MOVING_MSG = "Done moving";
	protected const string STOP_FIGHTING_MSG = "Done fighting";
	protected const string ARE_YOU_SURE_MSG = "Are you sure? Not all defenders moved.";
	protected bool imSure = true; //used to determine whether a player is sure that they're ready to go on when there are still defenders who can move


	//what turn is it?
	public int CurrentTurn { get; private set; }
	public int TotalTurns { get; private set; }


	//feedback for when the player loses
	private const string LOSE_MSG = "You lose! R to restart.";


	//feedback for when the player wins
	private const string WIN_MSG = "You win! R to restart.";



	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables
	public virtual void Setup(){
		turnMachine = new FSM<TurnManager>(this);
		TurnMachine = turnMachine;
		ResetTurnUI();
		turnMachine.TransitionTo<StartOfTurn>();
		phaseText = GameObject.Find(PHASE_OBJ).GetComponent<Text>();
		nextPhaseButton = GameObject.Find(NEXT_BUTTON_OBJ);
		phaseButtonText = nextPhaseButton.transform.Find(TEXT_OBJ).GetComponent<Text>();
		ToggleNextPhaseButton();
	}


	//go through one loop of the current state
	public virtual void Tick(){
		turnMachine.Tick();
	}


	/// <summary>
	/// Provide feedback for changing phases by turning the rulebook's pages
	/// </summary>
	protected void TurnRulebookPage(){
		TurnPageTask turnPage = new TurnPageTask();
		if (!Services.Tasks.CheckForTaskOfType(turnPage)) Services.Tasks.AddTask(turnPage);
	}


	/// <summary>
	/// Handle all effects relating to keeping track of which turn it is.
	/// </summary>
	protected void NewTurn(){
		CurrentTurn++;
		Services.UI.SetTurnText(CurrentTurn, TotalTurns);
	}


	/// <summary>
	/// Prepare the information the turn counter UI needs for a new wave.
	/// </summary>
	protected void ResetTurnUI(){
		CurrentTurn = 0;
		TotalTurns = Services.Attackers.GetTurnsThisWave();
	}


	/// <summary>
	/// Switch the next phase button on or off.
	/// </summary>
	protected void ToggleNextPhaseButton(){
		nextPhaseButton.SetActive(!nextPhaseButton.activeInHierarchy);
	}


	/// <summary>
	/// Check if an attacker has reached the final row of the board, meaning the player has lost.
	/// </summary>
	/// <returns><c>true</c> if the player has lost, <c>false</c> otherwise.</returns>
	protected bool CheckForLoss(){
		for (int i = 0; i < BoardBehavior.BOARD_WIDTH; i++){
			if (Services.Board.GeneralSpaceQuery(i, 0) == SpaceBehavior.ContentType.Attacker) return true;
		}

		return false;
	}


	/// <summary>
	/// Handles informing the player that they've won.
	/// </summary>
	protected void PlayerWinFeedback(){
		Services.UI.SetExtraText(WIN_MSG);
	}


	/// <summary>
	/// Handles informing the player that they've lost.
	/// </summary>
	protected void PlayerLoseFeedback(){
		Services.UI.SetExtraText(LOSE_MSG);
	}


	/////////////////////////////////////////////
	/// States
	/////////////////////////////////////////////


	//update the turn counter
	protected class StartOfTurn : FSM<TurnManager>.State {

		public override void OnEnter (){
			Context.NewTurn();
			Services.Events.Fire(new NewTurnEvent());
		}

		public override void Tick (){
			TransitionTo<AttackersAdvance>();
		}
	}


	/// <summary>
	/// State for the attackers moving south at the start of each turn.
	/// </summary>
	public class AttackersAdvance : FSM<TurnManager>.State {
		float timer;

		//tell the attacker manager to move the attackers.
		//this is routed through the attacker manager to avoid spreading control over the attackers over multiple classes.
		public override void OnEnter(){
			timer = 0.0f;
			Services.Attackers.SpawnNewAttackers(); //when the wave is done, don't spawn more attackers
			Services.Attackers.PrepareAttackerMove();
			Services.Attackers.MoveAttackers();
			Context.phaseText.text = ATTACKER_MOVE;
			Context.TurnRulebookPage();
		}


		//wait while the attackers move
		public override void Tick(){
			timer += Time.deltaTime;
			if (timer >= Context.attackerAdvanceDuration){

				//go to the Defenders Move phase, unless the player has now lost.
				if (!Context.CheckForLoss()) TransitionTo<PlayerMove>();
				else TransitionTo<PlayerLose>();
			}
		}


		public override void OnExit (){
			Services.Events.Fire(new EndPhaseEvent(Context.TurnMachine.CurrentState));
		}
	}

	/// <summary>
	/// State for the defenders' movement. This is public so that the momentum system can determine whether momentum has been used up.
	/// </summary>
	public class PlayerMove : FSM<TurnManager>.State {


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

			//go on to the Defenders Fight phase if all defenders have moved, or if the player clicks again after getting a warning that they
			//haven't
			if (Services.Defenders.CheckAllDoneMoving()) TransitionTo<PlayerFight>();
			else if (Context.imSure) TransitionTo<PlayerFight>();
			else {
				Context.imSure = true;
				Services.UI.SetExtraText(ARE_YOU_SURE_MSG);
			}
		}


		public override void OnEnter(){
			Services.Defenders.PrepareDefenderMovePhase();
			Context.phaseText.text = PLAYER_MOVE;
			Context.TurnRulebookPage();
			Services.Events.Register<InputEvent>(HandleMoveInputs);
			Services.Events.Register<EndPhaseEvent>(HandlePhaseEndInput);
			Context.phaseButtonText.text = STOP_MOVING_MSG;
			Context.ToggleNextPhaseButton();
			Services.UI.ToggleUndoButton();
			Services.Undo.PrepareToUndoMoves();
			Context.imSure = false;
		}


		public override void OnExit(){
			Services.Defenders.CompleteMovePhase();
			Services.Events.Unregister<InputEvent>(HandleMoveInputs);
			Services.Events.Unregister<EndPhaseEvent>(HandlePhaseEndInput);
			Context.ToggleNextPhaseButton();
			Services.UI.ToggleUndoButton();
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
			Context.phaseButtonText.text = STOP_FIGHTING_MSG;
			Context.ToggleNextPhaseButton();
		}


		public override void OnExit(){
			Services.Events.Unregister<InputEvent>(HandleFightInputs);
			Services.Events.Unregister<EndPhaseEvent>(HandlePhaseEndInput);
			Services.Defenders.CompleteFightPhase();
			Context.ToggleNextPhaseButton();
		}
	}


	protected class BesiegeWalls : FSM<TurnManager>.State {
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
	protected class EndPhase : FSM<TurnManager>.State {


		public override void OnEnter(){
			Services.Events.Fire(new EndPhaseEvent(Context.TurnMachine.CurrentState));
		}


		public override void Tick (){
			if(CheckStartNextTurn()) TransitionTo<StartOfTurn>();
			else TransitionTo<BetweenWaves>();
		}


		/// <summary>
		/// Don't go on to the start of the next turn if this is the last turn of the wave.
		/// </summary>
		private bool CheckStartNextTurn(){
			if (Context.CurrentTurn < Context.TotalTurns) return true;
			return false;
		}
	}


	/// <summary>
	/// When a wave is done:
	/// 	1. have the attacker manager go to the next wave,
	/// 	2. have the turn manager work out the new number of turns, and
	/// 	3. reset the visual combat deck
	/// </summary>
	protected class BetweenWaves : FSM<TurnManager>.State {


		public override void Tick (){
			Services.Attackers.RemoveAllAttackers();
			if (Services.Attackers.GoToNextWave()) {
				Services.AttackDeck.Reshuffle();
				Context.ResetTurnUI();
				Services.UI.RecreateCombatDeck();
				TransitionTo<StartOfTurn>();
			}
			else TransitionTo<PlayerWin>();
		}
	}


	/// <summary>
	/// The game enters this state when the player wins.
	/// 
	/// Right now there's no way out of this state! The player should reset the game.
	/// </summary>
	protected class PlayerWin : FSM<TurnManager>.State {


		public override void OnEnter (){
			Context.PlayerWinFeedback();
		}
	}


	/// <summary>
	/// The game enters this state when the player loses.
	/// 
	/// Right now there's no way out of this state! The player should reset the game.
	/// </summary>
	protected class PlayerLose : FSM<TurnManager>.State {


		public override void OnEnter (){
			Context.PlayerLoseFeedback();
		}
	}
}
