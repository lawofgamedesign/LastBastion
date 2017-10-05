/// <summary>
/// This is a base class for all Attackers. It includes all of the Attackers' "verbs"--everything they can do.
/// 
/// All Attackers inherit from this.
/// </summary>
using UnityEngine;

public class AttackerSandbox : MonoBehaviour {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//position in the grid and speed
	protected int xPos { get; set; }
	protected int zPos { get; set; }
	[SerializeField] protected int speed = 1; //speed in spaces/move, not 3D world speed


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
	}


	//set this attacker's position
	public void NewLoc(int x, int z){
		xPos = x;
		zPos = z;
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
		if (zPos - attemptedMove < 0) attemptedMove = zPos;


		//if the space the attacker wants to move to is empty, go there.
		//this moves by spaces in the grid; MoveTask is responsible for having grid positions turned into world coordinates
		if (Services.Board.GeneralSpaceQuery(xPos, zPos - attemptedMove) == SpaceBehavior.ContentType.None){

			//is this enemy trying to move through the wall? If so, block the move.
			if (zPos - attemptedMove == Services.Board.WallZPos){
				if (Services.Board.GetWallDurability(xPos) > 0) return;
			}

			//OK, not moving through a wall. Leave the current space, go into the new space, move on-screen, and update this attacker's
			//understanding of its own position
			Services.Board.TakeThingFromSpace(xPos, zPos);
			Services.Board.PutThingInSpace(gameObject, xPos, zPos - attemptedMove, SpaceBehavior.ContentType.Attacker);
			Services.Tasks.AddTask(new MoveTask(transform, xPos, zPos - attemptedMove, Services.Attackers.MoveSpeed));
			NewLoc(xPos, zPos - attemptedMove);
		}
	}


	/// <summary>
	/// A publicly-accessible way to find out what column this attacker is in.
	/// 
	/// Used for, frex., besieging the corerct wall.
	/// </summary>
	/// <returns>The column, zero-indexed.</returns>
	public int GetColumn(){
		return xPos;
	}
}
