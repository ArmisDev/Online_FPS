using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Armis.Utilities;

namespace Armis
{
    public static class Ballistics
    {
        public static void RaycastBullet(Vector3 orgin, Vector3 direction, Controller owner)
        {
            /* Here we are grabbing the bullet line using the object pooler script. 
             * Doing this allows for the use of creating and removing gameobjects without 
             * using expensive instantiate and destroy calls.
             */
            GameObject bulletGameObject = ObjectPooler.GetObject("bulletline");

            /* Here we define a new Vector3 and have it equal our orgin Vector (passed through above)
             * and add it against the direction Vector multiplied by 100. By doing this, we can track 
             * the orgin and postion(multiplied by our range, aka 100), thus equaling our hit position.
             */
            Vector3 hitPosition = orgin + (direction * 100f);
            RaycastHit hit;

            if (Physics.Raycast(orgin, direction, out hit, 100f))
            {
                //This allows us to store the point the raycast was hit into a Vector3
                hitPosition = hit.point;
            }

            //Here we are taking our bulletline gameobject and equaling it to the bullet game object set above.
            BulletLine bulletLine = bulletGameObject.GetComponentInChildren<BulletLine>();
            //Here we are setting the position of the bullet line to stem from our orgin to our hit position
            bulletLine.SetPositions(orgin, hitPosition);
            //Finally, to make the game object appear we set the game object to active
            bulletGameObject.SetActive(true);
        }
    }
}
