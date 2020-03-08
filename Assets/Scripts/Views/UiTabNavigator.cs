using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StlVault.Views
{
    public class UiTabNavigator : MonoBehaviour
    {
        private EventSystem _system;
 
        private void Start()
        {
            _system = EventSystem.current;
        }
 
        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Tab)) return;

            var selectable = _system.currentSelectedGameObject != null 
                ? _system.currentSelectedGameObject.GetComponent<Selectable>() 
                : null;
            
            if (selectable != null)
            {
                var next = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ?
                    selectable.FindSelectableOnLeft() ?? selectable.FindSelectableOnUp() :
                    selectable.FindSelectableOnRight() ?? selectable.FindSelectableOnDown();
 
                if (next != null)
                {
                    IPointerClickHandler pointer = next.GetComponent<TMP_InputField>();
                    pointer?.OnPointerClick(new PointerEventData(_system));

                    _system.SetSelectedGameObject(next.gameObject);
                }
                else
                {
                    next = Selectable.allSelectablesArray[0];
                    _system.SetSelectedGameObject(next.gameObject);
                }
            }
        }
    }
}