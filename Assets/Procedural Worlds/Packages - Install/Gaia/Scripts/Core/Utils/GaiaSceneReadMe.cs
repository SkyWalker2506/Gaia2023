using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gaia
{

    public class GaiaSceneReadMe : MonoBehaviour
    {
        public void DeleteAll()
        {
            var allReadmes = Resources.FindObjectsOfTypeAll(typeof(GaiaSceneReadMe));
            for (int i = 0; i < allReadmes.Length; i++)
            {
                GaiaSceneReadMe rm = (GaiaSceneReadMe)allReadmes[i];
                if (Application.isPlaying)
                {
                    Destroy(rm);
                }
                else
                {
                    DestroyImmediate(rm);
                }

            }

        }
    }
}
