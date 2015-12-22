using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Dialog = CutsceneMgr.Dialog;
using BGM = CutsceneMgr.BGM;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

public class LcDataEditor {
	[MenuItem ("Assets/Dialog/Import Dialog Korean English Xml")]
	static void CreateDialog01 () {
		foreach (Object obj in Selection.objects) {
			Selection.activeObject = obj;
			if (Selection.activeObject != null && Selection.activeObject.GetType () == typeof(TextAsset)) {
				TextAsset ta = Selection.activeObject as TextAsset;
				ReadDialogData (ta, 0);
			}
		}
	}

	[MenuItem ("Assets/Dialog/Import Dialog Taiwan English Xml")]
	static void CreateDialog02 () {
//		Debug.Log ("CreateDialog02 Selection.activeObject = " + Selection.activeObject);
//		Debug.Log ("CreateDialog02 Selection.objects = " + Selection.objects [0]);
		foreach (Object obj in Selection.objects) {
			Selection.activeObject = obj;
			if (Selection.activeObject != null && Selection.activeObject.GetType () == typeof(TextAsset)) {
				TextAsset ta = Selection.activeObject as TextAsset;
				ReadDialogData (ta, 1);
			}
		}
	}
	
	class Item {
		public AudioClip clip1;
		public AudioClip clip2;
		public string caption1;
		public string caption2;
	}
	
