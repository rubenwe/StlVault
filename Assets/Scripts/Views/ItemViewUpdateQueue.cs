using UnityEngine;

namespace StlVault.Views
{
    public class ItemViewUpdateQueue : MonoBehaviour
    {
        private static readonly object SyncRoot = new object();
        private static volatile bool _slotFree;

        public static bool ConsumeSlot()
        {
            if (!_slotFree) return false;
            lock (SyncRoot)
            {
                if (!_slotFree) return false;
                _slotFree = false;
                return true;
            }
        }

        private void Update()
        {
            _slotFree = true;
        }
    }
}