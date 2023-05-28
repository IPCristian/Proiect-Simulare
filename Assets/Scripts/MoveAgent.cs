using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Rendering;

//Testare alta logica in afara de AI: da disable la toate environmenturile in afara de 17 si bifeaza la 
// agent la behaviour parameters la behaviour type "heuristic"
//daca vrei sa inveti nu uita sa dai revert

//!!!SA AIBA TOATE ENVIRONMENTURILE ACEIASI PARAMETRII CA IN PREFAB CAND DAI LEARN (sa ti minte la care ai schimbat din hierarchy)
// 1) deschide powershell in folderul proiectului
// 2) ruleaza .\venv\Scripts\activate
// 3) mlagents-learn .\Assets\config\moveConfig.yaml --initialize-from=Parameters_Move3 --run-id=Parameters_Move*X*
// 4) In loc de *X* pune un nr unic (preferabil ultimul id + 1)

public class MoveAgent : Agent
{
    [SerializeField, Tooltip("If this is checked the player will start in a random valid position")]
    public bool _randomizeStart = true;
    [SerializeField, Tooltip("If this is checked the goal will start in a random valid position")]
    public bool _randomizeGoal = true;
    [SerializeField, Tooltip("If this is checked the labyrinth will not spawn")]
    public bool _disableLabyrinth = false;
    [SerializeField, Range(1.0f, 50.0f), Tooltip("The speed of the player")]
    public float _moveSpeed = 1.0f;
    [SerializeField, Range(0.5f, 10f), Tooltip("The minimum distance between the player and the goal")]
    public float _minSpawnDistance = 5.0f;

    [SerializeField] 
    private Transform _targetTransform;
    [SerializeField]
    private Material _winMaterial;
    private GameObject _postProcessing;
    [SerializeField]
    private Material _looseMaterial;
    [SerializeField]
    private MeshRenderer _floorMeshRenderer;
    [SerializeField]
    private GameObject _scoreKeeper;
    
    private readonly string _goalTag = "Goal";
    private readonly string _wallTag = "Wall";
    private readonly string _labyrinthWallTag = "Labyrinth_Wall";
    private readonly float _reward = 13.420f;
    private readonly float _targetBoundsExtra = 0.5f;
    private float _oldTime = 0.0f;
    private float _time = 0.0f;
    private IEnumerable<Collider> _wallsCollider = Enumerable.Empty<Collider>();
    private float _dist;
    private float _optimalTime = 0.0f;
    private float _oldOptimalTime;
    private float _rewardCoeficient = 2.0f;
    private float _penalizeCoeficient = 1.0f;

    private Vector3 _initPlayerPosition;
    private Vector3 _initGoalPosition;
    private bool _spawned = false;

    private void Awake()
    {
        _initPlayerPosition = transform.position;
        _initGoalPosition = _targetTransform.position;
    }

    private void Start()
    {
        _postProcessing = GameObject.Find("GlobalPostProcessing");

        if (!_disableLabyrinth)
        {
            _wallsCollider = GameObject.FindGameObjectsWithTag(_labyrinthWallTag)
               .Select(go => go.GetComponent<Collider>());
        }
        else
        {
            GameObject.FindGameObjectsWithTag(_labyrinthWallTag).ToList().ForEach(x => Destroy(x));
        }
            
    }

    public override void OnEpisodeBegin()
    {
        _spawned = false;

        if (_randomizeStart)
        {
            Spawn(transform);
        }
        else
        {
            transform.position = _initPlayerPosition;
        }

        if (_randomizeGoal)
        {
            Spawn(_targetTransform);
        }
        else
        {
            _targetTransform.position = _initGoalPosition;
        }

        _spawned = true;

        _dist = Vector3.Distance(transform.position, _targetTransform.position);

        _oldTime = _time;
        _time = 0.0f;
      
        _oldOptimalTime = _optimalTime;
        _optimalTime = _dist / _moveSpeed;
        if (_oldOptimalTime == 0)
        {
            _oldOptimalTime = _optimalTime;
        }
    }

    private void Spawn(Transform transform)
    {
        bool collided;
        var collider = transform.GetComponent<Collider>();

        transform.localPosition = new Vector3(Random.Range(-26f, 10f), 22f, Random.Range(-3f, 16f));
        Physics.SyncTransforms();

        do
        {
            collided = CheckCollisions(collider);

            if (collided)
            {
                transform.localPosition = new Vector3(Random.Range(-26f, 10f), 22f, Random.Range(-3f, 16f));
                Physics.SyncTransforms();
                //Debug.Log("Player collided");
            }

        } while (collided);

    }

    private bool CheckCollisions(Collider targetCollider)
    {
        var targetBounds = targetCollider.bounds;
        targetBounds.Expand(_targetBoundsExtra);

        foreach (var wallCollider in _wallsCollider)
        {
            if (targetBounds.Intersects(wallCollider.bounds))
            {
                return true;
            }
        }

        targetBounds = _targetTransform.GetComponent<Collider>().bounds;
        targetBounds.Expand(0.25f);

        if (_targetTransform.GetComponent<Collider>().bounds.Intersects(GetComponent<Collider>().bounds))
        {
            return true;
        }

        var dist = Vector3.Distance(transform.position, _targetTransform.position);

        if (dist < _minSpawnDistance)
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

    //Aici e partea importanta, trebuie umblat la rewarduri
    private void OnTriggerEnter(Collider other)
    {
        if (_spawned)
        {
            if (other.CompareTag(_goalTag))
            {
                Debug.Log("Win");
                Debug.Log("Euclidean distance: " + _dist);
                Debug.Log("Time: " + _time);
                Debug.Log("Optimal time: " + _optimalTime);
                _scoreKeeper.GetComponent<MoveAgentActivator>().wins += 1;

                //ALTE REWARDURI
                //1)
                /*if (_time < _oldTime)
                {
                    SetReward(_reward * 4);
                }
                else
                {
                    SetReward(_reward * 2);
                }*/
                //2)
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
                CancelInvoke("removePostFx");
                _postProcessing.GetComponent<Volume>().enabled = true;
                Invoke("removePostFx", 3);
                EndEpisode();
            }
            if (other.CompareTag(_wallTag) || other.CompareTag(_labyrinthWallTag))
            {
                Debug.Log("Lose");
                _scoreKeeper.GetComponent<MoveAgentActivator>().losses += 1;

                SetReward(-_reward * Mathf.Max(_time - _optimalTime, 1f) * _penalizeCoeficient);
                //SetReward(-_reward);
                _penalizeCoeficient += 0.5f;

                _floorMeshRenderer.material = _looseMaterial;
                EndEpisode();
            }
        }
       
    }

    void removePostFx()
    {
        _postProcessing.GetComponent<Volume>().enabled= false;
    }
}
