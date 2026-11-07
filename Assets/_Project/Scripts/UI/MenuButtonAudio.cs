using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLightHouse
{
    public sealed class MenuButtonAudio : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, ISelectHandler
    {
        [SerializeField] private MenuCameraController controller;

        public void SetController(MenuCameraController value) => controller = value;

        private void Awake()
        {
            if (!controller) controller = FindAnyObjectByType<MenuCameraController>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            controller?.PlayHoverSound();
        }

        public void OnSelect(BaseEventData eventData)
        {
            controller?.PlayHoverSound();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            controller?.PlayClickSound();
        }
    }
}
