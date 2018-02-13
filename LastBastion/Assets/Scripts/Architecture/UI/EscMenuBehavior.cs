using UnityEngine;

public abstract class EscMenuBehavior {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the menus
	protected GameObject menu;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//set up the menu. Each scene should override this with a new setup function
	public virtual void Setup(){
		Debug.Assert(menu != null, "No menu. Did you forget to override EscMenuBehavior.Setup?");

		menu.SetActive(false);
	
		Services.Events.Register<EscMenuEvent>(OpenMenu);
	}


	/// <summary>
	/// When the menu system detects an EscMenuEvent, display the menu.
	/// </summary>
	/// <param name="e">An EscMenuEvent.</param>
	protected void OpenMenu(Event e){
		Debug.Assert(e.GetType() == typeof(EscMenuEvent), "Non-EscMenuEvent in OpenMenu");

		menu.SetActive(true);
	}


	/// <summary>
	/// Call this whenever a scene ends to avoid memory leaks.
	/// </summary>
	public void Cleanup(){
		Services.Events.Unregister<EscMenuEvent>(OpenMenu);
	}
}
