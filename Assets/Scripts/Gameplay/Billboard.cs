using UnityEngine;

namespace MoonshineSim.Gameplay
{
    /// <summary>Keeps a world object facing the camera (for signs / info cards).</summary>
    public class Billboard : MonoBehaviour
    {
        [SerializeField] private bool flip = true;   // uGUI text reads correctly facing -forward

        private Camera _cam;

        private void LateUpdate()
        {
            if (_cam == null) _cam = Camera.main;
            if (_cam == null) return;

            Vector3 dir = transform.position - _cam.transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return;
            transform.rotation = Quaternion.LookRotation(flip ? dir : -dir, Vector3.up);
        }
    }
}
