using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObjects/EnemyData", order = 1)]
public class EnemyData : ScriptableObject
{
	[Serializable]
	public class EnemyDataEntry
	{
		//»ù´¡ÊôÐÔ
		public string enemyName;

		public EnemyType enemyType;
		public int damage;
		public int defend;
		public int maxHp;
		public float damageRate;
		public float hurtRate;
		public int speed;

		public GameObject battleObjPrefab;
		public List<SkillInfo> skills;
	}

	public List<EnemyDataEntry> data;
}
