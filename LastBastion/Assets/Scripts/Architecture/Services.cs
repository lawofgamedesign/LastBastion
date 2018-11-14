using UnityEngine;

public static class Services {

	private static BoardBehavior board;
	public static BoardBehavior Board {
		get {
			Debug.Assert(board != null, "No board. Did you forget to create one?");
			return board;
		}
		set { board = value; }
	}


	private static AttackerManager attackers;
	public static AttackerManager Attackers {
		get {
			Debug.Assert(attackers != null, "No attacker manager. Did you forget to create one?");
			return attackers;
		}
		set { attackers = value; }
	}


	private static TurnManager rulebook;
	public static TurnManager Rulebook {
		get {
			Debug.Assert(rulebook != null, "No TurnManager. Did you forget to create one?");
			return rulebook;
		}
		set { rulebook = value; }
	}


	private static TaskManager tasks;
	public static TaskManager Tasks {
		get {
			Debug.Assert(tasks != null, "No task manager. Did you forget to create one?");
			return tasks;
		}
		set { tasks = value; }
	}


	private static LinkedAttackerDeck attackDeck;
	public static LinkedAttackerDeck AttackDeck {
		get {
			Debug.Assert(attackDeck != null, "No attacker deck. Did you forget to create one?");
			return attackDeck;
		}
		set { attackDeck = value; }
	}


	private static DefenderManager defenders;
	public static DefenderManager Defenders {
		get {
			Debug.Assert(defenders != null, "No defender manager. Did you forget to create one?");
			return defenders;
		}
		set { defenders = value; }
	}


	private static EventManager events;
	public static EventManager Events {
		get {
			Debug.Assert(events != null, "No event manager. Are services being created out of order?");
			return events;
		}
		set { events = value; }
	}


	private static InputManager inputs;
	public static InputManager Inputs {
		get {
			Debug.Assert(inputs != null, "No input manager. Did you forget to create one?");
			return inputs;
		}
		set { inputs = value; }
	}


	private static ChatUI uI;
	public static ChatUI UI {
		get {
			Debug.Assert(uI != null, "No UI system. Are services being created in the wrong order?");
			return uI;
		}
		set { uI = value; }
	}


	private static UndoData undo;
	public static UndoData Undo {
		get {
			Debug.Assert(undo != null, "No undo data. Did you forget to create the repository for it?");
			return undo;
		}
		set { undo = value; }
	}


	private static MomentumManager momentum;
	public static MomentumManager Momentum {
		get {
			Debug.Assert(momentum != null, "No momentum manager. Did you forget to create one?");
			return momentum;
		}
		set { momentum = value; }
	}


	private static AudioManager sound;
	public static AudioManager Sound {
		get {
			Debug.Assert(sound != null, "No audio manager. Are services being created out of order?");
			return sound;
		}
		set { sound = value; }
	}


	private static CursorManager cursor;
	public static CursorManager Cursor {
		get {
			Debug.Assert(cursor != null, "No cursor manager. Are services being created out of order?");
			return cursor;
		}
		set { cursor = value; }
	}


	private static CameraBehavior camControl;
	public static CameraBehavior CamControl {
		get {
			Debug.Assert(camControl != null, "No controller for the camera. Are services being created out of order?");
			return camControl;
		}
		set { camControl = value; }
	}


	private static EnvironmentManager environment;
	public static EnvironmentManager Environment {
		get {
			Debug.Assert(environment != null, "No environment manager. Are services being created out of order?");
			return environment;
		}
		set { environment = value; }
	}


	private static EscMenuBehavior escapeMenu;
	public static EscMenuBehavior EscapeMenu {
		get {
			Debug.Assert(escapeMenu != null, "No escape menu. Did you forget to create one?");
			return escapeMenu;
		}
		set { escapeMenu = value; }
	}


	private static ScriptableObjectSource scriptableObjs;
	public static ScriptableObjectSource ScriptableObjs{
		get{
			Debug.Assert(scriptableObjs != null, "No scriptable object source. Are services being created out of order?");
			return scriptableObjs;
		}
		set { scriptableObjs = value; }
	}


	private static TutorialManager tutorials;
	public static TutorialManager Tutorials{
		get{
			Debug.Assert(tutorials != null, "No tutorial manager. Did you forget to create one?");
			return tutorials;
		}
		set { tutorials = value; }
	}
}
