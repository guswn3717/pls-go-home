using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public int level = 0;
    public int maxLevel = 6;
    public int maxEx = 0;
    public int curEx = 0;

    public Rigidbody rb;

    private List<int> ExNum = new List<int>()
    {
        0, 100, 200, 300, 500, 800, 1200
    };

    //HP
    public int MaxHp = 100;    // 0, 100, 200, 300, 500, 800, 1200
    public int HP = 100;

    //shild hp
    public int bohoHp = 0;
    public int MaxBohoHp = 200;
    public float bohoTime = 0;

    //마력
    public int MaxMp = 50;
    public int MP = 50;

    //기본 이동 속도
    public float baseMoveSpeed = 5.0f;

    //현제 이동 속도
    public float moveSpeed = 5.0f;

    //회전속도
    public float rotationSpeed = 90.0f;

    //공격 가능 거리
    public float attackRange = 10.0f;

    //공격간 딜레이
    public float attackDelay = 1.0f;
    private float attackTimer = 0f;

    //현재 기본 공격력
    public int attackDamage = 3;

    //탱크 발사대
    public Transform turret;
    public Transform firePoint;
    public float turretTurnSpeed = 50.0f;
    public float turretPitchSpeed = 30.0f;

    //총알 프리팹
    public GameObject bulletPrefab;

    //기관포 프리팹
    public GameObject cannonPrefab;

    //추적탄 프리팹
    public GameObject tracerPrefab;

    //적 탐지 거리
    public float detectionRange = 10.0f;

    //적 레이어
    public LayerMask enemyLayer;

    //회전각도 상에 들어왔을때 인식 범위(오차)
    public float minRot = 5.0f;

    //타겟 인식 했는지 확인
    private bool isTarget = false;

    //타겟
    [SerializeField]
    private Transform targetEnemy = null;

    //각 스킬별 사용 여부
    private List<bool> isSkill = new List<bool>()
    {
        false, false, false, false
    };

    //각 스킬 이름
    private enum SkillName
    {
        cannon = 0,
        tracer = 1,
        fast = 2,
        shield = 3
    }

    //스킬 딜레이
    List<float> skillDelay = new List<float>()
    {
        0.2f, 10.0f, 5.0f, 3.0f
    };

    //스킬 마력 소비량
    List<int> skillConsume = new List<int>()
    {
        1,10,5,20
    };

    //스킬 레벨
    public List<int> skillLevel = new List<int>()
    {
        0, 0, 0, 0
    };

    //메시지 박스
    public MessageBox messageBox;
    bool isStop = false;
    MainControl mainControl;

    private void Start()
    {
        mainControl = GameObject.Find("MainControl").GetComponent<MainControl>(); 
        
        rb = this.GetComponent<Rigidbody>();
    }

    void Attack()
    {
        if(isTarget)
        {
            attackTimer += Time.deltaTime;
        }
    }

    void Update()
    {
        TurnMove();
        Attack();
        if(bohoTime > 0)
        {
            bohoTime -= Time.deltaTime;
        }
    }


    void TurnMove()
    {
        float move = Input.GetAxis("Vertical") *
            moveSpeed * Time.deltaTime;
        float turn = Input.GetAxis("Horizontal") *
            moveSpeed * Time.deltaTime;

        //충돌 없이
        //this.transform.Translate(0, 0, move);

        //충돌 가능
        rb.MovePosition(rb.position + 
            (this.transform.forward.normalized * move));
        this.transform.Rotate(0, turn, 0);

        //목표 찾기
        FindTarget();

        if(targetEnemy != null)
        {
            RotateTurret();
        }
    }

    void FindTarget()
    {
        Collider[] hitcolliders =
            Physics.OverlapSphere(this.transform.position,
            detectionRange, enemyLayer);
        if(hitcolliders.Length > 0)
        {
            float saveDistance = Mathf.Infinity;
            foreach(var hit in hitcolliders)
            {
                float distance = Vector3.Distance(
                    this.transform.position, hit.transform.position);
                if(distance < saveDistance)
                {
                    saveDistance = distance;
                    targetEnemy = hit.transform;
                }
            }
        }    
    }

    public void SetEx(int ex)
    {
        curEx += ex;
        mainControl.EXBar.value = (float)curEx / maxEx;

        if(curEx > maxEx)
        {
            level++;
            curEx = 0;
            maxEx = ExNum[level];
            Time.timeScale = 0;
            MaxHp = (int)(MaxHp + (MaxHp * 0.1f));
            HP = MaxHp;
            MaxMp = (int)(MaxMp + (MaxMp * 0.1f));
            MP = MaxMp;
            attackDamage = (int)(attackDamage +
                (attackDamage * 0.1f));

            mainControl.LvText.text = "LV." + level;
            mainControl.StopLvText.text = "LV." + level;
            mainControl.HPBar.value = 1;
            mainControl.MPBar.value = 1;
            mainControl.OpenStopUI();
        }
    }

    void RotateTurret()
    {
        Vector3 dir = targetEnemy.position - turret.position;
        dir.y = 0;
        Quaternion targetRot = Quaternion.LookRotation(dir);
        turret.rotation = Quaternion.Lerp(turret.rotation,
            targetRot, Time.deltaTime * rotationSpeed);
        if(Vector3.Angle(turret.forward, dir) < minRot)
        {
            isTarget = true;
            if(attackTimer >= attackDelay)
            {
                attackTimer = 0;
                Shoot();
            }
        }
        else
        {
            isTarget = false;
        }
    }

    void Shoot()
    {
        Vector3 dir = new Vector3(targetEnemy.position.x, 0, targetEnemy.position.z) -
                      new Vector3(firePoint.position.x, 0, firePoint.position.z);
        Quaternion targetRot = Quaternion.LookRotation(dir);
        Bullet bullet = Instantiate(bulletPrefab, firePoint.position, targetRot).GetComponent<Bullet>();
        bullet.damage = attackDamage;
    }

    public void UseSkill(int skillName)
    {
        if (skillLevel[skillName] > 0)
        {
            if (!isSkill[skillName])
            {
                if(MP >= skillConsume[skillName])
                {
                    isSkill[skillName] = true;
                    switch(skillName)
                    {
                        case (int)SkillName.cannon:
                            StartCoroutine("Cannon");
                            break;
                        case (int)SkillName.tracer:
                            StartCoroutine("Tracer");
                            break;
                        case (int)SkillName.fast:
                            StartCoroutine("Fast");
                            break;
                        case (int)SkillName.shield:
                            StartCoroutine("Shield");
                            break;
                    }
                }
                else
                {
                    messageBox.ShowMessageBox("마나가 부족합니다.");
                }
            }
            else
            {
                messageBox.ShowMessageBox("스킬을 아직 사용할 수 없습니다.");
            }
        }
        else
        {
            messageBox.ShowMessageBox("스킬을 아직 획득하지 않았습니다.");
        }
    }

    IEnumerator Cannon()
    {
        int skillNum = (int)SkillName.cannon;

        if (!targetEnemy)
        {
            messageBox.ShowMessageBox("공격할 수 있는 적이 없습니다.");
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                yield return new WaitForSeconds(0.2f);
                Vector3 dir = new Vector3(targetEnemy.position.x, 0, targetEnemy.position.z) -
                              new Vector3(firePoint.position.x, 0, firePoint.position.z);
                Quaternion targetRot = Quaternion.LookRotation(dir);
                Bullet bullet = Instantiate(cannonPrefab, firePoint.position, targetRot).GetComponent<Bullet>();
                bullet.damage = attackDamage / 3;
                MP = MP - skillConsume[skillNum];
                mainControl.MPBar.value = (float)MP / MaxHp;
            }
        }
        //스킬이 다시 상용될 수 있는 시간
        yield return new WaitForSeconds(skillDelay[skillNum]);
        isSkill[skillNum] = false;
    }

    IEnumerator Fast()
    {
        int skillNum = (int)SkillName.fast;

        //속도 증가
        moveSpeed *= 1.3f;
        MP = MP - skillConsume[skillNum];
        mainControl.MPBar.value = (float)MP / MaxMp;
        
        //스킬이 다시 상용될 수 있는 시간
        yield return new WaitForSeconds(skillDelay[skillNum]);

        //속도를 원래대로 변경
        moveSpeed = baseMoveSpeed;
        isSkill[skillNum] = false;
    }

    IEnumerator Shield()
    {
        int skillNum = (int)SkillName.shield;

        //보호막 생성(보호막 체력 더하기)
        bohoHp = MaxHp;
        bohoTime = 5.0f;
        MP = MP - skillConsume[skillNum];
        mainControl.MPBar.value = (float)MP / MaxMp;

        //스킬이 다시 상용될 수 있는 시간
        yield return new WaitForSeconds(skillDelay[skillNum]);

        //원래대로 변경
        isSkill[skillNum] = false;

        //보호막 제거(보호막 체력 빼가 단 0보다 적다면 0)
        bohoHp = 0;
    }
    IEnumerator Tracer()
    {
        int skillNum = (int)SkillName.tracer;

        //발사
        if (!targetEnemy)
        {
            messageBox.ShowMessageBox("공격할 수 있는 적이 없습니다.");
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
            Vector3 dir = new Vector3(targetEnemy.position.x, 0, targetEnemy.position.z) -
                          new Vector3(firePoint.position.x, 0, firePoint.position.z);
            Quaternion targetRot = Quaternion.LookRotation(dir);

            Bullet bullet = Instantiate(tracerPrefab, firePoint.position, tracerPrefab.transform.rotation)
                .GetComponent<Bullet>();
            bullet.damage = attackDamage;
            bullet.SetEnemy(targetEnemy);
            MP = MP - skillConsume[skillNum];
            mainControl.MPBar.value = (float)MP / MaxHp;
        }
        //스킬이 다시 상용될 수 있는 시간
        yield return new WaitForSeconds(skillDelay[skillNum]);
        isSkill[skillNum] = false;
    }

    public void SetHp(int damage)
    {
        if(bohoHp > 0)
        {
            bohoHp -= damage;
        }
        else
        {
            HP -= damage;
            

            if(HP <= 0)
            {
                mainControl.HPBar.value = 0;
                HP = 0;
                Time.timeScale = 0;
                mainControl.OpenMenuUI();
            }
        }
    }
}

