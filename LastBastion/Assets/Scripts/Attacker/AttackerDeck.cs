using UnityEngine;

public class AttackerDeck {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	private Card[] values; //the cards in the deck

	private CardDeck<Card> attackerDeck; //the deck as a whole


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables
	public void Setup(){
		values = new Card[] { new Card(1), new Card(1), new Card(2), new Card(2), new Card(3), new Card(3), new Card(4), new Card(4) };
		attackerDeck = new CardDeck<Card>(values);
	}


	/// <summary>
	/// Draw a card from the attacker deck.
	/// </summary>
	/// <returns>The drawn card.</returns>
	public Card GetAttackerCard(){
		return attackerDeck.Draw();
	}
}
