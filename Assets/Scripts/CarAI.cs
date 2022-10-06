using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ArcadeCar))]
public class CarAI : MonoBehaviour
{
    private class DetectRay
    {
        public Vector3 start;
        public Quaternion direction;
        public string side;
    }

    private enum CarState { Forwards, Reverse };

    [SerializeField] private Vector3 boxSize;

    private ArcadeCar carController;
    private Rigidbody mainRb;
    private Transform target;
    private List<DetectRay> detectRays;
    private CarState currentState = CarState.Forwards;

    private void Start()
    {
        carController = GetComponent<ArcadeCar>();
        mainRb = GetComponent<Rigidbody>();

        // Set up detect rays
        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = boxSize;
        detectRays = new List<DetectRay>()
        {
            new DetectRay() { start = new Vector3(-collider.size.x / 2f + 0.1f, 0, collider.size.z / 2f - 0.1f), direction = Quaternion.AngleAxis(0f, Vector3.up), side = "Left" },
            new DetectRay() { start = new Vector3(-collider.size.x / 2f + 0.1f, 0, collider.size.z / 2f - 0.1f), direction = Quaternion.AngleAxis(-45f, Vector3.up), side = "Left" },
            new DetectRay() { start = new Vector3(-collider.size.x / 2f + 0.1f, 0, collider.size.z / 2f - 0.1f), direction = Quaternion.AngleAxis(45f, Vector3.up), side = "Right" },
            new DetectRay() { start = new Vector3(collider.size.x / 2f - 0.1f, 0, collider.size.z / 2f - 0.1f), direction = Quaternion.AngleAxis(-45f, Vector3.up), side = "Left" },
            new DetectRay() { start = new Vector3(collider.size.x / 2f - 0.1f, 0, collider.size.z / 2f - 0.1f), direction = Quaternion.AngleAxis(0f, Vector3.up), side = "Right" },
            new DetectRay() { start = new Vector3(collider.size.x / 2f - 0.1f, 0, collider.size.z / 2f - 0.1f), direction = Quaternion.AngleAxis(45f, Vector3.up), side = "Right" },
        };
    }

    private void Update()
    {
        // Assign target
        float closestCar = 999f;

        for (int i = 0; i < transform.parent.childCount; i++)
        {
            Transform car = transform.parent.GetChild(i);
            if (car == transform) continue;

            bool isWithinView = Vector3.Angle(transform.forward, (car.position - transform.position).normalized) <= 45f;

            float distance = (car.position - transform.position).magnitude;
            if (isWithinView && distance < closestCar)
            {
                closestCar = distance;
                target = car;
            }
        }

        // Initialize detect rays
        float currentSpeed = Mathf.Clamp(Vector3.Dot(transform.forward, mainRb.velocity), 10f, 999f);

        float closestHit = 999f;
        string hitSide = "";

        foreach (DetectRay detectRay in detectRays)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.TransformPoint(detectRay.start), detectRay.direction * transform.forward, out hit, currentSpeed / 2f))
            {
                if (hit.distance < closestHit)
                {
                    closestHit = hit.distance;
                    hitSide = detectRay.side;
                }
            }

            //Debug.DrawRay(transform.TransformPoint(detectRay.start), detectRay.direction * transform.forward * (currentSpeed / 2f), detectRay.side == "Left" ? Color.white : Color.red);
        }

        if (closestHit <= 1f && currentState == CarState.Forwards) StartCoroutine(ReverseCar());

        // Set acceleration direction
        //float playerSideFB = Vector3.Dot(transform.forward, (target.position - transform.position).normalized);
        carController.v = currentState == CarState.Forwards ? 1f : -1f;

        // Set steering direction
        if (currentState == CarState.Reverse)
        {
            carController.h = 0f;
        }
        else if (hitSide == "")
        {
            if (target != null)
            {
                float playerSideLR = Vector3.SignedAngle(transform.forward, (target.position - transform.position).normalized, Vector3.up);
                if (Mathf.Abs(playerSideLR) > 10f)
                {
                    carController.h = Mathf.Sign(playerSideLR);
                }
                else
                {
                    carController.h = 0f;
                }
            }
        }
        else
        {
            carController.h = hitSide == "Left" ? 1f : -1f;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.parent == transform.parent && currentState == CarState.Forwards)
        {
            bool isPossibleAttacker = Mathf.Abs(Vector3.SignedAngle(transform.forward, collision.relativeVelocity.normalized, Vector3.up)) > 100f;

            if (isPossibleAttacker) StartCoroutine(ReverseCar());
        }
    }

    private IEnumerator ReverseCar()
    {
        currentState = CarState.Reverse;
        yield return new WaitForSeconds(Random.Range(0.5f, 1f));
        currentState = CarState.Forwards;
    }
}
