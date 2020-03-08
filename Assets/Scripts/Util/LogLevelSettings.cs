using UnityEngine;

#pragma warning disable 0649

namespace StlVault.Util
{
    public class LogLevelSettings : MonoBehaviour
    {
        [SerializeField] private LogLevel _logLevel;

        private void Update()
        {
            UnityLogger.LogLevel = _logLevel;
        }
    }
}