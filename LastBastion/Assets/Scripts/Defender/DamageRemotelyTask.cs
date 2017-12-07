using UnityEngine;

public class DamageRemotelyTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//where the damage is to be done
	private readonly TwoDLoc loc;


	//how much damage should be done
	private readonly int damage;


	//who's responsible for the damage
	private readonly DefenderSandbox defender;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public DamageRemotelyTask(TwoDLoc loc, int damage, DefenderSandbox defender){
		this.loc = loc;
		this.damage = damage;
		this.defender = defender;
	}


	/// <summary>
	/// Try to damage any attackers in the relevant space, and then be done.
	/// </summary>
	public override void Tick (){
		if (Services.Board.GeneralSpaceQuery(loc.x, loc.z) == SpaceBehavior.ContentType.Attacker){
			AttackerSandbox attacker = Services.Board.GetThingInSpace(loc.x, loc.z).GetComponent<AttackerSandbox>();

			//credit the defender who's doing the damage with defeating the attacker, if appropriate
			if (attacker.Health <= damage) defender.DefeatAttacker();

			attacker.TakeDamage(damage);
		}

		SetStatus(TaskStatus.Success);
	}
}
