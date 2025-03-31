using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.JSGAONA.Unidad1.Scripts {

    // Se emplea este script para gestionar el controlador del Joystick en la UI
    public class Joystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler {
    
        // Variables visibles desde el inspector de Unity
        [SerializeField] private RectTransform joystickBackground;
        [SerializeField] private RectTransform joystickHandle;

        // Variables ocultas desde el inspector de Unity
        private Vector2 inputVector;

        // Propiedades
        public float Horizontal => inputVector.x;
        public float Vertical => inputVector.y;


        // Metodo de llamada de Unity.EventSystem - IDragHandler, se ejecuta mientras se presiona
        public void OnDrag(PointerEventData eventData) {
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(
                joystickBackground,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 position)){
                    // Convierte la posicion del joystick a un valor entre -1 y 1
                    // Evitando que se salga de su area maxima
                    position.x = position.x / joystickBackground.sizeDelta.x * 2;
                    position.y = position.y / joystickBackground.sizeDelta.y * 2;

                    // Si el valor excede el rango, el vector se normaliza para mantener 
                    // una distancia maxima de 1
                    inputVector = (position.magnitude > 1.0f) ? position.normalized : position;

                    // Mueve el joystick en funcion del desplazamiento del dedo
                    joystickHandle.anchoredPosition = new Vector2(
                        inputVector.x * (joystickBackground.sizeDelta.x / 2),
                        inputVector.y * (joystickBackground.sizeDelta.y / 2));
            }
        }


        // Metodo de llamada de Unity.EventSystem - IPointerDownHandler, se ejecuta una sola vez
        // cuando se presiona
        public void OnPointerDown(PointerEventData eventData) {
            OnDrag(eventData);
        }


        // Metodo de llamada de Unity.EventSystem - IPointerUpHandler, se ejecuta una sola vez
        // cuando se deja de presionar
        public void OnPointerUp(PointerEventData eventData) {
            inputVector = Vector2.zero;
            joystickHandle.anchoredPosition = Vector2.zero;
        }
    }
}