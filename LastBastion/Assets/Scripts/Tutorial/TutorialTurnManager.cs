namespace Tutorial
{
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.SceneManagement;
	using UnityEngine.UI;

	public class TutorialTurnManager : TurnManager {


		/////////////////////////////////////////////
		/// Fields
		/////////////////////////////////////////////


		//the specialized state machine for the tutorial
		private FSM<TutorialTurnManager> tutMachine;
		public FSM<TutorialTurnManager> TutMachine { 
			get { return tutMachine; } 
			private set { tutMachine = value; }
		}


		//used to turn indicators on and off
		public enum Indicate { Start, Stop };
		private GameObject indicator;
		private const string INDICATOR_OBJ = "Indicator particle";
		private const string INDICATOR_TAG = "Indicator";


		//the notices explaining where to move to
		private List<GameObject> moveNotices = new List<GameObject>();
		private GameObject rangerNotice;
		private GameObject guardianNotice;
		private GameObject brawlerNotice;
		private const string NOTICE = " move notice";
		private const string RANGER = "Ranger";
		private const string GUARDIAN = "Guardian";
		private const string BRAWLER = "Brawler";
		private const string NOTICE_TAG = "Notice";


		//scene to load when the tutorial is finished
		private const string NEXT_SCENE = "Game";


		/////////////////////////////////////////////
		/// Functions
		/////////////////////////////////////////////


		//initialize variables
		public override void Setup(){
			tutMachine = new FSM<TutorialTurnManager>(this);
			TutMachine = tutMachine;
			indicator = Resources.Load<GameObject>(INDICATOR_OBJ);
			rangerNotice = GameObject.Find(RANGER + NOTICE);
			guardianNotice = GameObject.Find(GUARDIAN + NOTICE);
			brawlerNotice = GameObject.Find(BRAWLER + NOTICE);
			moveNotices.AddRange(GameObject.FindGameObjectsWithTag(NOTICE_TAG));
			rangerNotice.SetActive(false);
			guardianNotice.SetActive(false);
			brawlerNotice.SetActive(false);

			TutMachine.TransitionTo<StartOfTutorial>();
		}


		//go through one loop of the current state
		public override void Tick(){
			TutMachine.Tick();
		}


		/// <summary>
		/// Move from one set of tutorial statements--tutorial text and button text--to the next.
		/// </summary>
		/// <param name="newTutorialText">The next thing the opponent says.</param>
		/// <param name="newPlayerText">The next thing the player says.</param>
		private void ChangeUIText(string newTutorialText, string newPlayerText){
			Services.UI.SimultaneousStatements(Services.UI.GetTutorialText(), Services.UI.GetPhaseButtonText());
			Services.UI.SetTutorialText(newTutorialText);
			Services.UI.SetButtonText(newPlayerText);
		}


		private void NoteObject(string tag, Indicate startOrStop){
			if (startOrStop == Indicate.Start){
				foreach (GameObject obj in GameObject.FindGameObjectsWithTag(tag)){
					MonoBehaviour.Instantiate<GameObject>(indicator,
														  new Vector3(obj.transform.position.x, indicator.transform.position.y, obj.transform.position.z + 
																																indicator.transform.position.z),
														  indicator.transform.rotation);
				}
			} else {
				foreach (GameObject indicator in GameObject.FindGameObjectsWithTag(INDICATOR_TAG)){
					indicator.SetActive(false);
				}
			}
		}


		/// <summary>
		/// Switch on the UI element indicating where a given defender should move, or switch it off when the defender arrives.
		/// </summary>
		/// <param name="defender">The moving defender, or null if all notices should be off.</param>
		private void ToggleMoveNotice(DefenderSandbox defender, Indicate startOrStop){
			if (defender == null){
				foreach (GameObject notice in moveNotices) notice.SetActive(false);
				return;
			}

			bool newState = startOrStop == Indicate.Start ? true : false;

			switch (defender.gameObject.name) {
				case RANGER:
					rangerNotice.SetActive(newState);
					break;
				case GUARDIAN:
					guardianNotice.SetActive(newState);
					break;
				case BRAWLER:
					brawlerNotice.SetActive(newState);
					break;
			}
		}
			


		/////////////////////////////////////////////
		/// States
		/////////////////////////////////////////////


		//update the turn counter
		public class StartOfTutorial : FSM<TutorialTurnManager>.State {

			//things the tutorial uses
			GameObject clickHereNotice;
			private const string CLICK_HERE_OBJ = "Click here notice";
			private const string DEFENDER_TAG = "Defender";



			private const string WELCOME_MSG = "OK, we're all set up.";
			private const string HOW_MSG = "How do we play?";
			private const string THEME_MSG = "In <b>Last Bastion</b>, you have to defend an innocent town from my skeleton horde.";
			private const string DEFENDERS_WHERE_MSG = "Defend with what?";
			private const string DEFENDERS_HERE_MSG = "These are your three defenders. Each one is a mighty champion--my horde can never hurt or kill them.";
			private const string CANT_LOSE_MSG = "Then then I always win?";
			private const string HOW_LOSE_MSG = "Your defenders can't die, but they might fail. If my horde ever reaches the back row of tiles, I win.";
			private const string TOWN_LOCATION_MSG = "That's the town.";
			private const string START_TURN_MSG = "Right. Are you ready to see how each turn goes?";
			private const string OK_MSG = "Sure.";


			private void OnButtonClick(global::Event e){
				switch (Services.UI.GetTutorialText()){
					case WELCOME_MSG:
						clickHereNotice.SetActive(false);
						Context.ChangeUIText(THEME_MSG, DEFENDERS_WHERE_MSG);
						break;
					case THEME_MSG:
						Context.NoteObject(DEFENDER_TAG, Indicate.Start);
						Context.ChangeUIText(DEFENDERS_HERE_MSG, CANT_LOSE_MSG);
						break;
					case DEFENDERS_HERE_MSG:
						Context.NoteObject(DEFENDER_TAG, Indicate.Stop);
						Services.Board.HighlightRow(0, BoardBehavior.OnOrOff.On);
						Context.ChangeUIText(HOW_LOSE_MSG, TOWN_LOCATION_MSG);
						break;
					case HOW_LOSE_MSG:
						Services.Board.HighlightRow(0, BoardBehavior.OnOrOff.Off);
						Context.ChangeUIText(START_TURN_MSG, OK_MSG);
						break;
					case START_TURN_MSG:
						Services.UI.TogglePhaseButton(ChatUI.OnOrOff.Off);
						TransitionTo<AttackersAdvance>();
						break;
				}
			} 


			public override void OnEnter (){
				Services.UI.SetTutorialText(WELCOME_MSG);
				Services.UI.ToggleTutorialText(ChatUI.OnOrOff.On);
				Services.UI.SetButtonText(HOW_MSG);
				Services.UI.TogglePhaseButton(ChatUI.OnOrOff.On);
				Services.Events.Register<TutorialClick>(OnButtonClick);
				FindTutorialObjects();
			}


			private void FindTutorialObjects(){
				clickHereNotice = GameObject.Find(CLICK_HERE_OBJ);
			}


			public override void OnExit (){
				Services.Events.Unregister<TutorialClick>(OnButtonClick);
			}
		}


		/// <summary>
		/// State for the attackers moving south at the start of each turn.
		/// </summary>
		public class AttackersAdvance : FSM<TutorialTurnManager>.State {
			float timer;
			bool doneAdvancing = false;

			private const string WALL_TAG = "Wall";
			private const string MOVE_PHASE_MSG = "Each turn starts with my horde advancing one space toward town.";
			private const string WALL_Q_MSG = "What about the castle?";
			private const string WALL_A_MSG = "Good question! My horde can't move through any part of the castle with guards on top.";
			private const string GUARDS_Q_MSG = "Can I lose guards?";
			private const string GUARDS_A_MSG = "Yes--we'll talk about it at the end of the turn. For now, let's talk about your defenders";
			private const string READY_PROGRESS_MSG = "OK.";


			private void OnButtonClick(global::Event e){
				switch (Services.UI.GetTutorialText()){
					case MOVE_PHASE_MSG:
						Context.ChangeUIText(WALL_A_MSG, GUARDS_Q_MSG);
						break;
					case WALL_A_MSG:
						Context.ChangeUIText(GUARDS_A_MSG, READY_PROGRESS_MSG);
						break;
					case GUARDS_A_MSG:
						Context.NoteObject(WALL_TAG, Indicate.Stop);
						TransitionTo<PlayerMove>();
						break;
				}
			}


			//tell the attacker manager to move the attackers.
			//this is routed through the attacker manager to avoid spreading control over the attackers over multiple classes.
			public override void OnEnter(){
				timer = 0.0f;
				Services.Attackers.SpawnNewAttackers(); //when the wave is done, don't spawn more attackers
				Services.Attackers.PrepareAttackerMove();
				Services.Attackers.RemoveAttackersSpawnedState();
				Services.Attackers.MoveAttackers();
				Services.UI.SetTutorialText(MOVE_PHASE_MSG);
				Services.Events.Register<TutorialClick>(OnButtonClick);
				//Services.Events.Fire(new TutorialPhaseStartEvent(Context.TutMachine.CurrentState));
			}


			//wait while the attackers move
			public override void Tick(){
				timer += Time.deltaTime;
				if (timer >= Context.attackerAdvanceDuration && !doneAdvancing){
					doneAdvancing = true;
					Services.UI.SetButtonText(WALL_Q_MSG);
					Services.UI.TogglePhaseButton(ChatUI.OnOrOff.On);
					Context.NoteObject(WALL_TAG, Indicate.Start);
				}
			}


			public override void OnExit (){
				Services.Events.Unregister<TutorialClick>(OnButtonClick);
			}
		}


		/// <summary>
		/// State for the defenders' movement.
		/// </summary>
		public class PlayerMove : FSM<TutorialTurnManager>.State {


			//the defenders; used to check their locations at the end of the tutorial
			private RangerBehavior ranger;
			private GuardianBehavior guardian;
			private BrawlerBehavior brawler;


			private const string MOVE_START_MSG = "After my horde advances, your defenders can move.";
			private const string HOW_MOVE_MSG = "Where do I click?";
			private const string MOVE_EXPLANATION_MSG = "Click each defender, then click tiles to make a path to a highlighted space.";
			private const string THERE_MSG = "They're there.";



			private void OnButtonClick(global::Event e){
				switch (Services.UI.GetTutorialText()){
					case MOVE_START_MSG:
						Services.UI.SetTutorialText(MOVE_EXPLANATION_MSG);
						Services.UI.TogglePhaseButton(ChatUI.OnOrOff.Off);
						Services.UI.ToggleUndoButton(ChatUI.OnOrOff.On);
						HighlightGoalSpaces();
						break;
					case MOVE_EXPLANATION_MSG:
						TransitionTo<PlayerFight>();
						break;
				}
			}


			private void HighlightGoalSpaces(){
				Services.Board.HighlightSpace(0, 3, BoardBehavior.OnOrOff.On);
				Services.Board.HighlightSpace(5, 3, BoardBehavior.OnOrOff.On);
				Services.Board.HighlightSpace(8, 3, BoardBehavior.OnOrOff.On);
			}


			private void HandleMoves(global::Event e){
				Debug.Assert(e.GetType() == typeof(MoveEvent), "Non-MoveEvent in HandleMoves");

				MoveEvent moveEvent = e as MoveEvent;

				Context.ToggleMoveNotice(moveEvent.movingObj.GetComponent<DefenderSandbox>(), Indicate.Stop);

				if (ranger.ReportGridLoc().x == 0 && ranger.ReportGridLoc().z == 3){
					if (guardian.ReportGridLoc().x == 5 && guardian.ReportGridLoc().z == 3){
						if (brawler.ReportGridLoc().x == 8 && brawler.ReportGridLoc().z == 3){
							Context.ToggleMoveNotice(null, Indicate.Stop); //sanity check; make sure all notices are off
							Services.Board.HighlightAll(BoardBehavior.OnOrOff.Off);
							Services.UI.SetButtonText(THERE_MSG);
							Services.UI.TogglePhaseButton(ChatUI.OnOrOff.On);
							Services.UI.ToggleUndoButton(ChatUI.OnOrOff.Off);
						}
					}
				}
			}


			private void HandleMoveInputs(global::Event e){
				InputEvent inputEvent = e as InputEvent;

				//don't double-count inputs that are intended to hide the character sheet
				if (Services.UI.GetCharSheetStatus() == CharacterSheetBehavior.SheetStatus.Displayed) return;

				if (Services.UI.GetTutorialText() != MOVE_EXPLANATION_MSG) return; //only move after the player has seen the explanation

				if (inputEvent.selected.tag == DEFENDER_TAG){
					Services.Defenders.SelectDefenderForMovement(inputEvent.selected.GetComponent<DefenderSandbox>());
					Context.ToggleMoveNotice(Services.Defenders.GetSelectedDefender(), Indicate.Start);
				} else if (inputEvent.selected.tag == BOARD_TAG){
					if (Services.Defenders.IsAnyoneSelected()){
						Services.Defenders.GetSelectedDefender().TryPlanMove(inputEvent.selected.GetComponent<SpaceBehavior>().GridLocation);
					}
				}
			}


			public override void OnEnter(){
				Services.Defenders.PrepareDefenderMovePhase();
				Services.Events.Register<InputEvent>(HandleMoveInputs);
				Services.Events.Register<MoveEvent>(HandleMoves);
				Services.Undo.PrepareToUndoMoves();
				Context.imSure = false;

				Context.ChangeUIText(MOVE_START_MSG, HOW_MOVE_MSG);

				ranger = GameObject.Find(RANGER).GetComponent<RangerBehavior>();
				guardian = GameObject.Find(GUARDIAN).GetComponent<GuardianBehavior>();
				brawler = GameObject.Find(BRAWLER).GetComponent<BrawlerBehavior>();

				Services.Events.Register<TutorialClick>(OnButtonClick);
			}


			public override void OnExit(){
				Services.Defenders.CompleteMovePhase();
				Services.Events.Unregister<InputEvent>(HandleMoveInputs);
				Services.Events.Unregister<MoveEvent>(HandleMoves);
				Services.Events.Unregister<TutorialClick>(OnButtonClick);
			}
		}
//
//
//		/// <summary>
//		/// State for the defenders' combat. This is public so that the Ranger can determine whether it's the Defenders Fight phase, and display--or not display--
//		/// the Showboating UI accordingly.
//		/// </summary>
		public class PlayerFight : FSM<TutorialTurnManager>.State {


			private int completedCombats = 0;
			private const int TOTAL_DEFENDERS = 3;
			private const string FIGHT_INTRO_MSG = "After your defenders move, they have the chance to fight my horde.";
			private const string HOW_FIGHT_MSG = "How do I fight?";
			private const string FIGHT_MECHANICS_MSG = "In this game we fight with cards. Each defender has three cards--these are the Guardian's.";
			private const string SAME_CARDS_MSG = "Can they be different?";
			private const string DIFFERENT_CARDS_MSG = "Yes. Each defender has their own cards. Try clicking them to see--let me know when you're done.";
			private const string DONE_CARDS_MSG = "OK, I'm ready.";
			private const string COMBAT_SEQUENCE_MSG = "When we fight, you choose one of your cards. I'll draw a card at random. High card wins.";
			private const string TIE_Q_MSG = "What if it's a tie?";
			private const string TIE_A_MSG = "I win ties. You're the heroines and heroes--it's not enough just to do OK against evil, you have to win!";
			private const string SENSE_MSG = "Makes sense.";
			private const string CHOOSE_CARDS_MSG = "To choose your card, pick a defender and one of their cards. Then click the skeleton in front of them.";
			private const string KEEP_FIGHTING_MSG = "Nice! Keep going--pick another defender, then a card, and then click the skeleton in front of them.";
			private const string WELL_FOUGHT_MSG = "Cool. I have some bad news about fighting, and some good news. First, the bad news:";
			private const string TWO_QS_MSG = "???";
			private const string MOMENTUM_MSG = "When I win, I gain 1 momentum. Every momentum moves <i>all</i> my horde 1 more space next turn.";
			private const string OUCH_MSG = "Ouch.";
			private const string INSPIRATION_MSG = "The good news is, when a defender wins enough fights, they power up.";
			private const string POWER_NOW_MSG = "Let's do that.";


			private void OnButtonClick(global::Event e){
				Debug.Assert(e.GetType() == typeof(TutorialClick), "Non-TutorialClick event in PlayerFight's OnButtonClick");


				switch(Services.UI.GetTutorialText()){
					case FIGHT_INTRO_MSG:
						Context.ChangeUIText(FIGHT_MECHANICS_MSG, SAME_CARDS_MSG);
						Services.Defenders.SelectDefenderForFight(GameObject.Find(GUARDIAN).GetComponent<DefenderSandbox>());
						break;
					case FIGHT_MECHANICS_MSG:
						Context.ChangeUIText(DIFFERENT_CARDS_MSG, DONE_CARDS_MSG);
						break;
					case DIFFERENT_CARDS_MSG:
						Context.ChangeUIText(COMBAT_SEQUENCE_MSG, TIE_Q_MSG);
						Services.Defenders.CompleteFightPhase();
						Services.Defenders.PrepareDefenderFightPhase();
						break;
					case COMBAT_SEQUENCE_MSG:
						Context.ChangeUIText(TIE_A_MSG, SENSE_MSG);
						break;
					case TIE_A_MSG:
						Services.UI.SetTutorialText(CHOOSE_CARDS_MSG);
						Services.UI.TogglePhaseButton(ChatUI.OnOrOff.Off);
						break;
					case WELL_FOUGHT_MSG:
						Context.ChangeUIText(MOMENTUM_MSG, OUCH_MSG);
						break;
					case MOMENTUM_MSG:
						Context.ChangeUIText(INSPIRATION_MSG, POWER_NOW_MSG);
						break;
					case INSPIRATION_MSG:
						TransitionTo<PowerUp>();
						break;
				}
			}


			private void HandleFightInputs(global::Event e){
				Debug.Assert(e.GetType() == typeof(InputEvent), "Non-InputEvent in HandleFightInputs");

				InputEvent inputEvent = e as InputEvent;

				if (inputEvent.selected.tag == DEFENDER_TAG &&
					(Services.UI.GetTutorialText() == DIFFERENT_CARDS_MSG ||
					 Services.UI.GetTutorialText() == CHOOSE_CARDS_MSG ||
					 Services.UI.GetTutorialText() == KEEP_FIGHTING_MSG)){
					Services.Defenders.SelectDefenderForFight(inputEvent.selected.GetComponent<DefenderSandbox>());
				} else if ((inputEvent.selected.tag == ATTACKER_TAG || inputEvent.selected.tag == MINION_TAG || inputEvent.selected.tag == LEADER_TAG) &&
					Services.Defenders.IsAnyoneSelected() &&
					Services.Defenders.GetSelectedDefender().GetChosenCardValue() != DefenderSandbox.NO_CARD_SELECTED &&
					(Services.UI.GetTutorialText() == CHOOSE_CARDS_MSG ||
					 Services.UI.GetTutorialText() == KEEP_FIGHTING_MSG)){

					Services.Defenders.GetSelectedDefender().TryFight(inputEvent.selected.GetComponent<AttackerSandbox>());
					Services.UI.SetTutorialText(KEEP_FIGHTING_MSG);
					completedCombats++;

					if (completedCombats >= TOTAL_DEFENDERS){
						Context.ChangeUIText(WELL_FOUGHT_MSG, TWO_QS_MSG);
						Services.UI.TogglePhaseButton(ChatUI.OnOrOff.On);
					}
				}
			}


			public override void OnEnter(){
				Services.Defenders.PrepareDefenderFightPhase();
				Services.Events.Register<InputEvent>(HandleFightInputs);
				Services.Events.Register<TutorialClick>(OnButtonClick);
				Context.ChangeUIText(FIGHT_INTRO_MSG, HOW_FIGHT_MSG);
			}


			public override void OnExit(){
				Services.Events.Unregister<InputEvent>(HandleFightInputs);
				Services.Events.Unregister<TutorialClick>(OnButtonClick);
			}
		}
//
//
//		/// <summary>
//		/// State for demonstrating powering up.
//		/// </summary>
		public class PowerUp : FSM<TutorialTurnManager>.State {


			private const string WHEN_POWER_MSG = "At the start of your turn, you'll choose a new power for every defender who's ready to power up.";
			private const string WHERE_CLICK_MSG = "How do I do that?";
			private const string SHEET_EXPLANATION_MSG = "You'll see the defender's character sheet. Click the power you want them to have.";
			private const string READY_MSG = "OK, let's try it.";


			#region upgrade code

			List<DefenderSandbox> upgraders = new List<DefenderSandbox>();


			private void BeginUpgrading(){
				Services.Events.Register<TutorialPowerChoiceEvent>(HandlePowerChoice);

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


			/// <summary>
			/// When the player chooses a power, see if the player is done upgrading and it's time to move on to the next phase.
			/// </summary>
			/// <param name="e">A PowerChoiceEvent sent out by the character sheet.</param>
			private void HandlePowerChoice(global::Event e){
				Debug.Assert(e.GetType() == typeof(TutorialPowerChoiceEvent), "Non-TutorialPowerChoiceEvent in HandlePowerChoice");

				TutorialPowerChoiceEvent powerEvent = e as TutorialPowerChoiceEvent;

				CheckForAllFinished(powerEvent.defender);
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
					Services.Events.Unregister<TutorialPowerChoiceEvent>(HandlePowerChoice);
					Services.Defenders.NoSelectedDefender();
				}
			}


			#endregion upgrade code


			private void OnButtonClick(global::Event e){
				Debug.Assert(e.GetType() == typeof(TutorialClick), "Non-TutorialClick in PowerUp's OnButtonClick");

				switch (Services.UI.GetTutorialText()){
				case WHEN_POWER_MSG:
					Context.ChangeUIText(SHEET_EXPLANATION_MSG, READY_MSG);
					break;
				case SHEET_EXPLANATION_MSG:
					Services.UI.ToggleTutorialText(ChatUI.OnOrOff.Off);
					Services.UI.TogglePhaseButton(ChatUI.OnOrOff.Off);
					BeginUpgrading();
					break;
				}
			}


			public override void OnEnter (){
				Services.UI.ToggleTutorialText(ChatUI.OnOrOff.On);
				Services.UI.TogglePhaseButton(ChatUI.OnOrOff.On);
				Context.ChangeUIText(WHEN_POWER_MSG, WHERE_CLICK_MSG);
				upgraders = Services.Defenders.GetAllDefenders();
				Services.Events.Register<TutorialClick>(OnButtonClick);
			}

			public override void Tick(){
				//only go on if no one needs to upgrade and there are no tasks running (i.e., wait for tankards to be dropped, etc.)
				if (upgraders.Count == 0 &&
					!Services.Tasks.CheckForAnyTasks() &&
					Services.UI.GetTutorialText() == SHEET_EXPLANATION_MSG) TransitionTo<BesiegeWalls>();
			}


			public override void OnExit (){
				Services.Events.Unregister<TutorialClick>(OnButtonClick);
			}
		}
//
//
		public class BesiegeWalls : FSM<TutorialTurnManager>.State {


			private const string POWER_CONCLUSION_MSG = "Great! You can always click a defender, and then the sheet, to see their powers.";
			private const string FIGHT_BACK_MSG = "When do you fight back?";
			private const string BESIEGE_INTRO_MSG = "Right now. At the end of the turn, my pieces remove guards from the wall.";
			private const string HOW_WORK_MSG = "How does that work?";
			private const string SPEND_CARDS_MSG = "Each of my pieces in front of the wall knocks one guard off the wall.";
			private const string CARD_USE_MSG = "Do I play a card?";
			private const string ATK_CARDS_MSG = "No--your defenders aren't fighting, just the guards.";
			private const string EXAMPLE_MSG = "Can I see an example?";

			

			private List<AttackerSandbox> besiegers;


			private void OnButtonClick(global::Event e){
				Debug.Assert(e.GetType() == typeof(TutorialClick), "Non-TutorialClick in BesiegeWalls' OnButtonClick");

				switch (Services.UI.GetTutorialText()){
				case POWER_CONCLUSION_MSG:
					Services.UI.SetTutorialText(BESIEGE_INTRO_MSG);
					Services.UI.SetButtonText(HOW_WORK_MSG);
					break;
				case BESIEGE_INTRO_MSG:
					Services.UI.SetTutorialText(SPEND_CARDS_MSG);
					Services.UI.SetButtonText(CARD_USE_MSG);
					break;
				case SPEND_CARDS_MSG:
					Services.UI.SetTutorialText(ATK_CARDS_MSG);
					Services.UI.SetButtonText(EXAMPLE_MSG);
					break;
				case ATK_CARDS_MSG:
					Besiege();
					break;
				}
			}
//
//
//			//are any enemies besieging the wall? Get a list of them
			public override void OnEnter (){
				Services.UI.SetTutorialText(POWER_CONCLUSION_MSG);
				Services.UI.ToggleTutorialText(ChatUI.OnOrOff.On);
				Services.UI.SetButtonText(FIGHT_BACK_MSG);
				Services.UI.TogglePhaseButton(ChatUI.OnOrOff.On);
				Services.Events.Register<TutorialClick>(OnButtonClick);
				besiegers = Services.Board.GetBesiegingAttackers();
			}


			public override void OnExit (){
				Services.Events.Unregister<TutorialClick>(OnButtonClick);
			}


			/// <summary>
			/// Besiege the wall, knocking out any guards adjacent to attackers.
			/// </summary>
			private void Besiege() {
				if (besiegers.Count > 0){
					int combatValue = Services.AttackDeck.GetAttackerCard().Value;

					if (combatValue > Services.Board.GetWallStrength(besiegers[0].GetColumn())){
						Services.Board.ChangeWallDurability(besiegers[0].GetColumn(), -besiegers[0].SiegeStrength);
					} else {
						Services.Board.FailToDamageWall(besiegers[0].GetColumn());
					}

					besiegers.RemoveAt(0);

					if (besiegers.Count > 0) Besiege(); //keep going until all besiegers have attacked the wall
					else TransitionTo<TutorialEnd>();
				}
			}
		}


		public class TutorialEnd : FSM<TutorialTurnManager>.State {


			private const string EVERYTHING_MSG = "That's everything we need to start playing.";
			private const string WARLORD_Q_MSG = "What about the ones in back?";
			private const string WARLORD_A_MSG = "My skeletons are led by warlords. They're tougher than skeletons.";
			private const string EXPLANATION_Q_MSG = "What do they do?";
			private const string EXPLANATION_A_MSG = "There are different types. If you see one you're not sure about during the game, just ask.";
			private const string OVERVIEW_REQ_MSG = "OK. Let's sum up . . . .";
			private const string OVERVIEW_1_MSG = "At the start of each turn, any of your defenders who are ready to power up will do that.";
			private const string ELLIPSIS_MSG = ". . . .";
			private const string OVERVIEW_2_MSG = "Then my horde moves forward. They move one space, plus 1 for each momentum I've gained.";
			private const string OVERVIEW_3_MSG = "After my horde moves, your defenders can move. You can move none of them, some, or all of them.";
			private const string OVERVIEW_4_MSG = "Then your defenders fight. Click a defender, choose a card, and finally click on my piece to attack it.";
			private const string OVERVIEW_5_MSG = "When you're done, any of my horde that's at the wall will knock a guard off.";
			private const string WAVES_MSG = "The game lasts three waves. If you reach the end of wave 3, you win!";
			private const string START_MSG = "Let's play!";
				

			private void OnButtonClick(global::Event e){
				Debug.Assert(e.GetType() == typeof(TutorialClick), "Non-TutorialClick in BesiegeWalls' OnButtonClick");

				switch (Services.UI.GetTutorialText()){
					case EVERYTHING_MSG:
						Services.UI.SetTutorialText(WARLORD_A_MSG);
						Services.UI.SetButtonText(EXPLANATION_Q_MSG);
						break;
					case WARLORD_A_MSG:
						Services.UI.SetTutorialText(EXPLANATION_A_MSG);
						Services.UI.SetButtonText(OVERVIEW_REQ_MSG);
						break;
					case EXPLANATION_A_MSG:
						Services.UI.SetTutorialText(OVERVIEW_1_MSG);
						Services.UI.SetButtonText(ELLIPSIS_MSG);
						break;
					case OVERVIEW_1_MSG:
						Services.UI.SetTutorialText(OVERVIEW_2_MSG);
						break;
					case OVERVIEW_2_MSG:
						Services.UI.SetTutorialText(OVERVIEW_3_MSG);
						break;
					case OVERVIEW_3_MSG:
						Services.UI.SetTutorialText(OVERVIEW_4_MSG);
						break;
					case OVERVIEW_4_MSG:
						Services.UI.SetTutorialText(OVERVIEW_5_MSG);
						break;
					case OVERVIEW_5_MSG:
						Services.UI.SetTutorialText(WAVES_MSG);
						Services.UI.SetButtonText(START_MSG);
						break;
					case WAVES_MSG:
						SceneManager.LoadScene(NEXT_SCENE);
						break;

				}
			}


			public override void OnEnter (){
				Services.UI.SetTutorialText(EVERYTHING_MSG);
				Services.UI.SetButtonText(WARLORD_Q_MSG);
				Services.Events.Register<TutorialClick>(OnButtonClick);
			}


			public override void OnExit (){
				Services.Events.Unregister<TutorialClick>(OnButtonClick);
			}
		}
	}
}
