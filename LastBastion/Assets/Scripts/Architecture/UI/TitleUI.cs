using UnityEngine;

public class TitleUI : ChatUI {

    /////////////////////////////////////////////
    /// Fields
    /////////////////////////////////////////////


    //the title screen UI
    private GameObject titleUI;
    private const string TITLE_UI_OBJ = "Title text canvas";
    private const string MENU_PATH = "Menus/";
    
    //the scene organizer
    private Transform sceneObj;
    private const string SCENE_ORGANIZER = "Scene";
    
    
    //choose which menu to display--the normal one, or the one for a player's first game
    private const string MENU_CANVAS_OBJ = "Title text canvas";
    private const string NORMAL_MENU = "Menu";
    private const string FIRST_GAME_MENU = "First game menu";
    private int numPlays = 0; //0 if game has never been played before
    private const string NUM_PLAYS_KEY = "NumPlays";
    private const int FIRST_PLAY_DEFAULT = 0;
    
    
    public override void Setup() {
        sceneObj = GameObject.Find(SCENE_ORGANIZER).transform;
        titleUI = MonoBehaviour.Instantiate(Resources.Load<GameObject>(MENU_PATH + TITLE_UI_OBJ), sceneObj);
    }


    public override void ToggleAllButtons(OnOrOff onOrOff){
        if (onOrOff == OnOrOff.On) titleUI.SetActive(true);
        else titleUI.SetActive(false);
    }
    
    
    /// <summary>
    /// Activates either the normal start-of-game menu or, if this is the first game on this machine, a special menu
    /// from which the player can load an introductory message. 
    /// </summary>
    public override void ChooseMenu(){	
        if (Application.isEditor) PlayerPrefs.SetInt(NUM_PLAYS_KEY, FIRST_PLAY_DEFAULT); //for testing purposes, always assume a new game
        numPlays = PlayerPrefs.GetInt(NUM_PLAYS_KEY, FIRST_PLAY_DEFAULT);
			
        if (numPlays == 0) titleUI.transform.Find(NORMAL_MENU).gameObject.SetActive(false);
        else titleUI.transform.Find(FIRST_GAME_MENU).gameObject.SetActive(false);

        numPlays++;
			
        PlayerPrefs.SetInt(NUM_PLAYS_KEY, numPlays);
    }
    
    
    /// <summary>
    /// Turns on the normal start-of-game menu and switches off the special first-game menu.
    /// </summary>
    public override void SwitchMenus(){
        titleUI.transform.Find(NORMAL_MENU).gameObject.SetActive(true);
        titleUI.transform.Find(FIRST_GAME_MENU).gameObject.SetActive(false);
    }
}
