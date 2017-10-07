using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackerDeck {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	private Card[] values; //the cards in the deck

	private CardDeck<Card> attackerDeck; //the deck as a whole


	//the UI for the deck
	private Text cardsInDeck;
	private Text playedCards;
	private List<Card> playedCardList = new List<Card>();
	private const string DECK_CANVAS = "Attacker deck canvas";
	private const string DECK_OBJ = "Deck";
	private const string PLAYED_CARDS_OBJ = "Played cards";
	private const string DECK_LABEL = "Deck";
	private const string PLAYED_LABEL = "Played";
	private const string NEWLINE = "\n";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables
	public void Setup(){
		values = new Card[] { new Card(1), 
						      new Card(1), 
						      new Card(2),
						      new Card(2),
						      new Card(3),
						      new Card(3),
						      new Card(4),
						      new Card(4) };
		attackerDeck = new CardDeck<Card>(values);
		cardsInDeck = GameObject.Find(DECK_CANVAS).transform.Find(DECK_OBJ).GetComponent<Text>();
		cardsInDeck.text = UpdateCardsInDeckUI(attackerDeck.RemainingCards());
		playedCards = GameObject.Find(DECK_CANVAS).transform.Find(PLAYED_CARDS_OBJ).GetComponent<Text>();
		playedCards.text = PLAYED_LABEL + NEWLINE;
	}


	/// <summary>
	/// Draw a card from the attacker deck, and then make sure the UI is up-to-date.
	/// </summary>
	/// <returns>The drawn card.</returns>
	public Card GetAttackerCard(){
		Card temp = attackerDeck.Draw();

		List<Card> remainingCards = attackerDeck.RemainingCards();
		cardsInDeck.text = UpdateCardsInDeckUI(remainingCards);
		playedCards.text = UpdatePlayedCardsUI(temp);

		return temp;
	}


	/// <summary>
	/// Helps display cards still in the attacker deck.
	/// 
	/// remainingCards is passed by reference, but it's a reference to a temporary list created by CardDeck.RemainingCards(),
	/// so the "true" deck is not affected.
	/// </summary>
	/// <returns>The cards still in the deck, as a string ordered lowest-highest.</returns>
	/// <param name="remainingCards">A list of cards still in the deck.</param>
	private string UpdateCardsInDeckUI(List<Card> remainingCards){
		string newText = DECK_LABEL + NEWLINE;
		remainingCards.Sort((card1, card2) => (int)card1.Value.CompareTo((int)card2.Value));

		foreach (Card card in remainingCards){
			newText += card.Value + NEWLINE;
		}

		return newText;
	}


	/// <summary>
	/// Displays cards the attacker's discard.
	/// </summary>
	/// <returns>The played cards U.</returns>
	/// <param name="newCard">New card.</param>
	private string UpdatePlayedCardsUI(Card newCard){
		playedCardList.Add(newCard);
		playedCardList.Sort((card1, card2) => (int)card1.Value.CompareTo((int)card2.Value));

		if (playedCardList.Count == attackerDeck.GetDeckSize()){
			playedCardList.Clear();
			return PLAYED_LABEL + NEWLINE;
		} else {
			string newText = PLAYED_LABEL + NEWLINE;

			foreach(Card card in playedCardList){
				newText += card.Value + NEWLINE;
			}

			return newText;
		}
	}
}
