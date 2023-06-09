using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class BallMovement : Agent
{
    #region Private Class Members
    private bool _finished;
    private readonly float _speed = 1.5f;
    private readonly float _torque = .3f;
    private bool _resetOnCollision = true;
    private float _currentDistance;
    #endregion
    
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer floorMeshRenderer;

    private void Start()
    {
        _finished = false;
        // Add all objects positions
    }

    /// <summary>
    /// OnEpisodeBegin - settings made on start of episode
    /// </summary>
    public override void OnEpisodeBegin()
    {
        if (_resetOnCollision)
        {
            transform.localPosition = new Vector3(-4.2f, 0.5f, 1f);
            transform.localRotation = Quaternion.identity * Quaternion.Euler(0, 110, 0);
            _resetOnCollision = false;
        }
    }
    
    /// <summary>
    /// Function that collects all data from environment
    /// </summary>
    /// <param name="sensor">A variable that store data used as input</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        //sensor.AddObservation(ObserveRay(1.5f, .5f, 150f));
        //sensor.AddObservation(ObserveRay(1.5f, .5f, 120f));
        sensor.AddObservation(ObserveRay(1.5f, .5f, 90f));
        sensor.AddObservation(ObserveRay(1.5f, .5f, 60f));
        sensor.AddObservation(ObserveRay(1.5f, .5f, 30f));
        sensor.AddObservation(ObserveRay(1.5f, 0f, 0f));
        sensor.AddObservation(ObserveRay(1.5f, -.5f, -30f));
        sensor.AddObservation(ObserveRay(1.5f, -.5f, -60f));
        sensor.AddObservation(ObserveRay(1.5f, -.5f, -90f));
        //sensor.AddObservation(ObserveRay(-1.5f, 0, -120f));
        //sensor.AddObservation(ObserveRay(-1.5f, 0, -150f));
        sensor.AddObservation(ObserveRay(-1.5f, 0, 180f));
    }

    private void MoveBall(float horizontal, float vertical, float dt)
    {
        // Translated in the direction the ball is facing
        float moveDist = _speed * vertical;
        transform.Translate(dt * moveDist * Vector3.forward);
        
        // Rotate alongside it up axis
        float rotation = horizontal * _torque * 90f;
        transform.Rotate(0f, rotation * dt, 0f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Move the ball, rotation and acceleration
        MoveBall(actions.ContinuousActions[0], Mathf.Abs(actions.ContinuousActions[1]), 
            Time.fixedDeltaTime);

        // If the distance to destination is improved, add a reward
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        // If ball get on checkpoint, we advance with next checkpoint
        if (other.CompareTag("Checkpoint"))
        {
            AddReward(0.5f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Goal"))
        {
            floorMeshRenderer.material = winMaterial;
            _resetOnCollision = true;
            _finished = true;
            Debug.Log("INFO: Ham reusit!");
            AddReward(+1f);
            EndEpisode();
            
        }
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            if (!_finished)
            {
                floorMeshRenderer.material = loseMaterial;
            }
            _resetOnCollision = true;
            SetReward(-1f);
            EndEpisode();
        }
    }

    private void FixedUpdate()
    {
        // Draw line of rayCast
        //DrawRay(.5f, .5f, 150f);
        //DrawRay(.5f, .5f, 120f);
        DrawRay(.5f, .5f, 90f);
        DrawRay(.5f, .5f, 60f);
        DrawRay(.5f, .5f, 30f);
        DrawRay(.5f, 0f, 0f);
        DrawRay(.5f, -.5f, -30f);
        DrawRay(.5f, -.5f, -60f);
        DrawRay(.5f, -.5f, -90f);
        //DrawRay(.5f, -.5f, -120f);
        //DrawRay(.5f, -.5f, -150f);
        DrawRay(-.5f, 0, 180f);
    }

    private float ObserveRay(float z, float x, float angle)
    {
        var tf = transform;
        
        // Get the start position of the ray
        var raySource = tf.position + Vector3.up / 2f;
        const float rayDist = 2.5f;
        var position = raySource + tf.forward * z + tf.right * x;
        
        // Get the angle of the ray
        var eulerAngle = Quaternion.Euler(0f, angle, 0f);
        var dir = eulerAngle * tf.forward;
        
        // Check if there is a hit in the given direction
        Physics.Raycast(position, dir, out var hit, rayDist);
        return hit.distance >= 0 ? hit.distance / rayDist : -1f;
    }

    private void DrawRay(float z, float x, float angle)
    {
        var tf = transform;
        
        // Get the start position of the ray
        var raySource = tf.position + Vector3.up / 2f;
        const float rayDist = 2.5f;
        var position = raySource + tf.forward * z + tf.right * x;
        
        // Get the angle of the ray
        var eulerAngle = Quaternion.Euler(0f, angle, 0f);
        var dir = eulerAngle * tf.forward * rayDist;
        
        // Draw line
        Debug.DrawRay(position, dir, Color.yellow);
    }

    // Update is called once per frame
    /*void Update()
    {
        if (Input.GetKeyDown("space") && _canJump)
        {
            GetComponent<Rigidbody>().AddForce(Vector3.up * 6, ForceMode.Impulse);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.CompareTag("Ground"))
        {
            _canJump = true;
            //Debug.Log("Putem sari");
        }
        
    }

    private void OnCollisionExit(Collision other)
    {
        _canJump = false;
        //Debug.Log("Nu mai Putem sari");
    }

    // Move ball
    private void FixedUpdate()
    {
        float horizontalMovement = Input.GetAxis("Horizontal");
        float verticalMovement = Input.GetAxis("Vertical");
        
        Vector3 moveBall = new Vector3(horizontalMovement,0,verticalMovement);
        gameObject.transform.GetComponent<Rigidbody>().AddForce(moveBall * _speed);
    }*/
}
