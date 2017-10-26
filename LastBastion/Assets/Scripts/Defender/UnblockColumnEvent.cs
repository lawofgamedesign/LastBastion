public class UnblockColumnEvent : Event {

	public int Column { get; set; }


	public UnblockColumnEvent(int column){
		Column = column;
	}
}
