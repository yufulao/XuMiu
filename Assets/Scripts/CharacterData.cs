using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "CharacterData", menuName = "ScriptableObjects/CharacterData", order = 1)]
public class CharacterData : ScriptableObject
{
	[Serializable]
	public class CharacterDataEntry
	{
        //基础属性
        public string characterName;
        public bool unLocked;
		public bool inTeam;

		public int damage;
		public int defend;
		public int maxHp;
		public float damageRate;
		public float hurtRate;
		public int originalHatred;
		public int originalBp;
		public int speed;
		 
		public Sprite teamSprite;
		public Sprite battlePortrait;
		public Sprite battlePortraitName;
		public GameObject battleObjPrefab;
		public List<SkillInfo> skills;
        public SkillInfo uniqueSkill;//skill的index为100
    }

	public List<CharacterDataEntry> data;
	public List<int> teamIndexList;//当前队伍中的角色的编号，list[i]=0，表示队伍的第i号位是编号为0的角色，-1表示为空位置
}
