using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

// 1) deschide powershell in folderul proiectului
// 2) ruleaza .\venv\Scripts\activate
// 3) mlagents-learn .\Assets\config\moveConfig.yaml --initialize-from=Parameters_Move3 --run-id=Parameters_Move*X*
// 4) In loc de *X* pune un nr unic (preferabil ultimul id + 1)

public class MoveAgent : Agent
{
    [SerializeField] 
    private Transform _targetTransform;
    [SerializeField]
    private Material _winMaterial;
    [SerializeField]
    private Material _looseMaterial;
    [SerializeField]
    private MeshRenderer _floorMeshRenderer;


    [SerializeField]
    private float _moveSpeed = 1.0f;
    private readonly string _goalTag = "Goal";
    private readonly string _wallTag = "Wall";
    private readonly float _reward = 13.420f;
    private float _oldTime = 0.0f;
    private float _time = 0.0f;
    private IEnumerable<Collider> _wallsCollider;
    private float dist;
    private float _optimalTime = 0.0f;
    private float _oldOptimalTime;
    private float _rewardCoeficient = 2.0f;
    private float _penalizeCoeficient = 1.0f;

    private void Start()
    {
        if (_wallsCollider == null)
            _wallsCollider = GameObject.FindGameObjectsWithTag(_wallTag)
                .Select(go => go.GetComponent<Collider>());
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(Random.Range(-26f, 10f), 22f, Random.Range(-3f, 16f));
        _targetTransform.localPosition = new Vector3(Random.Range(-26f, 10f), 22f, Random.Range(-3f, 16f));

        bool playerCollided;
        var playerCollider = GetComponent<Collider>();
        bool targetCollided;
        var targetCollider = _targetTransform.GetComponent<Collider>();

        do
        {
            playerCollided = CheckCollisions(playerCollider);

            if (playerCollided)
            {
                transform.localPosition = new Vector3(Random.Range(-26f, 10f), 22f, Random.Range(-3f, 16f));
                Physics.SyncTransforms();
                //Debug.Log("Player collided");
            }

        } while (playerCollided);

        do
        {
            targetCollided = CheckCollisions(targetCollider);

            if (targetCollided)
            {
                _targetTransform.localPosition = new Vector3(Random.Range(-26f, 10f), 22f, Random.Range(-3f, 16f));
                Physics.SyncTransforms();        
                //Debug.Log("Goal collided");
            }

        } while (targetCollided);

        dist = Vector3.Distance(transform.position, _targetTransform.position);

        _oldTime = _time;
        _time = 0.0f;
      
        _oldOptimalTime = _optimalTime;
        _optimalTime = dist / _moveSpeed;
        if (_oldOptimalTime == 0)
        {
            _oldOptimalTime = _optimalTime;
        }
        //transform.localPosition = new Vector3(-15f, 22f, 0f);
    }

    private bool CheckCollisions(Collider targetCollider)
    {
        foreach (var wallCollider in _wallsCollider)
        {
            if (targetCollider.bounds.Intersects(wallCollider.bounds))
            {
                return true;
            }
        }

        if (_targetTransform.GetComponent<Collider>().bounds.Intersects(GetComponent<Collider>().bounds))
        {
            return true;
        }

        return false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(_targetTransform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * _moveSpeed;
        _time += Time.deltaTime;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_goalTag))
        {
            Debug.Log("Euclidean distance: " + dist);
            Debug.Log("Time: " + _time);
            Debug.Log("Optimal time: " + _optimalTime);
            
            //ALTE REWARDURI
            /*if (_time < _oldTime)
            {
                SetReward(_reward * 4);
            }
            else
            {
                SetReward(_reward * 2);
            }*/
            //SetReward(_reward * 2);

            if (_time - _optimalTime < _oldTime - _oldOptimalTime)
            {
                SetReward(_reward / Mathf.Max(_time - _optimalTime, 1f) * _rewardCoeficient);
                _rewardCoeficient += 0.5f; 
            }
            else
            {
                //Versiune cu +
                //SetReward(_reward / Mathf.Max(_time - _optimalTime, 1f) * _rewardCoeficient / 2);
                SetReward(-_reward * Mathf.Max(_time - _optimalTime, 1f) * _penalizeCoeficient);
            }

            _floorMeshRenderer.material = _winMaterial;
            EndEpisode();
        }
        if (other.CompareTag(_wallTag))
        {
            SetReward(-_reward * Mathf.Max(_time - _optimalTime, 1f) * _penalizeCoeficient);
            //SetReward(-_reward);
            _penalizeCoeficient += 0.5f;

            _floorMeshRenderer.material = _looseMaterial;
            EndEpisode();
        }
    }
}
