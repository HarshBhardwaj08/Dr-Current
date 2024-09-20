using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Movement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 100f;
    public Transform playerCamera; // Reference to the camera attached to the player (capsule)
    
    private float verticalRotation = 0f;

    // Crosshair
    public Texture2D crosshairTexture;

    // Item pickup system
    public float pickupRange = 5f; // Range for item pickup
    public Transform itemHolder;   // Where the item will be held (in front of the player)
    private GameObject heldItem;   // Currently held item
    private Quaternion itemRotation;
    private float height;
    public List<string> items;
    public delegate void OnItemCollectedDelegate() ;
    public static event OnItemCollectedDelegate OnItemPickedUp;
    private void Start()
    { 
        // Lock the cursor to the center of the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Handle Mouse Look
        MouseLook();

        // Handle Movement
        Movements();

        // Handle Item Pickup/Drop
        HandlePickupAndDrop();
    }

    void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * -mouseSensitivity * Time.deltaTime;

        // Horizontal rotation (left-right)
        transform.Rotate(Vector3.up * mouseX);

        // Vertical rotation (up-down)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f); // Limit up-down look angle
        playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f); // Rotate the camera only on X-axis
    }

    void Movements()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // Move the capsule
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
    }

    void HandlePickupAndDrop()
    {
        // If holding an item and pressing the drop key (e.g., 'Q'), drop the item
        if (heldItem && Input.GetKeyDown(KeyCode.Q))
        {
            DropItem();
        }

        // If left mouse button is clicked, try to pick up an item
        if (!heldItem && Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, pickupRange))
            {
                // Check if the object hit has the "Pickup" tag (or another identifier)
                if (hit.transform.CompareTag("Battery"))
                {
                    PickupItem(hit.transform.gameObject);
                }
            }
        }
    }

    void PickupItem(GameObject item)
    {
        // Disable the item's collider and make it a child of the itemHolder
        if (!items.Contains(item.gameObject.transform.name))
        {
            items.Add(item.gameObject.transform.name);
        }
      
        item.GetComponent<Collider>().enabled = false;
        
        item.transform.SetParent(itemHolder);
        height = item.transform.position.y;
        itemRotation = item.gameObject.transform.rotation;
        item.transform.localPosition = Vector3.zero;  // Position it correctly in front of the player
        item.transform.localRotation = Quaternion.identity;  // Reset rotation
        heldItem = item; // Store the reference to the held item
    }

    void DropItem()
    {
        // Detach the item from the itemHolder and enable its collider again
        heldItem.transform.SetParent(null);
        heldItem.GetComponent<Collider>().enabled = true;
        heldItem.transform.rotation = itemRotation;
        heldItem.transform.position = new Vector3(heldItem.transform.position.x,height, heldItem.transform.position.z);
        heldItem = null; // Clear the reference to the held item
        if (items.Count >=3 )
        {
            OnItemPickedUp?.Invoke();
        }
    }

    private void OnGUI()
    {
        // Draw crosshair in the center of the screen
        float xMin = (Screen.width / 2) - (crosshairTexture.width / 2);
        float yMin = (Screen.height / 2) - (crosshairTexture.height / 2);
        GUI.DrawTexture(new Rect(xMin, yMin, crosshairTexture.width, crosshairTexture.height), crosshairTexture);
    }
}
