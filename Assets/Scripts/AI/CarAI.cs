using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(ArcadeCar))]
public class CarAI : StateMachine
{
    public enum DrivingMode { Pursue, Idle }

    [Space(12)]
    [Tooltip("This box is used to shoot the detection rays from. X and Z are used for size, Y is used for height placement.")]
    public Vector3 boxSize;
    [HideInInspector] public Rigidbody mainRb;
    [HideInInspector] public List<ArcadeCar> cars = new List<ArcadeCar>();

    // AI Atrributes
    [Header("Attributes")]
    [Range(0, 1)]
    [Tooltip("How eager the AI is to keep pursuing. 0 means it rather idles, 1 means it rather pursues.")]
    public float aggression = 0.25f;

    // Blackboard Variables
    [HideInInspector] public DrivingMode currentDrivingMode;
    [HideInInspector] public bool hitOpponent = false;
    [HideInInspector] public bool gotHit = false;
    [HideInInspector] public float idleTime;

    // States
    [HideInInspector] public BaseState pursuing;
    [HideInInspector] public BaseState avoiding;
    [HideInInspector] public BaseState reversing;
    [HideInInspector] public BaseState idle;

    private float gotHitDebounce = 0f;

    public void InitializeAI()
    {
        mainRb = GetComponent<Rigidbody>();

        ArcadeCar controller = GetComponent<ArcadeCar>();
        pursuing = new Pursuing(controller, this);
        avoiding = new Avoiding(controller, this);
        reversing = new Reversing(controller, this);
        idle = new Idle(controller, this);

        for (int i = 0; i < transform.parent.childCount; i++)
        {
            cars.Add(transform.parent.GetChild(i).GetComponent<ArcadeCar>());
        }

        int rndmStateNmbr = Random.Range(1, 4);
        if (rndmStateNmbr == 3)
        {
            currentDrivingMode = DrivingMode.Idle;
            idleTime = Random.Range(5f, 10f);
            Initialize(idle);
        }
        else
        {
            currentDrivingMode = DrivingMode.Pursue;
            Initialize(pursuing);
        }
    }

    private void LateUpdate()
    {
        gotHitDebounce -= Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.parent == transform.parent)
        {
            bool isPossibleAttacker = Mathf.Abs(Vector3.SignedAngle(transform.forward, collision.relativeVelocity.normalized, Vector3.up)) > 100f;

            if (isPossibleAttacker && currentState != reversing) hitOpponent = true;
            else if (currentDrivingMode == DrivingMode.Idle && gotHitDebounce <= 0f)
            {
                gotHit = true;
                gotHitDebounce = 0.4f;
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Handles.color = Color.black;
        Handles.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Handles.DrawWireCube(new Vector3(0, boxSize.y, 0), new Vector3(boxSize.x, 0, boxSize.z));
    }
#endif
}
