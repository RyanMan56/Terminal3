using UnityEngine;
using System.Collections;

public class TerminalButton : MonoBehaviour {
    private Collider coll;
    public Monitor terminalScript;
    private Rigidbody rb;
    private bool depressed = false;
    private ConfigurableJoint joint;
    public AudioSource inSound;
    public AudioSource outSound;


    // Use this for initialization
    void Start () {
        coll = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        joint = GetComponent<ConfigurableJoint>();
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector2((Screen.width - 1) / 2, (Screen.height - 1) / 2));
            RaycastHit hit;
            bool buttonPressed = Physics.Raycast(ray, out hit, 1.5f);
            if (buttonPressed && (hit.collider.Equals(coll)))
            {
                rb.AddRelativeForce(-Vector3.back * 5.0f);
            }
        }        

        if (!rb.IsSleeping())
        {
            FindDistanceBetweenBodies();
        }
    }

    void FindDistanceBetweenBodies()
    {
        Vector3 buttonAnchor = (transform.TransformPoint(joint.anchor));
        Vector3 backOfMonitor = joint.connectedBody.transform.TransformPoint(joint.connectedAnchor + -Vector3.back * 0.015f);

        float distance = Vector3.Distance(buttonAnchor, backOfMonitor);

        if (!depressed && distance < 0.005f)
        {
            depressed = true;
            inSound.Play();
        }
        else if (depressed && distance > 0.01f)
        {
            depressed = false;
            outSound.Play();
            if (terminalScript.on)
            {
                terminalScript.TurnScreenOff();

            }
            else
            {
                terminalScript.TurnScreenOn();
            }
        }
    }
}

