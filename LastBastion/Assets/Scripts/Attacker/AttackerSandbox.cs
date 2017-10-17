/// <summary>
/// This is a base class for all Attackers. It includes all of the Attackers' "verbs"--everything they can do.
/// 
/// All Attackers inherit from this.
/// </summary>
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AttackerSandbox : MonoBehaviour {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//position in the grid and speed
	public int XPos { get; set; }
	public int ZPos { get; set; }
	[SerializeField] protected int speed = 1; //speed in spaces/move, not 3D world speed

	//attacker stats
	public int AttackMod { get; set; }
	public int Armor { get; set; }
	public int Health { get; set; }
	protected int baseHealth = 1;


	//has this attacker already fought this turn?
	public bool FoughtThisTurn { get; set; }


	//how much damage does this attacker do to walls?
	public int SiegeStrength { get; private set; }
	[SerializeField] int startSiegeStrength = 1;


	//did this attacker just spawn? If so, it won't move this turn
	public bool SpawnedThisTurn { get; set; }


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables
	public void Setup(){
		FoughtThisTurn = false;
		SiegeStrength = startSiegeStrength;
		SpawnedThisTurn = true;
		AttackMod = 0;
		Armor = 0;
		Health = baseHealth;
	}


	//set this attacker's position
	public void NewLoc(int x, int z){
		XPos = x;
		ZPos = z;
	}


	/// <summary>
	/// Move this attacker.
	/// </summary>
	public void TryMoveSouth(){
		//don't move if this attacker spawned this turn, but get ready to move next turn
		if (SpawnedThisTurn){
			SpawnedThisTurn = false;
			return;
		}


		//sanity check; prevent this attacker from trying to move off the board
		int attemptedMove = speed;
		if (ZPos - attemptedMove < 0) attemptedMove = ZPos;


		//if the space the attacker wants to move to is empty, go there.
		//this moves by spaces in the grid; MoveTask is responsible for having grid positions turned into world coordinates
		if (Services.Board.GeneralSpaceQuery(XPos, ZPos - attemptedMove) == SpaceBehavior.ContentType.None){

			//is this enemy trying to move through the wall? If so, block the move.
			if (ZPos - attemptedMove == Services.Board.WallZPos){
				if (Services.Board.GetWallDurability(XPos) > 0) return;
			}

			//OK, not moving through a wall. Leave the current space, go into the new space, move on-screen, and update this attacker's
			//understanding of its own position
			Services.Board.TakeThingFromSpace(XPos, ZPos);
			Services.Board.PutThingInSpace(gameObject, XPos, ZPos - attemptedMove, SpaceBehavior.ContentType.Attacker);
			Services.Tasks.AddTask(new MoveTask(transform, XPos, ZPos - attemptedMove, Services.Attackers.MoveSpeed));
			NewLoc(XPos, ZPos - attemptedMove);
		}
	}


	/// <summary>
	/// A publicly-accessible way to find out what column this attacker is in.
	/// 
	/// Used for, frex., besieging the corerct wall.
	/// </summary>
	/// <returns>The column, zero-indexed.</returns>
	public int GetColumn(){
		return XPos;
	}


	public void TakeDamage(int damage){
		Health -= damage;

		if (Health <= 0) {
			Services.Attackers.EliminateAttacker(this);
			Services.Board.TakeThingFromSpace(XPos, ZPos);
			Services.Tasks.AddTask(new AttackerFallTask(GetComponent<Rigidbody>()));
		}
	}


	public void FailToDamage(){
		Debug.Log("Attacker not damaged!");
	}
}
