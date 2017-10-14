/// <summary>
/// Base class for defenders. All defender "verbs" are contained here.
/// </summary>
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Rigidbody))]
public class DefenderSandbox : MonoBehaviour {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//defender stats
	public int Speed { get; set; } //in spaces/turn
	public int AttackMod { get; set; }
	public int Armor { get; set; }


	//generic stats for testing purposes
	protected int baseSpeed = 4;
	protected int baseAttackMod = 1;
	protected int baseArmor = 1;


	//is this attacker currently selected? Also includes related variables
	public bool Selected;
	protected GameObject selectedParticle;
	protected const string SELECT_PARTICLE_OBJ = "Selected particle";


	//how many spaces of movement does the defender have left? Also other fields relating to movement
	protected int remainingSpeed = 0;
	protected List<TwoDLoc> moves = new List<TwoDLoc>();
	protected LineRenderer lineRend;
	protected Button doneButton;
	protected const string DONE_BUTTON_OBJ = "Done moving button";
	protected const string UI_CANVAS = "Defender canvas";
	[SerializeField] protected float moveSpeed = 1.0f; //movement on screen, as opposed to spaces on the grid
	protected Rigidbody rb;


	//location in the grid
	protected TwoDLoc GridLoc { get; set; }


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables
	public virtual void Setup(){
		Speed = baseSpeed;
		AttackMod = baseAttackMod;
		Armor = baseArmor;
		Selected = false;
		selectedParticle = transform.Find(SELECT_PARTICLE_OBJ).gameObject;
		lineRend = GetComponent<LineRenderer>();
		lineRend.positionCount = 0;
		doneButton = transform.Find(UI_CANVAS).Find(DONE_BUTTON_OBJ).GetComponent<Button>();
		rb = GetComponent<Rigidbody>();
		GridLoc = new TwoDLoc(0, 0); //default initialization
	}


	/// <summary>
	/// Sets this defender's location in the grid.
	/// </summary>
	/// <param name="x">The grid x coordinate (not world x-position!).</param>
	/// <param name="z">The grid z coordinate (not world z-position!).</param>
	public void NewLoc(int x, int z){
		GridLoc.x = x;
		GridLoc.z = z;
	}


	/// <summary>
	/// Reports whether this defender is in the middle of a move.
	/// </summary>
	/// <returns><c>true</c> if this instance is moving, <c>false</c> otherwise.</returns>
	public virtual bool IsMoving(){
		return remainingSpeed == Speed || remainingSpeed == 0 ? false : true;
	}


	/// <summary>
	/// Carries out all effects associated with being selected.
	/// </summary>
	public virtual void BeSelected(){
		Selected = true;
		selectedParticle.SetActive(true);
	}



	/// <summary>
	/// Does everything that needs to happen when another defender is selected.
	/// </summary>
	public virtual void BeUnselected(){
		Selected = false;
		selectedParticle.SetActive(false);
	}


	/// <summary>
	/// Call this at the start of the defender movement phase.
	/// </summary>
	public virtual void PrepareToMove(){
		moves.Clear();
		moves.Add(GridLoc);
		remainingSpeed = Speed;
		lineRend.positionCount++;
		lineRend.SetPosition(0, Services.Board.GetWorldLocation(GridLoc.x, GridLoc.z));
	}


	/// <summary>
	/// Whenever the player tries to move a defender, TurnManager calls this function to determine whether the move is legal--
	/// the defender has the movement remaining, the space is legal to enter, etc.
	/// </summary>
	/// <param name="loc">Location.</param>
	public virtual void TryPlanMove(TwoDLoc loc){
		if (moves.Count <= Speed + 1){ //the defender can move up to their speed; they get a + 1 "credit" for the space they're in.
			if (CheckAdjacent(loc, moves[Speed - remainingSpeed])){
				moves.Add(loc);
				remainingSpeed--;
				lineRend.positionCount++;
				lineRend.SetPosition(Speed - remainingSpeed, Services.Board.GetWorldLocation(loc.x, loc.z));
			}
		}
	}


	/// <summary>
	/// Called by the UI to move the defender.
	/// </summary>
	public virtual void Move(){
		//move on the screen
		Services.Tasks.AddTask(new MoveDefenderTask(rb, moveSpeed, moves));


		//move on the grid used for game logic
		Services.Board.TakeThingFromSpace(GridLoc.x, GridLoc.z);
		TwoDLoc destination = moves[moves.Count - 1];
		Services.Board.PutThingInSpace(gameObject, destination.x, destination.z, SpaceBehavior.ContentType.Defender);
		GridLoc.x = destination.x;
		GridLoc.z = destination.z;


		remainingSpeed = 0;
	}


	/// <summary>
	/// Determine whether two grid spaces are orthogonally adjacent.
	/// </summary>
	/// <returns><c>true</c>, if so, <c>false</c> if not.</returns>
	/// <param name="next">the grid space being checked.</param>
	/// <param name="current">The space being checked against.</param>
	protected bool CheckAdjacent(TwoDLoc next, TwoDLoc current){
		return ((next.x == current.x && Mathf.Abs(next.z - current.z) == 1) ||
				(Mathf.Abs(next.x - current.x) == 1 && next.z == current.z)) ? true : false;
	}
}
