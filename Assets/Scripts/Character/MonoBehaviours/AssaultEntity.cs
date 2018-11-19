using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assault
{
    public abstract class AssaultEntity : MonoBehaviour
    {

        public abstract void Spawn(Transform spawnPoint);
    }
}
