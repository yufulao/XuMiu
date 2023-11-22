using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SfxData", menuName = "ScriptableObjects/SfxData", order = 1)]
public class SfxData : ScriptableObject
{
	[Serializable]
	public class SFXDataEntry
	{
		public string m_name;

		public List<AudioClip> m_audioClips;

		[Range(0f, 1f)]
		public float m_volume;

		public bool m_oneShot;
	}

	public List<SFXDataEntry> data;

	public enum Flow
    {
		EditSlotBattleSlot,
		Slot,
		EditBattle,
		EditSlotBattle,
		EditBattleSlot
    }
}
