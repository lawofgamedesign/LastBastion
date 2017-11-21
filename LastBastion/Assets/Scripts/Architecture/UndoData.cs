using System.Collections.Generic;
using UnityEngine;

public class UndoData {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the dictionary that the defenders will update at the start of each Defenders Move phase
	private Dictionary<DefenderSandbox, DefenderData> defenders = new Dictionary<DefenderSandbox, DefenderData>();


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//put a new defender into the dictionary
	public void AddDefender(DefenderSandbox defender){
		defenders.Add(defender, new DefenderData(defender));
	}


	/// <summary>
	/// Changes the information associated with a defender.
	/// </summary>
	/// <param name="defender">The defender.</param>
	/// <param name="loc">The defender's current location.</param>
	/// <param name="movement">The defender's movement available.</param>
	public void ReviseDefenderState(DefenderSandbox defender, TwoDLoc loc, int movement){
		defenders[defender].Loc = loc;
		defenders[defender].Movement = movement;
	}


	/// <summary>
	/// Get a defender's location for an undo.
	/// </summary>
	/// <returns>The defender's location.</returns>
	/// <param name="defender">The defender.</param>
	public TwoDLoc GetDefenderLoc(DefenderSandbox defender){
		return defenders[defender].Loc;
	}


	/// <summary>
	/// Get a defender's available movement for an undo.
	/// </summary>
	/// <returns>The defender's movement.</returns>
	/// <param name="defender">The defender.</param>
	public int GetDefenderMovement(DefenderSandbox defender){
		return defenders[defender].Movement;
	}



	/// <summary>
	/// All the data about a defender that might be needed for an undo.
	/// </summary>
	private class DefenderData {
		public DefenderSandbox Defender { get; set; }
		public TwoDLoc Loc { get; set; }
		public int Movement { get; set; }

		//constructor
		public DefenderData (DefenderSandbox defender){
			Defender = defender;

			//default initializations; these are nonsense values that can be used for error checking
			Loc = new TwoDLoc(-1, -1);
			Movement = -1;
		}
	}
}
