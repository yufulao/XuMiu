using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "ChapterData", menuName = "ScriptableObjects/ChapterData", order = 1)]
public class ChapterData : ScriptableObject
{
	[Serializable]
	public class ChapterDataEntry
	{
		public int chapterIndex;
		public StageData stageData;
		public GameObject stageContent;
		public Sprite chapterBG;
		public Sprite chapterTitle;
		public string chapterBgmName;
	}

	public bool isBranch;
	public int currentChapterIndex;
	public List<ChapterDataEntry> data;
}
