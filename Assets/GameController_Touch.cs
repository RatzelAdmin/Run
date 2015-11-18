using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameController_Touch : MonoBehaviour {
	public GameObject testTarget;
	public Animator anim_Action;
	public Slider slider;
	public Text Dp_TPS;
	public Text Dp_Speed;
	public Text Dp_TTC;
	public Text DP_Sec;
	
	float speed = 0;
	float tps;
	
	int tapCount = 0;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		//0 idle, 1 walk, 2 run
		Dp_Speed.text = speed.ToString();
		DP_Sec.text = "" + Time.time;
		Dp_TPS.text = "" + Time.time * tapCount * 0.1f;
	}

	public void On_SlideChange(){
		speed = slider.value * 2;
		anim_Action.SetFloat ("Speed", speed);
	}

	public void func_tap(){
		Debug.Log("Tap");
		tapCount++;
		Dp_TTC.text = tapCount.ToString();
		
	}
}
