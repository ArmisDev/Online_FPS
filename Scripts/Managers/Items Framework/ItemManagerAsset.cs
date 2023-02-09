using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armis.Items
{
	[CreateAssetMenu]
	public class ItemManagerAsset : ScriptableObject
	{
		[System.NonSerialized]
		List<Item> allItems = new List<Item>();
		[System.NonSerialized]
		Dictionary<string, Item> itemsDict = new Dictionary<string, Item>();
		[System.NonSerialized]
		Dictionary<string, RuntimeItem> instancedRuntimeItems = new Dictionary<string, RuntimeItem>();

		public void Init()
		{
			allItems.Clear();

			Object[] objects = Resources.LoadAll("Items", typeof(Item));

			foreach (Object o in objects)
			{
				if (o is Item)
				{
					allItems.Add((Item)o);
				}
			}

			itemsDict.Clear();

			foreach (Item item in allItems)
			{
				itemsDict.Add(item.name, item);
			}
		}

		[System.NonSerialized]
		int instanceIndex;

		public RuntimeItem CreateItemInstance(string id, bool ignoreInstanceId = false, string hardcodeIsntanceId = "")
		{
			itemsDict.TryGetValue(id, out Item baseItem);
			if (baseItem == null)
			{
				Debug.LogWarning(id + " for item doesn't exist!");
				return null;
			}

			RuntimeItem result = new RuntimeItem();
			result.baseItem = baseItem;

			if (!ignoreInstanceId)
			{
				result.instanceId = instanceIndex.ToString();
				instanceIndex++;
			}
			else
			{
				result.instanceId = hardcodeIsntanceId;
			}

			instancedRuntimeItems.Add(result.instanceId, result);

			return result;
		}

		public RuntimeItem GetRuntimeItem(string instanceId)
		{
			instancedRuntimeItems.TryGetValue(instanceId, out RuntimeItem result);
			return result;
		}
	}
}
