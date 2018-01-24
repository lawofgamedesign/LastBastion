﻿using UnityEngine;

public class EnvironmentManager {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the places the game can take place
	public enum Place { Kitchen, Tavern };


	//the current place
	private Place currentPlace;


	//tavern environment settings
	private const string TAVERN_LIGHT_HEX = "#7E7E7EFF";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables
	public void Setup(){
		currentPlace = Place.Kitchen;
	}


	//When it's time to change the environment, call this.
	public void ChangeEnvironment(Place newPlace){
		ChangeFogTask fogGrows = new ChangeFogTask(ChangeFogTask.DenseOrLight.Dense);

		ChangeEnvironmentTask changePlace = new ChangeEnvironmentTask(newPlace, currentPlace, GetNextColor(newPlace));
		currentPlace = newPlace;

		fogGrows.Then(changePlace);
		changePlace.Then(new ChangeFogTask(ChangeFogTask.DenseOrLight.Light));

		Services.Tasks.AddTask(fogGrows);
	}


	/// <summary>
	/// Get the next location in which the game will take place.
	/// </summary>
	/// <returns>The next location.</returns>
	public Place GetNextPlace(){
		Place temp = Place.Kitchen; //default initialization

		switch (currentPlace){
			case Place.Kitchen:
				temp = Place.Tavern;
				break;
		}

		Debug.Assert(temp != Place.Kitchen, "Failed to get next place.");

		return temp;
	}


	private Color GetNextColor(Place newPlace){
		Color temp = Color.white; //default initializtion

		switch (newPlace){
			case Place.Tavern:
			ColorUtility.TryParseHtmlString(TAVERN_LIGHT_HEX, out temp);
			break;
		}

		Debug.Assert(temp != Color.white, "Could not get next color");

		return temp;
	}
}