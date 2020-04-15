using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    private float defaultSpeed = 6.0f, speed = 6.0f;
    private float mouseSensitivity = 2.0f;
    private Vector3 moveDirection = Vector3.zero;
    private CharacterController controller;
    private float verticalVelocity = 0f;
    private float verticalRotation = 0;
    private float jumpSpeed = 5.0f;
    public bool canMove;
    private Transform heldObject = null;
    private float? heldObjectDistance = null;
    float? oldYRot = null, oldXRot = null;
    private float throwForce = 10.0f;
    private float scrollSpeed = 30.0f;

	// Use this for initialization
	void Start () {
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<CharacterController>();
	}
	
	// Update is called once per frame
	void Update () {
        Move();
        CheckMoveExit();
	}

    private void LateUpdate()
    {
        if (canMove)
        {
            PickUpAndCarry();
        }
    }

    public float pushPower = 0.75f;
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic)
            return;

        if (hit.moveDirection.y < -0.3F)
            return;

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        body.velocity = pushDir * pushPower;
    }

    void Move()
    {
        if (canMove)
        {
            float yAxisRot = Input.GetAxis("Mouse X") * mouseSensitivity;
            transform.Rotate(0, yAxisRot, 0);

            verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            verticalRotation = Mathf.Clamp(verticalRotation, -90, 90);
            Camera.main.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);

            verticalVelocity += Physics.gravity.y * Time.deltaTime;

            if (controller.isGrounded && Input.GetButton("Jump"))
            {
                verticalVelocity = jumpSpeed;
            }
        }

        speed = canMove ? defaultSpeed : speed * 0.9f;

        moveDirection = new Vector3(Input.GetAxis("Horizontal") * speed, verticalVelocity, Input.GetAxis("Vertical") * speed);
        moveDirection = transform.TransformDirection(moveDirection);

        controller.Move(moveDirection * Time.deltaTime);
    }

    void CheckMoveExit()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            canMove = true;
        }
    }

    void PickUpAndCarry()
    {
        if (heldObject == null && Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector2((Screen.width - 1) / 2, (Screen.height - 1) / 2));
            RaycastHit hit;
            bool shouldPickUp = Physics.Raycast(ray, out hit, 1.5f) && hit.transform.tag == "Carryable";
            if (shouldPickUp)
            {
                heldObject = hit.transform;
                heldObjectDistance = Vector3.Distance(Camera.main.transform.position, hit.transform.position);
                heldObject.GetComponent<Rigidbody>().isKinematic = true;
                heldObject.SendMessage("OnCarry");
            }
            
        } else if (heldObject)
        {
            if (!heldObjectDistance.HasValue)
            {
                heldObjectDistance = 1.5f;
            }
            heldObject.position = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, heldObjectDistance.Value));
            if (oldYRot.HasValue)
            {
                heldObject.Rotate(Vector3.up * (transform.eulerAngles.y - oldYRot.Value), Space.World);
                //Debug.Log($"old: {oldYRot.Value}, new: {transform.eulerAngles.y}");

                //Vector3 perp = Vector3.Cross(Vector3.up, transform.forward).normalized;                
            }
            float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
            Debug.DrawRay(transform.position, Vector3.Cross(transform.forward, Vector3.up), Color.red);
            if (scrollDelta != 0.0f)
            {
                Debug.Log(scrollDelta);
                //heldObject.Rotate(Vector3.right * scrollDelta * scrollSpeed, Space.Self);
                heldObject.Rotate((Vector3.Cross(Vector3.up, transform.forward)) * scrollDelta * scrollSpeed, Space.World);
            }
            //if (oldXRot.HasValue)
            //{
            //    Debug.DrawRay(transform.position, transform.right, Color.green);
            //    Debug.DrawRay(transform.position, heldObject.right, Color.red);
            //    //heldObject.Rotate(heldObject.right * (Camera.main.transform.eulerAngles.x - oldXRot.Value), Space.Self);
            //    heldObject.Rotate(Vector3.right * (Camera.main.transform.eulerAngles.x - oldXRot.Value), Space.Self);
            //}
            oldYRot = transform.eulerAngles.y;
            oldXRot = Camera.main.transform.eulerAngles.x;

            if (Input.GetMouseButtonDown(0))
            {
                ThrowObject();
            }

            if (Input.GetMouseButtonDown(1))
            {
                ReleaseObject();
            }
        }
    }

    void ReleaseObject()
    {
        heldObject.GetComponent<Rigidbody>().isKinematic = false;
        heldObject.SendMessage("OnDrop");
        heldObject = null;
        heldObjectDistance = null;
        oldYRot = null;
        oldXRot = null;
    }

    void ThrowObject()
    {
        Rigidbody heldRb = heldObject.GetComponent<Rigidbody>();
        heldRb.isKinematic = false;
        heldRb.AddForce(Camera.main.transform.forward * throwForce, ForceMode.Impulse);
        heldObject.SendMessage("OnDrop");
        heldObject = null;
        heldObjectDistance = null;
        oldYRot = null;
        oldXRot = null;
    }

}
