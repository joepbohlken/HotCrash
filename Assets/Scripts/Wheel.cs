using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;

public struct HitData
{
    public Vector3 point;
    public Vector3 normal;
    public Vector3 direction;
    public Vector3 relativeVelocity;
    public float distance;
    public bool hasHit;
    public float surfaceFriction;
    public int surfaceType;
}

public class Wheel : MonoBehaviour
{
    private Rigidbody carRigidbody;
    private Suspension suspensionParent;

    [Header("Size")]
    [Tooltip("The radius of the wheel.")]
    public float radius;
    [Tooltip("The width of the wheel.")]
    public float width;

    [Header("Raycasts")]
    [Tooltip("The amount of rays in a single disk of raycasts going around the center of the wheel.")]
    [Range(15, 50)]
    public int rayCount = 100;
    [Tooltip("Amount of raycast disks covering the wheel width.")]
    [Range(2, 5)]
    public int rayDiskCount = 2;
    [Range(90, 180)]
    public int raycastAngle;
    public float raycastOffset;
    [Tooltip("The layers that the raycast can take into account.")]
    public LayerMask rayLayerMask;

    // Hit data
    public HitData averageOutput;
    public HitData shortestOutput;

    [NonSerialized]
    public bool isGrounded;

    // Rays
    public List<WheelcastRayData> wheelcastRays;

    // Data for processing raycasts
    public RayProcesing rayProcessing;

    public struct RayProcesing
    {
        public NativeArray<RaycastHit> raycast_hits;
        public RaycastHit raycast_hit;
        public NativeArray<RaycastCommand> raycast_commands;
        public RaycastCommand raycast_command;
        public JobHandle raycast_job_handle;
    }

    public struct WheelcastRayData
    {
        public Vector3 localPosition;
        public Vector3 localDirection;
        public Vector3 direction;
        public float distance;
        public Transform parentTransform;

        public HitData hitData;
    }

    // RayProcessing cleanup
    private void OnDisable()
    {
        // cleanup
        OnDestroy();
    }

    private void OnDestroy()
    {
        // cleanup
        try
        {
            rayProcessing.raycast_commands.Dispose();
            rayProcessing.raycast_hits.Dispose();
        }
        catch { }
    }

    private void Start()
    {
        carRigidbody = GetComponentInParent<Rigidbody>();
        suspensionParent = GetComponentInParent<Suspension>();
        transform.localPosition = -Vector3.up * suspensionParent.restLength;

        SetupRaycasts();
    }

    private void Update()
    {
        isGrounded = averageOutput.hasHit;
        float lowestPoint = suspensionParent.transform.TransformPoint(-Vector3.up * suspensionParent.restLength).y;
        float highestPoint = suspensionParent.transform.TransformPoint(-Vector3.up * (suspensionParent.restLength - suspensionParent.travelDist)).y;
        transform.position = isGrounded ? new Vector3(transform.position.x, Mathf.Clamp(averageOutput.point.y + radius, lowestPoint, highestPoint), transform.position.z) : suspensionParent.transform.TransformPoint(-Vector3.up * (suspensionParent.restLength - suspensionParent.travelDist * suspensionParent.compressionRatio));

        ProcessRays();
    }

    public void SetupRaycasts()
    {
        // WheelCast data
        wheelcastRays = new List<WheelcastRayData>();

        // Define some steps
        float startAngle = -raycastAngle / 2f;
        float cStep = width / (rayDiskCount - 1);
        float radianStep = (float)raycastAngle / (float)(rayCount - 1);
        float angle;
        float x;
        float y;
        float z;

        // Loop
        for (int c = 0; c < rayDiskCount; c++)
        {
            for (int r = 0; r < rayCount; r++)
            {
                // Create 
                var tmpRay = new WheelcastRayData();

                // Parent
                tmpRay.parentTransform = transform;

                // Local center
                tmpRay.localPosition = (-Vector3.right * width * 0.5f) + (Vector3.right * cStep * c);

                // Local direction
                angle = radianStep * r + startAngle;
                tmpRay.localDirection = Quaternion.AngleAxis(angle, transform.forward) * -transform.up;

                // Distance
                tmpRay.distance = radius + raycastOffset;

                wheelcastRays.Add(tmpRay);
            }
        }


        // Raycast jobs
        rayProcessing.raycast_commands = new NativeArray<RaycastCommand>(wheelcastRays.Count, Allocator.Persistent);
        rayProcessing.raycast_hits = new NativeArray<RaycastHit>(wheelcastRays.Count, Allocator.Persistent);
    }

