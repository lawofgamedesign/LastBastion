namespace Tutorial
{
	using UnityEngine;
	using UnityEngine.UI;

	public class TutorialTurnManager : TurnManager {


		/////////////////////////////////////////////
		/// Fields
		/////////////////////////////////////////////


		//the specialized state machine for the tutorial
		private FSM<TutorialTurnManager> tutMachine;
		public FSM<TutorialTurnManager> TutMachine { get { return tutMachine; } protected set { tutMachine = value; } }


		//the special UI for the tutorial
		private Text tutText;
		private Text nextText;
		private GameObject advanceButton;
		private const string TUTORIAL_TEXT_OBJ = "Tutorial text";
		private const string ADVANCE_BUTTON_OBJ = "Advance button";


		/////////////////////////////////////////////
		/// Functions
		/////////////////////////////////////////////


		//initialize variables
		public override void Setup(){
			tutMachine = new FSM<TutorialTurnManager>(this);
			TutMachine = tutMachine;
			ResetTurnUI();
			phaseText = GameObject.Find(PHASE_OBJ).GetComponent<Text>();
			nextPhaseButton = GameObject.Find(NEXT_BUTTON_OBJ);
			phaseButtonText = nextPhaseButton.transform.Find(TEXT_OBJ).GetComponent<Text>();
			ToggleNextPhaseButton();
			tutText = GameObject.Find(TUTORIAL_TEXT_OBJ).GetComponent<Text>();
			advanceButton = GameObject.Find(ADVANCE_BUTTON_OBJ);
			nextText = advanceButton.transform.Find(TEXT_OBJ).GetComponent<Text>();
			tutMachine.TransitionTo<StartOfTutorial>();
		}


		//go through one loop of the current state
		public override void Tick(){
			tutMachine.Tick();
		}


		public void SetTutorialText(string info){
			tutText.text = info;
		}


		public string GetTutorialText(){
			return tutText.text;
		}


		public void SetButtonText(string info){
			nextText.text = info;
		}


		public void ToggleAdvanceButton(){
			advanceButton.SetActive(!advanceButton.activeInHierarchy);
		}


		/////////////////////////////////////////////
		/// States
		/////////////////////////////////////////////


		//update the turn counter
		private class StartOfTutorial : FSM<TutorialTurnManager>.State {


			private const string WELCOME_MSG = "OK, we're all set up.";
			private const string HOW_MSG = "How do we play?";
			private const string THEME_MSG = "I'm the evil Necromancer. I try to get my horde to the bottom row of tiles.";
			private const string NEVER_MSG = "Never!";
			private const string HORDE_MSG = "My skeletal horde is endless. You have to hold out until daybreak, which dispels my necromancy.";
			private const string NONE_MSG = "None shall pass!";
			private const string WAVES_MSG = "My horde attacks in waves. Each wave lasts a certain number of turns.";
			private const string UNDERSTAND_MSG = "I understand";
			private const string ONE_THREE_MSG = "We start with the first wave. It will last three turns.";
			private const string END_OF_WAVE_MSG = "You win by surviving through the last turn of the third wave. That's when daybreak happens.";
			private const string VALIANT_MSG = "Great!";
			private const string ADVANCE_MSG = "Each turn begins with my horde advancing.";
			private const string DASTARDLY_MSG = "Stop that!";


			private void OnButtonClick(global::Event e){
				switch (Context.GetTutorialText()){
					case WELCOME_MSG:
						Context.SetTutorialText(THEME_MSG);
						Context.SetButtonText(NEVER_MSG);
						break;
					case THEME_MSG:
						Context.SetTutorialText(HORDE_MSG);
						Context.SetButtonText(NONE_MSG);
						break;
					case HORDE_MSG:
						Context.SetTutorialText(WAVES_MSG);
						Context.SetButtonText(UNDERSTAND_MSG);
						break;
					case WAVES_MSG:
						Context.SetTutorialText(ONE_THREE_MSG);
						break;
					case ONE_THREE_MSG:
						Context.SetTutorialText(END_OF_WAVE_MSG);
						Context.SetButtonText(VALIANT_MSG);
						break;
					case END_OF_WAVE_MSG:
						Context.SetTutorialText(ADVANCE_MSG);
						Context.SetButtonText(DASTARDLY_MSG);
						break;
					case ADVANCE_MSG:
						TransitionTo<AttackersAdvance>();
						break;
				}
			}


			public override void OnEnter (){
				Context.NewTurn();
				Context.SetTutorialText(WELCOME_MSG);
				Context.SetButtonText(HOW_MSG);
				Services.Events.Register<TutorialClick>(OnButtonClick);
			}


			public override void OnExit (){
				Services.Events.Unregister<TutorialClick>(OnButtonClick);
				Context.ToggleAdvanceButton();
			}
		}


		/// <summary>
		/// State for the attackers moving south at the start of each turn.
		/// </summary>
		protected class AttackersAdvance : FSM<TutorialTurnManager>.State {
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

					//go to the Defenders Move phase
					TransitionTo<PlayerMove>();
				}
			}
		}


		/// <summary>
		/// State for the defenders' movement. This is public so that the momentum system can determine whether momentum has been used up.
		/// </summary>
		public class PlayerMove : FSM<TutorialTurnManager>.State {


			private const string YOUR_TURN_MSG = "After my horde advances, it's your turn to move your defenders.";
			private const string OK_MSG = "Got it.";
			private const string SELECT_MSG = "Plan your move by clicking the spaces you want to move to. Click your defender, in the center of the board.";
			private const string PLAN_MSG = "Now choose your move by clicking each highlighted space.";
			private const string DONE_MSG = "Right, now lock in your move with the \"Done\" button.";
			private const string THERE_MSG = "I'm there.";
			private const string GO_ON_MSG = "When you've moved your defender, hit \"Done moving\" in the upper-right to fight.";
			private TwoDLoc[] requiredMoves = new TwoDLoc[2] { new TwoDLoc(3, 2), new TwoDLoc(3, 3) };
			private int moveIndex = 0;
			private Transform moveHighlight;
			private const string HIGHLIGHT_OBJ = "Board highlight";
			private Vector3 vertOffset = new Vector3(0.0f, 0.1f, 0.0f);
			private DefenderSandbox defender; //assumes there is only one defender in the tutorial


			private void OnButtonClick(global::Event e){
				switch (Context.GetTutorialText()){
					case YOUR_TURN_MSG:
						Context.SetTutorialText(SELECT_MSG);
						Context.ToggleAdvanceButton();
						break;
					case DONE_MSG:
						if (defender.ReportGridLoc().x == requiredMoves[moveIndex].x && defender.ReportGridLoc().z == requiredMoves[moveIndex].z){
							Context.SetTutorialText(GO_ON_MSG);
							Context.ToggleAdvanceButton();
						}
						break;
				}
			}


			private void HandleMoveInputs(global::Event e){
				InputEvent inputEvent = e as InputEvent;

				if (Context.GetTutorialText() == SELECT_MSG){
					if (inputEvent.selected.tag == DEFENDER_TAG){
						Services.Defenders.SelectDefenderForMovement(inputEvent.selected.GetComponent<DefenderSandbox>());
						Context.SetTutorialText(PLAN_MSG);
						moveHighlight.position = Services.Board.GetWorldLocation(requiredMoves[moveIndex].x, requiredMoves[moveIndex].z) + 
												 vertOffset;
					}
				} else if (Context.GetTutorialText() == PLAN_MSG){
					if (inputEvent.selected.tag == BOARD_TAG){
						SpaceBehavior space = inputEvent.selected.GetComponent<SpaceBehavior>();

						if (space.GridLocation.x == requiredMoves[moveIndex].x && space.GridLocation.z == requiredMoves[moveIndex].z){
							Services.Defenders.GetSelectedDefender().TryPlanMove(inputEvent.selected.GetComponent<SpaceBehavior>().GridLocation);

							if (moveIndex < requiredMoves.Length - 1){
								moveIndex++;
								moveHighlight.position = Services.Board.GetWorldLocation(requiredMoves[moveIndex].x, requiredMoves[moveIndex].z) +
														 vertOffset;
							}
							else {
								Context.SetTutorialText(DONE_MSG);
								Context.SetButtonText(THERE_MSG);
								Context.ToggleAdvanceButton();
								moveHighlight.position = new Vector3(-100.0f, -100.0f, 0.0f); //clearly out of view
							}
						}
					}
				}
			}


			private void HandlePhaseEndInput(global::Event e){
				Debug.Assert(e.GetType() == typeof(EndPhaseEvent), "Non-EndPhaseEvent in HandlePhaseEndInput");

				if (Context.GetTutorialText() == GO_ON_MSG) TransitionTo<PlayerFight>();
			}


			public override void OnEnter(){
				Services.Defenders.PrepareDefenderMovePhase();
				Context.phaseText.text = PLAYER_MOVE;
				Context.TurnRulebookPage();
				Services.Events.Register<InputEvent>(HandleMoveInputs);
				Services.Events.Register<EndPhaseEvent>(HandlePhaseEndInput);
				Context.phaseButtonText.text = STOP_MOVING_MSG;
				Context.ToggleNextPhaseButton();
				Context.imSure = false;
				Services.Events.Register<TutorialClick>(OnButtonClick);
				Context.SetTutorialText(YOUR_TURN_MSG);
				Context.SetButtonText(OK_MSG);
				Context.ToggleAdvanceButton();
				moveHighlight = GameObject.Find(HIGHLIGHT_OBJ).transform;
				defender = GameObject.FindGameObjectWithTag(DEFENDER_TAG).GetComponent<DefenderSandbox>();
			}


			public override void OnExit(){
				Services.Defenders.CompleteMovePhase();
				Services.Events.Unregister<InputEvent>(HandleMoveInputs);
				Services.Events.Unregister<EndPhaseEvent>(HandlePhaseEndInput);
				Context.ToggleNextPhaseButton();
				Services.Events.Unregister<TutorialClick>(OnButtonClick);
			}
		}


		/// <summary>
		/// State for the defenders' combat. This is public so that the Ranger can determine whether it's the Defenders Fight phase, and display--or not display--
		/// the Showboating UI accordingly.
		/// </summary>
		public class PlayerFight : FSM<TutorialTurnManager>.State {


			private const string FACING_MSG = "Normally your defenders can only fight someone directly ahead of them.";
			private const string OK_MSG = "In front, OK.";
			private const string CARD_MSG = "When we fight we each play a card and add any modifier our characters have. High value wins.";
			private const string CARD_WHERE_MSG = "What cards?";
			private const string ATK_CARD_MSG = "You can see my cards in the upper-left. It shows what cards I have in my deck, and what I've played.";
			private const string SEE_ATK_MSG = "I see the list.";
			private const string DEF_CARD_MSG = "Click on your defender to see your cards.";
			private const string HOW_FIGHT_MSG = "Choose a card, then click the skeleton in front of your defender.";
			private const string RESULT_MSG = "If your total is higher, the skeleton is removed. If not, my horde gains momentum.";
			private const string MOMENTUM_WHAT_MSG = "Momentum?";
			private const string MOMENTUM_MEAN_MSG = "Each momentum makes my horde advance one space further next turn.";
			private const string CAREFUL_MSG = "Dangerous!";
			private const string CARD_RULES_MSG = "A defender has to use all their cards once before they can use any again.";
			private const string RESET_OK_MSG = "All three, OK.";
			private const string DONE_FIGHT_MSG = "Click \"Done fighting\" in the upper-right when you're finished.";


			private void OnButtonClick(global::Event e){
				Debug.Assert(e.GetType() == typeof(TutorialClick), "Non-TutorialClick event in PlayerFight's OnButtonClick");

				switch (Context.GetTutorialText()){
					case FACING_MSG:
						Context.SetTutorialText(CARD_MSG);
						Context.SetButtonText(CARD_WHERE_MSG);
						break;
					case CARD_MSG:
						Context.SetTutorialText(ATK_CARD_MSG);
						Context.SetButtonText(SEE_ATK_MSG);
						break;
					case ATK_CARD_MSG:
						Context.SetTutorialText(DEF_CARD_MSG);
						Context.ToggleAdvanceButton();
						break;
					case RESULT_MSG:
						Context.SetTutorialText(MOMENTUM_MEAN_MSG);
						Context.SetButtonText(CAREFUL_MSG);
						break;
					case MOMENTUM_MEAN_MSG:
						Context.SetTutorialText(CARD_RULES_MSG);
						Context.SetButtonText(RESET_OK_MSG);
						break;
					case CARD_RULES_MSG:
						Context.SetTutorialText(DONE_FIGHT_MSG);
						Context.ToggleAdvanceButton();
						break;
				}
			}


			private void HandleFightInputs(global::Event e){
				InputEvent inputEvent = e as InputEvent;

				if (inputEvent.selected.tag == DEFENDER_TAG &&
					Context.GetTutorialText() == DEF_CARD_MSG){
					Services.Defenders.SelectDefenderForFight(inputEvent.selected.GetComponent<DefenderSandbox>());
					Context.SetTutorialText(HOW_FIGHT_MSG);
				} else if ((inputEvent.selected.tag == ATTACKER_TAG || inputEvent.selected.tag == MINION_TAG || inputEvent.selected.tag == LEADER_TAG) &&
					Services.Defenders.IsAnyoneSelected() &&
					Services.Defenders.GetSelectedDefender().GetChosenCardValue() != DefenderSandbox.NO_CARD_SELECTED &&
					Context.GetTutorialText() == HOW_FIGHT_MSG){

					Services.Defenders.GetSelectedDefender().TryFight(inputEvent.selected.GetComponent<AttackerSandbox>());
					Context.SetTutorialText(RESULT_MSG);
					Context.SetButtonText(MOMENTUM_WHAT_MSG);
					Context.ToggleAdvanceButton();
				} else if (inputEvent.selected.tag == ATTACKER_TAG || inputEvent.selected.tag == MINION_TAG || inputEvent.selected.tag == LEADER_TAG) {
					Services.UI.SetExtraText(inputEvent.selected.GetComponent<AttackerSandbox>().GetUIInfo());
				}
			}


			/// <summary>
			/// End the phase, but only if the cards aren't moving. This is a bodge to protect the Brawler, who otherwise might fail to
			/// unregister for InputEvents if they return from their DoneFighting() early.
			/// </summary>
			/// <param name="e">E.</param>
			private void HandlePhaseEndInput(global::Event e){
				Debug.Assert(e.GetType() == typeof(EndPhaseEvent), "Non-EndPhaseEvent in HandlePhaseEndInput");

				if (Context.GetTutorialText() == DONE_FIGHT_MSG) TransitionTo<PowerUp>();
			}


			public override void OnEnter(){
				Services.Defenders.PrepareDefenderFightPhase();
				Context.phaseText.text = PLAYER_FIGHT;
				Context.TurnRulebookPage();
				Services.Events.Register<InputEvent>(HandleFightInputs);
				Services.Events.Register<EndPhaseEvent>(HandlePhaseEndInput);
				Context.phaseButtonText.text = STOP_FIGHTING_MSG;
				Context.ToggleNextPhaseButton();
				Context.SetTutorialText(FACING_MSG);
				Context.SetButtonText(OK_MSG);
				Context.ToggleAdvanceButton();
				Services.Events.Register<TutorialClick>(OnButtonClick);
			}


			public override void OnExit(){
				Services.Events.Unregister<InputEvent>(HandleFightInputs);
				Services.Events.Unregister<EndPhaseEvent>(HandlePhaseEndInput);
				Services.Defenders.CompleteFightPhase();
				Context.ToggleNextPhaseButton();
				Services.Events.Unregister<TutorialClick>(OnButtonClick);
			}
		}


		/// <summary>
		/// State for demonstrating powering up.
		/// </summary>
		public class PowerUp : FSM<TutorialTurnManager>.State {

		}
	}
}
