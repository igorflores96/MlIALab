using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;

public class InteligentAgent : Agent
{
    [SerializeField] private Transform _goal;
    [SerializeField] private Transform _wrongOne;
    [SerializeField] private Transform _wrongTwo;
    [SerializeField] private Transform _nothing;

    [SerializeField] private float _moveSpeed = 1.5f;
    [SerializeField] private float _rotationSpeed = 180f;

    private int _currentEpisode;
    private float _cumulativeReward;

    private Queue<Vector3> _positions;

    public override void Initialize()
    {
        _currentEpisode = 0;
        _cumulativeReward = 0f;

        List<Vector3> positionsList = new List<Vector3>
        {
            new Vector3(-2.25f, 0f, -2.25f),
            new Vector3(2.25f, 0f, -2.25f),
            new Vector3(-2.25f, 0f, 2.25f),
            new Vector3(2.25f, 0f, 2.25f)
        };

        for (int i = 0; i < positionsList.Count; i++)
        {
            Vector3 temp = positionsList[i];
            int randomIndex = Random.Range(i, positionsList.Count);
            positionsList[i] = positionsList[randomIndex];
            positionsList[randomIndex] = temp;
        }

        _positions = new Queue<Vector3>(positionsList);

        SpawnObjects();

    }

    public override void OnEpisodeBegin()
    {
        _currentEpisode++;
        _cumulativeReward = 0f;

        List<Vector3> positionsList = new List<Vector3>
        {
            new Vector3(-2.25f, 0f, -2.25f),
            new Vector3(2.25f, 0f, -2.25f),
            new Vector3(-2.25f, 0f, 2.25f),
            new Vector3(2.25f, 0f, 2.25f)
        };

        for (int i = 0; i < positionsList.Count; i++)
        {
            Vector3 temp = positionsList[i];
            int randomIndex = Random.Range(i, positionsList.Count);
            positionsList[i] = positionsList[randomIndex];
            positionsList[randomIndex] = temp;
        }

        _positions = new Queue<Vector3>(positionsList);

        SpawnObjects();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float goalPosX_normalized = _goal.localPosition.x / 5f;
        float goalPosZ_normalized = _goal.localPosition.z / 5f;

        float nothingPosX_normalized = _nothing.localPosition.x / 5f;
        float nothingPosZ_normalized = _nothing.localPosition.z / 5f;

        float wrongOnePosX_normalized = _wrongOne.localPosition.x / 5f;
        float wrongOnePosZ_normalized = _wrongOne.localPosition.z / 5f;

        float wrongTwoPosX_normalized = _wrongTwo.localPosition.x / 5f;
        float wrongTwoPosZ_normalized = _wrongTwo.localPosition.z / 5f;

        float agentX_normalized = transform.localPosition.x / 5f;
        float agentZ_normalized = transform.localPosition.z / 5f;

        float agentRotation_normalized = (transform.localRotation.eulerAngles.y / 360f) * 2f - 1f;

        //Onze space Size no script do Behavior Parameters
        sensor.AddObservation(goalPosX_normalized);
        sensor.AddObservation(goalPosZ_normalized);

        sensor.AddObservation(nothingPosX_normalized);
        sensor.AddObservation(nothingPosZ_normalized);

        sensor.AddObservation(wrongOnePosX_normalized);
        sensor.AddObservation(wrongOnePosZ_normalized);

        sensor.AddObservation(wrongTwoPosX_normalized);
        sensor.AddObservation(wrongTwoPosZ_normalized);

        sensor.AddObservation(agentX_normalized);
        sensor.AddObservation(agentZ_normalized);

        sensor.AddObservation(agentRotation_normalized);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions.DiscreteActions);

        AddReward(-0.1f);

        float distance = Vector3.Distance(transform.localPosition, _goal.localPosition);
        float maxDistance = 5f;
        float distanceReward = Mathf.Clamp01(1 - (distance / maxDistance)) * 0.5f;
        AddReward(distanceReward);

        _cumulativeReward = GetCumulativeReward();
    }

    private void MoveAgent(ActionSegment<int> act)
    {
        var action = act[0];

        switch (action)
        {
            case 1:
                transform.position += transform.forward * _moveSpeed * Time.deltaTime;
                break;
            case 2:
                transform.Rotate(0f, -_rotationSpeed * Time.deltaTime, 0f);
                break;
            case 3:
                transform.Rotate(0f, _rotationSpeed * Time.deltaTime, 0f);
                break;
        }
    }

    private void SpawnObjects()
    {
        transform.localPosition = new Vector3(0f, 0.5f, 0f);
        transform.localRotation = Quaternion.identity;

        _goal.localPosition = _positions.Dequeue();
        _nothing.localPosition = _positions.Dequeue();
        _wrongOne.localPosition = _positions.Dequeue();
        _wrongTwo.localPosition = _positions.Dequeue();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Goal"))
        {
            _cumulativeReward = GetCumulativeReward();
            AddReward(1f);
            EndEpisode();
        }
        else if (other.gameObject.CompareTag("Wrong"))
        {
            _cumulativeReward = GetCumulativeReward();
            AddReward(-0.1f);
        }
    }


    private void OnCollisionStay(Collision other) {
        
        if (other.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.1f * Time.fixedDeltaTime);
        }
    }
}
