using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject hintArrow;
    public GameObject dropButton;

    public VirtualJoystick moveJoystick;
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    private Vector3 velocity;
    public float lookSpeed = 2f;
    private float verticalRotation = 0f;
    public float minPitch = -80f;
    public float maxPitch = 80f;
    private CharacterController controller;
    private Camera playerCamera;

    private bool isTouching = false;

    public Transform handTransform;

    public float throwForce = 1f;
    private GameObject heldObject = null;
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = Camera.main;
    }

    private void Update()
    {
        MovePlayer();
        HandleLook();

        HandleTouch();
    }

    private void HandleTouch()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 2f))
            {
                if (hit.collider.CompareTag("PickableObject") && heldObject == null)
                {
                    PickUpObject(hit.collider.gameObject);
                }
            }
        }
    }

    private void PickUpObject(GameObject obj)
    {
        heldObject = obj;
        hintArrow.SetActive(true);

        heldObject.transform.SetParent(handTransform);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        Collider col = heldObject.GetComponent<Collider>();

        col.enabled = false;
        rb.isKinematic = true;

        dropButton.SetActive(true);
    }

    public void DropObject()
    {
        if (heldObject)
        {
            heldObject.transform.SetParent(null);

            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            Collider col = heldObject.GetComponent<Collider>();

            col.enabled = true;
            rb.isKinematic = false;

            Vector3 throwDirection = Camera.main.transform.forward;

            rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);

            hintArrow.SetActive(false);
            heldObject = null;

            dropButton.SetActive(false);
        }
    }

    private void MovePlayer()
    {
        Vector3 move = transform.right * moveJoystick.Horizontal + transform.forward * moveJoystick.Vertical;
        controller.Move(move * moveSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleLook()
    {
        foreach (Touch touch in Input.touches)
        {
            if (touch.position.x > Screen.width / 2)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    isTouching = true;
                }
                else if (touch.phase == TouchPhase.Moved && isTouching)
                {
                    Vector2 delta = touch.deltaPosition;
                    float yaw = delta.x * lookSpeed * Time.deltaTime;
                    float pitch = -delta.y * lookSpeed * Time.deltaTime;

                    transform.Rotate(0, yaw, 0);

                    verticalRotation = Mathf.Clamp(verticalRotation + pitch, minPitch, maxPitch);

                    playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    isTouching = false;
                }
                break;
            }
        }
    }
}
