using UnityEngine;
using System.Collections;

public class TerminalButton : MonoBehaviour {
    private Vector3 offPos, onPos, depressedPos; // Not that depressed :(
    private Collider coll;
    private Monitor terminalScript;
    private Vector3? targetPos = null;
    private Rigidbody rb;
    private float springConstant = 2.0f;

    // Use this for initialization
    void Start () {
        coll = GetComponent<Collider>();
        terminalScript = GetComponentInParent<Monitor>();
        rb = GetComponent<Rigidbody>();

        offPos = new Vector3(transform.localPosition.x, transform.localPosition.y, -0.0255f);
        onPos = new Vector3(transform.localPosition.x, transform.localPosition.y, -0.0138f);
        depressedPos = new Vector3(transform.localPosition.x, transform.localPosition.y, -0.0074f);

        transform.localPosition = terminalScript.on ? onPos : offPos;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        if (targetPos.HasValue)
        {
            HandlePresses();
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(new Vector2((Screen.width - 1) / 2, (Screen.height - 1) / 2));
                RaycastHit hit;
                bool buttonPressed = Physics.Raycast(ray, out hit, 1.5f);
                if (buttonPressed && (hit.collider.Equals(coll)))
                {
                    targetPos = depressedPos;
                    rb.velocity = Vector3.zero;
                }
            }

            if (transform.localPosition.z > (terminalScript.on ? onPos.z : offPos.z))
            {
                CalculateSpring();
            }
            else
            {
                rb.velocity = Vector3.zero;
            }
        }
	}

    void HandlePresses()
    {
        if (transform.localPosition.z < targetPos.Value.z)
        {
            rb.AddForce(-Vector3.back * 0.005f);
        }
        else
        {
            targetPos = null;
            bool currentlyOn = terminalScript.on;
            
            if (currentlyOn)
            {                
                terminalScript.TurnScreenOff();
            }
            else
            {
                terminalScript.TurnScreenOn();
            }
        }
    }

    void CalculateSpring()
    {
        float x = terminalScript.on ? onPos.z - transform.localPosition.z : offPos.z - transform.localPosition.z;
        float F = Mathf.Abs(-springConstant * x);
        rb.AddForce(Vector3.back * F);
    }
}
