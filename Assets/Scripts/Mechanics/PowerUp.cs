using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public Player.PlayerBonus pu;
    [SerializeField] Mesh[] modelsList;
    [SerializeField] float rotationSpeed = 50f;

    private void Start()
    {
        GetComponent<MeshFilter>().mesh = modelsList[(int)pu];
        Destroy(gameObject, 7f);
    }
    private void Update()
    {
        transform.localEulerAngles = transform.localEulerAngles + Vector3.up * Time.deltaTime * rotationSpeed;
    }
}
