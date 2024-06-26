using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.GTS
{
    public class GTSMeshComponent : MonoBehaviour
    {
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Unhide()
        {
            gameObject.SetActive(true);
        }

    }
}

