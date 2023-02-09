using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armis.Items
{
	public abstract class Item : ScriptableObject
	{
		public ItemType itemType;
		public GameObject prefab;
	}
}
