using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Leaderboard : MonoBehaviour
{
    public GameObject rowsWrapper;
    public GameObject rowPrefab;

    public float rowAnimDuration = 0;
    public float rowShiftDistance = 400;

    public Animator animator { get; set; }

    private List<ScoreRow> rows = new List<ScoreRow>();
    private bool isReady = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (GameManager.main.playersLeft == 0 && GameManager.main.gameStarted && isReady)
        {
            List<CarScore> scoreboard = GameManager.main.scoreboard;

            UpdateScoreboard(scoreboard);
        }
    }

    private void UpdateScoreboard(List<CarScore> scoreboard)
    {
        float initialHeight = -rowPrefab.GetComponent<RectTransform>().sizeDelta.y /2;

        for (int i = 0; i < scoreboard.Count; i++)
        {
            ScoreRow row = rows.FirstOrDefault(r => r.score.car == scoreboard[i].car);

            // Update score texts
            if (row != null)
            {
                row.score = scoreboard[i];
                row?.UpdateScore();
            }

            RectTransform rect = row.GetComponent<RectTransform>();
            Vector2 pos = rect.anchoredPosition;
            float offset = 2f;

            float startingHeight = pos.y;
            float endHeight = initialHeight - (i * rect.sizeDelta.y + (i != 0 ? offset * i : 0));

            pos.y = Mathf.Lerp(startingHeight, endHeight, Time.deltaTime * 10f);
            rect.anchoredPosition = pos;
        }
    }

    public void CleanUp()
    {
        GameManager.main.CleanUpLevelScene();
    }

    public void InitializeBoard()
    {
        List<CarScore> scoreboard = GameManager.main.scoreboard;

        for (int i = 0; i < scoreboard.Count; i++)
        {
            ScoreRow row = Instantiate(rowPrefab).GetComponent<ScoreRow>();
            row.score = scoreboard[i];
            rows.Add(row);

            row.transform.SetParent(rowsWrapper.transform, false);

            // Set positioning
            RectTransform rect = row.GetComponent<RectTransform>();
            Vector2 pos = rect.anchoredPosition;
            float offset = 2f;

            pos.x -= rowShiftDistance;
            pos.y -= (i * rect.sizeDelta.y + (i != 0 ? offset * i : 0));
            rect.anchoredPosition = pos;

            row.GetComponent<CanvasGroup>().alpha = 0;

            StartCoroutine(AnimateScore(i, row));
        }

        isReady = true;
    }

    private IEnumerator AnimateScore(int i, ScoreRow row)
    {
        yield return new WaitForSeconds(i * .1f);

        RectTransform rect = row.GetComponent<RectTransform>();
        CanvasGroup cGroup = row.GetComponent<CanvasGroup>();
        Vector2 pos = rect.anchoredPosition;

        float startPosX = pos.x;
        float endPosX = pos.x + rowShiftDistance;

        for (float j = 0; j <= rowAnimDuration; j += Time.deltaTime)
        {
            cGroup.alpha = Mathf.Lerp(0, 1, j / rowAnimDuration);
            pos.x = Mathf.Lerp(startPosX, endPosX, j / rowAnimDuration);
            rect.anchoredPosition = pos;
            yield return null;
        }

        cGroup.alpha = 1;
        pos.x = endPosX;
        rect.anchoredPosition = pos;
    }
}
