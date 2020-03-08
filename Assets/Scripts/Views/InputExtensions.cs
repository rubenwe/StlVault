using UnityEngine;

namespace StlVault.Views
{
    public static class InputExtensions
    {
        public static bool Down(this KeyCode code) => Input.GetKeyDown(code);
        public static bool Pressed(this KeyCode code) => Input.GetKey(code);
    }
}