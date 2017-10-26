public class BlockColumnEvent : Event {

	public int Column { get; set; }


	public BlockColumnEvent(int column){
		Column = column;
	}
}
