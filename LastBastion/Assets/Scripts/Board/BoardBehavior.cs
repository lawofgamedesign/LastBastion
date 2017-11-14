﻿using System.Collections.Generic;
using UnityEngine;

public class BoardBehavior {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//putting down board tiles
	public const int BOARD_WIDTH = 9;
	public const int BOARD_HEIGHT = 6;
	private const string SPACE_OBJ = "Board space";
	private SpaceBehavior[,] spaces = new SpaceBehavior[BOARD_WIDTH, BOARD_HEIGHT];
	public Transform BoardOrganizer { get; private set; }
	private const string BOARD_ORGANIZER = "Board";


	//the wall
	private List<WallBehavior> walls = new List<WallBehavior>();
	private const string WALL_OBJ = "Wall_1_Str";


	//where is the wall? Rows higher than this number are offset to make space for the wall
	public int WallZPos { get; private set; }


	//how big is a space? Assumes spaces are square tiles
	public float SpaceSize { get; private set; }
	private const string MODEL_OBJ = "Model"; //tiles are at localscale (1, 1, 1); it's their model that gets resized


	//nonsense return value for when something is not on the board
	public readonly int NOT_FOUND = -1;



	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//set up the board
	public void Setup(){
		SpaceSize = Resources.Load<GameObject>(SPACE_OBJ).transform.Find(MODEL_OBJ).localScale.x;
		WallZPos = 2;
		BoardOrganizer = GameObject.Find(BOARD_ORGANIZER).transform;
		spaces = MakeBoard();
		walls = MakeWall();
	}


	/// <summary>
	/// Puts down the board tiles
	/// </summary>
	/// <returns>A 2D array containing all of the board spaces.</returns>
	private SpaceBehavior[,] MakeBoard(){
		SpaceBehavior[,] temp = new SpaceBehavior[BOARD_WIDTH, BOARD_HEIGHT];
		GameObject objToCreate = Resources.Load<GameObject>(SPACE_OBJ);

		for(int x = 0; x < BOARD_WIDTH; x++){
			for (int z = 0; z < BOARD_HEIGHT; z++){
				temp[x, z] = MonoBehaviour.Instantiate<GameObject>(objToCreate,
																   AssignWorldLocation(x, z),
																   objToCreate.transform.rotation,
																   BoardOrganizer).GetComponent<SpaceBehavior>();
				temp[x, z].contentType = SpaceBehavior.ContentType.None;
				temp[x, z].Lure = false;
				temp[x, z].WorldLocation = AssignWorldLocation(x, z);
				temp[x, z].GridLocation = new TwoDLoc(x, z);
			}
		}

		return temp;
	}


	/// <summary>
	/// Puts wall sections on the board. These are not added to the list of spaces; the wall doesn't occupy locations in the grid.
	/// </summary>
	/// <returns>A list of wall sections.</returns>
	private List<WallBehavior> MakeWall(){
		List<WallBehavior> temp = new List<WallBehavior>();
		GameObject objToCreate = Resources.Load<GameObject>(WALL_OBJ);

		for (int x = 0; x < BOARD_WIDTH; x++){

			//the location for instantiation is awkward, but we can't use AssignWorldLocation here; it's designed to skip over the wall's location
			temp.Add(MonoBehaviour.Instantiate<GameObject>(objToCreate,
														   new Vector3(x * SpaceSize, 0.0f, (WallZPos + 1) * SpaceSize),
														   Quaternion.identity,
														   BoardOrganizer).GetComponent<WallBehavior>());
			temp[x].gameObject.name = WALL_OBJ + x.ToString();
			temp[x].Setup();
		}
			
		Debug.Assert(temp.Count == BOARD_WIDTH, "Not enough walls to extend across the board.");

		return temp;
	}


	/// <summary>
	/// Assigns a tile a location in world space, based on its position in the grid.
	/// </summary>
	/// <returns>The world space location as a Vector3.</returns>
	/// <param name="x">The tile's x coordinate in the grid.</param>
	/// <param name="z">The tile's z coordinate in the grid.</param>
	private Vector3 AssignWorldLocation(int x, int z){
		float xPos = x;
		float zPos = z;
		if (zPos > WallZPos) zPos += 1.0f;

		return new Vector3(xPos * SpaceSize, 0.0f, zPos * SpaceSize);
	}


	public Vector3 GetWorldLocation(int x, int z){
		return spaces[x, z].WorldLocation;
	}


	/// <summary>
	/// Checks each spawn point. If no Attacker is present in it or the spaces around it, put it
	/// in a list for the AttackerManager to use to spawn warlords and their retinues.
	/// </summary>
	/// <returns>A list of the world-space coordinates for the open spawn points.</returns>
	public List<Vector3> FindOpenSpawnPoints(){
		return null;
	}


