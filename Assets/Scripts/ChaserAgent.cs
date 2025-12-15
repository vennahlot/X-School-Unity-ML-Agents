using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;

[RequireComponent(typeof(Rigidbody))]
public class ChaserAgent : Agent
{
    [Header("Chaser Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] LayerMask groundLayer;

    [Header("References")]
    [SerializeField] private Transform runner;
    private Rigidbody chaserRb;
    private Rigidbody runnerRb;
    private RunnerAgent runnerAgent;
    private float previousDistance;
    
    void Start()
    {
        chaserRb = GetComponent<Rigidbody>();
        runnerRb = runner.GetComponent<Rigidbody>();
        runnerAgent = runner.GetComponent<RunnerAgent>();
    }

    public override void OnEpisodeBegin()
    {
        GameManager.Instance.ResetGame();
        previousDistance = (transform.localPosition - runner.localPosition).magnitude;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        // TODO: Add observations for chaser agent. E.g.: sensor.AddObservation(...)
        // ...
        // END TODO
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Getting discrete actions.
        Vector3 controlSignal = Vector3.zero;
        int jump = 0;
        
        // TODO: Get control signals from actions. E.g.: actions.ContinuousActions[0];
        // ...
        // END TODO

        // Controls movements.
        chaserRb.linearVelocity = new(
            controlSignal.x * speed,
            chaserRb.linearVelocity.y,
            controlSignal.z * speed
        );

        // Handle jump input.
        if (jump == 1 && IsGrounded())
        {
            chaserRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Rotate to face movement direction.
        if (controlSignal.x != 0 || controlSignal.z != 0)
        {
            Vector3 lookDirection = new(controlSignal.x, 0, controlSignal.z);
            Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
            chaserRb.rotation = Quaternion.Slerp(
                chaserRb.rotation, lookRotation, Time.fixedDeltaTime * 10f
            );
        }
        
        // TODO: Calculate and assign rewards based on distance to the runner. E.g.: SetReward(...)
        // ...
        // END TODO
        
        // End the episode if time is up.
        if (GameManager.Instance.CheckTimeOut())
        {
            SetReward(-100f); // Penalty for not catching the runner in time.
            runnerAgent.Escape();
            print("Runner escaped! Chaser current reward: " + GetCumulativeReward());
            EndEpisode();
        }
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
    
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Runner"))
        {
            // Caught the runner.
            GameManager.Instance.RunnerCaught();
            
            // TODO: Assign rewards for catching the runner. E.g.: SetReward(...)
            // ...
            // END TODO
            
            runnerAgent.GetCaught();
            print("Runner caught! Chaser current reward: " + GetCumulativeReward());
            EndEpisode();
        }
    }
}
