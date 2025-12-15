using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;

[RequireComponent(typeof(Rigidbody))]
public class RunnerAgent : Agent
{
    [Header("Chaser Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] LayerMask groundLayer;

    [Header("References")]
    [SerializeField] private Transform chaser;
    private Rigidbody chaserRb;
    private Rigidbody runnerRb;
    private RayPerceptionSensorComponent3D raySensor;
    private float previousDistance;
    
    void Start()
    {
        runnerRb = GetComponent<Rigidbody>();
        chaserRb = chaser.GetComponent<Rigidbody>();
        raySensor = GetComponent<RayPerceptionSensorComponent3D>();
    }

    public override void OnEpisodeBegin()
    {
        GameManager.Instance.ResetGame();
        previousDistance = (transform.localPosition - chaser.localPosition).magnitude;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        // 1. Relative position to the runner.
        sensor.AddObservation(transform.localPosition - chaser.localPosition);  // 3
        // 2. State of myself.
        sensor.AddObservation(runnerRb.linearVelocity);  // 3
        // 3. State of the runner.
        sensor.AddObservation(chaserRb.linearVelocity);  // 3
        // 4. Add Ray Perceptions for obstacles.
        sensor.AddObservation(raySensor);
        // 4. Game state (time left).
        sensor.AddObservation(GameManager.Instance.GetTimeLeft());  // 1
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Getting discrete actions.
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];
        int jump = actions.DiscreteActions[0];

        // Controls movements.
        runnerRb.linearVelocity = new(
            controlSignal.x * speed,
            runnerRb.linearVelocity.y,
            controlSignal.z * speed
        );

        // Handle jump input.
        if (jump == 1 && IsGrounded())
        {
            runnerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Rotate to face movement direction.
        if (controlSignal.x != 0 || controlSignal.z != 0)
        {
            Vector3 lookDirection = new(controlSignal.x, 0, controlSignal.z);
            Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
            runnerRb.rotation = Quaternion.Slerp(
                runnerRb.rotation, lookRotation, Time.fixedDeltaTime * 10f
            );
        }
        
        // Reward shaping based on distance to the runner.
        float currentDistance = (transform.localPosition - chaser.localPosition).magnitude;
        SetReward(-10f * (previousDistance - currentDistance)); // Punishment for reducing distance.
        previousDistance = currentDistance;
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        var discreteActionsOut = actionsOut.DiscreteActions;
        // Map keyboard inputs to discrete actions.
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
        discreteActionsOut[0] = Convert.ToInt32(Input.GetKey(KeyCode.Space));
    }

    bool IsGrounded()
    {
        return Physics.CheckSphere(
            transform.position + Vector3.down * 0.5f, 0.1f, groundLayer);
    }

    public void GetCaught()
    {
        SetReward(-100f); // Penalty for being caught.
        print("Runner caught! Runner current reward: " + GetCumulativeReward());
        EndEpisode();
    }

    public void Escape()
    {
        SetReward(100f); // Reward for escaping the chaser.
        print("Runner escaped! Runner current reward: " + GetCumulativeReward());
        EndEpisode();
    }
}
