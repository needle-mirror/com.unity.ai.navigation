using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Unity.AI.Navigation.Samples
{
    /// <summary>
    /// Manipulating the camera with standard inputs
    /// </summary>
    public class FreeCam : MonoBehaviour
    {
        public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
        public RotationAxes axes = RotationAxes.MouseXAndY;
        public float sensitivityX = 15F;
        public float sensitivityY = 15F;

        public float minimumX = -360F;
        public float maximumX = 360F;

        public float minimumY = -60F;
        public float maximumY = 60F;

        public float moveSpeed = 1.0f;

        public bool lockHeight = false;

        const float k_MouseDeltaScale = 0.05f;
        const float k_MoveSmoothTime = 0.2f;

        float rotationY = 0F;

        Vector2 m_CurrentMove;
        Vector2 m_MoveSmoothVelocity;

        void Update()
        {
            var mouse = Mouse.current;
            var keyboard = Keyboard.current;
            if (mouse == null || keyboard == null)
                return;

            var mouseDelta = mouse.delta.ReadValue() * k_MouseDeltaScale;

            if (axes == RotationAxes.MouseXAndY)
            {
                float rotationX = transform.localEulerAngles.y + mouseDelta.x * sensitivityX;

                rotationY += mouseDelta.y * sensitivityY;
                rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

                transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
            }
            else if (axes == RotationAxes.MouseX)
            {
                transform.Rotate(0, mouseDelta.x * sensitivityX, 0);
            }
            else
            {
                rotationY += mouseDelta.y * sensitivityY;
                rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

                transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
            }

            var targetMove = new Vector2(
                GetAxis(keyboard.dKey, keyboard.rightArrowKey, keyboard.aKey, keyboard.leftArrowKey),
                GetAxis(keyboard.wKey, keyboard.upArrowKey, keyboard.sKey, keyboard.downArrowKey));
            m_CurrentMove = Vector2.SmoothDamp(m_CurrentMove, targetMove, ref m_MoveSmoothVelocity, k_MoveSmoothTime);

            var xAxisValue = m_CurrentMove.x;
            var zAxisValue = m_CurrentMove.y;
            if (lockHeight)
            {
                var dir = transform.TransformDirection(new Vector3(xAxisValue, 0.0f, zAxisValue) * moveSpeed);
                dir.y = 0.0f;
                transform.position += dir;
            }
            else
            {
                transform.Translate(new Vector3(xAxisValue, 0.0f, zAxisValue) * moveSpeed);
            }
        }

        // Composes a raw -1..1 target from a pair of "positive" and "negative" keys, replacing the
        // legacy Input.GetAxis("Horizontal"/"Vertical") raw input (smoothing is applied by the caller).
        static float GetAxis(KeyControl positive, KeyControl positiveAlt, KeyControl negative, KeyControl negativeAlt)
        {
            var value = 0f;
            if (positive.isPressed || positiveAlt.isPressed)
                value += 1f;
            if (negative.isPressed || negativeAlt.isPressed)
                value -= 1f;
            return value;
        }
    }
}
