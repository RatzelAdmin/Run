using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameController_Touch : MonoBehaviour {
	public GameObject testTarget;
	public Animator anim_Action;
	public Slider slider;

	float speed = 0;
	float state;
	float tps;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		//0 idle, 1 walk, 2 run
	}

	public void On_SlideChange(){
		speed = slider.value * 2;
		anim_Action.SetFloat ("Speed", speed);
	}

	public void func_tap(){
		if(slider.value < 1){
		}

	}
}
