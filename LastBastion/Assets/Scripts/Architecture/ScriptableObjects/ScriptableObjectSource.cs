using UnityEngine;

public class ScriptableObjectSource
{

	public CommonAnimationCurves curveSource;


	public void Setup(){
		curveSource = Resources.Load<CommonAnimationCurves>("Scriptable Objects/Common animation curves");
	}
}
