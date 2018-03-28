namespace Title
{
	using System.Collections.Generic;
	using UnityEngine;

	public class DemoAttackerCreator : AttackerManager {


		/////////////////////////////////////////////
		/// Fields
		/////////////////////////////////////////////


		//curated warlords to spawn, rather than the normal random ones
		private const string DEMO_FAST_OBJ = "Title/Demo Fast Warlord";
		private const string DEMO_ENRAGED_OBJ = "Title/Demo Enraged Warlord";
		private int warlordIndex = 0;
		private List<string> warlords = new List<string>() { PETTY_WARLORD_OBJ, DEMO_FAST_OBJ, DEMO_ENRAGED_OBJ };



		/////////////////////////////////////////////
		/// Functions
		/////////////////////////////////////////////


		//initialize variables and create the first 
		public override void Setup(){
			spawnPoints = CreateSpawnPoints();
			attackerOrganizer = GameObject.Find(ATTACKER_ORGANIZER).transform;
			SpawnDemoAttackers();
		}


		/// <summary>
		/// Queries the board for open spawn points. If there are any, puts a warlord there.
		/// </summary>
		/// <returns>The warlords.</returns>
		public void SpawnDemoAttackers(){
			foreach (TwoDLoc point in spawnPoints){
				if (Services.Board.GeneralSpaceQuery(point.x, point.z) != SpaceBehavior.ContentType.Attacker &&
					Services.Board.GeneralSpaceQuery(point.x - 1, point.z) != SpaceBehavior.ContentType.Attacker &&
					Services.Board.GeneralSpaceQuery(point.x, point.z - 1) != SpaceBehavior.ContentType.Attacker &&
					Services.Board.GeneralSpaceQuery(point.x + 1, point.z) != SpaceBehavior.ContentType.Attacker){

					MakeWarlord(ChooseWarlordType(), point.x, point.z);
					MakeRetinue(point.x, point.z);
				}
			}
		}


		/// <summary>
		/// This override puts the warlord directly in its starting space, rather than having it move onto the board.
		/// </summary>
		/// <returns>The warlord's AttackerSandbox (or inheriting script).</returns>
		/// <param name="type">The type of warlord to make.</param>
		/// <param name="x">The x coordinate on the board where the warlord is to be placed.</param>
		/// <param name="z">The z coordinate on the board where the warlord is to be placed.</param>
		protected override AttackerSandbox MakeWarlord(string type, int x, int z){
			Vector3 startLoc = Services.Board.GetWorldLocation(x, z);

			GameObject newWarlord = MonoBehaviour.Instantiate<GameObject>(Resources.Load<GameObject>(type),
																		  startLoc,
																		  Quaternion.identity,
																		  attackerOrganizer);

			Debug.Assert(newWarlord != null, "Failed to create warlord.");

			Services.Board.PutThingInSpace(newWarlord, x, z, SpaceBehavior.ContentType.Attacker);

			newWarlord.GetComponent<AttackerSandbox>().Setup();
			newWarlord.GetComponent<AttackerSandbox>().NewLoc(x, z);

			return newWarlord.GetComponent<AttackerSandbox>();
		}


		/// <summary>
		/// Make a retinue for a warlord by putting an Evil Warrior in the spaces to the west, south, and east of the warlord--if they're open.
		/// </summary>
		/// <returns>A list of retinue members created.</returns>
		/// <param name="x">The warlord's x coordinate on the board.</param>
		/// <param name="z">The warlord's z coordinate on the board.</param>
		protected override List<AttackerSandbox> MakeRetinue(int x, int z){
			List<AttackerSandbox> temp = new List<AttackerSandbox>();

			GameObject newRetinueMember = null;

			//try to make the retinue member to the west
			if (Services.Board.GeneralSpaceQuery(x - 1, z) == SpaceBehavior.ContentType.None){
				Vector3 startLoc = Services.Board.GetWorldLocation(x - 1, z);
				newRetinueMember = MonoBehaviour.Instantiate<GameObject>(Resources.Load<GameObject>(SKELETON_OBJ),
																		 startLoc,
																		 Quaternion.identity,
																		 attackerOrganizer);

				Debug.Assert(newRetinueMember != null, "Failed to create retinue member.");

				Services.Board.PutThingInSpace(newRetinueMember, x - 1, z, SpaceBehavior.ContentType.Attacker);

				newRetinueMember.GetComponent<AttackerSandbox>().Setup();
				newRetinueMember.GetComponent<AttackerSandbox>().NewLoc(x - 1, z);

				temp.Add(newRetinueMember.GetComponent<AttackerSandbox>());
			}

			//try to make the retinue member to the south
			if (Services.Board.GeneralSpaceQuery(x, z - 1) == SpaceBehavior.ContentType.None){
				Vector3 startLoc = Services.Board.GetWorldLocation(x, z - 1);
				newRetinueMember = MonoBehaviour.Instantiate<GameObject>(Resources.Load<GameObject>(SKELETON_OBJ),
																		 startLoc,
																		 Quaternion.identity,
																		 attackerOrganizer);

				Debug.Assert(newRetinueMember != null, "Failed to create retinue member.");

				Services.Board.PutThingInSpace(newRetinueMember, x, z - 1, SpaceBehavior.ContentType.Attacker);

				newRetinueMember.GetComponent<AttackerSandbox>().Setup();
				newRetinueMember.GetComponent<AttackerSandbox>().NewLoc(x, z - 1);

				temp.Add(newRetinueMember.GetComponent<AttackerSandbox>());
			}


			//try to make the retinue member to the east
			if (Services.Board.GeneralSpaceQuery(x + 1, z) == SpaceBehavior.ContentType.None){
				Vector3 startLoc = Services.Board.GetWorldLocation(x + 1, z);
				newRetinueMember = MonoBehaviour.Instantiate<GameObject>(Resources.Load<GameObject>(SKELETON_OBJ),
																		 startLoc,
																		 Quaternion.identity,
																		 attackerOrganizer);

				Debug.Assert(newRetinueMember != null, "Failed to create retinue member.");

				Services.Board.PutThingInSpace(newRetinueMember, x + 1, z, SpaceBehavior.ContentType.Attacker);

				newRetinueMember.GetComponent<AttackerSandbox>().Setup();
				newRetinueMember.GetComponent<AttackerSandbox>().NewLoc(x + 1, z);

				temp.Add(newRetinueMember.GetComponent<AttackerSandbox>());
			}

			return temp;
		}


		/// <summary>
		/// Select a warlord among those that can be spawned in the demo.
		/// </summary>
		/// <returns>The warlord type's name.</returns>
		protected override string ChooseWarlordType(){
			Debug.Assert(warlordIndex < warlords.Count, "Trying to spawn unavailable warlord");

			string temp = warlords[warlordIndex];

			warlordIndex++;

			return temp;
		}
	}
}
