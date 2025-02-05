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

    //����
    public int MaxMp = 50;
    public int MP = 50;

    //�⺻ �̵� �ӵ�
    public float baseMoveSpeed = 5.0f;

    //���� �̵� �ӵ�
    public float moveSpeed = 5.0f;

    //ȸ���ӵ�
    public float rotationSpeed = 90.0f;

    //���� ���� �Ÿ�
    public float attackRange = 10.0f;

    //���ݰ� ������
    public float attackDelay = 1.0f;
    private float attackTimer = 0f;

    //���� �⺻ ���ݷ�
    public int attackDamage = 3;

    //��ũ �߻��
    public Transform turret;
    public Transform firePoint;
    public float turretTurnSpeed = 50.0f;
    public float turretPitchSpeed = 30.0f;

    //�Ѿ� ������
    public GameObject bulletPrefab;

    //����� ������
    public GameObject cannonPrefab;

    //����ź ������
    public GameObject tracerPrefab;

    //�� Ž�� �Ÿ�
    public float detectionRange = 10.0f;

    //�� ���̾�
    public LayerMask enemyLayer;

    //ȸ������ �� �������� �ν� ����(����)
    public float minRot = 5.0f;

    //Ÿ�� �ν� �ߴ��� Ȯ��
    private bool isTarget = false;

    //Ÿ��
    [SerializeField]
    private Transform targetEnemy = null;

    //�� ��ų�� ��� ����
    private List<bool> isSkill = new List<bool>()
    {
        false, false, false, false
    };

    //�� ��ų �̸�
    private enum SkillName
    {
        cannon = 0,
        tracer = 1,
        fast = 2,
        shield = 3
    }

    //��ų ������
    List<float> skillDelay = new List<float>()
    {
        0.2f, 10.0f, 5.0f, 3.0f
    };

    //��ų ���� �Һ�
    List<int> skillConsume = new List<int>()
    {
        1,10,5,20
    };

    //��ų ����
    public List<int> skillLevel = new List<int>()
    {
        0, 0, 0, 0
    };

    //�޽��� �ڽ�
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

        //�浹 ����
        //this.transform.Translate(0, 0, move);

        //�浹 ����
        rb.MovePosition(rb.position + 
            (this.transform.forward.normalized * move));
        this.transform.Rotate(0, turn, 0);

        //��ǥ ã��
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
                    messageBox.ShowMessageBox("������ �����մϴ�.");
                }
            }
            else
            {
                messageBox.ShowMessageBox("��ų�� ���� ����� �� �����ϴ�.");
            }
        }
        else
        {
            messageBox.ShowMessageBox("��ų�� ���� ȹ������ �ʾҽ��ϴ�.");
        }
    }

    IEnumerator Cannon()
    {
        int skillNum = (int)SkillName.cannon;

        if (!targetEnemy)
        {
            messageBox.ShowMessageBox("������ �� �ִ� ���� �����ϴ�.");
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
        //��ų�� �ٽ� ���� �� �ִ� �ð�
        yield return new WaitForSeconds(skillDelay[skillNum]);
        isSkill[skillNum] = false;
    }

    IEnumerator Fast()
    {
        int skillNum = (int)SkillName.fast;

        //�ӵ� ����
        moveSpeed *= 1.3f;
        MP = MP - skillConsume[skillNum];
        mainControl.MPBar.value = (float)MP / MaxMp;
        
        //��ų�� �ٽ� ���� �� �ִ� �ð�
        yield return new WaitForSeconds(skillDelay[skillNum]);

        //�ӵ��� ������� ����
        moveSpeed = baseMoveSpeed;
        isSkill[skillNum] = false;
    }

    IEnumerator Shield()
    {
        int skillNum = (int)SkillName.shield;

        //��ȣ�� ����(��ȣ�� ü�� ���ϱ�)
        bohoHp = MaxHp;
        bohoTime = 5.0f;
        MP = MP - skillConsume[skillNum];
        mainControl.MPBar.value = (float)MP / MaxMp;

        //��ų�� �ٽ� ���� �� �ִ� �ð�
        yield return new WaitForSeconds(skillDelay[skillNum]);

        //������� ����
        isSkill[skillNum] = false;

        //��ȣ�� ����(��ȣ�� ü�� ���� �� 0���� ���ٸ� 0)
        bohoHp = 0;
    }
    IEnumerator Tracer()
    {
        int skillNum = (int)SkillName.tracer;

        //�߻�
        if (!targetEnemy)
        {
            messageBox.ShowMessageBox("������ �� �ִ� ���� �����ϴ�.");
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
        //��ų�� �ٽ� ���� �� �ִ� �ð�
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

