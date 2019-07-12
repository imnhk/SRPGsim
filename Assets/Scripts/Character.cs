using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Character : MonoBehaviour
{
    [SerializeField]
    protected Character attackTarget;

    [SerializeField]
    protected int moveDistance;
    protected Vector2 position;
    protected bool isAlive;

    [SerializeField]
    protected int hp;
    [SerializeField]
    protected int attackDmg;
    [SerializeField]
    protected int attackSpeed;
    protected float atkInterval;
    protected float atttackTimer = 0f;

    public int Hp
    {
        set { this.hp = value; }
    }
    public int Attack
    {
        set { this.attackDmg = value; }
    }
    public int AttackSpeed
    {
        set
        {
            this.attackSpeed = value;
            this.atkInterval = 100f / attackSpeed;
        }
    }
    public bool IsAlive
    {
        get { return this.isAlive; }
        set { isAlive = value; }
    }
    public Vector2 Position
    {
        get { return this.position; }
        set { position = value; }
    }
    public int MoveDist
    {
        get { return moveDistance; }
    }
    public virtual void MoveTo(Vector2 pos)
    {
        MapManager.Instance.field.Tile[(int)Position.x, (int)Position.y] = MapManager.TILE.EMPTY;
        this.Position = pos;
    }


    protected virtual void Die()
    {
        Debug.Log($"{this.gameObject.name} 가 사망하였습니다.");
        this.isAlive = false;
    }

    protected void Hit(int damage)
    {
        this.hp -= damage;
    }

    protected void AttackCharacter(Character target)
    {
        target.Hit(this.attackDmg);
        Debug.Log($"{this.gameObject.name}가 {target.gameObject.name}을 공격했습니다. {target.name}가 {this.attackDmg}의 피해를 입었습니다. {target.name} HP: {target.hp}");

    }
}