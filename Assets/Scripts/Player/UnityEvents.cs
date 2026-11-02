using UnityEngine.Events;

public static class UnityEvents
{
   public static UnityEvent<int> onHealthChanged = new UnityEvent<int>();
   public static UnityEvent<bool> onCooldownChanged = new UnityEvent<bool>();
   public static UnityEvent<bool> shielded = new UnityEvent<bool>();
   public static UnityEvent onPowerUpTriggered = new UnityEvent();
   public static UnityEvent onPerkActive = new UnityEvent();
}
