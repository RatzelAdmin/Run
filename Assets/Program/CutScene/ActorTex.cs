using UnityEngine;
using System.Collections;

public class ActorTex : ActorBase {
	Texture2D defaultTexture;
	public Texture2D disabledTexture;

	// Use this for initialization
	protected override void Start () {
		defaultTexture = renderer.material.mainTexture as Texture2D;
		base.Start ();
	}
	
	// Update is called once per frame
	protected override void Update () {
		base.Update ();
	}
	
	public override void WaitTouch(float delay, bool enable) {
		if (enable) {
			renderer.material.mainTexture = defaultTexture;
		} else {
			if (disabledTexture != null)
				renderer.material.mainTexture = disabledTexture;
		}

		base.WaitTouch(delay, enable);
	}	
}
