using UnityEngine;

public class MomentumManager {

	public int Momentum { get; private set; }
	private const int START_MOMENTUM = 0;


	//initialize variables
	public void Setup(){
		Momentum = START_MOMENTUM;
		Services.Events.Register<MissedFightEvent>(IncreaseMomentum);
		Services.Events.Register<EndPhaseEvent>(ResetMomentum);
		Services.UI.SetMomentumText(Momentum);
	}


	public void IncreaseMomentum(Event e){
		Debug.Assert(e.GetType() == typeof(MissedFightEvent), "Non-MissedFightEvent in IncreaseMomentum.");

		Momentum++;

		Services.UI.SetMomentumText(Momentum);
	}


	public void ResetMomentum(Event e){
		Debug.Assert(e.GetType() == typeof(EndPhaseEvent), "Non-EndPhaseEvent in ResetMomentum");

		EndPhaseEvent endEvent = e as EndPhaseEvent;

		if (endEvent.Phase.GetType() == typeof(TurnManager.PlayerMove)){
			Momentum = START_MOMENTUM;

			Services.UI.SetMomentumText(Momentum);
		}
	}
}
