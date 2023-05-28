using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoveAgentActivator : MonoBehaviour
{
    private bool randomizeStart = true;
    private bool randomizeGoal = true;
    private bool disableLabyrinth = false;
    private float moveSpeed = 1.0f;
    private float minSpawnDistance = 5.0f;

    public int wins = 0;
    public int losses = 0;

    [SerializeField]
    Toggle randomizeStartToggle;
    [SerializeField]
    Toggle randomizeGoalToggle;
    [SerializeField]
    Toggle disableLabyrinthToggle;
    [SerializeField]
    Slider moveSpeedSlider;
    [SerializeField]
    Slider minSpawnDistanceSlider;
    [SerializeField]
    TextMeshProUGUI _wins;
    [SerializeField]
    TextMeshProUGUI _losses;
    [SerializeField]
    GameObject settingsPanel;
    [SerializeField]
    GameObject scorePanel;

    private void Update()
    {
        if (scorePanel.activeSelf)
        {
            _wins.text = "Total wins: \n" + wins;
            _losses.text = "Total losses: \n" + losses;
        }
    }

    public void Activate()
    {
        randomizeStart = randomizeStartToggle.isOn;
        randomizeGoal = randomizeGoalToggle.isOn;
        disableLabyrinth = disableLabyrinthToggle.isOn;
        moveSpeed = moveSpeedSlider.value;
        minSpawnDistance = minSpawnDistanceSlider.value;

        foreach (GameObject agent in GameObject.FindGameObjectsWithTag("Agent"))
        {
            agent.GetComponent<MoveAgent>()._randomizeGoal = randomizeGoal;
            agent.GetComponent<MoveAgent>()._randomizeStart = randomizeStart;
            agent.GetComponent<MoveAgent>()._disableLabyrinth = disableLabyrinth;
            agent.GetComponent<MoveAgent>()._moveSpeed = moveSpeed;
            agent.GetComponent<MoveAgent>()._minSpawnDistance = minSpawnDistance;
            agent.GetComponent<MoveAgent>().enabled = true;
        }

        settingsPanel.SetActive(false);
        scorePanel.SetActive(true);
    }
}
