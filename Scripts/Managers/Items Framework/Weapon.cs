using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armis.Items
{
	[CreateAssetMenu(menuName ="Items/Weapon")]
	public class Weapon : Item
	{
		public float _fireRate = 0.1f;
		public int roundsInMag = 30;
		public int maxRounds = 120;
	}
}
