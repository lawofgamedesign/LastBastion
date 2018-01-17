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
		public FSM<TutorialTurnManager> TutMachine { 
			get { return tutMachine; } 
			protected set { tutMachine = value; }
		}


		/////////////////////////////////////////////
		/// Functions
		/////////////////////////////////////////////


		//initialize variables
		public override void Setup(){
			ResetTurnUI();
			tutMachine = new FSM<TutorialTurnManager>(this);
			TutMachine = tutMachine;
			tutMachine.TransitionTo<StartOfTutorial>();
		}


		//go through one loop of the current state
		public override void Tick(){
			tutMachine.Tick();
		}
			


		/////////////////////////////////////////////
		/// States
		/////////////////////////////////////////////


		//update the turn counter
		public class StartOfTutorial : FSM<TutorialTurnManager>.State {


			private const string WELCOME_MSG = "OK, we're all set up.";
			private const string HOW_MSG = "How do we play?";
			private const string THEME_MSG = "I'm the evil Necromancer. I try to get my horde to the bottom row of tiles.";
			private const string NEVER_MSG = "Never!";
			private const string HORDE_MSG = "My skeletal horde is endless. You have to hold out until daybreak, which dispels my necromancy.";
			private const string NONE_MSG = "None shall pass!";
			private const string WAVES_MSG = "My horde attacks in waves. Each wave lasts a certain number of turns.";
			private const string UNDERSTAND_MSG = "I understand.";
			private const string ONE_THREE_MSG = "We start with the first wave. It lasts three turns.";
			private const string END_OF_WAVE_MSG = "You win by surviving through the last turn of the third wave. That's when daybreak happens.";
			private const string VALIANT_MSG = "Great!";
			private const string ADVANCE_MSG = "Each turn begins with my horde advancing.";
			private const string DASTARDLY_MSG = "Stop that!";


			private void OnButtonClick(global::Event e){
				switch (Services.UI.GetTutorialText()){
					case WELCOME_MSG:
						Services.UI.SimultaneousStatements(WELCOME_MSG, HOW_MSG);
						Services.UI.SetTutorialText(THEME_MSG);
						Services.UI.SetButtonText(NEVER_MSG);
						Services.Board.HighlightRow(0, BoardBehavior.OnOrOff.On);
						break;
					case THEME_MSG:
						Services.UI.SimultaneousStatements(THEME_MSG, NEVER_MSG);
						Services.UI.SetTutorialText(HORDE_MSG);
						Services.UI.SetButtonText(NONE_MSG);
						Services.Board.HighlightRow(0, BoardBehavior.OnOrOff.Off);
						break;
					case HORDE_MSG:
						Services.UI.SimultaneousStatements(HORDE_MSG, NONE_MSG);
						Services.UI.SetTutorialText(WAVES_MSG);
						Services.UI.SetButtonText(UNDERSTAND_MSG);
						break;
					case WAVES_MSG:
						Services.UI.SimultaneousStatements(WAVES_MSG, UNDERSTAND_MSG);
						Services.UI.SetTutorialText(ONE_THREE_MSG);
						break;
					case ONE_THREE_MSG:
						Services.UI.SimultaneousStatements(ONE_THREE_MSG, UNDERSTAND_MSG);
						Services.UI.SetTutorialText(END_OF_WAVE_MSG);
						Services.UI.SetButtonText(VALIANT_MSG);
						break;
					case END_OF_WAVE_MSG:
						Services.UI.SimultaneousStatements(END_OF_WAVE_MSG, VALIANT_MSG);
						Services.UI.SetTutorialText(ADVANCE_MSG);
						Services.UI.SetButtonText(DASTARDLY_MSG);
						break;
					case ADVANCE_MSG:
						Services.UI.SimultaneousStatements(ADVANCE_MSG, DASTARDLY_MSG);
						TransitionTo<AttackersAdvance>();
						break;
				}
			} 


			public override void OnEnter (){
				Context.NewTurn();
				Services.UI.SetTutorialText(WELCOME_MSG);
				Services.UI.ToggleTutorialText(ChatUI.OnOrOff.On);
				Services.UI.SetButtonText(HOW_MSG);
				Services.Events.Register<TutorialClick>(OnButtonClick);
				Services.Events.Fire(new TutorialPhaseStartEvent(Context.TutMachine.CurrentState));

			}


			public override void OnExit (){
				Services.Events.Unregister<TutorialClick>(OnButtonClick);
				Services.UI.ToggleTutorialText(ChatUI.OnOrOff.Off);
			}
		}


		/// <summary>
		/// State for the attackers moving south at the start of each turn.
		/// </summary>
		public class AttackersAdvance : FSM<TutorialTurnManager>.State {
			float timer;

			//tell the attacker manager to move the attackers.
			//this is routed through the attacker manager to avoid spreading control over the attackers over multiple classes.
			public override void OnEnter(){
				timer = 0.0f;
				Services.Attackers.SpawnNewAttackers(); //when the wave is done, don't spawn more attackers
				Services.Attackers.PrepareAttackerMove();
				Services.Attackers.MoveAttackers();
				Services.Events.Fire(new TutorialPhaseStartEvent(Context.TutMachine.CurrentState));
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
		/// State for the defenders' movement.
		/// </summary>
		public class PlayerMove : FSM<TutorialTurnManager>.State {


			private const string YOUR_TURN_MSG = "After my horde advances, it's your turn to move your defenders.";
			private const string OK_MSG = "Got it.";
			private const string SELECT_MSG = "Plan your move by clicking the spaces you want to move to. First, click your defender.";
			private const string PLAN_MSG = "Now choose your move by clicking each highlighted space.";
			private const string DONE_MSG = "Right, now lock in your move with the \"Go here\" button.";
			private const string PLEASE_MOVE_MSG = "Oops, you didn't move far enough. Try going to the last highlighted space.";
			private const string MOVE_ALL_MSG = "Great! In the real game, you'll have three defenders you can move, not just one.";
			private TwoDLoc[] requiredMoves = new TwoDLoc[2] { new TwoDLoc(3, 2), new TwoDLoc(3, 3) };
			private int moveIndex = 0;
			private DefenderSandbox defender; //assumes there is only one defender in the tutorial


			private void OnButtonClick(global::Event e){
				switch (Services.UI.GetTutorialText()){
					case YOUR_TURN_MSG:
						Services.UI.SimultaneousStatements(YOUR_TURN_MSG, OK_MSG);
						Services.UI.SetTutorialText(SELECT_MSG);
						Services.UI.TogglePhaseButton(ChatUI.OnOrOff.Off);
						break;
					case MOVE_ALL_MSG:
						Services.UI.SimultaneousStatements(MOVE_ALL_MSG, OK_MSG);
						TransitionTo<PlayerFight>();
						break;
				}
			}



			private void HandleHighlight(){
				if (moveIndex < requiredMoves.Length){
					Services.Board.HighlightSpace(requiredMoves[moveIndex].x, requiredMoves[moveIndex].z, BoardBehavior.OnOrOff.On);
				}

				if (moveIndex > 0 && moveIndex <= requiredMoves.Length){
					Services.Board.HighlightSpace(requiredMoves[moveIndex - 1].x, requiredMoves[moveIndex - 1].z, BoardBehavior.OnOrOff.Off);
				}
			}

			private void HandleMoveInputs(global::Event e){
				InputEvent inputEvent = e as InputEvent;


				if (Services.UI.GetTutorialText() == SELECT_MSG ||
					Services.UI.GetTutorialText() == PLEASE_MOVE_MSG){
					if (inputEvent.selected.tag == DEFENDER_TAG){
						Services.Defenders.SelectDefenderForMovement(inputEvent.selected.GetComponent<DefenderSandbox>());
						Services.UI.SetTutorialText(PLAN_MSG);
						HandleHighlight();
					}
				} else if (Services.UI.GetTutorialText() == PLAN_MSG){
					if (moveIndex >= requiredMoves.Length) return; //don't do anything with extra clicks on the board
					else if (inputEvent.selected.tag == BOARD_TAG){
						SpaceBehavior space = inputEvent.selected.GetComponent<SpaceBehavior>();

						if (space.GridLocation.x == requiredMoves[moveIndex].x &&
							space.GridLocation.z == requiredMoves[moveIndex].z){

							Services.Defenders.GetSelectedDefender().TryPlanMove(space.GridLocation);

							moveIndex++;

							HandleHighlight();
						}

						//upon arrival, tell the player to move on
						if (moveIndex == requiredMoves.Length){
							Services.UI.SetTutorialText(DONE_MSG);
						}
					}
				}
			}


			private void ResetMoveTutorial(global::Event e){
				Debug.Assert(e.GetType() == typeof(UndoMoveEvent), "Non-UndoMoveEvent in ResetMoveTutorial");

				//if the player is resetting while in the middle of the path, clear the highlight for the
				//next space
				if (moveIndex != 0 && moveIndex < requiredMoves.Length){
					Services.Board.HighlightSpace(requiredMoves[moveIndex].x,
												  requiredMoves[moveIndex].z,
												  BoardBehavior.OnOrOff.Off);
				}

				moveIndex = 0;
				HandleHighlight();
				Services.Undo.UndoMovePhase();

				Services.UI.SetTutorialText(PLAN_MSG);
			}


			private void TestMove(global::Event e){
				Debug.Assert(e.GetType() == typeof(MoveEvent), "Non-MoveEvent in TestMove");

				MoveEvent moveEvent = e as MoveEvent;


				if (moveEvent.endPos.x == requiredMoves[requiredMoves.Length - 1].x &&
					moveEvent.endPos.z == requiredMoves[requiredMoves.Length - 1].z){

					Services.UI.OpponentStatement(DONE_MSG);
					Services.UI.SetTutorialText(MOVE_ALL_MSG);
					Services.UI.SetButtonText(OK_MSG);
					Services.UI.TogglePhaseButton(ChatUI.OnOrOff.On);
				} else {
					ResetMoveTutorial(new UndoMoveEvent());
					Services.Defenders.PrepareDefenderMovePhase();
					Services.UI.SetTutorialText(PLEASE_MOVE_MSG);
				}
			}


			public override void OnEnter(){
				Services.Defenders.PrepareDefenderMovePhase();
				Services.Events.Fire(new TutorialPhaseStartEvent(Context.TutMachine.CurrentState));
				Services.UI.SetTutorialText(YOUR_TURN_MSG);
				Services.UI.ToggleTutorialText(ChatUI.OnOrOff.On);
				Services.UI.SetButtonText(OK_MSG);
				Services.Events.Register<TutorialClick>(OnButtonClick);
				Services.Events.Register<InputEvent>(HandleMoveInputs);
				Services.Events.Register<UndoMoveEvent>(ResetMoveTutorial);
				Services.Events.Register<MoveEvent>(TestMove);
				Services.Undo.PrepareToUndoMoves();
				defender = GameObject.FindGameObjectWithTag(DEFENDER_TAG).GetComponent<DefenderSandbox>();
			}


			public override void OnExit(){
				Services.Events.Unregister<TutorialClick>(OnButtonClick);
				Services.Events.Unregister<InputEvent>(HandleMoveInputs);
				Services.Events.Unregister<UndoMoveEvent>(ResetMoveTutorial);
				Services.Events.Unregister<MoveEvent>(TestMove);
			}
		}
//
//
//		/// <summary>
//		/// State for the defenders' combat. This is public so that the Ranger can determine whether it's the Defenders Fight phase, and display--or not display--
//		/// the Showboating UI accordingly.
//		/// </summary>
		public class PlayerFight : FSM<TutorialTurnManager>.State {


			private const string FIGHT_NOW_MSG = "After your defenders move, they can fight my horde.";
			private const string SEQUENCE_MSG = "Move, then fight.";
			private const string FACING_MSG = "Normally your defenders can only fight someone directly ahead of them.";
			private const string OK_MSG = "In front, OK.";
			private const string CARD_MSG = "When we fight we each play a card and add any modifier our characters have. High value wins.";
			private const string CARD_WHERE_MSG = "What cards?";
			private const string ATK_CARD_MSG = "I have a deck of cards in the upper-left. Next to it is a list of cards still in my deck, and what I've played.";
			private const string SEE_ATK_MSG = "I see the list.";
			private const string DEF_CARD_MSG = "Click on your defender to see your cards.";
			private const string HOW_FIGHT_MSG = "Click on a card (in the lower-left), then click the skeleton in front of your defender.";
//			private const string RESULT_MSG = "If your total is higher, the skeleton is removed. If not, my horde gains momentum.";
//			private const string MOMENTUM_WHAT_MSG = "Momentum?";
//			private const string MOMENTUM_MEAN_MSG = "Each momentum makes my horde advance one space further next turn.";
//			private const string CAREFUL_MSG = "Dangerous!";
//			private const string CARD_RULES_MSG = "A defender has to use all their cards once before they can use any again.";
//			private const string RESET_OK_MSG = "All three, OK.";
//			private const string ARMOR_RULES_MSG = "Skeletons are easy to beat, but the warlords are tough. Click my pieces to see their stats.";
//			private const string EXTRA_INFO_MSG = "In the lower left.";
//			private const string MATH_MSG = "The lower-left sheet will also show the math behind each combat, if you want precise information.";
//			private const string HELPFUL_MSG = "That's a help.";
//			private const string DONE_FIGHT_MSG = "Click \"Done fighting\" in the upper-right when you're finished.";
//
//
			private void OnButtonClick(global::Event e){
				Debug.Assert(e.GetType() == typeof(TutorialClick), "Non-TutorialClick event in PlayerFight's OnButtonClick");


				switch(Services.UI.GetTutorialText()){
					case FIGHT_NOW_MSG:
						Services.UI.SimultaneousStatements(FIGHT_NOW_MSG, SEQUENCE_MSG);
						Services.UI.SetTutorialText(FACING_MSG);
						Services.UI.SetButtonText(OK_MSG);
						break;
					case FACING_MSG:
						Services.UI.SimultaneousStatements(FACING_MSG, OK_MSG);
						Services.UI.SetTutorialText(CARD_MSG);
						Services.UI.SetButtonText(CARD_WHERE_MSG);
						break;
					case CARD_MSG:
						Services.UI.SimultaneousStatements(CARD_MSG, CARD_WHERE_MSG);
						Services.UI.SetTutorialText(ATK_CARD_MSG);
						Services.UI.SetButtonText(SEE_ATK_MSG);
						break;
					case ATK_CARD_MSG:
						Services.UI.SimultaneousStatements(ATK_CARD_MSG, SEE_ATK_MSG);
						Services.UI.SetTutorialText(DEF_CARD_MSG);
						Services.UI.TogglePhaseButton(ChatUI.OnOrOff.Off);
						break;
				}
			}


			private void HandleFightInputs(global::Event e){
				InputEvent inputEvent = e as InputEvent;

				if (inputEvent.selected.tag == DEFENDER_TAG &&
					Services.UI.GetTutorialText() == DEF_CARD_MSG){
					Services.Defenders.SelectDefenderForFight(inputEvent.selected.GetComponent<DefenderSandbox>());
					Services.UI.SetTutorialText(HOW_FIGHT_MSG);
				} else if ((inputEvent.selected.tag == ATTACKER_TAG || inputEvent.selected.tag == MINION_TAG || inputEvent.selected.tag == LEADER_TAG) &&
					Services.Defenders.IsAnyoneSelected() &&
					Services.Defenders.GetSelectedDefender().GetChosenCardValue() != DefenderSandbox.NO_CARD_SELECTED &&
					Services.UI.GetTutorialText() == HOW_FIGHT_MSG){

					Services.Defenders.GetSelectedDefender().TryFight(inputEvent.selected.GetComponent<AttackerSandbox>());
				}
//				} else if (inputEvent.selected.tag == ATTACKER_TAG || inputEvent.selected.tag == MINION_TAG || inputEvent.selected.tag == LEADER_TAG) {
//					TutorialGameManager.OldUI.SetExtraText(inputEvent.selected.GetComponent<AttackerSandbox>().GetUIInfo());
//				}
			}
//
//
//			/// <summary>
//			/// End the phase, but only if the player has reached the end of the tutorial for the phase.
//			/// </summary>
//			/// <param name="e">E.</param>
//			private void HandlePhaseEndInput(global::Event e){
//				Debug.Assert(e.GetType() == typeof(EndPhaseEvent), "Non-EndPhaseEvent in HandlePhaseEndInput");
//
//				if (Context.GetTutorialText() == DONE_FIGHT_MSG) TransitionTo<PowerUp>();
//			}


			public override void OnEnter(){
				Services.UI.SetTutorialText(FIGHT_NOW_MSG);
				Services.UI.SetButtonText(SEQUENCE_MSG);
				Services.Defenders.PrepareDefenderFightPhase();
//				Context.phaseText.text = PLAYER_FIGHT;
//				Context.TurnRulebookPage();
				Services.Events.Register<InputEvent>(HandleFightInputs);
//				Services.Events.Register<EndPhaseEvent>(HandlePhaseEndInput);
//				Context.phaseButtonText.text = STOP_FIGHTING_MSG;
//				Context.ToggleNextPhaseButton();
//				Context.SetTutorialText(FACING_MSG);
//				Context.SetButtonText(OK_MSG);
//				Context.ToggleAdvanceButton();
				Services.Events.Register<TutorialClick>(OnButtonClick);
			}


			public override void OnExit(){
				Services.Events.Unregister<InputEvent>(HandleFightInputs);
//				Services.Events.Unregister<EndPhaseEvent>(HandlePhaseEndInput);
//				Services.Defenders.CompleteFightPhase();
//				Context.ToggleNextPhaseButton();
				Services.Events.Unregister<TutorialClick>(OnButtonClick);
			}
		}
//
//
//		/// <summary>
//		/// State for demonstrating powering up.
//		/// </summary>
		public class PowerUp : FSM<TutorialTurnManager>.State {
//
//
//			private const string UI_TAG = "UI";
//			private const string POWER_MSG = "Let's stop for a second, and make your defender more awesome.";
//			private const string AWESOME_MSG = "Awesome is good.";
//			private const string SHEET_MSG = "Select your defender, then click the character sheet in the lower-right.";
//			private const string NEXT_CURRENT_MSG = "The upgrades you have so far are on the bottom row. You can choose from upgrades on the top row.";
//			private const string OK_MSG = "OK.";
//			private const string TRACKS_MSG = "Each column is a different set of upgrades. You can choose either column each time you upgrade.";
//			private const string CHOOSE_MSG = "Choose an upgrade by clicking either of the choices in the top row.";
//			private const string FREE_MSG = "Great! You can upgrade your defenders at the start of the game for free.";
//			private const string WHEN_MSG = "And after that?";
//			private const string HOPE_MSG = "Defenders earn upgrades by defeating my horde. The next one takes three victories, then four, etc.";
//			private const string HOW_MUCH_MSG = "If I forget?";
//			private const string NEXT_UPGRADE_MSG = "Check the defender's character sheet. It will tell you.";
//			private const string HIDE_MSG = "Click the board to put the character sheet back down.";
//			private const string READY_MSG = "Ready.";
//
//
//			public override void OnEnter (){
//				Context.SetTutorialText(POWER_MSG);
//				Context.SetButtonText(AWESOME_MSG);
//				Context.ToggleAdvanceButton();
//				Services.Events.Register<UpgradeEvent>(HandlePowerUpgrade);
//				Services.Events.Register<TutorialClick>(OnButtonClick);
//			}
//
//
//			public override void OnExit (){
//				Services.Events.Unregister<UpgradeEvent>(HandlePowerUpgrade);
//				Services.Events.Unregister<InputEvent>(HandlePowerInputs);
//				Services.Events.Unregister<TutorialClick>(OnButtonClick);
//			}
//
//
//			private void OnButtonClick(global::Event e){
//				Debug.Assert(e.GetType() == typeof(TutorialClick), "Non-TutorialClick in PowerUp's OnButtonClick");
//
//				switch (Context.GetTutorialText()){
//					case POWER_MSG:
//						Context.SetTutorialText(SHEET_MSG);
//						Context.ToggleAdvanceButton();
//						Services.Events.Register<InputEvent>(HandlePowerInputs);
//						break;
//					case NEXT_CURRENT_MSG:
//						Context.SetTutorialText(TRACKS_MSG);
//						break;
//					case TRACKS_MSG:
//						Context.SetTutorialText(CHOOSE_MSG);
//						Context.ToggleAdvanceButton();
//						break;
//					case FREE_MSG:
//						Context.SetTutorialText(HOPE_MSG);
//						Context.SetButtonText(HOW_MUCH_MSG);
//						break;
//					case HOPE_MSG:
//						Context.SetTutorialText(NEXT_UPGRADE_MSG);
//						Context.SetButtonText(OK_MSG);
//						break;
//					case NEXT_UPGRADE_MSG:
//						Context.SetTutorialText(HIDE_MSG);
//						Context.SetButtonText(READY_MSG);
//						break;
//					case HIDE_MSG:
//						TransitionTo<BesiegeWalls>();
//						break;
//				}
//			}
//
//
//			private void HandlePowerInputs(global::Event e){
//				Debug.Assert(e.GetType() == typeof(InputEvent), "Non-InputEvent in PowerUp's HandlePowerEvents");
//
//				InputEvent inputEvent = e as InputEvent;
//
//				if (inputEvent.selected.tag == UI_TAG){
//					if (Context.GetTutorialText() == SHEET_MSG){
//						Context.SetTutorialText(NEXT_CURRENT_MSG);
//						Context.SetButtonText(OK_MSG);
//						Context.ToggleAdvanceButton();
//					}
//				}
//
//				if (inputEvent.selected.tag == DEFENDER_TAG){
//					if (Context.GetTutorialText() == SHEET_MSG){
//						Services.Defenders.PrepareDefenderFightPhase();
//						Services.Defenders.SelectDefenderForFight(inputEvent.selected.GetComponent<DefenderSandbox>());
//					}
//				}
//			}
//
//			private void HandlePowerUpgrade(global::Event e){
//				Debug.Assert(e.GetType() == typeof(UpgradeEvent), "Non-UpgradeEvent in HandlePowerUpgrade");
//
//				if (Context.GetTutorialText() == CHOOSE_MSG){
//					Context.SetTutorialText(FREE_MSG);
//					Context.SetButtonText(WHEN_MSG);
//					Context.ToggleAdvanceButton();
//				} else if (Context.GetTutorialText() == NEXT_CURRENT_MSG ||
//						   Context.GetTutorialText() == TRACKS_MSG){
//					Context.SetTutorialText(FREE_MSG);
//					Context.SetButtonText(WHEN_MSG);
//				}
//			}
		}
//
//
		public class BesiegeWalls : FSM<TutorialTurnManager>.State {
//
//
//			private const string BESIEGE_MSG = "The last thing that happens each turn is my horde besieges the wall.";
//			private const string BACK_MSG = "Back off!";
//			private const string ADJACENT_MSG = "Any piece I have that's in front of the wall plays a card and defeats its guard.";
//			private const string POOR_MSG = "Poor guard!";
//			private const string HAHA_MSG = "Behold, the power of my skeleton!";
//			private const string REVENGE_MSG = "I'll get you!";
//			private const string READY_MSG = "That's enough to get started. Let's reset the board and start playing. Good luck, have fun!";
//			private const string GLHF_MSG = "You too!";
//			private const string GAME_SCENE = "Game";
//
//
//			List<AttackerSandbox> besiegers;
//
//
//			private void OnButtonClick(global::Event e){
//				Debug.Assert(e.GetType() == typeof(TutorialClick), "Non-TutorialClick in BesiegeWalls' OnButtonClick");
//
//				switch (Context.GetTutorialText()){
//				case BESIEGE_MSG:
//					Context.SetTutorialText(ADJACENT_MSG);
//					Context.SetButtonText(POOR_MSG);
//					break;
//				case ADJACENT_MSG:
//					Context.SetTutorialText(HAHA_MSG);
//					Context.SetButtonText(REVENGE_MSG);
//					Besiege();
//					break;
//				case HAHA_MSG:
//					Context.SetTutorialText(READY_MSG);
//					Context.SetButtonText(GLHF_MSG);
//					break;
//				case READY_MSG:
//					SceneManager.LoadScene(GAME_SCENE);
//					break;
//				}
//			}
//
//
//			//are any enemies besieging the wall? Get a list of them
//			public override void OnEnter (){
//				besiegers = Services.Board.GetBesiegingAttackers();
//				Context.phaseText.text = BESIEGE;
//				Context.TurnRulebookPage();
//				Context.SetTutorialText(BESIEGE_MSG);
//				Context.SetButtonText(BACK_MSG);
//				Services.Events.Register<TutorialClick>(OnButtonClick);
//			}
//
//
//			public override void OnExit (){
//				Services.Events.Unregister<TutorialClick>(OnButtonClick);
//			}
//
//
//			/// <summary>
//			/// Besiege the wall, knocking out any guards adjacent to attackers.
//			/// </summary>
//			private void Besiege() {
//				if (besiegers.Count > 0){
//					int combatValue = Services.AttackDeck.GetAttackerCard().Value;
//
//					if (combatValue > Services.Board.GetWallStrength(besiegers[0].GetColumn())){
//						Services.Board.ChangeWallDurability(besiegers[0].GetColumn(), -besiegers[0].SiegeStrength);
//					} else {
//						Services.Board.FailToDamageWall(besiegers[0].GetColumn());
//					}
//
//					besiegers.RemoveAt(0);
//				}
//			}
//
//
//			/// <summary>
//			/// Shut off the defender's combat cards and character sheet when it's safe to do so.
//			/// </summary>
//			public override void Tick (){
//				if (!Services.Tasks.CheckForTaskOfType<MoveCharSheetTask>() &&
//					Services.UI.GetCharSheetStatus() == CharacterSheetBehavior.SheetStatus.Hidden){
//					Services.Defenders.CompleteFightPhase();
//				}
//			}
		}
	}
}
