using UnityEngine.EventSystems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private PlayerShoot playerShoot;
    [SerializeField] private Button[] slotButtons;

    //----- NUEVO CODIGO ----- Recuerda sobre qué slot se abrió el menú.
    [SerializeField] private GameObject contextMenu;
    [SerializeField] private Button equipButton;
    [SerializeField] private Button discardButton;


    private string[] bulletNames = new string[]
    {
        "9mm Rounds",
        "5.6x45mm Rounds",
        "50mm Rounds",
        "5.7mm Rounds",
        "BuckShot Rounds",
        "Rockets",
    };

    //----- NUEVO CODIGO -----

    private TextMeshProUGUI[] slotLabels;

    //----- NUEVO CODIGO -----
    private int menuSlot = -1;

    //----- NUEVO CODIGO -----

    private void Awake()
    {
        slotLabels = new TextMeshProUGUI[slotButtons.Length];
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int slot = i;
            slotLabels[i] = slotButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            //-----Codigo borrado----- slotButtons[i].onClick.AddListener(() => playerShoot.EquipFromSlot(slot)); -----Codigo borrado-----//  El clic izquierdo ya no equipa.

            //----- NUEVO CODIGO ----- Cada slot escucha sus clics con AddClick 
            AddClick(slotButtons[i].gameObject, data => OnSlotClicked(slot, data));
            //----- NUEVO CODIGO ----- 
        }
        //----- NUEVO CODIGO ----- El panel también escucha clics, se conectan los botones del menú y arranca oculto
        AddClick(gameObject, data => HideMenu());
        equipButton.onClick.AddListener(EquipFromMenu);
        discardButton.onClick.AddListener(DiscardFromMenu);
        HideMenu();
        //----- NUEVO CODIGO ----- 

    }


    //----- NUEVO CODIGO ----- AddClick agrega el EventTrigger, OnSlotClicked abre el menú con clic derecho, EquipFromMenu y DiscardFromMenu ejecutan la opción, HideMenu lo oculta
    private void AddClick(GameObject target, System.Action<PointerEventData> onClick)
    {
        EventTrigger trigger = target.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener(data => onClick((PointerEventData)data));
        trigger.triggers.Add(entry);
    }

    private void OnSlotClicked(int slot, PointerEventData eventData)
    {
        Inventory.Item item = inventory.GetSlot(slot);

        if (eventData.button != PointerEventData.InputButton.Right || item.item == Inventory.itemType.Empty)
        {
            HideMenu();
            return;
        }

        menuSlot = slot;
        equipButton.interactable = item.item == Inventory.itemType.Weapon;
        contextMenu.transform.position = eventData.position;
        contextMenu.SetActive(true);
    }

    private void EquipFromMenu()
    {
        int slot = menuSlot;
        HideMenu();
        playerShoot.EquipFromSlot(slot);
    }

    private void DiscardFromMenu()
    {
        inventory.DiscardSlot(menuSlot, out _);
        HideMenu();
    }

    private void HideMenu()
    {
        if (contextMenu != null) contextMenu.SetActive(false);
        menuSlot = -1;
    }
    //----- NUEVO CODIGO -----


    private void OnEnable()
    {
        inventory.OnInventoryChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        inventory.OnInventoryChanged -= Refresh;
        //----- NUEVO CODIGO ----- Cierra el menú al cerrar el inventario
        HideMenu();
        //----- NUEVO CODIGO -----
    }

    private void Refresh()
    {
        for (int i = 0; i < slotLabels.Length; i++)
        {
            Inventory.Item item = inventory.GetSlot(i);

            if (item.item == Inventory.itemType.Empty) slotLabels[i].text = "";
            else if (item.item == Inventory.itemType.Weapon) slotLabels[i].text = item.index.ToString();
            else slotLabels[i].text = item.index + "\nx" + item.amount;
        }
    }
}