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
		


	/////////////////////////////////////////////
	/// Lifespan
	/////////////////////////////////////////////


	//Called when added to the deck.
	public virtual void Added() { }


	//Called when this card is drawn.
	public virtual void Drawn() { }


	//Called when this card is discarded after having its effect
	public virtual void Discarded() { }
}
