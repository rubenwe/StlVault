using UnityEngine;

#pragma warning disable 0649

namespace StlVault.Util.Logging
{
    public class LogLevelSettings : MonoBehaviour
    {
        [SerializeField] public LogLevel LogLevel;

        private void Update()
        {
            UnityLogger.LogLevel = LogLevel;
        }
    }
}