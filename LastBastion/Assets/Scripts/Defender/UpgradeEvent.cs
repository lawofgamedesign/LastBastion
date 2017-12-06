using UnityEngine;

public class UpgradeEvent : Event {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//who upgraded to what
	public readonly GameObject defender;
	public readonly string upgradeName;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public UpgradeEvent(GameObject defender, string upgradeName){
		this.defender = defender;
		this.upgradeName = upgradeName;
	}
}
