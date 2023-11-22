using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "BranchData", menuName = "ScriptableObjects/BranchData", order = 1)]
public class BranchData : ScriptableObject
{
	[Serializable]
	public class BranchDataEntry
	{
		public string branchName;
		public StageData stageData;
	}

	public int currentBranchIndex;
	public List<BranchDataEntry> data;
}
