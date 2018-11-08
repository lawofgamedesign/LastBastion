using UnityEngine;

[CreateAssetMenu(fileName = "CommonAnimationCurves", menuName = "Common Animation Curves")]
public class CommonAnimationCurves : ScriptableObject{

	public AnimationCurve basicEaseIn;
	public AnimationCurve baseEaseOut;
	public AnimationCurve easeOutSudden;
}
