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
        //��������
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
        public SkillInfo uniqueSkill;//skill��indexΪ100
    }

	public List<CharacterDataEntry> data;
	public List<int> teamIndexList;//��ǰ�����еĽ�ɫ�ı�ţ�list[i]=0����ʾ����ĵ�i��λ�Ǳ��Ϊ0�Ľ�ɫ��-1��ʾΪ��λ��
}
