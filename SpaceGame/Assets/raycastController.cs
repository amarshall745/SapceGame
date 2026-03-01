using UnityEngine;

public class raycastController : MonoBehaviour
{
    [Header("References")]
    public CharacterController playerController;
    public Transform gizmoPoint;
    public GameObject holdingArea;

    [Header("Ranges")]
    public float pickUpRange = 3f;
    public float interactRange = 6f;

    [Header("Held Item")]
    public GameObject pickedUpItem;
    public bool holdingItem;

    [Header("Scale")]
    public float heldScale = 0.7f;
    public float droppedScale = 1f;

    private GameObject hitGO;
    private Vector3 hitPoint = Vector3.zero;

    // Store settings so we can restore them when dropping
    private Rigidbody heldRb;
    private RigidbodyConstraints originalConstraints;
    private bool originalUseGravity;
    private bool originalIsKinematic;

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;

        var ray = new Ray(gizmoPoint.position, gizmoPoint.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            hitPoint = hit.point;
            hitGO = hit.transform.gameObject;

            float distance = Vector3.Distance(transform.position, hit.point);

            // 1) LOCK INTERACTION (requires holding a key)
            if (distance <= interactRange && hitGO.GetComponent<Lock>() != null)
            {
                if (pickedUpItem != null && pickedUpItem.CompareTag("key"))
                {
                    hitGO.GetComponent<Lock>().unLock();

                    Destroy(pickedUpItem);
                    pickedUpItem = null;
                    holdingItem = false;

                }

                if (hitGO.CompareTag("lever")) {
                    hitGO.GetComponent<Lock>().unLock();
                };
                return;
            }

            // 2) ABILITY PICKUP
            if (distance <= interactRange && hitGO.CompareTag("activateAbility"))
            {
                GetComponent<PlayerController>().enableAbility();
                Destroy(hitGO);
                return;
            }

            if (distance <= interactRange && hitGO.GetComponent<NextLevel>())
            {
                hitGO.GetComponent<NextLevel>().changeLevel(gameObject);
                return;
            }

            // 3) PICKUP / DROP
            if (distance <= pickUpRange && hitGO.layer == LayerMask.NameToLayer("Pickup"))
            {
                if (!holdingItem)
                {
                    pickedUpItem = hitGO;
                    PickUpHeldItem();
                }
                else
                {
                    DropHeldItem();
                }
                return;
            }

            // If you pressed E but hit something non-interactable, drop if holding
            if (holdingItem)
            {
                DropHeldItem();
                return;
            }
        }
        else
        {
            // No ray hit: drop if holding
            if (holdingItem)
            {
                DropHeldItem();
                return;
            }
        }
    }

    private void PickUpHeldItem()
    {
        if (pickedUpItem == null) return;

        heldRb = pickedUpItem.GetComponent<Rigidbody>();
        if (heldRb == null) return;

        // Save original RB settings
        originalUseGravity = heldRb.useGravity;
        originalIsKinematic = heldRb.isKinematic;
        originalConstraints = heldRb.constraints;

        // Make it stable while held
        heldRb.useGravity = false;
        heldRb.isKinematic = true; // ? prevents physics forces/torque affecting it
        heldRb.constraints = RigidbodyConstraints.FreezeRotation;

        heldRb.linearVelocity = Vector3.zero;
        heldRb.angularVelocity = Vector3.zero;

        // Parent to holding area
        pickedUpItem.transform.SetParent(holdingArea.transform, false);
        pickedUpItem.transform.localPosition = Vector3.zero;
        pickedUpItem.transform.localRotation = Quaternion.identity;

        // Scale while held
        pickedUpItem.transform.localScale = Vector3.one * heldScale;

        holdingItem = true;
    }

    private void DropHeldItem()
    {
        if (pickedUpItem == null || heldRb == null)
        {
            holdingItem = false;
            pickedUpItem = null;
            heldRb = null;
            return;
        }

        // Unparent
        pickedUpItem.transform.SetParent(null);

        // Reset scale
        pickedUpItem.transform.localScale = Vector3.one * droppedScale;

        // Restore RB settings
        heldRb.isKinematic = originalIsKinematic;
        heldRb.useGravity = originalUseGravity;
        heldRb.constraints = originalConstraints;

        // Give it a little carry velocity
        heldRb.linearVelocity = playerController != null ? playerController.velocity : Vector3.zero;

        holdingItem = false;
        pickedUpItem = null;
        heldRb = null;
    }

    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(hitPoint, 0.2f);
    //}
}