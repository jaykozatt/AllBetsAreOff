using UnityEngine;
using UnityEngine.Events;

namespace AllBets
{
    public class ButtonEvents : MonoBehaviour
    {
        public UnityEvent onPressNHold;
        public bool IsHeldDown {get; private set;}
        
        private void Start() {
            IsHeldDown = false;
        }

        private void Update() {
            if (IsHeldDown) onPressNHold?.Invoke();
        }

        public void OnPress ()
        {
            IsHeldDown = true;
            print("Button Pressed!");
        }

        public void OnRelease ()
        {
            IsHeldDown = false;
            print("Button Released!");
        }
    }
}