	public static void ReadDialogData(TextAsset ta, int type) {
		//type : 0   Korean - English
		//type : 1   Taiwan - English
		List<Item> list = new List<Item>();
		
		Item item = new Item();
		
		item.caption2 = item.caption1 = "(None)";
		list.Add(item);
		item = null;
		
		if (ta != null) {
			string dir = Path.GetDirectoryName(AssetDatabase.GetAssetPath (Selection.activeObject));

			//Regex rgx = new Regex("(^" + EditorGlobal.dialogPath + "/)(.+)(/)");
			//string dirEng = rgx.Replace(dir, "$1English$3");

			string dirSecond ;
			if( type == 0 )
				dirSecond =  dir.Replace("Korean", "English");
			else if( type == 1 )
				dirSecond =  dir.Replace("Taiwan", "English");
			else
				dirSecond =  dir.Replace("Korean", "English");

//			string pathSecond = Path.Combine(dirSecond, ta.name);
//			string pathNative = Path.Combine(dir, ta.name);
			string pathSecond = Path.Combine(dirSecond, "voice");
			string pathNative = Path.Combine(dir, "voice");

			string languageNative = pathNative.Split('/')[4];
			string languageSecond = pathSecond.Split('/')[4];

//			Debug.Log ( "pathEng = "+ pathEng);
//			Debug.Log ( "pathNative = "+ pathNative);
	
			XmlReader reader = XmlReader.Create(new StringReader(ta.text));
	
			while (reader.Read())
		    {
				// Only detect start elements.
				if (reader.IsStartElement())
				{
					switch (reader.Name) {
					case "Item":
						if (item != null) {
							list.Add(item);
						}
						item = new Item();
						break;
						
					case "filename":
						if (reader.Read() && reader.NodeType == XmlNodeType.Text) {
							if(reader.Value.Contains("_mute") || reader.Value == "1.5s" || reader.Value == "1.5s_mute" || reader.Value == "3s" || reader.Value == "3s_mute" || reader.Value == "5s" || reader.Value == "5s_mute"){// if filename has "mute" string
								item.clip1 = AssetDatabase.LoadAssetAtPath(Path.Combine("Assets/Design/Sound/Dialog/_TempSound/", reader.Value + ".ogg"), typeof(AudioClip)) as AudioClip;
								item.clip2 = AssetDatabase.LoadAssetAtPath(Path.Combine("Assets/Design/Sound/Dialog/_TempSound/", reader.Value + ".ogg"), typeof(AudioClip)) as AudioClip;

							}else if(reader.Value.Contains("_home")){
								item.clip1 = AssetDatabase.LoadAssetAtPath(Path.Combine("Assets/Design/Sound/Dialog/" + languageNative + "/common/voice", reader.Value + ".ogg"), typeof(AudioClip)) as AudioClip;
								item.clip2 = AssetDatabase.LoadAssetAtPath(Path.Combine("Assets/Design/Sound/Dialog/" + languageSecond + "/common/voice", reader.Value + ".ogg"), typeof(AudioClip)) as AudioClip;

							}else{
								item.clip1 = AssetDatabase.LoadAssetAtPath(Path.Combine(pathNative, reader.Value + ".ogg"), typeof(AudioClip)) as AudioClip;
								item.clip2 = AssetDatabase.LoadAssetAtPath(Path.Combine(pathSecond, reader.Value + ".ogg"), typeof(AudioClip)) as AudioClip;

							}
						}
						break;
						
					case "caption1":
						if (reader.Read() && reader.NodeType == XmlNodeType.Text)
							item.caption1 = reader.Value;
						break;
	
					case "caption2":
						if (reader.Read() && reader.NodeType == XmlNodeType.Text)
							item.caption2 = reader.Value;
						break;
					}
			    }
			}
			
			if (item != null)
				list.Add(item);
		}
		
		string fileName = Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject));
		string pathTemp = AssetDatabase.GetAssetPath (Selection.activeObject);
		pathTemp = Path.GetDirectoryName(pathTemp);
		pathTemp = Path.Combine(pathTemp, Path.ChangeExtension(fileName, ".asset"));
		
		DialogData data;
		data = AssetDatabase.LoadAssetAtPath(pathTemp, typeof(DialogData)) as DialogData;
		if (data == null) {
			data = ScriptableObject.CreateInstance<DialogData>();
			AssetDatabase.CreateAsset (data, pathTemp);
		}
		
		data.dialogs1 = new Dialog[list.Count];
		data.dialogs2 = new Dialog[list.Count];
		
		for (int i = 0; i < list.Count; i++) {
			item = list[i];
			data.dialogs1[i] = new Dialog(item.clip1, item.caption1);
			data.dialogs2[i] = new Dialog(item.clip2, item.caption2);
		}
		
		EditorUtility.SetDirty(data);
		
		AssetDatabase.SaveAssets ();
		EditorUtility.FocusProjectWindow ();
		Selection.activeObject = data;		
	}
	
	[MenuItem ("Assets/Dialog/Import BGM Korean English Xml")]
	static void CreateBGM01 () {
		foreach (Object obj in Selection.objects) {
			Selection.activeObject = obj;
			if (Selection.activeObject != null && Selection.activeObject.GetType () == typeof(TextAsset)) {
				TextAsset ta = Selection.activeObject as TextAsset;
				ReadBGMData (ta, 0);
			}
		}
	}

	[MenuItem ("Assets/Dialog/Import BGM Taiwan English Xml")]
	static void CreateBGM02 () {
		foreach (Object obj in Selection.objects) {
			Selection.activeObject = obj;
			if (Selection.activeObject != null && Selection.activeObject.GetType () == typeof(TextAsset)) {
				TextAsset ta = Selection.activeObject as TextAsset;
				ReadBGMData (ta, 1);
			}
		}
	}
	
	class BGMItem {
		public AudioClip clip1;
		public AudioClip clip2;
	}

	public static void ReadBGMData(TextAsset ta, int type) {
		//type : 0   Korean - English
		//type : 1   Taiwan - English
		List<BGMItem> list = new List<BGMItem>();
		
		BGMItem item = new BGMItem();
		
		item.clip1 = null;
		item.clip2 = null;
		//item.caption2 = item.caption1 = "(None)";
		list.Add(item);
		item = null;
		
		if (ta != null) {
			string dir = Path.GetDirectoryName(AssetDatabase.GetAssetPath (Selection.activeObject));

			//Regex rgx = new Regex("(^" + EditorGlobal.dialogPath + "/)(.+)(/)");
			//string dirEng = rgx.Replace(dir, "$1English$3");
			string dirSecond ;
			if( type == 0 )
				dirSecond =  dir.Replace("Korean", "English");
			else if( type == 1 )
				dirSecond =  dir.Replace("Taiwan", "English");
			else
				dirSecond =  dir.Replace("Korean", "English");
			
//			string pathSecond = Path.Combine(dirSecond, ta.name);
//			string pathNative = Path.Combine(dir, ta.name);

			string pathSecond = Path.Combine(dirSecond, "song");
			string pathNative = Path.Combine(dir, "song");

			string languageNative = pathNative.Split('/')[4];
			string languageSecond = pathSecond.Split('/')[4];
			
//			Debug.Log ( "pathEng = "+ pathEng);
//			Debug.Log ( "pathNative = "+ pathNative);
	
			XmlReader reader = XmlReader.Create(new StringReader(ta.text));
	
			while (reader.Read())
		    {
				// Only detect start elements.
				if (reader.IsStartElement())
				{
					switch (reader.Name) {
					case "Item":
						if (item != null) {
							list.Add(item);
						}
						item = new BGMItem();
						break;
						
					case "filename":
						if (reader.Read() && reader.NodeType == XmlNodeType.Text) {
							if(reader.Value.Contains("_mute") || reader.Value == "1.5s" || reader.Value == "1.5s_mute" || reader.Value == "3s" || reader.Value == "3s_mute" || reader.Value == "5s" || reader.Value == "5s_mute"){// if filename has "mute" string
								item.clip1 = AssetDatabase.LoadAssetAtPath(Path.Combine("Assets/Design/Sound/Dialog/_TempSound/", reader.Value + ".ogg"), typeof(AudioClip)) as AudioClip;
								item.clip2 = AssetDatabase.LoadAssetAtPath(Path.Combine("Assets/Design/Sound/Dialog/_TempSound/", reader.Value + ".ogg"), typeof(AudioClip)) as AudioClip;

							}else if(reader.Value.Contains("_home")){
								item.clip1 = AssetDatabase.LoadAssetAtPath(Path.Combine("Assets/Design/Sound/Dialog/" + languageNative + "/common/song", reader.Value + ".ogg"), typeof(AudioClip)) as AudioClip;
								item.clip2 = AssetDatabase.LoadAssetAtPath(Path.Combine("Assets/Design/Sound/Dialog/" + languageSecond + "/common/song", reader.Value + ".ogg"), typeof(AudioClip)) as AudioClip;

							}else{
								item.clip1 = AssetDatabase.LoadAssetAtPath(Path.Combine(pathNative, reader.Value + ".ogg"), typeof(AudioClip)) as AudioClip;
								item.clip2 = AssetDatabase.LoadAssetAtPath(Path.Combine(pathSecond, reader.Value + ".ogg"), typeof(AudioClip)) as AudioClip;

							}
						}
						break;
					}
			    }
			}
			
			if (item != null)
				list.Add(item);
		}
		
		string fileName = Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject));
		string pathTemp = AssetDatabase.GetAssetPath (Selection.activeObject);
		pathTemp = Path.GetDirectoryName(pathTemp);
		pathTemp = Path.Combine(pathTemp, Path.ChangeExtension(fileName, ".asset"));
		
		BGMData data;
		data = AssetDatabase.LoadAssetAtPath(pathTemp, typeof(BGMData)) as BGMData;
		if (data == null) {
			data = ScriptableObject.CreateInstance<BGMData>();
			AssetDatabase.CreateAsset (data, pathTemp);
		}
		
		data.bgm1 = new BGM[list.Count];
		data.bgm2 = new BGM[list.Count];
		
		for (int i = 0; i < list.Count; i++) {
			item = list[i];
			data.bgm1[i] = new BGM(item.clip1);
			data.bgm2[i] = new BGM(item.clip2);
		}
		
		EditorUtility.SetDirty(data);
		
		AssetDatabase.SaveAssets ();
		EditorUtility.FocusProjectWindow ();
		Selection.activeObject = data;		
	}

	//Added By SH
	[MenuItem("Assets/Optimize Animators In Scene")]
	static public void OptimizeAnimatorInScene(){
		GameObject root = GameObject.Find("Root");

		if(root == null){
			Debug.LogError("Please, make gameObject named Root");
			return;
		}

		Animator[] anims = root.GetComponentsInChildren<Animator>(true);
		CutsceneMgr[] cutsceneMgrs = root.GetComponentsInChildren<CutsceneMgr>(true);
		SongData[] songDatas = root.GetComponentsInChildren<SongData>(true);

		List<UnityEditorInternal.AnimatorController> newAnimConList = new List<UnityEditorInternal.AnimatorController>();

		string[] scenePathParts = EditorApplication.currentScene.Split(char.Parse("/"));
		
		string sceneName = scenePathParts[scenePathParts.Length - 1].Split(char.Parse("."))[0];
		string animatorEpisodeDirectoryPath = null;
//		string textFilePath  = null;
//		string textLine = null;
		string episodeName = null;
//		string filename = null;
		string newCharAnimConPath = null;

		if(sceneName.Contains("episode_e")){//check english scene name
			episodeName = sceneName.Split(char.Parse("_"))[1] + "_" + sceneName.Split(char.Parse("_"))[2];
		}else if(sceneName.Contains("review_e")){
			episodeName = sceneName.Split(char.Parse("_"))[0] + "_" + sceneName.Split(char.Parse("_"))[1];
		}else{
			episodeName = sceneName.Split(char.Parse("_"))[0];
		}

		if(!Directory.Exists("Assets/AnimatorOptimize/")){
			AssetDatabase.CreateFolder("Assets", "AnimatorOptimize");
		}

		if(!Directory.Exists("Assets/AnimatorOptimize/" + episodeName)){
			AssetDatabase.CreateFolder("Assets/AnimatorOptimize", episodeName);
		}

		animatorEpisodeDirectoryPath = "Assets/AnimatorOptimize/" + episodeName + "/";

		int actionNum = -1;

		UnityEditorInternal.AnimatorController ac = null;
		UnityEditorInternal.AnimatorControllerLayer layer = null;
		UnityEditorInternal.StateMachine sm = null;
		
		UnityEditorInternal.AnimatorController nac = null;
		UnityEditorInternal.AnimatorControllerLayer nLayer = null;
		UnityEditorInternal.StateMachine nsm = null;


		for(int i = 0 ; i < anims.Length ; i++){//Create New Animator
			if(anims[i] != null && anims[i].runtimeAnimatorController != null){
				ac = anims[i].runtimeAnimatorController as UnityEditorInternal.AnimatorController;
				layer = ac.GetLayer(0);
				sm = layer.stateMachine;
				
				newCharAnimConPath = animatorEpisodeDirectoryPath + ac.name + "_" + episodeName + ".controller";
				
				if(AssetDatabase.LoadAssetAtPath(newCharAnimConPath, typeof(UnityEditorInternal.AnimatorController)) == null){
					AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(ac), newCharAnimConPath);
					AssetDatabase.ImportAsset(newCharAnimConPath);
					
					nac = AssetDatabase.LoadAssetAtPath(newCharAnimConPath, typeof(UnityEditorInternal.AnimatorController)) as UnityEditorInternal.AnimatorController;
					nLayer = nac.GetLayer(0);
					nsm = nLayer.stateMachine;

					for(int k = 0 ; k < nsm.stateCount ; k++){
						UnityEditorInternal.State nState = nsm.GetState(k);
						if(nState.name == "LoopAction" || nState.name == "Action"){
							UnityEditorInternal.BlendTree nbt = nState.GetMotion() as UnityEditorInternal.BlendTree;
							
							for(int l = nbt.childCount - 1 ; l > 0 ; l--){
								nbt.RemoveChild(l);
							}
						}
					}
				}else{
					nac = AssetDatabase.LoadAssetAtPath(newCharAnimConPath, typeof(UnityEditorInternal.AnimatorController)) as UnityEditorInternal.AnimatorController;
				}
				newAnimConList.Add(nac);
			}
		}
		
		for(int i = 0 ; i < cutsceneMgrs.Length ; i++){// actions in cutscenes added at new animator

			CutsceneStep[] steps = cutsceneMgrs[i].steps.ToArray();

			for(int j = 0 ; j < steps.Length ; j++){
				if(steps[j].actor != null){

					ActorChar actor = steps[j].actor as ActorChar;
					CutsceneStep.Action stepAction = steps[j].action;
					Animator anim = null;

					if(actor != null){
						anim = actor.GetComponent<Animator>();
					}

					if(anim != null && anim.runtimeAnimatorController != null){

						if(stepAction == CutsceneStep.Action.Action || stepAction == CutsceneStep.Action.LoopAction || stepAction == CutsceneStep.Action.SpeechTo || stepAction == CutsceneStep.Action.TalkTo){

							switch(stepAction){
							case CutsceneStep.Action.Action:
								actionNum = steps[j].intVal;
								break;

							case CutsceneStep.Action.LoopAction:
								actionNum = steps[j].intVal;
								break;

							case CutsceneStep.Action.SpeechTo:
								actionNum = steps[j].intVal2;
								break;

							case CutsceneStep.Action.TalkTo:
								actionNum = steps[j].intVal2;
								break;

							default:
								break;
							}

//							filename = null;

							ac = anim.runtimeAnimatorController as UnityEditorInternal.AnimatorController;
							layer = ac.GetLayer(0);
							sm = layer.stateMachine;

							nac = null;
							nLayer = null;
							nsm = null;

							newCharAnimConPath = animatorEpisodeDirectoryPath + ac.name + "_" + episodeName + ".controller";

							if(AssetDatabase.LoadAssetAtPath(newCharAnimConPath, typeof(UnityEditorInternal.AnimatorController)) != null){
								nac = AssetDatabase.LoadAssetAtPath(newCharAnimConPath, typeof(UnityEditorInternal.AnimatorController)) as UnityEditorInternal.AnimatorController;
								nLayer = nac.GetLayer(0);
								nsm = nLayer.stateMachine;
							}

							if(stepAction == CutsceneStep.Action.LoopAction){

								for(int k = 0 ; k < sm.stateCount ; k++){
									UnityEditorInternal.State state = sm.GetState(k);
									UnityEditorInternal.State nState = nsm.GetState(k);

									if(state.name == "LoopAction"){
										UnityEditorInternal.BlendTree bt = state.GetMotion() as UnityEditorInternal.BlendTree;
										UnityEditorInternal.BlendTree nbt = nState.GetMotion() as UnityEditorInternal.BlendTree;

										Motion mo = null;
//										Motion nmo = null;

										int l = 0;
										int m = 0;

										for(l = 0 ; l < bt.childCount ; l++){
											if(actionNum == bt.GetChildThreshold(l) && bt.GetMotion(l) != null){
												mo = bt.GetMotion(l);
//												filename = mo.name;
												break;
											}
										}

										for(m = 0 ; m < nbt.childCount ; m++){
											if(mo == null || mo == nbt.GetMotion(m)){
												break;

											}else if(m == nbt.childCount - 1){
												nbt.AddAnimationClip(mo as AnimationClip);
												nbt.SetChildThreshold(nbt.childCount - 1, bt.GetChildThreshold(l));
												nbt.SetChildTimeScale(nbt.childCount - 1, bt.GetChildTimeScale(l));
											}
										}
									}
								}

//								textLine = "LoopAction Number : " + actionNum + "   LoopAction Description : " + filename;

							}else{

								for(int k = 0 ; k < sm.stateCount ; k++){
									UnityEditorInternal.State state = sm.GetState(k);
									UnityEditorInternal.State nState = nsm.GetState(k);
									
									if(state.name == "Action"){
										UnityEditorInternal.BlendTree bt = state.GetMotion() as UnityEditorInternal.BlendTree;
										UnityEditorInternal.BlendTree nbt = nState.GetMotion() as UnityEditorInternal.BlendTree;
										
										Motion mo = null;
//										Motion nmo = null;
										
										int l = 0;
										int m = 0;
										
										for(l = 0 ; l < bt.childCount ; l++){
											if(actionNum == bt.GetChildThreshold(l) && bt.GetMotion(l) != null){
												mo = bt.GetMotion(l);
//												filename = mo.name;
												break;
											}
										}
										
										for(m = 0 ; m < nbt.childCount ; m++){
											if(mo == null ||  mo == nbt.GetMotion(m)){
												break;

											}else if(m == nbt.childCount - 1){
												nbt.AddAnimationClip(mo as AnimationClip);
												nbt.SetChildThreshold(nbt.childCount - 1, bt.GetChildThreshold(l));
												nbt.SetChildTimeScale(nbt.childCount - 1, bt.GetChildTimeScale(l));
											}
										}
									}
								}

//								textLine = "Action Number : " + actionNum + "   Action Description : " + filename;
							}

//							textFilePath = animatorEpisodeDirectoryPath + sceneName + "_" + actor.name + "_PlayActions.txt";
//
//							if(!System.IO.File.Exists(textFilePath)){//write textfile
//								using(System.IO.StreamWriter sw = System.IO.File.CreateText(textFilePath)){
//									sw.WriteLine(textLine);
//									sw.Close();
//								}
//							}else{
//								string[] textAllLine = System.IO.File.ReadAllLines(textFilePath);
//								using(System.IO.StreamWriter sw = System.IO.File.AppendText(textFilePath)){
//									for(int k = 0 ; k < textAllLine.Length ; k++){
//										if(textAllLine[k].Equals(textLine)){
//											break;
//										}
//
//										if(k == textAllLine.Length - 1){
//											sw.WriteLine(textLine);
//											sw.Close();
//										}
//									}
//								}
//							}
						}
					}
				}
			}
		}
		
		for(int i = 0 ; i < songDatas.Length ; i++){// actions in songdata added at new animator

			SongData songdata = songDatas[i];

			for(int j = 0 ; j < songdata.Notes.Count ; j++){

				CutsceneStep step = songdata.Notes[j].Step;
				if(step.actor != null){
					
					ActorChar actor = step.actor as ActorChar;
					CutsceneStep.Action stepAction = step.action;
					Animator anim = null;
					
					if(actor != null){
						anim = actor.GetComponent<Animator>();
					}

					if(anim != null && anim.runtimeAnimatorController != null){
					
						if(stepAction == CutsceneStep.Action.Action || stepAction == CutsceneStep.Action.LoopAction || stepAction == CutsceneStep.Action.SpeechTo || stepAction == CutsceneStep.Action.TalkTo){

							switch(stepAction){
							case CutsceneStep.Action.Action:
								actionNum = step.intVal;
								break;
								
							case CutsceneStep.Action.LoopAction:
								actionNum = step.intVal;
								break;
								
							case CutsceneStep.Action.SpeechTo:
								actionNum = step.intVal2;
								break;
								
							case CutsceneStep.Action.TalkTo:
								actionNum = step.intVal2;
								break;
								
							default:
								break;
							}
							
//							filename = null;
							
							ac = anim.runtimeAnimatorController as UnityEditorInternal.AnimatorController;
							layer = ac.GetLayer(0);
							sm = layer.stateMachine;
							
							nac = null;
							nLayer = null;
							nsm = null;
							
							newCharAnimConPath = animatorEpisodeDirectoryPath + ac.name + "_" + episodeName + ".controller";
							
							if(AssetDatabase.LoadAssetAtPath(newCharAnimConPath, typeof(UnityEditorInternal.AnimatorController)) != null){
								nac = AssetDatabase.LoadAssetAtPath(newCharAnimConPath, typeof(UnityEditorInternal.AnimatorController)) as UnityEditorInternal.AnimatorController;
								nLayer = nac.GetLayer(0);
								nsm = nLayer.stateMachine;
							}

							if(stepAction == CutsceneStep.Action.LoopAction){
								
								for(int k = 0 ; k < sm.stateCount ; k++){
									UnityEditorInternal.State state = sm.GetState(k);
									UnityEditorInternal.State nState = nsm.GetState(k);
									
									if(state.name == "LoopAction"){
										UnityEditorInternal.BlendTree bt = state.GetMotion() as UnityEditorInternal.BlendTree;
										UnityEditorInternal.BlendTree nbt = nState.GetMotion() as UnityEditorInternal.BlendTree;
										
										Motion mo = null;
//										Motion nmo = null;
										
										int l = 0;
										int m = 0;
										
										for(l = 0 ; l < bt.childCount ; l++){
											if(actionNum == bt.GetChildThreshold(l) && bt.GetMotion(l) != null){
												mo = bt.GetMotion(l);
//												filename = mo.name;
												break;
											}
										}
										
										for(m = 0 ; m < nbt.childCount ; m++){
											if(mo == null || mo == nbt.GetMotion(m)){
												break;
												
											}else if(m == nbt.childCount - 1){
												nbt.AddAnimationClip(mo as AnimationClip);
												nbt.SetChildThreshold(nbt.childCount - 1, bt.GetChildThreshold(l));
												nbt.SetChildTimeScale(nbt.childCount - 1, bt.GetChildTimeScale(l));
											}
										}
									}
								}
								
//								textLine = "LoopAction Number : " + actionNum + "   LoopAction Description : " + filename;
								
							}else{
								
								for(int k = 0 ; k < sm.stateCount ; k++){
									UnityEditorInternal.State state = sm.GetState(k);
									UnityEditorInternal.State nState = nsm.GetState(k);
									
									if(state.name == "Action"){
										UnityEditorInternal.BlendTree bt = state.GetMotion() as UnityEditorInternal.BlendTree;
										UnityEditorInternal.BlendTree nbt = nState.GetMotion() as UnityEditorInternal.BlendTree;
										
										Motion mo = null;
//										Motion nmo = null;
										
										int l = 0;
										int m = 0;
										
										for(l = 0 ; l < bt.childCount ; l++){
											if(actionNum == bt.GetChildThreshold(l) && bt.GetMotion(l) != null){
												mo = bt.GetMotion(l);
//												filename = mo.name;
												break;
											}
										}
										
										for(m = 0 ; m < nbt.childCount ; m++){
											if(mo == null ||  mo == nbt.GetMotion(m)){
												break;
												
											}else if(m == nbt.childCount - 1){
												nbt.AddAnimationClip(mo as AnimationClip);
												nbt.SetChildThreshold(nbt.childCount - 1, bt.GetChildThreshold(l));
												nbt.SetChildTimeScale(nbt.childCount - 1, bt.GetChildTimeScale(l));
											}
										}
									}
								}
								
//								textLine = "Action Number : " + actionNum + "   Action Description : " + filename;
							}

//							textFilePath = animatorEpisodeDirectoryPath + sceneName + "_" + actor.name + "_PlayActions.txt";
//
//							if(!System.IO.File.Exists(textFilePath)){//write textfile
//								using(System.IO.StreamWriter sw = System.IO.File.CreateText(textFilePath)){
//									sw.WriteLine(textLine);
//									sw.Close();
//								}
//							}else{
//								string[] textAllLine = System.IO.File.ReadAllLines(textFilePath);
//								using(System.IO.StreamWriter sw = System.IO.File.AppendText(textFilePath)){
//									for(int k = 0 ; k < textAllLine.Length ; k++){
//										if(textAllLine[k].Equals(textLine)){
//											break;
//										}
//										
//										if(k == textAllLine.Length - 1){
//											sw.WriteLine(textLine);
//											sw.Close();
//										}
//									}
//								}
//							}
						}
					}
				}
			}
		}

		for(int i = 0 ; i < newAnimConList.Count ; i++){//sort blend tree children
			if(newAnimConList[i] != null){
				UnityEditorInternal.AnimatorControllerLayer tempLayer = newAnimConList[i].GetLayer(0);
				UnityEditorInternal.StateMachine tempSM = tempLayer.stateMachine;
				
				for(int j = 0 ; j < tempSM.stateCount ; j++){
					UnityEditorInternal.State tempState = tempSM.GetState(j);
					
					if(tempState.name == "Action" || tempState.name == "LoopAction"){
						UnityEditorInternal.BlendTree tempBT = tempState.GetMotion() as UnityEditorInternal.BlendTree;
						tempBT.SortChildren();
						//Debug.Log(newAnimConList[i].name + " is sorted!");
					}
				}
			}
		}
		Debug.Log(sceneName + " Animators optimize complete!");
	}

	[MenuItem("Assets/Link Optimize Animator")]
	static public void LinkOptimaizeAnimator(){
		GameObject root = GameObject.Find("Root");
		
		if(root == null){
			Debug.LogError("Please, make gameObject named Root");
			return;
		}
		
		Animator[] anims = root.GetComponentsInChildren<Animator>(true);
		
		string[] scenePathParts = EditorApplication.currentScene.Split(char.Parse("/"));
		
		string sceneName = scenePathParts[scenePathParts.Length - 1].Split(char.Parse("."))[0];
		string animatorEpisodeDirectoryPath = null;
		string episodeName = null;
//		string newCharAnimConPath = null;
		
		if(sceneName.Contains("episode_e")){//check english scene name
			episodeName = sceneName.Split(char.Parse("_"))[1] + "_" + sceneName.Split(char.Parse("_"))[2];
		}else if(sceneName.Contains("review_e")){
			episodeName = sceneName.Split(char.Parse("_"))[0] + "_" + sceneName.Split(char.Parse("_"))[1];
		}else{
			episodeName = sceneName.Split(char.Parse("_"))[0];
		}

		if(!Directory.Exists("Assets/AnimatorOptimize/" + episodeName)){
			Debug.Log("Execute Optimize Animator In Scene first!");
			return;
		}
		
		animatorEpisodeDirectoryPath = "Assets/AnimatorOptimize/" + episodeName + "/";

		for(int i = 0 ; i < anims.Length ; i++){
			if(anims[i].runtimeAnimatorController != null){
				string optimizeAnimatorConPath = animatorEpisodeDirectoryPath + anims[i].runtimeAnimatorController.name + "_" + episodeName + ".controller";

				RuntimeAnimatorController rac = AssetDatabase.LoadAssetAtPath(optimizeAnimatorConPath, typeof(UnityEditorInternal.AnimatorController)) as RuntimeAnimatorController;
				if(rac != null){
					anims[i].runtimeAnimatorController = rac;
				}
			}
		}
		Debug.Log(sceneName + " Animators link complete!");
	}

