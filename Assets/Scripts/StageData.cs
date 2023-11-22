using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "StageData", menuName = "ScriptableObjects/StageData", order = 1)]
public class StageData : ScriptableObject
{
	[Serializable]
	public class StageDataEntry
	{
		public string stageName;
		public string stageInfoTitle;
		public string stageInfoText;
		public StageFlowEnum stageFlow;
		public string PlotAName;
		public string PlotBName;
		public List<int> fixedTeamIndexList;
		public List<int> enemyTeam;

		public bool isUnlock;
		public bool isPassed;
	}

	public List<StageDataEntry> data;
}

