using UnityEngine;

public class ChooseToDefeatTask : Task {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the defender who's choosing someone to defeat
	private readonly DefenderSandbox defender;


	//What kind of attacker can the player choose?
	public enum AttackerTypes { Minion, Warlord }
	private readonly AttackerTypes attackerType;


	//statements explaining what's happening
	private const string CHOOSING_MSG_START = "I'm going to choose a ";
	private const string CHOOSING_MSG_END = " to defeat.";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public ChooseToDefeatTask(DefenderSandbox defender, AttackerTypes attackerType){
		this.defender = defender;
		this.attackerType = attackerType;
	}


	protected override void Init (){
		Services.Events.Register<InputEvent>(ChooseAttacker);
		Services.UI.PlayerPhaseStatement(CHOOSING_MSG_START + attackerType.ToString() + CHOOSING_MSG_END);
	}


	protected override void Cleanup (){
		Services.Events.Unregister<InputEvent>(ChooseAttacker);
	}


	private void ChooseAttacker(global::Event e){
		Debug.Assert(e.GetType() == typeof(InputEvent), "Non-InputEvent in ChooseAttacker");

		InputEvent inputEvent = e as InputEvent;

		if (inputEvent.selected.tag == attackerType.ToString()){
			AttackerSandbox attacker = inputEvent.selected.GetComponent<AttackerSandbox>();

			attacker.TakeDamage(attacker.Health);
			defender.DefeatAttacker();

			SetStatus(TaskStatus.Success);
		}
	}
}