/*	[MenuItem("Assets/Textures/ReduceTextureInFolder")]
	static public void ReduceTexureSizeInFolder(){

		int formatChangedTexCnt = 0;
		int sizeChangedTexCnt = 0;
		int checkPixelSize = 512;
		bool isTextureChanged;

		string objPath = null;

		System.DateTime dt1 = System.DateTime.Now;
		System.DateTime dt2;

		for(int h = 0 ; h < Selection.objects.Length ; h++){
			Selection.activeObject = Selection.objects[h];
			objPath = AssetDatabase.GetAssetPath(Selection.activeObject);

			FileAttributes attr = File.GetAttributes(objPath);

			if(attr == FileAttributes.Directory){//isDirectory?
				DirectoryInfo di = new DirectoryInfo(objPath);
				
				FileInfo[] fileInfos = di.GetFiles("*", SearchOption.AllDirectories);
				string[] stringSeparators = new string[]{"Assets/"};

				Texture2D tempTexture = null;
				TextureImporter tempTextureImporter = null;
				
				for(int i = 0 ; i < fileInfos.Length ; i++){
					isTextureChanged = false;

					string path = Path.Combine( "Assets/", fileInfos[i].FullName.Split(stringSeparators, System.StringSplitOptions.None)[1]);

					tempTexture =  AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;

					if(tempTexture != null && tempTexture.GetType() == typeof(Texture2D)){
						tempTextureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
						
						if(tempTextureImporter.textureFormat != TextureImporterFormat.AutomaticCompressed){
							tempTextureImporter.textureFormat = TextureImporterFormat.AutomaticCompressed;
							formatChangedTexCnt++;
							isTextureChanged = true;
						}
						
						if(tempTexture.width >= checkPixelSize){
							tempTextureImporter.maxTextureSize = tempTexture.width / 2;
							sizeChangedTexCnt++;
							isTextureChanged = true;

						}else if(tempTexture.height >= checkPixelSize){
							tempTextureImporter.maxTextureSize = tempTexture.height / 2;
							sizeChangedTexCnt++;
							isTextureChanged = true;
						}

						if(isTextureChanged){
							AssetDatabase.ImportAsset(path);
						}else{
							if(i % 30 == 0)
								EditorUtility.UnloadUnusedAssets();//Release Unused Assets
						}
					}
				}
			}
		}
		dt2 = System.DateTime.Now;

		Debug.Log(Selection.activeObject.name + " folder and sub folder texures are resized!");
		Debug.Log("Changed size Texture count : " + (formatChangedTexCnt + sizeChangedTexCnt));
		Debug.Log("Resize Textures ProccessTime : " + new System.TimeSpan(dt2.Ticks - dt1.Ticks));

	}*/
}
