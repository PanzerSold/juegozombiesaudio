using UnityEngine;
using System.Collections;

public class Vault : Interaction
{
    [Header("Vault Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private float vaultDuration = 1.875f;
    [SerializeField] private float vaultDistance = 0.85f;
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Vault Direction")]
    [SerializeField] private Transform vaultDirection;
    [SerializeField] private float rotationSpeed = 720f;

    private CharacterController cc;
    private bool isVaulting = false;

    protected override void Start()
    {
        base.Start();

        fullString = "Vault (E)";
        cc = player.GetComponent<CharacterController>();
    }

    protected override void Interactuar()
    {
        if (isVaulting || !jugadorDentro) return;

        StartCoroutine(VaultRoutine());
    }

    public override void PlayerEntered()
    {
        if (isVaulting) return;

        base.PlayerEntered();
    }

    private IEnumerator VaultRoutine()
    {
        isVaulting = true;
        jugadorDentro = false;

        textoInteraccion.gameObject.SetActive(false);

        // Bloquear controles
        player.IsVaulting = true;

        // Guardar posición y rotación
        Vector3 startPosition = player.transform.position;

        Quaternion targetRotation = Quaternion.LookRotation(
            vaultDirection.forward
        );

        // Dirección obligatoria del movimiento
        Vector3 direction = vaultDirection.forward;
        direction.y = 0f;
        direction.Normalize();

        Vector3 targetPosition = startPosition + direction * vaultDistance;

        // Desactivar colisiones
        cc.enabled = false;

        // Activar animación
        animator.SetLayerWeight(1, 0f);
        animator.SetTrigger("Vault");

        float elapsed = 0f;

        // Movimiento y rotación
        while (elapsed < vaultDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / vaultDuration);
            float curveValue = movementCurve.Evaluate(progress);

            player.transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                curveValue
            );

            player.transform.rotation = Quaternion.RotateTowards(
                player.transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            yield return null;
        }

        // Asegurar posición y rotación finales
        player.transform.position = targetPosition;
        player.transform.rotation = targetRotation;

        // Restaurar colisiones
        cc.enabled = true;

        // Desbloquear controles
        player.IsVaulting = false;
        isVaulting = false;
        animator.SetLayerWeight(1, 1f);
    }
}
