public class AttackerDefeatedEvent : Event {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the attacker who was defeated
	public readonly AttackerSandbox attacker;


	//where the attacker was when defeated
	public readonly TwoDLoc location;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public AttackerDefeatedEvent(AttackerSandbox attacker){
		this.attacker = attacker;
		location = new TwoDLoc(this.attacker.XPos, this.attacker.ZPos);
	}
}
