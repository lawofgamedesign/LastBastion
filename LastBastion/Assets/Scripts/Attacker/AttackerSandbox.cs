﻿/// <summary>
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


	//is something stopping this attacker from moving?
	public bool Blocked { get; set; }


	//left and right; used for being lured to the side
	protected const int WEST = -1;
	protected const int EAST = 1;


	//for the UI, when the player needs info on this attacker
	protected string attackerName = "Skeleton";
	protected const string ATTACK = "Attack: ";
	protected const string ARMOR = "Armor: ";
	protected const string HEALTH = "Health: ";
	protected const string NEWLINE = "\n";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables
	public virtual void Setup(){
		FoughtThisTurn = false;
		SiegeStrength = startSiegeStrength;
		SpawnedThisTurn = true;
		AttackMod = 0;
		Armor = 0;
		Health = baseHealth;
		Blocked = false;
		RegisterForEvents();
	}


	#region events


	/// <summary>
	/// Call this to register for all events this attacker cares about.
	/// </summary>
	protected virtual void RegisterForEvents(){
		Services.Events.Register<BlockColumnEvent>(BecomeBlocked);
		Services.Events.Register<UnblockColumnEvent>(BecomeUnblocked);
	}


	/// <summary>
	/// Movement is blocked when this attacker receives an event indicating that its column cannot move.
	/// </summary>
	/// <param name="e">The BlockColumn event.</param>
	protected void BecomeBlocked(Event e){
		BlockColumnEvent blockEvent = e as BlockColumnEvent;

		if (blockEvent.Column == XPos) Blocked = true;
	}


	/// <summary>
	/// Movement is unblocked when this attacker receives a relevant event.
	/// </summary>
	/// <param name="e">The UnblockColumn event.</param>
	protected void BecomeUnblocked(Event e){
		UnblockColumnEvent unblockEvent = e as UnblockColumnEvent;

		if (unblockEvent.Column == XPos) Blocked = false;
	}


	/// <summary>
	/// Call this when the attacker is being taken off the board to unregister.
	/// </summary>
	protected virtual void UnregisterForEvents(){
		Services.Events.Unregister<BlockColumnEvent>(BecomeBlocked);
		Services.Events.Unregister<UnblockColumnEvent>(BecomeBlocked);
	}


	#endregion events


	//set this attacker's position
	public void NewLoc(int x, int z){
		XPos = x;
		ZPos = z;
	}


	/// <summary>
	/// This function manages limits and controls on movement, deciding whether and where the attacker should move.
	/// </summary>
	public void TryMove(){
		//don't move if this attacker spawned this turn, but get ready to move next turn
		if (SpawnedThisTurn){
			SpawnedThisTurn = false;
			return;
		}

		//don't move if the attacker is blocked by an "off the board" game effect.
		if (Blocked){
			return;
		}
			
		//move west or east if being lured there and there is space to do so
		//note that this privileges westward movement; it checks westward movement first, and will therefore go west in preference to being lured east
		if (Services.Board.CheckIfLure(XPos + WEST, ZPos)){
			if (TryMoveLateral(WEST)) return;
		} else if (Services.Board.CheckIfLure(XPos + EAST, ZPos)){
			if (TryMoveLateral(EAST)) return;
		}

		//if the attacker gets this far, it can make a normal move to the south.
		TryMoveSouth();
	}


	/// <summary>
	/// Move this attacker south a number of spaces based on their speed.
	/// </summary>
	protected void TryMoveSouth(){
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
	/// Try to move the attacker one space east or west.
	/// </summary>
	/// <returns><c>true</c> if the attacker was able to move into an empty space, <c>false</c> if the space was occupied, blocking movement.</returns>
	/// <param name="dir">The direction of movement, east (1) or west (-1).</param>
	protected bool TryMoveLateral(int dir){
		//if the space one to the east is empty, go there.
		if (Services.Board.GeneralSpaceQuery(XPos + dir, ZPos) == SpaceBehavior.ContentType.None){
			Services.Board.TakeThingFromSpace(XPos, ZPos);
			Services.Board.PutThingInSpace(gameObject, XPos + dir, ZPos, SpaceBehavior.ContentType.Attacker);
			Services.Tasks.AddTask(new MoveTask(transform, XPos + dir, ZPos, Services.Attackers.MoveSpeed));
			NewLoc(XPos + dir, ZPos);
			return true;
		} else return false;
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


	/// <summary>
	/// Call this when an attacker suffers damage.
	/// </summary>
	/// <param name="damage">The amount of damage sustained, after all modifiers.</param>
	public virtual void TakeDamage(int damage){
		Health -= damage;

		if (Health <= 0) {
			BeDefeated();
		}
	}


	/// <summary>
	/// Call this when this attacker is defeated by a defender.
	/// </summary>
	public virtual void BeDefeated(){
		Services.Attackers.EliminateAttacker(this);
		Services.Board.TakeThingFromSpace(XPos, ZPos);
		Services.Events.Fire(new AttackerDefeatedEvent(this));

		AttackerFallTask fallTask = new AttackerFallTask(GetComponent<Rigidbody>());
		EjectAttackerTask throwTask = new EjectAttackerTask(GetComponent<Rigidbody>());
		fallTask.Then(throwTask);
		throwTask.Then(new DestroyAttackerTask(gameObject));
		Services.Tasks.AddTask(fallTask);
	}


	/// <summary>
	/// Call this when the attacker is taken out of the game by something that the defenders aren't rewarded for (e.g., the end of a wave).
	/// </summary>
	public virtual void BeRemovedFromBoard(){
		Services.Board.TakeThingFromSpace(XPos, ZPos);
		EjectAttackerTask throwTask = new EjectAttackerTask(GetComponent<Rigidbody>());
		throwTask.Then(new DestroyAttackerTask(gameObject));
		Services.Tasks.AddTask(throwTask);
	}


	public void FailToDamage(){
		Debug.Log("Attacker not damaged!");
	}


	/// <summary>
	/// Provides information on this attacker when the attacker is clicked.
	/// </summary>
	/// <returns>This attacker's name and stats.</returns>
	public string GetUIInfo(){
		return attackerName + NEWLINE +
			   ATTACK + AttackMod.ToString() + NEWLINE +
			   ARMOR + Armor.ToString() + NEWLINE +
			   HEALTH + Health.ToString() + NEWLINE;
	}
}