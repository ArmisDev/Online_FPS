using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armis.Items
{
	public static class ItemManager  
	{
		static ItemManagerAsset _ItemManagerAsset;

		public static ItemManagerAsset singleton {
			get {
				if (_ItemManagerAsset == null)
				{
					_ItemManagerAsset = Resources.Load("ItemManagerAsset") as ItemManagerAsset;
					_ItemManagerAsset.Init();
				}

				return _ItemManagerAsset;
			}
		}
	}
}
