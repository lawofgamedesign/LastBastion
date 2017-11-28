using UnityEngine;
using UnityEngine.UI;

public class ThrowAwayCardTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the deck this card will be discarded from
	private readonly Transform startLoc;


	//the card to be discarded
	private Transform card;
	private readonly int value;
	private const string CARD_OBJ = "Combat card";
	private const string VALUE_OBJ = "Value";


	//the card's movement
	private float speed = -50.0f;
	private float rotSpeed = 135.0f; //degrees/second
	private const float OFF_SCREEN = -200.0f;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public ThrowAwayCardTask(Transform startLoc, int value){
		this.startLoc = startLoc;
		this.value = value;
	}


	/// <summary>
	/// Create a card to discard, and put its value on its face
	/// </summary>
	protected override void Init (){
		card = MonoBehaviour.Instantiate(Resources.Load<GameObject>(CARD_OBJ), 
										 startLoc).transform;
		card.position = startLoc.position;
		card.Find(VALUE_OBJ).GetComponent<Text>().text = value.ToString();
	}


	/// <summary>
	/// Move the card off the screen, spinning it as it goes.
	/// </summary>
	public override void Tick (){
		card.localPosition += Vector3.right * speed * Time.deltaTime;
		card.Rotate(Vector3.forward, rotSpeed * Time.deltaTime);

		if (card.localPosition.x <= OFF_SCREEN) SetStatus(TaskStatus.Success);
	}


	/// <summary>
	/// The card is now off-screen; destroy it to free up resources.
	/// </summary>
	protected override void OnSuccess (){
		MonoBehaviour.Destroy(card.gameObject);
	}
}
