namespace Tutorial
{
	using TMPro;
	using UnityEngine;
	using UnityEngine.UI;

	public class TutorialChatUI : ChatUI {


		public override void Setup(){
			speechBalloon = Resources.Load<GameObject>(BALLOON_OBJ);
			chatContent = GameObject.Find(CHAT_OBJ).transform.Find(VIEWPORT_OBJ).Find(CONTENT_OBJ);
			phaseOverButton = GameObject.Find(PHASE_BUTTON_OBJ);
			phaseText = phaseOverButton.transform.Find(TEXT_OBJ).GetComponent<TextMeshProUGUI>();
			phaseOverButton.SetActive(false);
			undoButton = GameObject.Find(UNDO_BUTTON_OBJ);
			undoButton.SetActive(false);
			Services.Events.Register<TutorialPhaseStartEvent>(PhaseStartHandling);


			//turn UI setup
			turnText = GameObject.Find(TURN_CANVAS).transform.Find(TEXT_OBJ).GetComponent<Text>();


			//character sheet setup
			charSheet = GameObject.Find(CHAR_SHEET_OBJ).GetComponent<CharacterSheetBehavior>();


			//combat deck setup
			deckOrganizer = GameObject.Find(COMBAT_CARD_ORGANIZER).transform;
			discardOrganizer = GameObject.Find(DISCARD_ORGANIZER).transform;
			combatDeck.Clear(); //sanity check
			combatDeck = CreateCombatDeck();


			//speech balloon setup
			attackerBalloonStart = GameObject.Find(BALLOON_START_OBJ).transform.position;


			//tutorial canvas setup
			tutorialCanvas = GameObject.Find(TUTORIAL_CANVAS_OBJ);
			tutText = tutorialCanvas.transform.Find(TUTORIAL_TEXT_OBJ).GetComponent<TextMeshProUGUI>();
			tutorialCanvas.SetActive(false);
		}


		protected override void PhaseStartHandling (global::Event e){
			Debug.Assert(e.GetType() == typeof(TutorialPhaseStartEvent));

			TutorialPhaseStartEvent startEvent = e as TutorialPhaseStartEvent;

			if (startEvent.Phase.GetType() == typeof(TutorialTurnManager.StartOfTutorial)){
				TogglePhaseButton(OnOrOff.On);
			} else if (startEvent.Phase.GetType() == typeof(TutorialTurnManager.AttackersAdvance)){
				TogglePhaseButton(OnOrOff.Off);
			} else if (startEvent.Phase.GetType() == typeof(TutorialTurnManager.PlayerMove)){
				TogglePhaseButton(OnOrOff.On);
			}
		}
	}
}
