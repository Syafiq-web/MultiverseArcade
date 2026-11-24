using UnityEngine;

namespace MultiverseArcade.Syafiq.Brawl
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] VirtualJoystick moveStick;
        [SerializeField] float moveSpeed = 4.5f;
        [SerializeField] float turnSpeed = 14f;
        [SerializeField] Transform model; // child visual, supaya rotasi tidak ganggu collider

        CharacterController cc;
        Vector3 knockback; // dorongan dari serangan lawan
        public Vector3 AimOverrideDir { get; set; } // dipakai AimController saat membidik

        void Awake() => cc = GetComponent<CharacterController>();

        void Update()
        {
            // Joystick kiri -> bidang XZ (top-down), dengan fallback keyboard WASD jika joystick diam
            Vector2 input = Vector2.zero;
            if (moveStick != null && moveStick.Direction != Vector2.zero)
            {
                input = moveStick.Direction;
            }
            else
            {
                input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            }

            Vector3 move = new Vector3(input.x, 0, input.y) * moveSpeed;

            // Knockback meluruh seiring waktu
            knockback = Vector3.Lerp(knockback, Vector3.zero, 8f * Time.deltaTime);

            if (cc != null && cc.enabled)
            {
                cc.Move((move + knockback + Physics.gravity * 0.1f) * Time.deltaTime);
            }

            // Arah hadap: ke arah bidik jika sedang membidik, kalau tidak ke arah gerak
            Vector3 look = AimOverrideDir != Vector3.zero ? AimOverrideDir : move;
            if (look.sqrMagnitude > 0.01f && model != null)
            {
                Quaternion target = Quaternion.LookRotation(look.normalized, Vector3.up);
                model.rotation = Quaternion.Slerp(model.rotation, target, turnSpeed * Time.deltaTime);
            }
        }

        public void AddKnockback(Vector3 force) => knockback += force;
        public void ResetMotion() => knockback = Vector3.zero;
        public void SetMoveStick(VirtualJoystick stick) => moveStick = stick;
        public void SetModel(Transform visualModel) => model = visualModel;
    }
}
