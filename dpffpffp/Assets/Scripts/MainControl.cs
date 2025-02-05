using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainControl : MonoBehaviour
{
    public int stageNum = 0;
    public int maxStageNum = 2;
    public EnemyControl enemyControl;
    public PlayerController playerControl;
    public int score = 0;

    public GameObject StopUI;
    public GameObject MenuUI;
    public Text LvText;
    public Text scoreText;
    public Slider HPBar;
    public Slider MPBar;
    public Slider EXBar;

    public Text StopLvText;

    public List<Text> TextSkill;
    public List<string> SkillName = new List<string>()
    {
        "기관포","추적탄","빠른전진","보호막"
    };

    // Start is called before the first frame update
    void Start()
    {
        enemyControl = GameObject.Find("EnemyControl")
            .GetComponent<EnemyControl>();
        enemyControl.RepeatRespawn();
        playerControl = GameObject.Find("Player")
            .GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(MenuUI.activeInHierarchy)
            {
                Time.timeScale = 1;
                MenuUI.SetActive(false);
            }
            else
            {
                Time.timeScale = 0;
                OpenMenuUI();
            }
        }
    }

    public void BtnSkill(int num)
    {
        playerControl.skillLevel[num]++;
        TextSkill[num].text = SkillName[num]
            + "\n" + "Lv." + playerControl.skillLevel[num];
        Time.timeScale = 1;
        EXBar.value = 0;
        StopUI.SetActive(false);
    }

    public void OpenStopUI()
    {
        StopUI.SetActive(true);
    }
    public void OpenMenuUI()
    {
        MenuUI.SetActive(true);
    }
    public void BtnRestart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Game");
    }
    public void BtnExit()
    {
        Application.Quit();
    }
    public void SetScore(int num)
    {
        score += num;
        scoreText.text = score.ToString();
    }
}
