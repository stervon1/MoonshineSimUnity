using UnityEngine;
using UnityEngine.InputSystem;

namespace MoonshineSim.Gameplay
{
    /// <summary>
    /// Prototype first-person walk controller (PowerWash / Car Mechanic style).
    ///
    /// Reads the Keyboard/Mouse devices directly through the new Input System,
    /// so it needs no InputActionAsset wiring. Navigation only — there are no
    /// hard-fail states here, it just moves the player around the workshop.
    ///
    /// Yaw is applied to this GameObject; pitch to <see cref="cameraPivot"/>
    /// (a child camera transform).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Look")]
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private float lookSensitivity = 0.08f;
        [SerializeField] private float pitchClamp = 85f;

        [Header("Move")]
        [SerializeField] private float walkSpeed = 3.5f;
        [SerializeField] private float sprintSpeed = 6f;
        [SerializeField] private float gravity = -18f;
        [SerializeField] private float jumpHeight = 1.1f;

        private CharacterController _cc;
        private float _pitch;
        private float _verticalVel;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            if (cameraPivot == null && Camera.main != null)
            {
                cameraPivot = Camera.main.transform;
            }
        }

        private void OnEnable() => SetCursor(locked: true);

        private void OnDisable() => SetCursor(locked: false);

        private void Update()
        {
            var kb = Keyboard.current;
            var mouse = Mouse.current;

            // Esc frees the cursor; clicking back into the view recaptures it.
            if (kb != null && kb.escapeKey.wasPressedThisFrame)
            {
                SetCursor(locked: false);
            }
            if (mouse != null && mouse.leftButton.wasPressedThisFrame &&
                Cursor.lockState == CursorLockMode.None)
            {
                SetCursor(locked: true);
            }

            bool active = Cursor.lockState == CursorLockMode.Locked;

            // --- look --------------------------------------------------------
            if (active && mouse != null)
            {
                Vector2 d = mouse.delta.ReadValue() * lookSensitivity;
                transform.Rotate(0f, d.x, 0f, Space.Self);
                _pitch = Mathf.Clamp(_pitch - d.y, -pitchClamp, pitchClamp);
                if (cameraPivot != null)
                {
                    cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
                }
            }

            // --- move --------------------------------------------------------
            Vector2 input = Vector2.zero;
            bool sprint = false;
            if (active && kb != null)
            {
                if (kb.wKey.isPressed) input.y += 1f;
                if (kb.sKey.isPressed) input.y -= 1f;
                if (kb.dKey.isPressed) input.x += 1f;
                if (kb.aKey.isPressed) input.x -= 1f;
                sprint = kb.leftShiftKey.isPressed;
            }
            input = Vector2.ClampMagnitude(input, 1f);

            Vector3 move = transform.right * input.x + transform.forward * input.y;
            float speed = sprint ? sprintSpeed : walkSpeed;

            if (_cc.isGrounded)
            {
                _verticalVel = -2f;
                if (active && kb != null && kb.spaceKey.wasPressedThisFrame)
                {
                    _verticalVel = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }
            }
            _verticalVel += gravity * Time.deltaTime;

            _cc.Move((move * speed + Vector3.up * _verticalVel) * Time.deltaTime);
        }

        private static void SetCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
