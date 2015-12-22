using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LcChoice : MonoBehaviour {
	public class ChoiceSingleton {
		Dictionary<string, bool> dicBool = new Dictionary<string, bool>();
		string lastChoice;
		
		static ChoiceSingleton shared = null;
		public static ChoiceSingleton sharedInstance {
			get {
				if (shared == null)
					shared = new ChoiceSingleton();
				
				return shared;
			}
		}
			
		void SetBool(string name) {
			lastChoice = name;
			dicBool[name] = true;
		}
		
		bool IsBool(string name) {
			lastChoice = null;
			if (!dicBool.ContainsKey(name))
				return false;
				
			return dicBool[name];
		}
		
		public void Clear() {
			shared = null;
		}
		
		bool IsLastChoice(string name) {
			bool rlt = false;
			
			if (name == lastChoice) {
				rlt = IsBool(name);
				lastChoice = null;
			}
			
			return rlt;
		}
		
		public static string GetName(int idx, string name) {
			return idx + name;
		}
		
		public void SetBool(int idx, string name) {
			SetBool (GetName(idx, name));
		}
		
		public bool IsBool(int idx, string name) {
			return IsBool(GetName (idx, name));
		}
		
		public bool IsLastChoice(int idx, string name) {
			return IsLastChoice(GetName(idx, name));
		}
	}

	[System.Serializable]
	public class ChoiceData {
		public ActorBase actor;
	}
	
	[System.Serializable]
	public class ChoiceSet {
		public ActorBase[] choices;
		public CutsceneMgr endCutscene;
	}
	
	public ChoiceSet[] choiceSets;

	// Use this for initialization
	void Awake () {
		ChoiceSingleton sing = ChoiceSingleton.sharedInstance;
		
		if (Global.IsSceneRecent()) {
			for (int i = 0; i < choiceSets.Length; i++) {
				foreach (ActorBase c in choiceSets[i].choices) {
					if (sing.IsLastChoice(i, c.name)) {
						return;
					}
				}
			}
		}
		
		sing.Clear();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public bool IsBool(int idx, string name) {
		ChoiceSingleton sing = ChoiceSingleton.sharedInstance;
		bool done = true;
		bool rlt = false;
		
		idx--;
		
		ChoiceSet s = choiceSets[idx];
		foreach (ActorBase c in s.choices) {
			if (sing.IsBool(idx, c.name)) {
				if (name == c.name) {
					rlt = true;
				}
			} else 
				done = false;
		}
		
		if (done) {
			s.endCutscene.gameObject.SetActive(true);
			
			for (int i = 0; i < choiceSets.Length; i++) {
				foreach (ActorBase c in choiceSets[i].choices) {
					if (!sing.IsBool(i, c.name)) {
						goto EndDoneCheck;
					}
				}
			}
			sing.Clear();
		}
		
EndDoneCheck:
			
		return rlt;
	}
	
	public void SetBool(int idx, string name) {
		idx--;
		
		ChoiceSingleton sing = ChoiceSingleton.sharedInstance;
		sing.SetBool(idx, name);
	}
}
