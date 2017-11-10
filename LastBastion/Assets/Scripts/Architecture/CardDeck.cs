/// <summary>
/// This is a base class for combat decks. It provides basic functionality: make a deck, shuffle it, get values out of it, if the deck
/// is empty reshuffle it and start over.
/// </summary>
using System.Collections.Generic;
using UnityEngine;

public class CardDeck<T> {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the deck
	protected List<T> deck;


	//where we are in the deck
	protected int index = 0;


	//return value of List<T>.LastIndexOf when the item isn't in the list
	protected const int NOT_FOUND = -1;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor; produces a shuffled deck
	public CardDeck(T[] values){
		deck = new List<T>();

		for (int i = 0; i < values.Length; i++){
			deck.Add(values[i]);
		}

		deck = ShuffleDeck(deck);
	}


	/// <summary>
	/// Draw a new card. If the deck is going to be empty after the draw, do whatever is appropriate for the game.
	/// </summary>
	public T Draw(){
		Debug.Assert(index < deck.Count, "Drawing from an empty deck.");

		T temp = deck[index];
		index++;

		if (index >= deck.Count){
			HandleEmptyDeck();
		}

		return temp;
	}


	/// <summary>
	/// Do whatever should be done when the deck is empty. By default, reshuffle the deck to be used again.
	/// </summary>
	protected virtual void HandleEmptyDeck(){
		deck = ShuffleDeck(deck);
		index = 0;
	}


	/// <summary>
	/// Shuffles a deck using the Fisher-Yates algorithm.
	/// </summary>
	/// <returns>The deck, as a shuffled list.</returns>
	/// <param name="startingDeck">The deck before shuffling.</param>
	protected List<T> ShuffleDeck(List<T> startingDeck){
		System.Random random = new System.Random();

		int n = startingDeck.Count;
		T temp;

		for (int i = 0; i < n; i++){
			int newRandom = i + (int)(random.NextDouble() * (n - i));
			temp = startingDeck[newRandom];
			startingDeck[newRandom] = startingDeck[i];
			startingDeck[i] = temp;
		}

		return startingDeck;
	}


	/// <summary>
	/// Get a list of all cards currently in the deck.
	/// </summary>
	/// <returns>The list.</returns>
	/// <param name="currentDeck">The deck to be checked.</param>
	public List<T> RemainingCards(){
		List<T> temp = new List<T>();

		for (int i = index; i < deck.Count; i++){
			temp.Add(deck[i]);
		}
			
		Debug.Assert(temp.Count == deck.Count - index, "Did not add all remaining cards");


		return temp;
	}


	/// <summary>
	/// Get a list of everything in the deck
	/// </summary>
	/// <returns>The list.</returns>
	public List<T> GetDeck(){
		List<T> temp = new List<T>();

		foreach (T card in deck) temp.Add(card);

		Debug.Assert(temp.Count == deck.Count, "Did not add all cards");

		return temp;
	}


	/// <summary>
	/// Convenience method for getting the number of cards in the deck when it is full.
	/// </summary>
	/// <returns>The total number of cards in the deck when it is full.</returns>
	public int GetDeckSize(){
		return deck.Count;
	}


	/// <summary>
	/// Take a card out of the deck.
	/// 
	/// This removes the last instance of the card, so it will attempt to remove an instance that's still in the deck
	/// to be drawn before removing from those already drawn.
	/// </summary>
	/// <param name="card">The card to remove.</param>
	public void RemoveCard(T card){
		int temp = deck.LastIndexOf(card);

		if (temp == NOT_FOUND) return; //stop if there was no such card

		if (index <= temp && index > 0) index--;

		deck.RemoveAt(temp);
	}


	/// <summary>
	/// Put a card into the deck.
	/// 
	/// This function always adds the card into the still-to-be-drawn deck, never into the discard.
	/// </summary>
	/// <param name="card">The card to add.</param>
	public void AddCard(T card){
		int tempIndex = Random.Range(index, deck.Count);

		deck.Insert(tempIndex, card);
	}
}
