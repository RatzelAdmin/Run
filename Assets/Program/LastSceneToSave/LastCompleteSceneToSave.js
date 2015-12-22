#pragma strict

enum enumThisSceneType  { chapter, doraWorld };
var endingPosition : enumThisSceneType;

function Start ()
{
		PlayerPrefs.SetString( "level2LastPlace", endingPosition.ToString() );
		this.gameObject.SetActive( false );
}

function Update ()
{
}