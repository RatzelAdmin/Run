#pragma strict

//var successEpName:GUIText;
//var successChName:GUIText;


public var checkSuccessChapters : boolean[];
public var checkSuccessCollections : boolean[];
public var bCollectionMode : boolean = false;
public var bClearChapter : boolean = false;
public var bMeetFriend : boolean = false;
public var bFirstStarWorld : boolean = false;
public var index_CurrentChapter : int;
public var index_SuccessChapter : int;

public var nowSuccessEpisodeName : String;
public var nowSuccessChapterNumber : String;

function Awake()
{
	DontDestroyOnLoad (transform.gameObject);  
    Screen.sleepTimeout = SleepTimeout.NeverSleep;
	
	//index_SuccessChapter = -1;
	ReSetNameString();
}

public function SetSuccessChapter( )
{
	//index_SuccessChapter = index_CurrentChapter;
	//index_CurrentChapter = -1;
	
	
	// 현재 플레이 중인 에피소드 네임을 얻어온다.
	var getEpisodeName : String = PlayerPrefs.GetString("SuccessChapterName",Application.loadedLevelName);

	Debug.Log("getEpisodeName = "+getEpisodeName);
//	if( getEpisodeName[7] == 'S' || getEpisodeName[7] == 's' )
//	{
//		print("씨즌 에피소드");
//	}
//	else
//	{
//		print("일반 에피소드");
	if( getEpisodeName != "" )
	{
		var tmpStr:String[] = getEpisodeName.Split("_"[0]);
		if( tmpStr.Length >= 2)
		{
			nowSuccessEpisodeName = tmpStr[0].ToString();
			nowSuccessChapterNumber = tmpStr[1].ToString();
			Debug.Log("nowSuccessEpisodeName = "+nowSuccessEpisodeName);
			Debug.Log("nowSuccessChapterNumber = "+nowSuccessChapterNumber);
		}
	}
		
//		successEpName.text = "Success Episode Name = " + nowSuccessEpisodeName.ToString();
//		successChName.text = "Success Chapter Name = " + nowSuccessChapterNumber.ToString();
//	}
	
	
}

public function ReSetNameString()
{
	nowSuccessEpisodeName = null;
	nowSuccessChapterNumber = null;
}

function CheckSuccessChapter() : boolean
{
	var successCount : int = 0;
	for( var i : int = 0; i < checkSuccessChapters.Length ; ++i )
	{
		if( checkSuccessChapters[i] )
			++successCount;
	}
	if( successCount == checkSuccessChapters.Length )
	{
		return true;
	}
		
	return false;
}
public function ResetSuccessChapter() 
{
	Debug.Log("ResetSuccessChapter");
	ReSetNameString();
	for( var i : int = 0; i < checkSuccessChapters.Length ; ++i )
		 checkSuccessChapters[i]  = false;
		 
}
function CheckSuccessCollection() : boolean
{
	var successCount : int = 0;
	for( var i : int = 0; i < checkSuccessCollections.Length ; ++i )
	{
		if( checkSuccessCollections[i] )
			++successCount;
	}
	if( successCount == checkSuccessCollections.Length )
		return true;
		
	return false;
}
function CheckNothingCollection() : boolean
{
	var successCount : int = 0;
	for( var i : int = 0; i < checkSuccessCollections.Length ; ++i )
	{
		if( checkSuccessCollections[i] )
			++successCount;
	}
	if( successCount == 0 )
		return true;
		
	return false;
}