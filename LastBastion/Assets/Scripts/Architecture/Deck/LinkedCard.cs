public class LinkedCard {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//has this card been drawn?
	protected bool drawn;


	//does this card have an int value? If so, it's stored here
	public int Value { get; private set; }


	/////////////////////////////////////////////
	/// Interaction with LinkedCardDeck
	/////////////////////////////////////////////


	//constructor
	public LinkedCard(int value = 0){
		drawn = false;
		Value = value;
	}


	public bool CheckIfDrawn(){
		return drawn;
	}


	public void DrawThis(){
		drawn = true;
	}


	public void BackToDeck(){
		drawn = false;
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
