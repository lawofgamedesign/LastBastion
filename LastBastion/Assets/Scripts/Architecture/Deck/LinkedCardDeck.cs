using System.Collections.Generic;
using UnityEngine;

public class LinkedCardDeck {

	//the deck
	protected LinkedList<LinkedCard> deck;


	//constructor; produces a shuffled deck
	public LinkedCardDeck(LinkedCard[] values){
		Debug.Assert(values.Length > 0, "Trying to make a deck without values");

		deck = new LinkedList<LinkedCard>();

		deck.AddFirst(values[0]);

		for (int i = 1; i < values.Length; i++){
			deck.AddLast(values[i]);
		}

		Debug.Assert(deck.Count == values.Length, "Failed to add all cards");

		ShuffleDeck();
	}


	/// <summary>
	/// Iterate through the deck until encountering a card that hasn't been drawn. When one is found,
	/// set it as drawn and then draw it.
	/// 
	/// If none is found, shuffle the deck and start over.
	/// </summary>
	/// <param name="reshuffled">True if the deck had to be reshuffled to get a card, false otherwise.</param>
	public LinkedCard Draw(out bool reshuffled){
		reshuffled = false;
		LinkedListNode<LinkedCard> currentCard = deck.First;

		while (currentCard.Value.CheckIfDrawn()){
			if (currentCard.Value != null) currentCard = currentCard.Next;
			else {
				ShuffleDeck();
				reshuffled = true;
				currentCard = deck.First;
			}
		}

		currentCard.Value.DrawThis();
		return currentCard.Value;
	}


	/// <summary>
	/// Shuffle the deck by converting it into a list and then Fisher-Yates shuffling the list.
	/// Then rebuild the linked list, setting each card in it to not have been drawn.
	/// </summary>
	public void ShuffleDeck(){
		if (deck.Count < 2) return; //sanity check; don't try to shuffle a 1-card deck

		List<LinkedCard> temp = new List<LinkedCard>();

		for (LinkedListNode<LinkedCard> card = deck.First; card != null; card = card.Next){
			temp.Add(card.Value);
		}

		Debug.Assert(temp.Count == deck.Count, "Failed to find all cards for shuffling");

		temp = ShuffleList(temp);

		deck.Clear();

		deck.AddFirst(temp[0]);
		deck.First.Value.BackToDeck();

		for (int i = 1; i < temp.Count; i++){
			deck.AddLast(temp[i]);
			deck.Last.Value.BackToDeck();
		}

		Debug.Assert(deck.Count == temp.Count, "Failed to add all cards back in");
	}


	/// <summary>
	/// Randomize a list using the Fisher-Yates algorithm.
	/// </summary>
	/// <returns>The randomized list.</returns>
	/// <param name="startingList">The list to be randomized list.</param>
	protected List<LinkedCard> ShuffleList(List<LinkedCard> startingList){
		System.Random random = new System.Random();

		int n = startingList.Count;
		LinkedCard temp;

		for (int i = 0; i < n; i++){
			int newRandom = i + (int)(random.NextDouble() * (n - i));
			temp = startingList[newRandom];
			startingList[newRandom] = startingList[i];
			startingList[i] = temp;
		}

		return startingList;
	}


	/// <summary>
	/// Remove a card from the deck. This will try to remove a card in the deck rather than
	/// a card in the discard.
	/// 
	/// Does not reshuffle because--unlike in the real world--the player does not
	/// see the cards going by while the search is occurring.
	/// </summary>
	/// <returns><c>true</c>, if a card was removed, <c>false</c> otherwise.</returns>
	/// <param name="number">The value of the card to be removed.</param>
	/// param name="removedFromDiscard">Was the card removed pulled from the discard?</param>
	public bool RemoveCard(int number, out bool removedFromDiscard){
		LinkedListNode<LinkedCard> currentCard = deck.Last; //start at the back; that's where undrawn cards will be
		removedFromDiscard = false;

		while (currentCard != null){
			if (currentCard.Value.Value == number){
				if (currentCard.Value.CheckIfDrawn()) removedFromDiscard = true;
				deck.Remove(currentCard);

				return true;
			} else {
				currentCard = currentCard.Previous;
			}
		}

		Debug.Assert(currentCard == null, "Fell out of loop with a card to test");

		return false;
	}


	/// <summary>
	/// Add a new card in a random position.
	/// </summary>
	/// <param name="number">The value of the new card.</param>
	public void AddCard(int number){
		LinkedListNode<LinkedCard> currentCard = deck.First;

		int numSteps = Random.Range(0, deck.Count + 1); //+1 because we want the ability to add after the last card

		while (numSteps > 0){
			currentCard = currentCard.Next;
		}


		//if currentCard is not null, add before that card
		//if currentCard is null, we want to add before a "ghost" last card--which can be accomplished with AddLast
		if (currentCard != null) deck.AddBefore(currentCard, new LinkedCard(number));
		else deck.AddLast(new LinkedCard(number));
	}


	public LinkedList<LinkedCard> GetDeck(){
		return deck;
	}
}
