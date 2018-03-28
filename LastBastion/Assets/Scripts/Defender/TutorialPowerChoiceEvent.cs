public class TutorialPowerChoiceEvent : Event {

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
	public TutorialPowerChoiceEvent(DefenderSandbox defender, int tree){
		this.defender = defender;
		this.tree = tree;
	}
}
