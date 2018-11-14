using TMPro;
using UnityEngine;

public class EndEscMenuBehavior : EscMenuBehavior {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the specific menu loaded for the end of the game
	private const string END_MENU_OBJ = "Menus/End menu";
	
	
	//the text box where the opponent speaks
	private TextMeshProUGUI opponentStatement;
	private const string CANVAS_OBJ = "Menu canvas";
	private const string MENU_OBJ = "Menu";
	private const string OPPONENT_STATEMENT_OBJ = "Opponent statement";
	private const string TEXT_OBJ = "Text";
	
	
	//things the opponent can say
	private const string VICTORY = "Wow!/nI didn't think you'd get me. That was great play.";
	private const string CLOSE_LOSS = "That came down to the wire./nAnother game?";
	private const string EARLY_LOSS = "The necromancer gets all the advantages./nAnother try?";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables
	public override void Setup (){
		menu = MonoBehaviour.Instantiate<GameObject>(Resources.Load<GameObject>(END_MENU_OBJ));

		opponentStatement = menu.transform.Find(CANVAS_OBJ).Find(MENU_OBJ).Find(OPPONENT_STATEMENT_OBJ)
			.Find(TEXT_OBJ).GetComponent<TextMeshProUGUI>();
		opponentStatement.text = ChooseText();
		
		base.Setup ();
	}


	/// <summary>
	/// Select a context-appropriate thing for the opponent to say at the end of the game.
	/// </summary>
	/// <returns></returns>
	private string ChooseText(){
		if (Services.Rulebook.TurnMachine.CurrentState.GetType() == typeof(TurnManager.PlayerWin)){
			return VICTORY;
		} else {
			if (Services.Attackers.GetCurrentWave() == (Services.Attackers.GetTotalWaves() - 1)){
				return CLOSE_LOSS;
			} else return EARLY_LOSS;
		}
	}
}
