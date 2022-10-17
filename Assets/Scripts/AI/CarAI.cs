using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(ArcadeCar))]
public class CarAI : StateMachine
{
    [Space(12)]
    [Tooltip("This box is used to shoot the detection rays from. X and Z are used for size, Y is used for height placement.")]
    public Vector3 boxSize;
    [HideInInspector] public Rigidbody mainRb;

    // Blackboard Variables
    [HideInInspector] public bool hitOpponent = false;

    // States
    [HideInInspector] public BaseState pursuing;
    [HideInInspector] public BaseState avoiding;
    [HideInInspector] public BaseState reversing;

    private void Start()
    {
        mainRb = GetComponent<Rigidbody>();

        ArcadeCar controller = GetComponent<ArcadeCar>();
        pursuing = new Pursuing(controller, this);
        avoiding = new Avoiding(controller, this);
        reversing = new Reversing(controller, this);

        Initialize(pursuing);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.parent == transform.parent && currentState != reversing)
        {
            bool isPossibleAttacker = Mathf.Abs(Vector3.SignedAngle(transform.forward, collision.relativeVelocity.normalized, Vector3.up)) > 100f;

            if (isPossibleAttacker) hitOpponent = true;
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