	/// <summary>
	/// Check whether given coordinates fall within the board.
	/// </summary>
	/// <returns><c>true</c> if the coordinates are valid, <c>false</c> otherwise.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	public bool CheckValidSpace(int x, int z){
		if (!CheckValidColumn(x) ||
			!CheckValidRow(z)) return false;
		else return true;
	}


	/// <summary>
	/// Makes sure a given column number is found within the board
	/// </summary>
	/// <returns><c>true</c> if the number represents a valid column, <c>false</c> otherwise.</returns>
	/// <param name="x">The number to check.</param>
	private bool CheckValidColumn(int x){
		if (x < 0 || x > BOARD_WIDTH - 1) return false;
		else return true;
	}


	/// <summary>
	/// Makes sure a given row number is found within the board
	/// </summary>
	/// <returns><c>true</c> if the number represents a valid row, <c>false</c> otherwise.</returns>
	/// <param name="x">The number to check.</param>
	private bool CheckValidRow(int z){
		if (z < 0 || z > BOARD_HEIGHT - 1) return false;
		else return true;
	}


	/// <summary>
	/// Get the type--in game terms, not as a c# class--of a space's contents
	/// </summary>
	/// <returns>The type of the space's contents.</returns>
	/// <param name="x">The x coordinate of the space.</param>
	/// <param name="z">The z coordinate of the space.</param>
	public SpaceBehavior.ContentType GeneralSpaceQuery(int x, int z){
		Debug.Assert(CheckValidSpace(x, z), "Invalid coordinates: " + x + ", " + z);
		Debug.Assert(spaces[x, z] != null, "No space at " + x + ", " + z);

		return spaces[x, z].contentType;
	}


	/// <summary>
	/// Get the gameobject in a given space.
	/// 
	/// It is up to the calling function to check for null returns!
	/// </summary>
	/// <returns>The gameobject, if any, in the space.</returns>
	/// <param name="x">The x coordinate of the space.</param>
	/// <param name="z">The z coordinate of teh space.</param>
	public GameObject GetThingInSpace(int x, int z){
		Debug.Assert(CheckValidSpace(x, z), "Invalid coordinates: " + x + ", " + z);
		Debug.Assert(spaces[x, z] != null, "No space at " + x + ", " + z);

		return spaces[x, z].Contents;
	}


	/// <summary>
	/// Put an object in a space.
	/// </summary>
	/// <param name="obj">The gameobject that is entering the space.</param>
	/// <param name="x">The x coordinate of the space, zero-indexed.</param>
	/// <param name="z">The z coordinate of the space, zero-indexed.</param>
	/// <param name="type">The type of the object in game terms (not in the sense of a c# class!).</param>
	public void PutThingInSpace(GameObject obj, int x, int z, SpaceBehavior.ContentType type){
		Debug.Assert(CheckValidSpace(x, z), "Invalid coordinates: " + x + ", " + z);
		Debug.Assert(spaces[x, z] != null, "No space at " + x + ", " + z);

		spaces[x, z].Contents = obj;
		spaces[x, z].contentType = type;
	}


	/// <summary>
	/// Blank a space.
	/// </summary>
	/// <param name="x">The x coordinate of the space to be blanked.</param>
	/// <param name="z">The z coordinate of the space to be blanked.</param>
	public void TakeThingFromSpace(int x, int z){
		Debug.Assert(CheckValidSpace(x, z), "Invalid coordinates: " + x + ", " + z);
		spaces[x, z].Contents = null;
		spaces[x, z].contentType = SpaceBehavior.ContentType.None;
	}


	/// <summary>
	/// Create a list of attackers ordered according to their position on the board.
	/// 
	/// This starts in the southeast, goes east, then goes one row north and counts again, etc.
	/// 
	/// By following this list it's possible to avoid recursion in figruing out whether an attacker can move. Each
	/// attacker gets one chance; when it's used its chance, that's it.
	/// </summary>
	/// <returns>The ordered list.</returns>
	public List<AttackerSandbox> GetOrderedAttackerList(){
		List<AttackerSandbox> orderedAttackers = new List<AttackerSandbox>();

		for (int x = 0; x < BOARD_WIDTH; x++){
			for (int z = 0; z < BOARD_HEIGHT; z++){
				if (spaces[x, z].contentType == SpaceBehavior.ContentType.Attacker) orderedAttackers.Add(spaces[x, z].Contents.GetComponent<AttackerSandbox>());
			}
		}

		Debug.Assert(orderedAttackers.Count == Services.Attackers.GetAttackers().Count);

		return orderedAttackers;
	}


	/// <summary>
	/// Gets a list of all attackers who are at the wall and have not already fought this turn.
	/// </summary>
	/// <returns>The besieging attackers.</returns>
	public List<AttackerSandbox> GetBesiegingAttackers(){
		List<AttackerSandbox> besiegingAttackers = new List<AttackerSandbox>();

		for (int x = 0; x < BOARD_WIDTH; x++){
			if (spaces[x, WallZPos + 1].contentType == SpaceBehavior.ContentType.Attacker){ //check the spaces north of the wall
				if (!spaces[x, WallZPos + 1].Contents.GetComponent<AttackerSandbox>().FoughtThisTurn){
					besiegingAttackers.Add(spaces[x, WallZPos + 1].Contents.GetComponent<AttackerSandbox>());
				}
			}
		}

		return besiegingAttackers;
	}


	/// <summary>
	/// Get the durability of the wall in a given column.
	/// </summary>
	/// <returns>The wall's durability.</returns>
	/// <param name="column">The column, zero-indexed.</param>
	public int GetWallDurability(int column){
		Debug.Assert(column < walls.Count);

		return walls[column].Durability;
	}


	/// <summary>
	/// Changes the durability of a wall. All changes are routed through here, rather than being sent directly to the associated wall,
	/// to decouple as much as possible.
	/// </summary>
	/// <param name="column">The column of the wall to be affected.</param>
	/// <param name="change">The change in durability.</param>
	public void ChangeWallDurability(int column, int change){
		Debug.Assert(column < walls.Count);

		walls[column].ChangeDurability(change);
	}


	/// <summary>
	/// Triggers feedback for when attackers fail to damage a wall. This is routed through the board manager in the name of decoupling.
	/// </summary>
	/// <param name="column">The attacker's, and thus the wall's, column, zero-indexed.</param>
	public void FailToDamageWall(int column){
		Debug.Assert(column < walls.Count);

		walls[column].NoDamageEffects();
	}


	/// <summary>
	/// Get the defensive strength of the wall in a given column.
	/// </summary>
	/// <returns>The wall's strength.</returns>
	/// <param name="column">The column, zero-indexed.</param>
	public int GetWallStrength(int column){
		Debug.Assert(column < walls.Count);

		return walls[column].Strength;
	}


	public void EliminateAttacker(AttackerSandbox attacker){

	}


	/// <summary>
	/// Gets the first empty space in a given column, counting from the south side of the board.
	/// </summary>
	/// <returns>The grid coordinate of the row of the empty space.</returns>
	/// <param name="x">The grid column to be checked.</param>
	public int GetFirstEmptyInColumn(int x){
		Debug.Assert(CheckValidColumn(x), "Trying to find the first empty space in a non-existent column: " + x);

		int temp = NOT_FOUND;

		for (int z = 0; z <= BOARD_HEIGHT - 1; z++){
			if (GeneralSpaceQuery(x, z) == SpaceBehavior.ContentType.None) temp = z;
			else break;
		}

		return temp;
	}


	/// <summary>
	/// As GetFirstEmptyInColumn, above, but counting southward from a given row.
	/// </summary>
	/// <returns>The grid coordinate of the row of the empty space.</returns>
	/// <param name="x">The grid column to be checked.</param>
	public int GetFirstEmptyInColumn(int x, int z){
		Debug.Assert(CheckValidColumn(x), "Trying to find the first empty space in a non-existent column: " + x);

		for (int index = z; index >= 0; index--){
			if (GeneralSpaceQuery(x, index) == SpaceBehavior.ContentType.None) return index;
		}

		return NOT_FOUND;
	}


	/// <summary>
	/// Call this to check whether attackers are being lured into a given space.
	/// 
	/// Unlike most grid-checking functions, this one simply returns false if the checked space is off the grid rather than throwing an exception.
	/// This makes things simpler for the attackers, since they don't have to be careful about not checking if they're at the edge of the board.
	/// </summary>
	/// <returns><c>true</c> if the space contains a lure, <c>false</c> otherwise.</returns>
	/// <param name="x">The x grid coordinate of the space.</param>
	/// <param name="z">The z grid coordinate of the space.</param>
	public bool CheckIfLure(int x, int z){
		if (!CheckValidSpace(x, z)) return false; //spaces that aren't on the grid can't be luring attackers

		if (spaces[x, z].Lure) return true;
		else return false;
	}


	/// <summary>
	/// Provides a list of all spaces in a given column so that, frex., they can all become lures.
	/// </summary>
	/// <returns>The all spaces in column.</returns>
	/// <param name="x">The x coordinate.</param>
	public List<SpaceBehavior> GetAllSpacesInColumn(int x){
		Debug.Assert(CheckValidColumn(x), "Invalid column in GetAllSpacesInColumn: " + x);

		List<SpaceBehavior> temp = new List<SpaceBehavior>();

		for (int z = 0; z < BOARD_HEIGHT; z++){
			temp.Add(spaces[x, z]);
		}

		Debug.Assert(temp.Count == BOARD_HEIGHT); //make sure all spaces in the column were added

		return temp;
	}
}
