using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armis.Items
{
	public class RuntimeItem 
	{
		public string instanceId;
		public Item baseItem;

		public int currentRoundsInMag;
		public int totalRounds;

		float lastFired;

		public bool canFire()
        {
			if(currentRoundsInMag > 0)
            {
				Weapon w = (Weapon)baseItem;
				return (Time.realtimeSinceStartup - lastFired) > w._fireRate;
			}

			return false;
        }

		public void Shoot()
        {
			lastFired = Time.realtimeSinceStartup;
			currentRoundsInMag--;
        }

		public void Reload()
        {
			Weapon w = (Weapon) baseItem;

			if(totalRounds >= w.roundsInMag)
            {
				currentRoundsInMag = w.roundsInMag;
				totalRounds -= w.roundsInMag;
            }

            else
            {
				int difference = w.roundsInMag - totalRounds;
				currentRoundsInMag = difference;
				totalRounds = 0;
            }
        }
	}
}
