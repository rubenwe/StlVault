using TMPro;
using UnityEngine;

#pragma warning disable 0649

namespace StlVault.Util.Unity
{
    public class AppVersionDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;

        private void Start()
        {
            _text.text = "v" + Application.version;
        }
    }
}