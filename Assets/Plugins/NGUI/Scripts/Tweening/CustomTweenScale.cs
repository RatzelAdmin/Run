// platform ngui version

using UnityEngine;
using System.Collections;

public class CustomTweenScale : TweenScale {

	UITable mTable1;

	protected override void OnUpdate (float factor, bool isFinished)
	{
		value = from * (1f - factor) + to * factor;
		
		if (updateTable)
		{
			if (mTable == null)
			{
				base.mTable = NGUITools.FindInParents<UITable>(gameObject);
				if (base.mTable == null) { updateTable = false; return; }

				if (base.mTable != null)
					mTable1 = NGUITools.FindInParents<UITable>(base.mTable.transform.parent.gameObject);
			}
			
			base.mTable.repositionNow = true;
			if (mTable1 != null) 
				mTable1.repositionNow = true;
		}
	}
}
