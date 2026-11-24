using System.Collections.Generic;
using UnityEngine;

namespace MultiverseArcade.Syafiq.Brawl
{
    // Prefab: model kristal + Collider (Is Trigger) + Rigidbody (Is Kinematic)
    public class Gem : MonoBehaviour
    {
        public static readonly List<Gem> All = new List<Gem>();
        public static int ActiveCount => All.Count;

        [SerializeField] float friction = 4f;
        Vector3 vel;

        void OnEnable()  => All.Add(this);
        void OnDisable() => All.Remove(this);

        // Lontaran kecil saat muncul dari tambang / dijatuhkan
        public void Pop(Vector2 dir) => vel = new Vector3(dir.x, 0, dir.y);

        void Update()
        {
            transform.position += vel * Time.deltaTime;
            vel = Vector3.Lerp(vel, Vector3.zero, friction * Time.deltaTime);
            transform.Rotate(0, 90f * Time.deltaTime, 0);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out GemCarrier c))
            {
                c.Add(1);
                Destroy(gameObject);
            }
        }
    }
}
