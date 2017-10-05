/// <summary>
/// A class for the wall; most notably, includes its current strength.
/// </summary>
using UnityEngine;

public class WallBehavior : MonoBehaviour {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the wall's current strength; accessed by other scripts
	public int Durability { get; set; }
	[SerializeField] private int startDurability = 3;


	//the guards representing the wall's strength
	private GameObject guard1;
	private GameObject guard2;
	private GameObject guard3;
	private const string GUARD_LABEL = "Guard ";


	//fx for loss of durability
	private GameObject guardHitParticle;
	private const string GUARD_HIT_PARTICLE = "Guard hit particle";


	//the wall's defensive strength; attackers must get a higher value than this to do damage
	public int Strength { get; set; }
	[SerializeField] private int startStrength = 1;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables
	public void Setup(){
		Durability = startDurability;
		guardHitParticle = transform.Find(GUARD_HIT_PARTICLE).gameObject;
		Strength = startStrength;
	}


	//alter the strength of this wall, and trigger associated feedback
	public void ChangeDurability(int change){
		if (Durability > 0){
			Vector3 guardPosition = transform.Find(GUARD_LABEL + Durability.ToString()).localPosition;
			guardHitParticle.transform.localPosition = new Vector3(guardPosition.x, 
																   guardHitParticle.transform.localPosition.y,
																   guardHitParticle.transform.localPosition.z);
			guardHitParticle.SetActive(false);
			guardHitParticle.SetActive(true);
			Services.Tasks.AddTask(new GuardFallTask(transform.Find(GUARD_LABEL + Durability.ToString()).GetComponent<Rigidbody>()));
			Durability += change;
		}
	}
}
