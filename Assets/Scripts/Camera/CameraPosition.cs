using UnityEngine;
using UnityEngine.InputSystem;

public class CameraPosition : MonoBehaviour
{
    [SerializeField] private bool disabled = false;
    [SerializeField] private Camera camara;
    [SerializeField] private float camSpeed = 1;
    [SerializeField] private float sensibility = 1;
    [SerializeField] private float smoothing = 1;
    [SerializeField] private LayerMask coll;
    [SerializeField] private float MaxUpAngle = 80;
    [SerializeField] private float MaxDownAngle = -80;
    [SerializeField] private float mouseSensibility = 1f;
    [SerializeField] private float stickSensibility = 15f;
    private PlayerInputs pi;
    [SerializeField] private Vector3 camPosOffset = new Vector3(0.471f,0.7f,-1.8f);
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject pivoti;

    [Header("Seguridad de Colisión (0 - 1)")]
    // dejar entre >0 y 1, 1 siendo que la camara se pega al jugador si choca una pared
    [Range(0f, 1f)]
    [SerializeField] private float ColisionSafety = 0.1f;

    

    void Start()
    {
        camara.transform.position = transform.position;
        camara.transform.rotation = transform.rotation;
        if (player != null)
        {
            pi = player.GetComponent<PlayerInputs>();
        }

    }

    
    void Update()
    {
        if (!disabled)
        {
            Vector2 lookAround = pi.lookAction.ReadValue<Vector2>();

            bool isGamepad = pi.lookAction.activeControl?.device is Gamepad;

            float currentSensibility = isGamepad ? stickSensibility : mouseSensibility;
            float userMult = isGamepad ? GameSettings.StickSensitivity : GameSettings.MouseSensitivity;
            float mult = currentSensibility * Time.deltaTime * sensibility * userMult;

            Vector3 newDesiredAngleX = player.transform.eulerAngles + Vector3.up * lookAround.x * mult;
            player.transform.eulerAngles = Vector3.Slerp(player.transform.eulerAngles, newDesiredAngleX, smoothing);

            float newDesiredAngleY = camara.transform.eulerAngles.x - lookAround.y * mult;
            
            newDesiredAngleY = Mathf.Lerp(camara.transform.eulerAngles.x, newDesiredAngleY, smoothing); 
            newDesiredAngleY = Mathf.Clamp(Normalize(newDesiredAngleY), MaxDownAngle, MaxUpAngle);

            Vector3 camRot = new Vector3(newDesiredAngleY, player.transform.eulerAngles.y, 0);
            camara.transform.eulerAngles = camRot;

            Quaternion rotar_newPos = Quaternion.Euler(camara.transform.eulerAngles.x,player.transform.eulerAngles.y,0);

            Vector3 new_camPosOffset = new Vector3(camPosOffset.x,camPosOffset.y,-1.8f / Mathf.Max(1,Mathf.Abs(newDesiredAngleY/20)));
            transform.position = player.transform.position + rotar_newPos*new_camPosOffset;

            Vector3 difference = transform.position - pivoti.transform.position;
            float difLength = difference.magnitude;

            RaycastHit hitInfo;

            if(Physics.Raycast(pivoti.transform.position, difference, out hitInfo, 3f, coll))
            {
                transform.position = hitInfo.point-(difference*ColisionSafety);
            }
            
            camara.transform.position = Vector3.Lerp(camara.transform.position, transform.position, Time.deltaTime * camSpeed);
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
