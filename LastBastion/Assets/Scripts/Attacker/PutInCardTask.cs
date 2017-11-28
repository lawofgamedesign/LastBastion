using UnityEngine;
using UnityEngine.UI;

public class PutInCardTask : Task {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the deck this card will be put into
	private readonly Transform endLoc;


	//the card to be added
	private Transform card;
	private readonly int value;
	private const string CARD_OBJ = "Combat card";
	private const string VALUE_OBJ = "Value";


	//the card's movement
	private float speed = 250.0f;
	private const float OFF_SCREEN = -200.0f;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public PutInCardTask(Transform endLoc, int value){
		this.endLoc = endLoc;
		this.value = value;
	}


	/// <summary>
	/// Create the card that will be added to the deck, and put its value on its face
	/// </summary>
	protected override void Init (){
		card = MonoBehaviour.Instantiate<GameObject>(Resources.Load<GameObject>(CARD_OBJ),
													 endLoc).transform;
		card.localPosition = new Vector3(OFF_SCREEN, 0.0f, 0.0f);
		card.Find(VALUE_OBJ).GetComponent<Text>().text = value.ToString();
	}


	/// <summary>
	/// Each frame, move the card toward the deck to which it's being added.
	/// </summary>
	public override void Tick (){
		card.localPosition += Vector3.right * speed * Time.deltaTime;

		if (card.localPosition.x >= 0.0f){
			SetStatus(TaskStatus.Success);
		}
	}


	/// <summary>
	/// The card is now in the deck; destroy it so that it doesn't give the impression there are more cards in the deck than there are.
	/// </summary>
	protected override void OnSuccess (){
		MonoBehaviour.Destroy(card.gameObject);
	}
}
