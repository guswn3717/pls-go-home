using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyControl : MonoBehaviour
{
    public enum EnemyName
    {
        Bomb,
        Shoot,
        fighter,
        Boss1,
        Boss2,
        Boss3
    }

    public enum EnemyScore
    {
        Bomb = 10,
        Shoot = 10,
        fighter = 20,
        Boss1 = 500,
        Boss2 = 1000,
        Boss3 = 2000
    }

    public enum EnemyEx
    {
        Bomb = 10,
        Shoot = 20,
        fighter = 30,
        Boss1 = 500,
        Boss2 = 500,
        Boss3 = 900
    }

    public enum StageEnemyNum
    {
        stage01 = 50,
        stage02 = 100,
        stage03 = 150,
    }

    public MainControl mainControl;
    public GameObject enemyPrefab;
    public int addEnemyNum = 0;

    public void RepeatRespawn()
    {
        InvokeRepeating("Respawn", 3.0f, 3.0f);
    }

    void Respawn()
    {
        //x축 기준 -80~80, 50,-80
        switch(mainControl.stageNum)
        {
            case 0: //스테이지1 맵 상단 기준 적 생성
                if(addEnemyNum < (int)StageEnemyNum.stage01)
                {
                    Instantiate(enemyPrefab,
                        new Vector3(Random.Range(-80, 80), 0, 50),
                        enemyPrefab.transform.rotation);
                    addEnemyNum++;
                }
                else
                {
                    CancelInvoke("Respawn");
                }
                break;

            case 1:         //스테이지2 맵 상단 좌/우 기준 생성
                if(addEnemyNum < (int)StageEnemyNum.stage02)
                {
                    float tempX = Random.Range(-80, 80);
                    int EnemyNum = Random.Range(0, 2);
                    if(tempX > -50 && tempX < 50)
                    {
                        Enemy enemy = Instantiate(enemyPrefab,
                            new Vector3(tempX, 0, 50),
                            enemyPrefab.transform.rotation)
                            .GetComponent<Enemy>();
                        enemy.SetEnemy((EnemyName)EnemyNum);
                    }
                    else
                    {
                        Enemy enemy = Instantiate(enemyPrefab,
                            new Vector3(tempX, 0,
                            Random.Range(-80, 80)),
                            enemyPrefab.transform.rotation)
                            .GetComponent<Enemy>();
                        enemy.SetEnemy((EnemyName)EnemyNum);
                    }
                    addEnemyNum++;
                }
                break;

            case 2://스테이지3 맵 상/하/좌/우
                if(addEnemyNum < (int)StageEnemyNum.stage03)
                {
                    float tempX = Random.Range(-80, 80);
                    int EnemyNum = Random.Range(0, 3);
                    if (tempX > -50 && tempX < 50)
                    {
                        int minus = 1;
                        if(Random.Range(0,2) == 1)
                        {
                            minus = -1;
                        }
                        Enemy enemy = Instantiate(enemyPrefab,
                            new Vector3(tempX, 0, 50 * minus),
                            enemyPrefab.transform.rotation)
                            .GetComponent<Enemy>();
                        enemy.SetEnemy((EnemyName)EnemyNum);
                    }
                    else
                    {
                        Enemy enemy = Instantiate(enemyPrefab,
                            new Vector3(tempX, 0,
                            Random.Range(-80, 80)),
                            enemyPrefab.transform.rotation)
                            .GetComponent<Enemy>();
                        enemy.SetEnemy((EnemyName)EnemyNum);
                    }
                    addEnemyNum++;
                }
                break;

                
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
