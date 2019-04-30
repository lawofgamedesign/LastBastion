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
	protected const float BASE_ADVANCE_DURATION = 1.0f; //minimum duration of the advance phase
	protected const float INCREMENTAL_ADV_DURATION = 0.1f; //added each time an attacker moves


	//tags for selecting and clicking on things
	protected const string DEFENDER_TAG = "Defender";
	protected const string BOARD_TAG = "Board";
	protected const string ATTACKER_TAG = "Attacker";
	protected const string LEADER_TAG = "Leader";
	protected const string MINION_TAG = "Minion";


	//button for ending the current phase and moving to the next
	protected GameObject nextPhaseButton;
	protected Text phaseButtonText;
	protected const string TEXT_OBJ = "Text";
	protected const string NEXT_BUTTON_OBJ = "Next phase button";
	protected const string STOP_MOVING_MSG = "Done moving";
	protected const string STOP_FIGHTING_MSG = "Done fighting";
	protected const string ARE_YOU_SURE_MSG = "Are you sure? Not all defenders moved.";
	protected const string WAIT_MSG = "You need to make a choice before we go on.";
	protected bool imSure = true; //used to determine whether a player is sure that they're ready to go on when there are still defenders who can move


	//what turn is it?
	public int CurrentTurn { get; private set; }
	public int TotalTurns { get; private set; }


	//feedback for when the player loses
	private const string LOSE_MSG = "Good game. This scenario is tough for the defenders.";


	//feedback for when the player wins
	private const string WIN_MSG = "Wow! I can't believe you made it.";


	//help text, and related variables
	protected const string MOVE_REMINDER_MSG = "Remember, click a defender, then click the path you want them to take (no diagonals!).";
	protected const string FIGHT_REMINDER_MSG = "Click a defender, choose one of their cards, and then click a skeleton in front of them to attack.";



	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables
	public virtual void Setup(){
		turnMachine = new FSM<TurnManager>(this);
		TurnMachine = turnMachine;
		ResetTurnUI();
		Services.UI.SetWaveText(Services.Attackers.GetCurrentWave() + 1, Services.Attackers.GetTotalWaves()); //+1 because waves are zero-indexed
		turnMachine.TransitionTo<StartOfTurn>();
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
		Services.UI.OpponentStatement(WIN_MSG);
	}


	/// <summary>
	/// Handles informing the player that they've lost.
	/// </summary>
	protected void PlayerLoseFeedback(){
		Services.UI.OpponentStatement(LOSE_MSG);
	}


	public void IncreaseAdvanceDuration(){
		attackerAdvanceDuration += INCREMENTAL_ADV_DURATION;
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
			TransitionTo<PlayerUpgrade>();
		}
	}


	/// <summary>
	/// Upgrading occurs here.
	/// </summary>
	public class PlayerUpgrade : FSM<TurnManager>.State {


		List<DefenderSandbox> upgraders = new List<DefenderSandbox>();


		private List<DefenderSandbox> GetUpgradeableDefenders(){
			List<DefenderSandbox> upgradeables = new List<DefenderSandbox>();

			foreach (DefenderSandbox defender in Services.Defenders.GetAllDefenders()){
				if (defender.ReadyToUpgrade()) upgradeables.Add(defender);
			}

			Debug.Assert(upgradeables.Count <= Services.Defenders.GetAllDefenders().Count &&
						 upgradeables.Count >= 0, "Impossible number of defenders ready to upgrade: " + upgradeables.Count);

			return upgradeables;
		}


		/// <summary>
		/// Remove an upgrading defender from the list of defenders who need to upgrade. If that was the last defender who needed to upgrade,
		/// move on.
		/// </summary>
		/// <param name="defender">The defender who upgraded.</param>
		public void CheckForAllFinished(DefenderSandbox defender){
			Debug.Assert(upgraders.Contains(defender), "Trying to remove a defender who doesn't need to upgrade.");

			upgraders.Remove(defender);

			if (upgraders.Count <= 0) {
				//unregister here rather than in OnExit because there may not have been a registration if no defender needed to upgrade
				Services.Events.Unregister<PowerChoiceEvent>(HandlePowerChoice);
				Services.Defenders.NoSelectedDefender();
			}
		}


		/// <summary>
		/// When the player chooses a power, see if the player is done upgrading and it's time to move on to the next phase.
		/// </summary>
		/// <param name="e">A PowerChoiceEvent sent out by the character sheet.</param>
		private void HandlePowerChoice(Event e){
			Debug.Assert(e.GetType() == typeof(PowerChoiceEvent), "Non-PowerChoiceEvent in HandlePowerChoice");

			PowerChoiceEvent powerEvent = e as PowerChoiceEvent;

			CheckForAllFinished(powerEvent.defender);
		}


		/// <summary>
		/// See if any defenders need to upgrade. If not, skip this phase.
		/// 
		/// If so, set up tasks that will have the player choose an upgrade for each defender who needs to upgrade. Listen
		/// for those choices, so that the system will move on when the player has made them all.
		/// </summary>
		public override void OnEnter (){
			upgraders = GetUpgradeableDefenders();

			if (upgraders.Count == 0){
				//do nothing; on the first frame, transition to AttackersAdvance
			} else {
				Services.UI.RemindPhase(Context.TurnMachine.CurrentState);
				Services.Events.Register<PowerChoiceEvent>(HandlePowerChoice);

				foreach (DefenderSandbox defender in upgraders){

					//if this is the first defender who needs to upgrade, add an appropriate task
					if (!Services.Tasks.CheckForTaskOfType<UpgradeTask>()) {
						Services.Tasks.AddTask(new UpgradeTask(defender));
					}


					//this is the second or third defender who needs to upgrade. If it's the third, GetLastTaskOfType() will still return null;
					//delay the third defender's UpgradeTask until the second's can be found
					else if (Services.Tasks.GetLastTaskOfType<UpgradeTask>() == null){
						Services.Tasks.AddTask(new DelayedUpgradeTask(defender));
					}


					//this is the second defender who needs to upgrade; add an appropriate task
					else {
						Services.Tasks.GetLastTaskOfType<UpgradeTask>().Then(new UpgradeTask(defender));
					}
				}
			}
		}


		public override void OnExit (){
			base.OnExit ();
		}

		public override void Tick(){
			//only go on if no one needs to upgrade and there are no tasks running (i.e., wait for tankards to be dropped, etc.)
			if (upgraders.Count == 0 && !Services.Tasks.CheckForAnyTasks()) TransitionTo<AttackersAdvance>();
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
			Context.attackerAdvanceDuration = TurnManager.BASE_ADVANCE_DURATION;
			Services.Attackers.SpawnNewAttackers(); //when the wave is done, don't spawn more attackers
			Services.Attackers.PrepareAttackerMove();
			Services.Attackers.MoveAttackers();
			Services.UI.RemindPhase(Context.TurnMachine.CurrentState);
		}


		//wait while the attackers move
		public override void Tick(){
			timer += Time.deltaTime;
			if (timer >= Context.attackerAdvanceDuration){
				if (Services.Tasks.CheckForAnyTasks()) return;
				//if the defenders are being pushed, don't transition to the next phase until that happens.
				//if (Services.Tasks.CheckForTaskOfType<MoveDefenderTask>()) return;

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
	public class PlayerMove : FSM<TurnManager>.State
	{
		private bool needWait = false;

		private void HandleMoveInputs(Event e){
			InputEvent inputEvent = e as InputEvent;

			//don't double-count inputs that are intended to hide the character sheet
			if (Services.UI.GetCharSheetStatus() == CharacterSheetBehavior.SheetStatus.Displayed) return;

			if (inputEvent.selected.tag == DEFENDER_TAG){
				Services.Defenders.SelectDefenderForMovement(inputEvent.selected.GetComponent<DefenderSandbox>());
			//clicking directly on the board to move is disabled, since movement is now handled by draggint the mini using DefenderMoveTask
//			} else if (inputEvent.selected.tag == BOARD_TAG){
//				if (Services.Defenders.IsAnyoneSelected()){
//					Services.Defenders.GetSelectedDefender().TryPlanMove(inputEvent.selected.GetComponent<SpaceBehavior>().GridLocation);
//				}
			} else if (inputEvent.selected.tag == ATTACKER_TAG || inputEvent.selected.tag == MINION_TAG || inputEvent.selected.tag == LEADER_TAG){
				Services.UI.OpponentStatement(inputEvent.selected.GetComponent<AttackerSandbox>().GetUIInfo());
			}
		}


		private void HandlePhaseEndInput(Event e){
			Debug.Assert(e.GetType() == typeof(EndPhaseEvent), "Non-EndPhaseEvent in HandlePhaseEndInput");

			//go on to the Defenders Fight phase if all defenders have moved, or if the player clicks again after getting a warning that they
			//haven't
			//go to the special Waiting state if the Guardian needs to make a choice for the Hold the Line track
			if (Services.Defenders.CheckAllDoneMoving() && !needWait){
				TransitionTo<PlayerFight>();
			}
			else if (Context.imSure && !needWait) TransitionTo<PlayerFight>();
			else if (!Context.imSure && !Services.Defenders.CheckAllDoneMoving()){
				Context.imSure = true;
				Services.UI.OpponentStatement(ARE_YOU_SURE_MSG);
			}
			else if (needWait){
				Services.UI.OpponentStatement(WAIT_MSG);
				TransitionTo<Waiting>();
			}
		}


		/// <summary>
		/// Is there a need to wait before going on to PlayerFight? There is if:
		/// 
		/// 1. The Guardian needs to select a column for the Hold the Line track.
		///
		/// IMPORTANT: this assumes that there is only ever going to be one reason to wait, so there will never be
		/// conflicting needs (i.e., one defender still needs the system to wait while another is saying to go ahead).
		/// If conflicting needs are possible, this will need to be refactored to take that into account. 
		/// </summary>
		private void CheckNeedWait(global::Event e){
			Debug.Assert(e.GetType() == typeof(NeedWaitEvent), "Non-NeedWaitEvent in CheckNeedWait");
			
			NeedWaitEvent waitEvent = e as NeedWaitEvent;

			needWait = waitEvent.needWait;
		}
		


		public override void OnEnter(){
			Services.Defenders.PrepareDefenderMovePhase();
			Services.Events.Register<InputEvent>(HandleMoveInputs);
			Services.Events.Register<EndPhaseEvent>(HandlePhaseEndInput);
			Services.Events.Register<NeedWaitEvent>(CheckNeedWait);
			Services.Events.Fire(new PhaseStartEvent(Context.TurnMachine.CurrentState));
			Services.UI.RemindPhase(Context.TurnMachine.CurrentState);
			Services.UI.ToggleExplainButton(ChatUI.OnOrOff.On);
			Services.UI.ToggleTutorialButton(ChatUI.OnOrOff.On);
			Services.Undo.PrepareToUndoMoves();
			Context.imSure = false;
			//display help text for the first move phase
			//this has been deactivated with the new tutorial system
			/*if (Services.Attackers.GetCurrentWave() == 0 &&
				Context.CurrentTurn == 1){

				Services.UI.SetTutorialText(MOVE_REMINDER_MSG);
				Services.UI.ToggleTutorialText(ChatUI.OnOrOff.On);
			}*/
		}


		public override void OnExit(){
			Services.Defenders.CompleteMovePhase();
			Services.Events.Unregister<InputEvent>(HandleMoveInputs);
			Services.Events.Unregister<EndPhaseEvent>(HandlePhaseEndInput);
			Services.Events.Unregister<NeedWaitEvent>(CheckNeedWait);
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
				Services.UI.OpponentStatement(inputEvent.selected.GetComponent<AttackerSandbox>().GetUIInfo());
			} else if (inputEvent.selected.tag == BOARD_TAG){
				SpaceBehavior space = inputEvent.selected.GetComponent<SpaceBehavior>();

				//if the player clicked the board when it seems like they must be trying to fight an enemy there, let them do so
				if (Services.Board.GeneralSpaceQuery(space.GridLocation.x, space.GridLocation.z) == SpaceBehavior.ContentType.Attacker){
					if (Services.Defenders.IsAnyoneSelected() &&
						Services.Defenders.GetSelectedDefender().GetChosenCardValue() != DefenderSandbox.NO_CARD_SELECTED){
						Services.Defenders.GetSelectedDefender().TryFight(Services.Board.GetThingInSpace(space.GridLocation.x,
																										 space.GridLocation.z).GetComponent<AttackerSandbox>());
					}
				} else {
					Services.Events.Fire(new BoardClickedEvent(space.GridLocation));
				}
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

			//if the Ranger had the opportunity to teleport via The Last Chance, the opportunity ends 
			if (Services.Tasks.CheckForTaskOfType<TeleportDefenderTask>()){
				Services.Tasks.GetCurrentTaskOfType<TeleportDefenderTask>().SetStatusExternally(Task.TaskStatus.Aborted);
			}
		}


		public override void OnEnter(){
			Services.Defenders.PrepareDefenderFightPhase();
			Services.UI.RemindPhase(Context.TurnMachine.CurrentState);
			Services.Events.Register<InputEvent>(HandleFightInputs);
			Services.Events.Register<EndPhaseEvent>(HandlePhaseEndInput);
			Services.Events.Fire(new PhaseStartEvent(Context.TurnMachine.CurrentState));
			//display help text for the first fight phase
			//this has been deactivated in light of the new tutorial system
			/*if (Services.Attackers.GetCurrentWave() == 0 &&
				Context.CurrentTurn == 1) Services.UI.SetTutorialText(FIGHT_REMINDER_MSG);*/
		}


		public override void OnExit(){
			Services.Events.Unregister<InputEvent>(HandleFightInputs);
			Services.Events.Unregister<EndPhaseEvent>(HandlePhaseEndInput);
			Services.Defenders.CompleteFightPhase();
			Services.UI.ToggleExplainButton(ChatUI.OnOrOff.Off);
			Services.UI.ToggleTutorialButton(ChatUI.OnOrOff.Off);
			/*if (Services.Attackers.GetCurrentWave() == 0 &&
				Context.CurrentTurn == 1) Services.UI.ToggleTutorialText(ChatUI.OnOrOff.Off);*/
		}
	}


	public class BesiegeWalls : FSM<TurnManager>.State {
		float timer;
		List<AttackerSandbox> besiegers;

		//how long the system waits between resolving besieging attacks
		private float besiegeWallDuration = 0.75f;


		//are any enemies besieging the wall? Get a list of them
		public override void OnEnter (){
			Services.Events.Fire(new PhaseStartEvent(Context.TurnMachine.CurrentState));
			Services.UI.RemindPhase(Context.TurnMachine.CurrentState);
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
				Services.UI.SetWaveText(Services.Attackers.GetCurrentWave() + 1, Services.Attackers.GetTotalWaves()); //+1 for the wave after this one, zero-indexed
				Services.Environment.ChangeEnvironment(Services.Environment.GetNextPlace());
				TransitionTo<StartOfTurn>();
			}
			else TransitionTo<PlayerWin>();
		}
	}


	/// <summary>
	/// The game enters this state when the player wins.
	/// </summary>
	public class PlayerWin : FSM<TurnManager>.State {


		private float timer = 0.0f;
		private float resetWait = 2.0f;


		public override void OnEnter (){
			Services.Environment.ChangeEnvironment(EnvironmentManager.Place.Kitchen);
			timer = 0.0f;
			Services.EscapeMenu.Cleanup();
			Services.EscapeMenu = new EndEscMenuBehavior();
			Services.EscapeMenu.Setup();
			Context.PlayerWinFeedback();
		}


		public override void Tick (){
			timer += Time.deltaTime;

			if (timer >= resetWait){
				Services.Events.Fire(new PauseEvent(PauseEvent.Pause.Pause));
				Services.Events.Fire(new EscMenuEvent());
			}
		}
	}


	/// <summary>
	/// The game enters this state when the player loses.
	/// </summary>
	public class PlayerLose : FSM<TurnManager>.State {


		private float timer = 0.0f;
		private float resetWait = 2.0f;


		public override void OnEnter (){
			Services.Environment.ChangeEnvironment(EnvironmentManager.Place.Kitchen);
			timer = 0.0f;
			Services.EscapeMenu.Cleanup();
			Services.EscapeMenu = new EndEscMenuBehavior();
			Services.EscapeMenu.Setup();
			Context.PlayerLoseFeedback();
		}


		public override void Tick (){
			timer += Time.deltaTime;

			if (timer >= resetWait){
				Services.Events.Fire(new PauseEvent(PauseEvent.Pause.Pause));
				Services.Events.Fire(new EscMenuEvent());
			}
		}
	}


	/// <summary>
	/// This is a special class used when something needs to happen before going to the next state.
	///
	/// For example, the game enters this state when waiting for the Guardian to choose a column to block
	/// in the Hold the Line track.
	/// </summary>
	public class Waiting : FSM<TurnManager>.State {
		
		
		/// <summary>
		/// Listen for the reasons the TurnManager could be waiting.
		/// </summary>
		public override void OnEnter (){
			Services.Events.Register<BlockColumnEvent>(GoToFight);
		}


		/// <summary>
		/// Go to the PlayerFight phase.
		/// </summary>
		/// <param name="e">Any event that signals the reason to wait before the PlayerFight phase is over.</param>
		private void GoToFight(global::Event e){
			TransitionTo<PlayerFight>();
		}

		
		/// <summary>
		/// Always unregister from all reasons to wait.
		/// </summary>
		public override void OnExit(){
			Services.Events.Unregister<BlockColumnEvent>(GoToFight);
		}
	}
}
