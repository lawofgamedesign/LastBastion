public class Card {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////

	//used for most purposes
	public int Value { get; private set; }


	//relevant to defenders; is this card available for use?
	public bool Available { get; set; }


	//constructor
	public Card(int value){
		Value = value;
		Available = true;
	}
}
