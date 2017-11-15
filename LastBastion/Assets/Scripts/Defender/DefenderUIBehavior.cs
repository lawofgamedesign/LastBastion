/// <summary>
/// It's much easier, with Unity, to have a script that's always in the scene for buttons to work with than to assign
/// the button's function at runtime.
/// 
/// This script serves that purpose.
/// </summary>
using UnityEngine;
using UnityEngine.UI;

public class DefenderUIBehavior : MonoBehaviour {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the color a card turns upon being selected
	private Color unselectedColor = Color.white;
	private Color selectedColor = Color.blue;
	private Color unavailableColor = Color.red;


	//for flipping cards face-down
	private const float FACE_DOWN_Y = 180.0f;
	private const string CARD_BACK_OBJ = "Card back";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables
	public void Setup(){
		//no action required
	}


	/// <summary>
	/// Called when a card is clicked.
	/// </summary>
	/// <param name="card">The card clicked, numbered left to right, zero-indexed.</param>
	public void OnCardClicked(int index){
		Services.Defenders.HandleCardChoice(index);
	}


	public void FlipCardDown(int index){
		transform.GetChild(index).Find(CARD_BACK_OBJ).gameObject.SetActive(true);
	}


	/// <summary>
	/// Turn all cards over, by setting their local rotation and displaying the card back.
	/// </summary>
	public void FlipAllCardsDown(){
		foreach (RectTransform child in transform){
			//child.localRotation = Quaternion.Euler(new Vector3(child.localRotation.eulerAngles.x, FACE_DOWN_Y, child.localRotation.eulerAngles.z));
			child.Find(CARD_BACK_OBJ).gameObject.SetActive(true);
		}
	}


	public void FlipCardUp(int index){
		transform.GetChild(index).Find(CARD_BACK_OBJ).gameObject.SetActive(false);
	}


	public void FlipAllCardsUp(){
		foreach (RectTransform child in transform) child.Find(CARD_BACK_OBJ).gameObject.SetActive(false);

	}


	/// <summary>
	/// Deactivates the cards' gameobjects when the player is done with them.
	/// </summary>
	public void ShutCardsOff(){
		foreach (Transform child in transform) child.gameObject.SetActive(false);
	}


	public void TurnSelectedColor(int index){
		foreach (RectTransform child in transform){
			if (child.GetComponent<Image>().color == selectedColor) child.GetComponent<Image>().color = unselectedColor;
		}

		transform.GetChild(index).GetComponent<Image>().color = selectedColor;
	}


	/// <summary>
	/// Indicates that there is no currently-selected card by changing all cards' colors to the unselected color.
	/// </summary>
	public void ClearAllSelectedColor(){
		foreach (Transform child in transform) child.GetComponent<Image>().color = unselectedColor;
	}


	public void TurnUnavailableColor(int index){
		transform.GetChild(index).GetComponent<Image>().color = unavailableColor;
	}
}
