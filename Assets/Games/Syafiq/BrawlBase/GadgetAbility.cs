using UnityEngine;

namespace MultiverseArcade.Syafiq.Brawl
{
    // Kelas dasar gadget. Use() mengembalikan false jika gagal (jangan kurangi jatah pakai).
    public abstract class GadgetAbility : MonoBehaviour
    {
        public abstract bool Use();
    }
}