    public void ProcessRays()
    {
        // Process into world space
        for (int i = 0; i < wheelcastRays.Count; i++)
        {
            var w = wheelcastRays[i];

            // Center position
            rayProcessing.raycast_command.from = w.parentTransform.TransformPoint(w.localPosition);

            // Direction
            w.direction = w.parentTransform.TransformDirection(w.localDirection);
            rayProcessing.raycast_command.direction = w.direction;

            // Other params
            rayProcessing.raycast_command.distance = w.distance;
            rayProcessing.raycast_command.layerMask = rayLayerMask;
            rayProcessing.raycast_command.maxHits = 1;

            // Add to batch process
            rayProcessing.raycast_commands[i] = rayProcessing.raycast_command;

            // Debugging
            Debug.DrawRay(rayProcessing.raycast_command.from, rayProcessing.raycast_command.direction.normalized * rayProcessing.raycast_command.distance, Color.magenta);

            // Store
            wheelcastRays[i] = w;
        }

        // Process 
        rayProcessing.raycast_job_handle = RaycastCommand.ScheduleBatch(rayProcessing.raycast_commands, rayProcessing.raycast_hits, 10);
        rayProcessing.raycast_job_handle.Complete();

        // Outputs
        averageOutput = NewHitData();
        shortestOutput = NewHitData();

        float weight = 0f;
        float total_weight = 0f;
        int count = 0;
        float shortestDistance = 1000f;

        for (int i = 0; i < wheelcastRays.Count; i++)
        {
            // Collect data
            var w = wheelcastRays[i];
            w.hitData = NewHitData();


            // No hit, skip
            rayProcessing.raycast_hit = rayProcessing.raycast_hits[i];
            if (!(rayProcessing.raycast_hit.distance > 0))
            {
                continue;
            }

            // Store data
            w.hitData.distance = rayProcessing.raycast_hit.distance;
            w.hitData.hasHit = true;
            w.hitData.normal = rayProcessing.raycast_hit.normal;
            w.hitData.point = rayProcessing.raycast_hit.point;
            w.hitData.relativeVelocity = transform.InverseTransformDirection(transform.position);
            w.hitData.direction = w.direction;
            w.hitData.surfaceFriction = 1f;

            // Get weight
            weight = rayProcessing.raycast_hit.distance;
            // Weight = Mathf.Pow(weight, 5);
            total_weight += weight;

            // Shortest
            if (rayProcessing.raycast_hit.distance < shortestDistance)
            {
                shortestOutput = w.hitData;
                shortestDistance = rayProcessing.raycast_hit.distance;
            }

            // Average
            averageOutput.hasHit = true;
            averageOutput.point += rayProcessing.raycast_hit.point * weight;
            averageOutput.normal += rayProcessing.raycast_hit.normal * weight;
            averageOutput.distance += rayProcessing.raycast_hit.distance * weight;
            averageOutput.direction += w.hitData.direction * weight;

            wheelcastRays[i] = w;

            count++;
        }

        averageOutput.normal = averageOutput.hasHit ? Vector3.Normalize(averageOutput.normal) : Vector3.zero;
        averageOutput.point = averageOutput.hasHit ? averageOutput.point / total_weight : Vector3.zero;
        averageOutput.distance = averageOutput.hasHit ? averageOutput.distance / total_weight : 0f;
        averageOutput.direction = averageOutput.hasHit ? averageOutput.direction / total_weight : Vector3.zero;

        // Debugging
        Debug.DrawRay(shortestOutput.point, -shortestOutput.direction * shortestOutput.distance, Color.yellow, 0, false);
    }

    public HitData NewHitData()
    {
        var hitData = new HitData();
        hitData.point = Vector3.zero;
        hitData.normal = Vector3.zero;
        hitData.relativeVelocity = Vector3.zero;
        hitData.distance = 0;
        hitData.hasHit = false;
        hitData.surfaceFriction = 0;
        hitData.surfaceType = 0;
        return hitData;
    }

}
