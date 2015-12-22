using UnityEngine;
using System.Collections;

public class DialogData : ScriptableObject {
	public CutsceneMgr.Dialog[] dialogs1;
	public CutsceneMgr.Dialog[] dialogs2;
	public CutsceneMgr.Dialog[] dialogs3;
	public CutsceneMgr.Dialog[] dialogs4;
	public CutsceneMgr.Dialog[] dialogs5;
	public CutsceneMgr.Dialog[] dialogs6;
	public CutsceneMgr.Dialog[] dialogs7;
	public CutsceneMgr.Dialog[] dialogs8;
	public CutsceneMgr.Dialog[] dialogs9;
	public CutsceneMgr.Dialog[] dialogs10;
	
	public CutsceneMgr.Dialog[] GetDialogs(int idx) {
		switch (idx) {
		case 0:
			return dialogs1;
		case 1:
			return dialogs2;
		case 2:
			return dialogs3;
		case 3:
			return dialogs4;
		case 4:
			return dialogs5;
		case 5:
			return dialogs6;
		case 6:
			return dialogs7;
		case 7:
			return dialogs8;
		case 8:
			return dialogs9;
		case 9:
			return dialogs10;
			
		default:
			return dialogs1;
		}
	}
}
