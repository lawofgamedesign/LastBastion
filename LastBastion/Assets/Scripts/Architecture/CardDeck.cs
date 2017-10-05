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
}
