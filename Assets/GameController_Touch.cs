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
	float sTime = 0; 
	float time = 0;
	int tapCount = 0;
	// Use this for initialization
	bool isTouch = false;
	
	float nextUpdate = 0.0f;
	float updateRate = 1.0f;
	
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		//0 idle, 1 walk, 2 run
		Dp_Speed.text = speed.ToString();
		DP_Sec.text = "" + sTime;
		Dp_TTC.text = tapCount.ToString();
		Dp_TPS.text = ""+ tps;
		sTime += Time.deltaTime;
		
		if(Time.time > nextUpdate){
			nextUpdate = Time.time + 1.0f/updateRate;
			tps = tapCount * updateRate;
			tapCount = 0;
			
		}
		if(tps == 0){
			isTouch = false;
		}
		
		if(slider.value >= 0 && slider.value <= 1){
			//  slider.value -= 0.01f;
			slider.value = slider.value + (-0.01f+0.002f*(tps*0.7f));
		}
		
		//  if((tps > 1 && tps <= 2 )&& slider.value != 1 && isTouch){
		//  	slider.value += 0.0002f;
		//  	Debug.Log("touch 2");
		//  }else if((tps > 2 && tps <= 4 ) && slider.value != 1 && isTouch){
		//  	slider.value += 0.0007f;
		//  	Debug.Log("touch 3");
		//  }else if((tps > 4 && tps <= 8 ) && slider.value != 1 && isTouch){
		//  	slider.value += 0.001f;
		//  	Debug.Log("touch 4");
		//  }else if((tps > 8) && slider.value != 1 && isTouch){
		//  	slider.value += 0.005f;
		//  	Debug.Log("touch 5");
		//  }
		
	}
	void tps_Calc(){
		
	}
	public void On_SlideChange(){
		speed = slider.value * 2;
		anim_Action.SetFloat ("Speed", speed);
	}

	public void func_tap(){
		Debug.Log("Tap");
		tapCount++;
		isTouch = true;
	}
}
