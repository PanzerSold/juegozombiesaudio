using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerViewInput : MonoBehaviour
{
    [SerializeField] private bool disabled = false;
    [SerializeField] private float sensibility = 1;
    [SerializeField] private float smoothing = 1;
    [SerializeField] private float MaxUpAngle = 80;
    [SerializeField] private float MaxDownAngle = -80;
    [SerializeField] private float mouseSensibility = 1f;
    [SerializeField] private float stickSensibility = 15f;
    
    private PlayerInputs pi;
    [SerializeField] private GameObject player;
    [SerializeField] private Transform pivoti;

    // Almacenamiento interno para el ángulo vertical actual del pivote
    private float currentAngleY = 0f;

    private PlayerShoot playerAmmo;

    void Start()
    {
        if (player == null)
        {
            player = gameObject;
        }

        if (player != null)
        {
            playerAmmo = player.GetComponent<PlayerShoot>();
            pi = player.GetComponent<PlayerInputs>();
        }

        // Inicializamos el ángulo vertical con la rotación actual del pivote si existe
        if (pivoti != null)
        {
            currentAngleY = Normalize(pivoti.localEulerAngles.x);
        }
    }

    void Update()
    {
        if (!disabled && pi != null && pi.lookAction != null && !playerAmmo.InventoryOpen)
        {
            Vector2 lookAround = pi.lookAction.ReadValue<Vector2>();

            // Validar si el dispositivo actual es un Gamepad
            bool isGamepad = pi.lookAction.activeControl?.device is Gamepad;

            float currentSensibility = isGamepad ? stickSensibility : mouseSensibility;
            float userMult = isGamepad ? GameSettings.StickSensitivity : GameSettings.MouseSensitivity;
            float mult = currentSensibility * Time.deltaTime * sensibility * userMult;

            // 1. ROTACIÓN HORIZONTAL (Eje Y del Player)
            Vector3 newDesiredAngleX = player.transform.eulerAngles + Vector3.up * lookAround.x * mult;
            player.transform.eulerAngles = Vector3.Slerp(player.transform.eulerAngles, newDesiredAngleX, smoothing);

            // 2. ROTACIÓN VERTICAL (Eje X del Pivoti) - Lógica idéntica a tu cámara anterior
            if (pivoti != null)
            {
                float targetAngleY = currentAngleY - lookAround.y * mult;
                
                // Suavizado e interpolación
                currentAngleY = Mathf.Lerp(currentAngleY, targetAngleY, smoothing);
                // Restricción de ángulos máximos y mínimos
                currentAngleY = Mathf.Clamp(Normalize(currentAngleY), MaxDownAngle, MaxUpAngle);

                // Aplicamos la rotación local en X al pivote (mantiene el eje Y y Z en 0 respecto al jugador)
                Vector3 pivotiRot = new Vector3(currentAngleY, 0f, 0f);
                pivoti.localEulerAngles = pivotiRot;
            }
        }
    }

    private float Normalize(float angle)
    {
        angle %= 360f;
        if (angle > 180f)
        {
            angle -= 360f;
        }
        return angle;
    }
}