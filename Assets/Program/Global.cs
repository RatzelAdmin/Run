/*
Global class
Written by Jong Lee
11/14/2012
*/

// use World class for features needed from both c# and unityscript

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Global : MonoBehaviour {
	class ActorParts {
		public string name;
		public List<Transform> parts = new List<Transform>();
	}
	
	class SceneData {
		public string name;
		public string resumeCutscene;
	}
//#if EnglishVersion
//	public static int langIdx = 1;
//	public static int langCaptionIdx = 1;
//#else
//	public static int langIdx = 0;
//	public static int langCaptionIdx = 0;
//#endif
	static Dictionary<string, ActorParts> dicActorParts = new Dictionary<string, ActorParts>();
	static List<SceneData> sceneFlow = new List<SceneData>();
	const float defaultScreenHeight = 800f;
	const float defaultScreenHeightUI = 2048f;
	public const float uiGap = 20f;
	public AudioClip touchSound;
	public AudioClip touchStartSound;
	public GameObject button_I;
	ActorBase subtitleSource;
	string subtitle;
	float screenScale;
	float screenScaleFont;
	public Font captionFont;
	public GUIStyle textStyle;
	public Texture2D texTouchInput;
	float touchDetectedTimer;
	[HideInInspector]
	public bool waitingTouch;
	public Transform preUfo;
	static public float LoadSceneCoolTime = 3f;
	static public float ProcessLoadSceneTime;
	
	static public float GetScreenScaleFont() {
		return Screen.height / defaultScreenHeight;
	}

	static public float GetScreenScaleUI() {
		return Screen.height / defaultScreenHeightUI;
	}
	
	static public void ClearSceneData()
	{
		Debug.Log ("ClearSceneData");
		sceneFlow.Clear();
		dicActorParts.Clear();
	}
	
	static public void ClearResumeSceneData()
	{
		sceneFlow.Clear();
	}

	static public void SetScene(CutsceneMgr resumeCutscene) {
		if(resumeCutscene == null) return; 

		SceneData data = null;
		
		string sceneName = World.loadedLevelName;
		
		if( sceneName == "")
			sceneName = Application.loadedLevelName;
		
		if (sceneFlow.Count > 0) {
			data = sceneFlow[sceneFlow.Count - 1];
			if (data.name != sceneName) {
				data = null;
			}
		}
		
		if (data == null) {
			if (sceneFlow.Count > 1) {
				sceneFlow.RemoveRange(0, sceneFlow.Count - 1);
			}
			
			data = new SceneData();
			data.name = sceneName;
			sceneFlow.Add(data);
		}
		
		if (resumeCutscene != null) {
			data.resumeCutscene = resumeCutscene.name;
		} else 
			data.resumeCutscene = null;
	}
	
	static public string GetScene() {
		string resumeCutscene = null;
		
		string sceneName = World.loadedLevelName;
		if( sceneName == "" )
			sceneName = Application.loadedLevelName;
		
		foreach (SceneData data in sceneFlow) {
			if (data.name == sceneName) {
				resumeCutscene = data.resumeCutscene;
				sceneFlow.Remove( data );
				break;
			}
		}
		
		SetScene(null);
		
		return resumeCutscene;
	}
	
	static public bool IsSceneRecent() {
		
		string sceneName = World.loadedLevelName;
		
		foreach (SceneData data in sceneFlow) {
			if (data.name == sceneName) {
				return true;
			}
		}
		
		return false;
	}	
	
	static public void ClearAttach( ActorBase actor, Transform parts) {
			
		if (parts == null)
			return;
			
		string name = actor.name;
		ActorParts actorParts;
		if (dicActorParts.ContainsKey(name)) {
			actorParts = dicActorParts[name];
		} 
		else
			return;
				
			
		actorParts.parts.Remove(parts);
			
		actor.ClearAttach(parts);
			
		
	}
		
	static public void ClearParts(ActorBase actor) {
		if (actor == null) {
			dicActorParts.Clear ();
		} else {
			dicActorParts.Remove(actor.name);
		}
	}

	static public void AddParts(string name, Transform parts) {

		Debug.Log ("AddParts Name : " + name.ToString() );
		if (parts == null)
			return;
		
		ActorParts actorParts;
		if (dicActorParts.ContainsKey(name)) {
			actorParts = dicActorParts[name];
		} else {
			actorParts = new ActorParts();
			actorParts.name = name;
			dicActorParts[name] = actorParts;
		}
		
		bool exists = false;
		foreach (Transform form in actorParts.parts) {
			if (form == parts) {
				exists = true;
				break;
			}
		}
		
		if (!exists) {
			actorParts.parts.Add(parts);
		}
	}
	
	static public void RestoreParts(ActorBase actor) {
		if (dicActorParts.Count == 0)
			return;

		if (actor == null) {
			ActorBase[] objs = GameObject.FindObjectsOfType(typeof(ActorBase)) as ActorBase[];
			foreach (ActorBase obj in objs) {
				RestoreParts(obj);
			}
		} else if (dicActorParts.ContainsKey(actor.name)) {
			foreach (Transform form in dicActorParts[actor.name].parts) {
				actor.ChangeInto(form, 0);
			}
		}
	}
	
	static public void LoadScene(string scene, int effect) {


		if( ( Time.time - ProcessLoadSceneTime ) < LoadSceneCoolTime )
			return;
		else
			ProcessLoadSceneTime = Time.time;

		if (scene != "" && scene != null) 
		{
			if (scene.Contains ("GoToStart"))
				ClearSceneData ();

			World.loadedLevelName = scene;
			PlayerPrefs.SetString("World.loadedLevelName",World.loadedLevelName);
		}
		
		GameObject obj = GameObject.FindGameObjectWithTag("AppMgr");
		if (obj != null) {Debug.Log("11111");
			obj.SendMessage("SetChangeSceneMode", effect);
			obj.SendMessage("SetSceneName", scene);
		} else if (!string.IsNullOrEmpty(scene))
		{Debug.Log("22222");
			Application.LoadLevel(scene);
		}
			
	}

	static public void LoadSceneShortCut(string scene, int effect)
	{
		foreach (SceneData data in sceneFlow) {
			if (data.name == scene) {
				data.resumeCutscene = null;
				break;
			}
		}

		LoadScene(scene, effect);
	}
	
	void Start() {
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		
		screenScaleFont = Global.GetScreenScaleFont();
		screenScale = Global.GetScreenScaleUI();
		if (textStyle == null)
			textStyle = new GUIStyle();
		textStyle.fontSize = (int) (textStyle.fontSize * screenScaleFont);

		if( captionFont != null )
		{
			textStyle.font = captionFont;
		}

	}
	
	void Update() {
		waitingTouch = false;
		
		if (touchDetectedTimer > 0)
			touchDetectedTimer -= Time.deltaTime;
	}
	
	void LateUpdate() {
		if (waitingTouch && Time.timeScale > 0) {
			#if UNITY_EDITOR
			if(Input.GetMouseButtonDown(0))	{
				bool consumed = false;
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit)) {
					
					if(Time.timeScale == 0f ) return;
					/*					testAudioSrc.clip = testSound;
					testAudioSrc.loop = false;
					testAudioSrc.Play();*/
					ActorBase actor = hit.transform.GetComponent<ActorBase>();
					if (actor != null) {
						Debug.Log("hahahah Find it");
						consumed = actor.TouchDown(hit.point);
					}
					//					hit.transform.SendMessage("TouchDown", hit.point, SendMessageOptions.DontRequireReceiver);
				}
				
				if (consumed) {
					touchDetectedTimer = 1f;
				} else
				if (touchSound != null) {
					//					Vector3 pos = Vector3.zero;
					//					if (Camera.main != null)
					//						pos = Camera.main.transform.position;
					Debug.Log ("TouchSound");
					AudioSource.PlayClipAtPoint(touchSound,Vector3.zero);
				}
			}
			
			if(preUfo != null && Input.GetMouseButtonDown(0)){
				Camera cam = Camera.main;
				if (cam != null) {
					Vector3 mousePos = Input.mousePosition;
					
					CamMgr mgr = cam.transform.root.GetComponentInChildren<CamMgr>();
					if (mgr != null) {
						mousePos.z = Mathf.Min (mgr.focusDist / 2, 2f);
					} else
						mousePos.z = 2f;
					
					Vector3 pos = cam.ScreenToWorldPoint(mousePos);
					/*				Transform form = GameObject.Instantiate(touchEffects[Random.Range(0, touchEffects.Length)]
	 * , pos, Quaternion.identity) as Transform;
					form.parent = transform;*/
					
					GameObject ufo = GameObject.FindGameObjectWithTag("UFO");
					if (ufo == null)
						GameObject.Instantiate(preUfo, pos, Quaternion.identity);
					else
						ufo.SendMessage("Position", pos);
				}
			}


			#else
			if (Input.touchCount > 0) {
				for(int i = 0; i < Input.touchCount; i++) {
					Touch t = Input.GetTouch(i);
					
					if (t.phase == TouchPhase.Began) {
						//touchBegan(t.position);
						bool consumed = false;
						Ray ray = Camera.main.ScreenPointToRay (t.position);
						RaycastHit hit;
						if (Physics.Raycast(ray, out hit)) {
							
							if(Time.timeScale == 0f ) return;
							/*					
							 * testAudioSrc.clip = testSound;
							testAudioSrc.loop = false;
							testAudioSrc.Play();*/
							ActorBase actor = hit.transform.GetComponent<ActorBase>();
							if (actor != null) {
								Debug.Log("hahahah Find it");
								consumed = actor.TouchDown(hit.point);
							}
							//					hit.transform.SendMessage("TouchDown", hit.point, SendMessageOptions.DontRequireReceiver);
						}
						
						if (consumed) {
							touchDetectedTimer = 1f;
						} else
						if (touchSound != null) {
							//					Vector3 pos = Vector3.zero;
							//					if (Camera.main != null)
							//						pos = Camera.main.transform.position;
							Debug.Log ("TouchSound");
							AudioSource.PlayClipAtPoint(touchSound,Vector3.zero);
						}
						break;
					}
					
					
					if(preUfo != null && t.phase == TouchPhase.Began){
						//touchRelease(t.position);
						Camera cam = Camera.main;
						if (cam != null) {
							Vector3 mousePos = t.position;
							
							CamMgr mgr = cam.transform.root.GetComponentInChildren<CamMgr>();
							if (mgr != null) {
								mousePos.z = Mathf.Min (mgr.focusDist / 2, 2f);
							} else
								mousePos.z = 2f;
							
							Vector3 pos = cam.ScreenToWorldPoint(mousePos);
							/*Transform form = GameObject.Instantiate(touchEffects[Random.Range(0, touchEffects.Length)] * , pos, Quaternion.identity) as Transform; form.parent = transform;*/
							
							GameObject ufo = GameObject.FindGameObjectWithTag("UFO");
							if (ufo == null)
								GameObject.Instantiate(preUfo, pos, Quaternion.identity);
							else
								ufo.SendMessage("Position", pos);
						}


						break;
					}
				}
			}
			#endif
		}
	}
	
	void OnGUI() {
		if (subtitle != null) {
			if( subtitleSource == null )
			{
				GUI.skin.label.alignment = TextAnchor.MiddleCenter;
				Rect rect = new Rect(Screen.width * 0.1f, Screen.height - 115f * screenScaleFont, Screen.width * 0.8f, 100);
				textStyle.normal.textColor = new Color(60f / 255, 8f / 255, 8f / 255, 255);
#if TaiwanVersion
				textStyle.contentOffset = new Vector2(-1, 0);
				GUI.Label(rect, subtitle, textStyle);
				textStyle.contentOffset = new Vector2(2, 0);
				GUI.Label(rect, subtitle, textStyle);
				textStyle.contentOffset = new Vector2(0, -1);
				GUI.Label(rect, subtitle, textStyle);
				textStyle.contentOffset = new Vector2(0, 2);
				GUI.Label(rect, subtitle, textStyle);
				textStyle.normal.textColor = Color.white;
				textStyle.contentOffset = Vector2.zero;
				GUI.Label(rect, subtitle, textStyle);
#else
				textStyle.contentOffset = new Vector2(-3, 0);
				GUI.Label(rect, subtitle, textStyle);
				textStyle.contentOffset = new Vector2(4, 0);
				GUI.Label(rect, subtitle, textStyle);
				textStyle.contentOffset = new Vector2(0, -3);
				GUI.Label(rect, subtitle, textStyle);
				textStyle.contentOffset = new Vector2(0, 4);
				GUI.Label(rect, subtitle, textStyle);
				textStyle.normal.textColor = Color.white;
				textStyle.contentOffset = Vector2.zero;
				GUI.Label(rect, subtitle, textStyle);
#endif
			}
			else
			{
				if (!subtitleSource.isTalking)
					subtitle = null;
				else {
					GUI.skin.label.alignment = TextAnchor.MiddleCenter;
					Rect rect = new Rect(Screen.width * 0.1f, Screen.height - 115f * screenScaleFont, Screen.width * 0.8f, 100);
					textStyle.normal.textColor = new Color(60f / 255, 8f / 255, 8f / 255, 255);
#if TaiwanVersion
					textStyle.contentOffset = new Vector2(-1, 0);
					GUI.Label(rect, subtitle, textStyle);
					textStyle.contentOffset = new Vector2(2, 0);
					GUI.Label(rect, subtitle, textStyle);
					textStyle.contentOffset = new Vector2(0, -1);
					GUI.Label(rect, subtitle, textStyle);
					textStyle.contentOffset = new Vector2(0, 2);
					GUI.Label(rect, subtitle, textStyle);
					textStyle.normal.textColor = Color.white;
					textStyle.contentOffset = Vector2.zero;
					GUI.Label(rect, subtitle, textStyle);
#else
					textStyle.contentOffset = new Vector2(-3, 0);
					GUI.Label(rect, subtitle, textStyle);
					textStyle.contentOffset = new Vector2(4, 0);
					GUI.Label(rect, subtitle, textStyle);
					textStyle.contentOffset = new Vector2(0, -3);
					GUI.Label(rect, subtitle, textStyle);
					textStyle.contentOffset = new Vector2(0, 4);
					GUI.Label(rect, subtitle, textStyle);
					textStyle.normal.textColor = Color.white;
					textStyle.contentOffset = Vector2.zero;
					GUI.Label(rect, subtitle, textStyle);
#endif

				}
			}
		}
		
		if (texTouchInput != null && (waitingTouch || touchDetectedTimer > 0)) {
			if( waitingTouch )
			{
				if (touchDetectedTimer > 0)
				{
					GUI.color = Color.yellow;
					DrawInputIcon(texTouchInput, screenScale*1.1f);
				}
				else
					DrawInputIcon(texTouchInput, screenScale);
			}
		}
	}
	
	public void DisplaySubtitle(string msg, ActorBase src) {
		subtitle = msg;
		subtitleSource = src;
	}
	
	public static void DrawInputIcon(Texture2D tex, float scale) {
		Rect rect = new Rect(0, 0, tex.width * scale, tex.height * scale);
		rect.x = Screen.width - rect.width - Global.uiGap * scale;
		rect.y = Screen.height - rect.height - Global.uiGap * scale;
		GUI.DrawTexture(rect, tex);
	}
	public void StartWaitTouchSound() {
		Debug.Log ("StartWaitTouchSound");
		if (touchStartSound != null)
			AudioSource.PlayClipAtPoint(touchStartSound,Vector3.zero);
	}
	
	void Show_Button_I()
	{
		button_I.SetActive(true);
	}
	
	void Hide_Button_I()
	{
		button_I.SetActive(false);
	}
}
