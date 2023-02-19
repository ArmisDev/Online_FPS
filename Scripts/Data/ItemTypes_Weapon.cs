using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armis.ItemTypes.Weapon
{
    [CreateAssetMenu(fileName = "Armis", menuName = "Armis/ItemTypes/Weapon", order = 1)]
    public class ItemTypes_Weapon : ScriptableObject
    {
        public int damage;
        public float rangeOfWeapon;
        public string calliberOfRound;
        public float fireRate;
        public int magSize;
        public int totalRounds;
        public float reloadTime;
    }
}
