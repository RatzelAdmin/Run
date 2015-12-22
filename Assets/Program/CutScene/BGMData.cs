using UnityEngine;
using System.Collections;

public class BGMData : ScriptableObject  {
	
	public CutsceneMgr.BGM[] bgm1;
	public CutsceneMgr.BGM[] bgm2;
	public CutsceneMgr.BGM[] bgm3;
	public CutsceneMgr.BGM[] bgm4;
	public CutsceneMgr.BGM[] bgm5;
	public CutsceneMgr.BGM[] bgm6;
	public CutsceneMgr.BGM[] bgm7;
	public CutsceneMgr.BGM[] bgm8;
	public CutsceneMgr.BGM[] bgm9;
	public CutsceneMgr.BGM[] bgm10;
	
	
	public CutsceneMgr.BGM[] GetBGMs(int idx) {
		switch (idx) {
		case 1:
			return bgm2;
		case 2:
			return bgm3;
		case 3:
			return bgm4;
		case 4:
			return bgm5;
		case 5:
			return bgm6;
		case 6:
			return bgm7;
		case 7:
			return bgm8;
		case 8:
			return bgm9;
		case 9:
			return bgm10;
			
		default:
			return bgm1;
		}
	}
}
