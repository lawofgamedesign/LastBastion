public class SequencedMoveEvent : Event {

	public readonly AttackerSandbox attacker;
	public readonly bool moved;

	public SequencedMoveEvent(AttackerSandbox attacker, bool moved){
		this.attacker = attacker;
		this.moved = moved;
	}
}
