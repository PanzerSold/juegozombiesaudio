using UnityEngine;

public class RagdollTimer : MonoBehaviour
{
    [SerializeField] private float tiempoParaDesaparecer = 5f;

    void Start()
    {
        // Se destruye a sí mismo pasados los segundos indicados
        Destroy(gameObject, tiempoParaDesaparecer);
    }
}