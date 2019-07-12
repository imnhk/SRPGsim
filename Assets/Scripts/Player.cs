using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    private void Start()
    {
        isAlive = true;
        atkInterval = 100f / attackSpeed;
    }

    void Update()
    {
        if (!BattleSimulator.Instance.isBattleOn)
            return;

        if (this.hp <= 0)
        {
            this.Die();
            return;
        }

        this.atttackTimer += Time.deltaTime;
        if (this.atttackTimer > this.atkInterval)
        {
            this.attackTarget = BattleSimulator.Instance.TargetEnemy;
            this.AttackCharacter(this.attackTarget);
            this.atttackTimer = 0;
        }
    }

    protected override void Die()
    {
        base.Die();
        MapManager.Instance.field.Tile[(int)position.x, (int)position.y] = MapManager.TILE.EMPTY;
        MapManager.Instance.RefreshMap(MapManager.Instance.field);

    }

    public override void MoveTo(Vector2 pos)
    {
        // 원래 위치를 비우고
        MapManager.Instance.field.Tile[(int)Position.x, (int)Position.y] = MapManager.TILE.EMPTY;
        // 새 위치로 변경
        this.Position = pos;
        BattleSimulator.Instance.LeftTurns -= 1;
    }
}