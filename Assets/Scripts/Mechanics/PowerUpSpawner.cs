using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    [System.Serializable]
    public class PowerUpOption 
    {
        public GameObject prefab;
        public Player.PlayerBonus effect;
    }

    [Header("Opciones de Power-Up")]
    [SerializeField] private PowerUpOption[] options;

    [Header("Probabilidad de aparición")]
    [SerializeField, Range(0f, 1f)]
    private float dropChance = 0.1f;

    public void Spawn()
    {
        if (Random.value >= dropChance) return;
        if (options == null || options.Length == 0) return;

        int index = Random.Range(0, options.Length);
        PowerUpOption choosed = options[index];

        if (choosed.prefab == null) return;

        // hola filo, disculpa que no use tu sistema
        // att, luca

        GameObject newPowerUp = Instantiate(choosed.prefab, transform.position, Quaternion.Euler(Vector3.left*90));
        PowerUp component = newPowerUp.GetComponent<PowerUp>();

        if (component != null) component.pu = choosed.effect;
    }
}
