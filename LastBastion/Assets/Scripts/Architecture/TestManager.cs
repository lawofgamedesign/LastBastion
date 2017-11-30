namespace Test {

	using UnityEngine;

	public class TestManager : MonoBehaviour {

		private void Awake(){
			Services.AttackDeck = new AttackerDeck();
			Services.AttackDeck.Setup();
			Services.UI = new UIManager();
			Services.UI.TestSetup();
			Services.Tasks = new TaskManager();
		}


		private void Update(){
			if (Input.GetKeyDown(KeyCode.D)){
				Services.AttackDeck.GetAttackerCard();
			} else if (Input.GetKeyDown(KeyCode.A)){
				//Services.AttackDeck.PutCardInDeck(5);
			} else if (Input.GetKeyDown(KeyCode.R)){
				Services.AttackDeck.RemoveCardFromDeck(GameObject.Find("Main Camera").transform, 1);
			} else if (Input.GetKeyDown(KeyCode.S)){
				Services.AttackDeck.Reshuffle();
			}
		}
	}
}
