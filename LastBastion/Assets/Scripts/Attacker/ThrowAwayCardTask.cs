using TMPro;
using UnityEngine;

public class ThrowAwayCardTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the deck this card will be discarded from
	private readonly Transform startLoc;


	//the attacker discarding the card
	private readonly Transform endLoc;


	//the card to be discarded
	private RectTransform card;
	private readonly int value;
	private const string CARD_OBJ = "Combat card";
	private const string VALUE_OBJ = "Value";


	//the card's movement
	private float speed = 15.0f;
	private float rotSpeed = 135.0f; //degrees/second
	private const float OFF_SCREEN = -200.0f;
	private float tolerance = 0.5f; //when the card gets this close to its destination, it's done


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public ThrowAwayCardTask(Transform startLoc, Transform endLoc, int value){
		this.startLoc = startLoc;
		this.endLoc = endLoc;
		this.value = value;
	}


	/// <summary>
	/// Create a card to discard, and put its value on its face
	/// </summary>
	protected override void Init (){
		card = MonoBehaviour.Instantiate(Resources.Load<GameObject>(CARD_OBJ), 
				startLoc).GetComponent<RectTransform>();
		card.name = endLoc.name + CARD_OBJ;
		card.position = startLoc.position;
		card.Find(VALUE_OBJ).GetComponent<TextMeshProUGUI>().text = value.ToString();
	}


	/// <summary>
	/// Move the card off the screen, spinning it as it goes.
	/// </summary>
	public override void Tick (){
		card.Translate((endLoc.position - card.position).normalized * speed * Time.deltaTime, Space.World);

		card.Rotate(Vector3.forward, rotSpeed * Time.deltaTime);

		if (Vector3.Distance(card.position, endLoc.position) <= tolerance) SetStatus(TaskStatus.Success);
	}


	/// <summary>
	/// The card is now off-screen; destroy it to free up resources.
	/// </summary>
	protected override void OnSuccess (){
		MonoBehaviour.Destroy(card.gameObject);
	}
}
