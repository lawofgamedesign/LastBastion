using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttackerDeck {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	private Card[] values; //the cards in the deck

	private CardDeck<Card> attackerDeck; //the deck as a whole


	//the UI for the deck
	private TextMeshProUGUI cardsInDeck;
	private TextMeshProUGUI playedCards;
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
		cardsInDeck = GameObject.Find(DECK_CANVAS).transform.Find(DECK_OBJ).GetComponent<TextMeshProUGUI>();
		cardsInDeck.text = UpdateCardsInDeckUI(attackerDeck.RemainingCards());
		playedCards = GameObject.Find(DECK_CANVAS).transform.Find(PLAYED_CARDS_OBJ).GetComponent<TextMeshProUGUI>();
		playedCards.text = PLAYED_LABEL + NEWLINE;
	}


	/// <summary>
	/// Draw a card from the attacker deck, and then make sure the UI is up-to-date.
	/// </summary>
	/// <returns>The drawn card.</returns>
	public Card GetAttackerCard(){
		bool reshuffled = false; //did this draw pull the last card in the deck?
		Card temp = attackerDeck.Draw(out reshuffled);


		//display the drawn card
		if (reshuffled) Services.UI.RecreateCombatDeck();
		Services.UI.DrawCombatCard(temp.Value);

		//lifespan function
		temp.Drawn();

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
	/// <returns>The played cards.</returns>
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


	/// <summary>
	/// As above, but does not involve a new card being played.
	/// </summary>
	/// <returns>The played cards.</returns>
	/// <param name="newCard">New card.</param>
	private string UpdatePlayedCardsUI(){
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


	/// <summary>
	/// Take a card with the given value out of the deck, and then update the UI accordingly.
	/// 
	/// This favors later cards in the list of cards in the deck, which are more likely to still be available to draw.
	/// </summary>
	/// <param name="attacker">The attacker removing the card.</param>
	/// <param name="value">The value of the card to remove.</param>
	public void RemoveCardFromDeck(Transform attacker, int value){
		if (TakeOutCard(attacker, value)){
			List<Card> remainingCards = attackerDeck.RemainingCards();
			cardsInDeck.text = UpdateCardsInDeckUI(remainingCards);
			playedCards.text = UpdatePlayedCardsUI();
		}
	}

	/// <summary>
	/// Remove a single card from the deck, preferring cards still to be drawn.
	/// </summary>
	/// <returns><c>true</c> if a card was removed, <c>false</c> otherwise.</returns>
	/// <param name="attacker">The attacker removing the card.</param>
	/// <param name="value">The Value of the card to remove.</param>
	private bool TakeOutCard(Transform attacker, int value){
		List<Card> tempDeck = attackerDeck.GetDeck();
		bool tookOut = false;

		for (int i = tempDeck.Count - 1; i >= 0; i--){
			if (tempDeck[i].Value == value){
				attackerDeck.RemoveCard(tempDeck[i]);
				if (playedCardList.Contains(tempDeck[i])){
					playedCardList.Remove(tempDeck[i]);
					Services.UI.RemoveCardFromDiscard(attacker, value);
				} else {
					Services.UI.RemoveCardFromDeck(attacker, value);
				}
				tookOut = true;
				break;
			}
		}

		return tookOut;
	}


	/// <summary>
	/// Adds a card to the deck.
	/// </summary>
	/// <param name="attacker">The attacker putting the card into the deck.</param>
	/// <param name="value">The card's value.</param>
	public void PutCardInDeck(Transform attacker, int value){
		attackerDeck.AddCard(new Card(value));
		List<Card> remainingCards = attackerDeck.RemainingCards();
		cardsInDeck.text = UpdateCardsInDeckUI(remainingCards);
		Services.UI.AddCardToDeck(attacker, value);
	}


	/// <summary>
	/// As above, but adds a card rather than making a new card with a given value; this allows for calling the Added() function on the card.
	/// </summary>
	/// <param name="attacker">The attacker putting the card into the deck.</param>
	/// <param name="card">The card to add.</param>
	public void PutCardInDeck(Transform attacker, Card card){
		attackerDeck.AddCard(card);
		card.Added();
		List<Card> remainingCards = attackerDeck.RemainingCards();
		cardsInDeck.text = UpdateCardsInDeckUI(remainingCards);
		Services.UI.AddCardToDeck(attacker, card.Value);
	}


	/// <summary>
	/// Reshuffle the deck, e.g., between waves.
	/// </summary>
	public void Reshuffle(){
		attackerDeck.Reshuffle();
		cardsInDeck.text = UpdateCardsInDeckUI(attackerDeck.RemainingCards());
		playedCardList.Clear();
		playedCards.text = UpdatePlayedCardsUI();
	}


	/// <summary>
	/// Makes the size of the deck accessible. Note that this is the size of the deck
	/// with no cards drawn.
	/// </summary>
	/// <returns>The number of cards in the deck.</returns>
	public int GetDeckCount(){
		return attackerDeck.GetDeckSize();
	}
}
