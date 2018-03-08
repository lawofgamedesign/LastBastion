using UnityEngine;

public class LinkedAttackerDeck {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	private LinkedCardDeck attackerDeck; //the deck as a whole


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public LinkedAttackerDeck(){
		LinkedCard[] values = new LinkedCard[] { new LinkedCard(1), 
			new LinkedCard(1), 
			new LinkedCard(2),
			new LinkedCard(2),
			new LinkedCard(3),
			new LinkedCard(3),
			new LinkedCard(4),
			new LinkedCard(4) };
		attackerDeck = new LinkedCardDeck(values);
	}


	public LinkedCard GetAttackerCard(){
		bool reshuffled = false;

		LinkedCard temp = attackerDeck.Draw(out reshuffled);

		if (reshuffled) Services.UI.RecreateCombatDeck();
		Services.UI.DrawCombatCard(temp.Value);

		return temp;
	}


	public void AddCard(Transform attacker, int value){
		attackerDeck.AddCard(value);
		Services.UI.AddCardToDeck(attacker, value);
	}


	public void RemoveCard(Transform attacker, int value){
		bool removedFromDiscard = false;
		if (attackerDeck.RemoveCard(value, out removedFromDiscard)){
			if (!removedFromDiscard) Services.UI.RemoveCardFromDeck(attacker, value);
			else Services.UI.RemoveCardFromDiscard(attacker, value);
		}
	}


	public void Reshuffle(){
		attackerDeck.ShuffleDeck();
	}


	public int GetDeckCount(){
		return attackerDeck.GetDeck().Count;
	}
}
