using TMPro;
using UnityEngine;

public class PutInCardTask : Task {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the attacker putting the card into the deck
	private readonly Transform startLoc;


	//the deck this card will be put into
	private readonly Transform endLoc;


	//the card to be added
	private Transform card;
	private readonly int value;
	private const string CARD_OBJ = "Combat card";
	private const string VALUE_OBJ = "Value";


	//a canvas to display the card, but where it won't be subject to destruction by AttackerDeck's RemoveCard functions
	private const string DECK_CANVAS = "Attacker deck canvas";


	//the card's movement
	private float speed = 15.0f;
	private const float OFF_SCREEN = -200.0f;
	private Vector3 direction;
	private const float tolerance = 0.5f; //when the card is this close to the destination, it's done moving


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public PutInCardTask(Transform startLoc, Transform endLoc, int value){
		this.startLoc = startLoc;
		this.endLoc = endLoc;
		this.value = value;
	}


	/// <summary>
	/// Create the card that will be added to the deck, and put its value on its face
	/// </summary>
	protected override void Init (){
		card = MonoBehaviour.Instantiate<GameObject>(Resources.Load<GameObject>(CARD_OBJ),
													 GameObject.Find(DECK_CANVAS).transform).transform;
		card.position = startLoc.position;
		card.Find(VALUE_OBJ).GetComponent<TextMeshProUGUI>().text = value.ToString();

		direction = (endLoc.position - startLoc.position).normalized;
	}


	/// <summary>
	/// Each frame, move the card toward the deck to which it's being added.
	/// </summary>
	public override void Tick (){
		card.Translate(direction * speed * Time.deltaTime, Space.World);

		if (Vector3.Distance(card.position, endLoc.position) <= tolerance){
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
