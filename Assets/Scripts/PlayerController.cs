using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject hintArrow;
    
    public VirtualJoystick moveJoystick;
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    private Vector3 velocity;
    public float lookSpeed = 2f;
    private CharacterController controller;
    private Camera playerCamera;

    private bool isTouching = false;

    public Transform handTransform;
    private GameObject heldObject = null;
    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = Camera.main;
    }

    void Update()
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
                else if (hit.collider.CompareTag("ObjectsStorage") && heldObject != null)
                {
                    TrunkController trunkController = hit.collider.gameObject.GetComponent<TrunkController>();
                    if (trunkController.storagePlaces.Count > 0) {
                        Transform newPost = trunkController.storagePlaces[0];
                        DropObject(newPost);
                        trunkController.storagePlaces.Remove(trunkController.storagePlaces[0]);
                    }
                    
                }
            }
        }
    }

    void PickUpObject(GameObject obj)
    {
        heldObject = obj;
        hintArrow.SetActive(true);

        heldObject.transform.SetParent(handTransform);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;
    }

    void DropObject(Transform dropPosition)
    {
        if (heldObject)
        {
            heldObject.transform.SetParent(null);
            heldObject.GetComponent<Collider>().enabled = false;

            heldObject.transform.position = dropPosition.position;
            heldObject.transform.eulerAngles = dropPosition.eulerAngles;

            hintArrow.SetActive(false);
            heldObject = null;
        }
    }

    void MovePlayer()
    {
        if(controller.isGrounded && velocity.y < 0)
            velocity.y = 0;

        Vector3 move = transform.right * moveJoystick.Horizontal + transform.forward * moveJoystick.Vertical;
        controller.Move(move * moveSpeed * Time.deltaTime);
        
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleLook()
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
                playerCamera.transform.Rotate(pitch, 0, 0);
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
