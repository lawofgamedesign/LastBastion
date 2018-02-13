using TMPro;
using UnityEngine;

public class ExplainButtonBehavior : MonoBehaviour {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//is the button asking for an explanation, or saying you're done with an explanation?
	public enum CurrentState { Request, OK };
	private CurrentState currentState = CurrentState.Request;


	//the button's text
	private TextMeshProUGUI buttonText;
	private const string TEXT_OBJ = "Text";
	private const string REQUEST_MSG = "What do your pieces do?";
	private const string OK_MSG = "OK, thanks.";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables
	public void Setup(){
		buttonText = transform.Find(TEXT_OBJ).GetComponent<TextMeshProUGUI>();
	}


	//called when the button is pressed
	public void RequestExplanation(){
		Services.Events.Fire(new ExplanationEvent(currentState));

		currentState = SwapState();

		buttonText.text = SetText();
	}


	/// <summary>
	/// Switch between the explanation button's two states.
	/// </summary>
	/// <returns>The new state.</returns>
	private CurrentState SwapState(){
		if (currentState == CurrentState.Request) return CurrentState.OK;
		else return CurrentState.Request;
	}


	/// <summary>
	/// Provides the explanation button's text for each state.
	/// </summary>
	/// <returns>The text.</returns>
	private string SetText(){
		if (currentState == CurrentState.Request) return REQUEST_MSG;
		else return OK_MSG;
	}
}
