using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Enemy : Character
{
    private void Start()
    {
        this.gameObject.SetActive(false);
        isAlive = true;
        atkInterval = 100f / attackSpeed;
    }
    void Update()
    {
        if (!BattleSimulator.Instance.isBattleOn || !this.IsAlive)
            return;

        if (this.hp <= 0)
        {
            this.Die();
            
            return;
        }

        this.atttackTimer += Time.deltaTime;
        if (this.atttackTimer > this.atkInterval)
        {
            this.AttackCharacter(this.attackTarget);
            this.atttackTimer = 0;
        }
    }

    protected override void Die()
    {
        base.Die();
        MapManager.Instance.field.Tile[(int)position.x, (int)position.y] = MapManager.TILE.EMPTY;
        MapManager.Instance.Enemies.Remove(this);
        MapManager.Instance.RefreshMap(MapManager.Instance.field);
    }
}
