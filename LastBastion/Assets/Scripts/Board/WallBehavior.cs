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
	private string MOVABLES_ORGANIZER = "Movables";


	//fx for loss of durability
	private GameObject guardHitParticle;
	private GameObject noDamageParticle;
	private const string GUARD_HIT_PARTICLE = "Guard hit particle";
	private const string NO_DAMAGE_PARTICLE = "No damage particle";



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
		noDamageParticle = transform.Find(NO_DAMAGE_PARTICLE).gameObject;
		Strength = startStrength;
	}


	//alter the strength of this wall, and trigger associated feedback
	public void ChangeDurability(int change){
		if (Durability > 0){
			Vector3 guardPosition = transform.Find(MOVABLES_ORGANIZER).Find(GUARD_LABEL + Durability.ToString()).localPosition;
			guardHitParticle.transform.localPosition = new Vector3(guardPosition.x, 
																   guardHitParticle.transform.localPosition.y,
																   guardHitParticle.transform.localPosition.z);
			guardHitParticle.SetActive(false);
			guardHitParticle.SetActive(true);
			Services.Tasks.AddTask(new GuardFallTask(transform.Find(MOVABLES_ORGANIZER).Find(GUARD_LABEL + Durability.ToString()).GetComponent<Rigidbody>()));
			Durability += change;
		}
	}


	/// <summary>
	/// FX for when attackers besiege the wall, and draw a card that does not exceed its strength.
	/// </summary>
	public void NoDamageEffects(){
		if (Durability > 0){
			Vector3 guardPosition = transform.Find(MOVABLES_ORGANIZER).Find(GUARD_LABEL + Durability.ToString()).localPosition;
			noDamageParticle.transform.localPosition = new Vector3(guardPosition.x, 
																   noDamageParticle.transform.localPosition.y,
																   noDamageParticle.transform.localPosition.z);
			noDamageParticle.SetActive(false);
			noDamageParticle.SetActive(true);
		}
	}


	/// <summary>
	/// Lift the wall so that something can move through it.
	/// </summary>
	public void LiftWall(){
		LiftWallTask liftTask = new LiftWallTask(transform.Find(MOVABLES_ORGANIZER), LiftWallTask.UpOrDown.Up);
		WaitForSecondsTask waitTask = new WaitForSecondsTask(0.75f);
		liftTask.Then(waitTask);
		waitTask.Then(new LiftWallTask(transform.Find(MOVABLES_ORGANIZER), LiftWallTask.UpOrDown.Down));
		Services.Tasks.AddTask(liftTask);
	}
}
