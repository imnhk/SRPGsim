using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleSimulator : MonoBehaviour
{
    // Singleton 패턴.
    private static BattleSimulator _instance;

    public static BattleSimulator Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        if ((_instance != null) && (_instance != this))
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public bool isBattleOn
    {
        get; set;
    }

    private Player player = null;
    private List<Enemy> enemies = new List<Enemy>();
    public Player Player
    {
        get { return player; }
        set { player = value; }
    }
    public List<Enemy> Enemies
    {
        get { return enemies; }
    }

    [SerializeField]
    private InputField LeftTurnInputs = null;
    public int LeftTurns
    {
        get { return int.Parse(LeftTurnInputs.text); }
        set { LeftTurnInputs.text = value.ToString(); }
    }

    // 플레이어의 공격 우선순위 설정.
    public Enemy TargetEnemy
    {
        get
        {
            foreach(var enemy in enemies)
            { 
                if (enemy.IsAlive)
                    return enemy;
            }
            Debug.LogError("No available target");
            return null;
        }
    }

    void Start()
    {
        isBattleOn = false;
    }

    void Update()
    {
        if (isBattleOn)
        {
            if (!this.player.IsAlive)
            {
                Debug.Log("플레이어 사망, 전투 종료");
                this.StopBattle();
            }

            if (enemies.TrueForAll(enemy => enemy.IsAlive == false))
            {
                Debug.Log("적 모두 사망, 전투 승리");
                this.StopBattle();
            }

        }
    }

    // Builder 패턴. 구조 변경 후 사용하지 않음.
   
    public class CharacterBuilder
    {
        private int hp;
        private int attack;
        private int attackSpeed;

        public CharacterBuilder SetHp(int hp)
        {
            this.hp = hp;
            return this;
        }
        public CharacterBuilder SetAttack(int attack)
        {
            this.attack = attack;
            return this;
        }
        public CharacterBuilder SetAttackSpeed(int attackSpeed)
        {
            this.attackSpeed = attackSpeed;
            return this;
        }

        public void Build(Character character)
        {
            character.Hp = this.hp;
            character.Attack = this.attack;
            character.AttackSpeed = this.attackSpeed;
            character.IsAlive = true;
        }
    }
    
    public void StartBattle()
    {
        isBattleOn = true;
        foreach (var enemy in enemies)
            enemy.gameObject.SetActive(true);
    }

    private void StopBattle()
    {
        isBattleOn = false;
        foreach (var enemy in enemies)
            enemy.gameObject.SetActive(false);
    }

    private bool IsThereEnemies
    {
        get
        {
            
            foreach (Enemy e in this.enemies)
            {
                if (e.IsAlive)
                    return true;
            }
            return false;
        }
    }
}
