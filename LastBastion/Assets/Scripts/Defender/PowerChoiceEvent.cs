public class PowerChoiceEvent : Event {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the defender who will power up
	public readonly DefenderSandbox defender;


	//the upgrade tree the player chose
	public readonly int tree;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public PowerChoiceEvent(DefenderSandbox defender, int tree){
		this.defender = defender;
		this.tree = tree;
	}
}
