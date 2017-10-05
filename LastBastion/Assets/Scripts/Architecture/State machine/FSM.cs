/// <summary>
/// A state machine.
/// </summary>
using System;
using System.Collections.Generic;
using UnityEngine;

public class FSM<TContext> {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the thing that created this state machine
	private readonly TContext _context;


	//dictionary of state types
	private readonly Dictionary<Type, State> _stateCache = new Dictionary<Type, State>();


	//the machine's current state
	public State CurrentState { get; private set; }


	//keep track of the next state to transition into, to help avoid transitions in the middle of update loops
	private State _pendingState;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public FSM(TContext context){
		_context = context;
	}


	//called each loop
	public void Tick(){
		//handle pending transitions if a function external to the FSM called TransitionTo() (try to avoid this!)
		PerformPendingTransition();

		//handle current state
		Debug.Assert(CurrentState != null, "No current state. Did you forget to transition to a starting state?");
		CurrentState.Tick();

		//handle any pending transitions created within the state machine
		PerformPendingTransition();
	}


	/// <summary>
	/// Allows classes other than the state machine to queue up a transition to a new state.
	/// 
	/// Try not to do this!
	/// </summary>
	/// <typeparam name="TState">The state type.</typeparam>
	public void TransitionTo<TState>() where TState : State {
		_pendingState = GetOrCreateState<TState>();
	}


	/// <summary>
	/// Move to a new state.
	/// </summary>
	private void PerformPendingTransition(){
		if (_pendingState != null){
			if (CurrentState != null) CurrentState.OnExit();
			CurrentState = _pendingState;
			CurrentState.OnEnter();
			_pendingState = null;
		}
	}


	/// <summary>
	/// Get a state.
	/// </summary>
	/// <returns>The state.</returns>
	/// <typeparam name="TState">The type of the state to be returned.</typeparam>
	private TState GetOrCreateState<TState>() where TState : State {
		State state;

		if (_stateCache.TryGetValue(typeof(TState), out state)){
			return (TState) state;
		} else {
			var newState = Activator.CreateInstance<TState>();
			newState.Parent = this;
			newState.Init();
			_stateCache[typeof(TState)] = newState;
			return newState;
		}
	}


	//a base class for states
	//this is tied to the context type of the FSM to prevent accidental
	//transitions to states for a different context types
	public abstract class State
	{
		//the FSM this state belongs to
		internal FSM<TContext> Parent { get; set; }

		//an easy way to access the parent FSM's context
		protected TContext Context { get { return Parent._context; } }


		//transition to a different state
		protected void TransitionTo<TState>() where TState : State {
			Parent.TransitionTo<TState>();
		}


		/////////////////////////////////////////////
		/// Lifecycle methods
		/////////////////////////////////////////////


		//called when first created
		public virtual void Init() { }

		//called when state becomes active
		public virtual void OnEnter() { }

		//called when state becomes inactive
		public virtual void OnExit() { }

		//called each Unity update loop while active
		public virtual void Tick() { }

		//called when state machine is cleared
		public virtual void Cleanup() { }
	}
}
