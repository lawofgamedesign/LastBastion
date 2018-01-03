using TMPro;
using UnityEngine;

public class CombatExplanationTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the attacker and defender
	private DefenderSandbox defenderScript;
	private AttackerSandbox attackerScript;
	private readonly string attackerName;
	private readonly string defenderName;


	//combat card values
	private readonly int attackerValue;
	private readonly int defenderValue;


	//attack mods
	private readonly int attackerMod;
	private readonly int defenderMod;
	private const string SIZE_START = "<size=20>";
	private const string ATTACK_MOD = "'s attack bonus:\n";
	private const string SIZE_END = "</size>";


	//damage inflicted
	private readonly int damage;


	//total
	private Color attackerColor;
	private Color defenderColor;
	private const string WINNER_MSG = " wins by";
	private const string TIE_MSG = "Tie, ";
	private const string TIE_WIN_MSG = " wins";
	private const string DEFENDER_COLOR_HEX = "#003F96FF";
	private const string ATTACKER_COLOR_HEX = "#960300FF";


	//UI elements
	private TextMeshProUGUI attackerNameText;
	private TextMeshProUGUI defenderNameText;
	private TextMeshProUGUI attackerCardText;
	private TextMeshProUGUI defenderCardText;
	private TextMeshProUGUI attackerModText;
	private TextMeshProUGUI defenderModText;
	private TextMeshProUGUI attackerTotalText;
	private TextMeshProUGUI defenderTotalText;
	private TextMeshProUGUI totalText;
	private TextMeshProUGUI winnerText;
	private const string ATTACKER = "Attacker";
	private const string DEFENDER = "Defender";
	private const string NAME = " name";
	private const string VALUE = " value";
	private const string CARD = "'s card";
	private const string MOD = " mod";
	private const string TOTAL = " total";
	private const string OVERALL_TOTAL = "Total";
	private const string WINNER = "Winner";
	private const string BLANK = "";


	//time between each element displaying
	private const float DISPLAY_DELAY = 1.0f;


	//state machine for displaying text sequentially
	private FSM<CombatExplanationTask> explainMachine;


	//the gameobject that is activated and deactivated to show and hide the explanation
	private GameObject display;
	private const string CANVAS_OBJ = "Combat result canvas";
	private const string DISPLAY_OBJ = "Combat result display";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public CombatExplanationTask(AttackerSandbox attacker,
								 DefenderSandbox defender,
								 int attackerValue,
								 int defenderValue,
								 int attackerMod,
								 int defenderMod,
								 int damage){
		attackerScript = attacker;
		defenderScript = defender;

		int index = 0;
		index = attacker.gameObject.name.IndexOf(AttackerManager.DIVIDER);
		attackerName = attacker.gameObject.name.Substring(0, index);

		defenderName = defender.gameObject.name;
		this.attackerValue = attackerValue;
		this.defenderValue = defenderValue;
		this.attackerMod = attackerMod;
		this.defenderMod = defenderMod;
		this.damage = damage;
	}


	/// <summary>
	/// Get references, set colors, and create the state machine.
	/// </summary>
	protected override void Init (){
		display = GameObject.Find(CANVAS_OBJ).transform.Find(DISPLAY_OBJ).gameObject;
		display.SetActive(true);
		attackerNameText = GameObject.Find(ATTACKER + NAME).GetComponent<TextMeshProUGUI>();
		defenderNameText = GameObject.Find(DEFENDER + NAME).GetComponent<TextMeshProUGUI>();
		attackerCardText = GameObject.Find(ATTACKER + VALUE).GetComponent<TextMeshProUGUI>();
		defenderCardText = GameObject.Find(DEFENDER + VALUE).GetComponent<TextMeshProUGUI>();
		attackerModText = GameObject.Find(ATTACKER + MOD).GetComponent<TextMeshProUGUI>();
		defenderModText = GameObject.Find(DEFENDER + MOD).GetComponent<TextMeshProUGUI>();
		attackerTotalText = GameObject.Find(ATTACKER + TOTAL).GetComponent<TextMeshProUGUI>();
		defenderTotalText = GameObject.Find(DEFENDER + TOTAL).GetComponent<TextMeshProUGUI>();
		totalText = GameObject.Find(OVERALL_TOTAL).GetComponent<TextMeshProUGUI>();
		winnerText = GameObject.Find(WINNER).GetComponent<TextMeshProUGUI>();
		ColorUtility.TryParseHtmlString(ATTACKER_COLOR_HEX, out attackerColor);
		ColorUtility.TryParseHtmlString(DEFENDER_COLOR_HEX, out defenderColor);
		explainMachine = new FSM<CombatExplanationTask>(this);
		explainMachine.TransitionTo<Intro>();
		Services.Events.Register<InputEvent>(BeDone);
	}


	/// <summary>
	/// Run the explanation's state machine.
	/// </summary>
	public override void Tick (){
		explainMachine.Tick();
	}


	/// <summary>
	/// Shut off the explanation when combat results have been completely displayed.
	/// </summary>
	protected override void OnSuccess (){
		display.SetActive(false);

		//clear all text for next time
		attackerNameText.text = BLANK;
		defenderNameText.text = BLANK;
		attackerCardText.text = BLANK;
		defenderCardText.text = BLANK;
		attackerModText.text = BLANK;
		defenderModText.text = BLANK;
		attackerTotalText.text = BLANK;
		defenderTotalText.text = BLANK;
		totalText.text = BLANK;
		winnerText.text = BLANK;

		Services.UI.ExplainCombat(defenderValue, defenderScript, attackerScript, attackerValue, damage);

		Services.Events.Unregister<InputEvent>(BeDone);

		if (damage > 0) attackerScript.TakeDamage(damage);
	}


	private void BeDone(Event e){
		Debug.Assert(e.GetType() == typeof(InputEvent));

		SetStatus(TaskStatus.Success);
	}


	/////////////////////////////////////////////
	/// States
	/////////////////////////////////////////////


	private class Intro : FSM<CombatExplanationTask>.State {


		private float timer = 0.0f;


		public override void OnEnter (){
			Context.attackerNameText.text = Context.attackerName + CombatExplanationTask.CARD;
			Context.defenderNameText.text = Context.defenderName + CombatExplanationTask.CARD;
		}


		public override void Tick (){
			timer += Time.deltaTime;

			if (timer >= CombatExplanationTask.DISPLAY_DELAY) TransitionTo<ShowValues>();
		}
	}


	private class ShowValues : FSM<CombatExplanationTask>.State {


		private float timer = 0.0f;


		public override void OnEnter (){
			Context.attackerCardText.text = Context.attackerValue.ToString();
			Context.defenderCardText.text = Context.defenderValue.ToString();
		}


		public override void Tick (){
			timer += Time.deltaTime;

			if (timer >= CombatExplanationTask.DISPLAY_DELAY) TransitionTo<ShowMods>();
		}
	}


	private class ShowMods : FSM<CombatExplanationTask>.State {


		private float timer = 0.0f;


		public override void OnEnter (){
			Context.attackerModText.text = CombatExplanationTask.SIZE_START +
										   Context.attackerName +
										   CombatExplanationTask.ATTACK_MOD +
										   CombatExplanationTask.SIZE_END +
										   Context.attackerMod.ToString();
			Context.defenderModText.text = CombatExplanationTask.SIZE_START +
										   Context.defenderName +
										   CombatExplanationTask.ATTACK_MOD +
										   CombatExplanationTask.SIZE_END +
										   Context.defenderMod.ToString();
		}


		public override void Tick (){
			timer += Time.deltaTime;

			if (timer >= CombatExplanationTask.DISPLAY_DELAY) TransitionTo<ShowIndivTotals>();
		}
	}


	private class ShowIndivTotals : FSM<CombatExplanationTask>.State {


		private float timer = 0.0f;


		public override void OnEnter (){
			Context.attackerTotalText.text = (Context.attackerValue + Context.attackerMod).ToString();
			Context.defenderTotalText.text = (Context.defenderValue + Context.defenderMod).ToString();
		}


		public override void Tick (){
			timer += Time.deltaTime;

			if (timer >= CombatExplanationTask.DISPLAY_DELAY) TransitionTo<ShowOverallTotal>();
		}
	}


	private class ShowOverallTotal : FSM<CombatExplanationTask>.State {


		private int total = 0;


		public override void OnEnter (){
			total = (Context.attackerValue + Context.attackerMod) - (Context.defenderValue + Context.defenderMod);

			//attacker won
			if (total > 0){
				Context.winnerText.text = Context.attackerName + CombatExplanationTask.WINNER_MSG;
				Context.totalText.text = total.ToString();
				Context.winnerText.color = Context.attackerColor;
				Context.totalText.color = Context.attackerColor;

				//defender won
			} else if (total < 0) {
				Context.winnerText.text = Context.defenderName + CombatExplanationTask.WINNER_MSG;
				Context.totalText.text = Mathf.Abs(total).ToString();
				Context.winnerText.color = Context.defenderColor;
				Context.totalText.color = Context.defenderColor;

				//tie; attacker wins
			} else {
				Context.winnerText.text = CombatExplanationTask.TIE_MSG + Context.attackerName + CombatExplanationTask.TIE_WIN_MSG;
				Context.totalText.text = total.ToString();
				Context.winnerText.color = Context.attackerColor;
				Context.totalText.color = Context.attackerColor;
			}
		}


		public override void Tick (){
			//wait for the player to click to move on
		}
	}
}
