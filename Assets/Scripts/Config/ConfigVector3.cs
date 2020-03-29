using UnityEngine;

namespace StlVault.Config
{
    public struct ConfigVector3
    {
        public float x;
        public float y;
        public float z;

        private ConfigVector3(float rotX, float rotY, float rotZ)
        {
            x = rotX;
            y = rotY;
            z = rotZ;
        }

        public static implicit operator Vector3(ConfigVector3 rot)
        {
            return new Vector3(rot.x, rot.y, rot.z);
        }
        
        public static implicit operator ConfigVector3(Vector3 rot)
        {
            return new ConfigVector3(rot.x, rot.y, rot.z);
        }

        public (int x, int y, int z) GetRoundedValues()
        {
            return (
                Mathf.RoundToInt(x),
                Mathf.RoundToInt(y),
                Mathf.RoundToInt(z)
            );
        }
    }
}