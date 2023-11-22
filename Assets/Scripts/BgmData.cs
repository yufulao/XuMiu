using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BgmData", menuName = "ScriptableObjects/BgmData", order = 1)]
public class BgmData : ScriptableObject
{
	[Serializable]
	public class BGMDataEntry
	{
		public string m_name;

		public AudioClip m_AudioClip;

	}

	public List<BGMDataEntry> data;
}