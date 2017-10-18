using System;
using System.Collections.Generic;

public class EventManager {
	//dictionary of handers to be called, indexed by the event that triggers the handlers
	private Dictionary<Type, Event.Handler> registeredHandlers = new Dictionary<Type, Event.Handler>();


	/// <summary>
	/// Objects call this function to subscribe to events of a given type.
	/// </summary>
	/// <param name="handler">The function to be called when the event occurs.</param>
	/// <typeparam name="T">The type of the event.</typeparam>
	public void Register<T>(Event.Handler handler) where T : Event{
		Type type = typeof(T);

		/*
		 * 
		 * If there's already an event of that type, add this handler to the list of handlers for that event.
		 * 
		 * If not, add that event to the dictionary, with this handler as the first item in the list.
		 * 
		 * Note that there's no explicit list, because in C# delegates are both references to a function and a list
		 * of functions.
		 * 
		 */
		if (registeredHandlers.ContainsKey(type)){
			registeredHandlers[type] += handler;
		} else {
			registeredHandlers[type] = handler;
		}
	}


	/// <summary>
	/// Objects call this to unsubscribe to events of a given type.
	/// </summary>
	/// <param name="handler">The function that was called when the event occurred.</param>
	/// <typeparam name="T">The type of the event.</typeparam>
	public void Unregister<T>(Event.Handler handler) where T : Event{
		Type type = typeof(T);

		Event.Handler handlers;

		if (registeredHandlers.TryGetValue(type, out handlers)){
			handlers -= handler;

			if (handlers == null){
				registeredHandlers.Remove(type);
			} else {
				registeredHandlers[type] = handlers;
			}
		}
	}


	/// <summary>
	/// Objects call this to publish an event.
	/// </summary>
	/// <param name="e">The event to publish.</param>
	public void Fire(Event e){
		Type type = e.GetType();

		Event.Handler handlers;

		if (registeredHandlers.TryGetValue(type, out handlers)){
			handlers(e);
		}
	}
}
