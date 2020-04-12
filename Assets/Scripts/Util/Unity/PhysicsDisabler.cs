using UnityEngine;

namespace StlVault.Util.Unity
{
    public class PhysicsDisabler : MonoBehaviour
    {
        public void Awake()
        {
            Physics.autoSimulation = false;
            Physics2D.autoSimulation = false;
        }
    }
}