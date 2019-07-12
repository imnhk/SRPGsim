using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSelector : MonoBehaviour
{
    private MapManager map = MapManager.Instance;
    public Vector2 Position
    {
        get;
        set;
    }

    private void OnMouseDown()
    {
        map.Player.MoveTo(this.Position);
        map.RefreshMap(map.field);

        Vector2 nextPos;
        foreach (var enemy in map.Enemies)
        {
            // Debug.Log(enemy.MoveDist);
            // 찾은 경로의 (MoveDist) 번째 좌표
            nextPos = map.pathFinder.FindPath(enemy.Position, map.Player.Position)[enemy.MoveDist - 1];
            if(map.field.Tile[(int)nextPos.x, (int)nextPos.y]!=MapManager.TILE.ENEMY)
                enemy.MoveTo(nextPos);
            map.RefreshMap(map.field);
        }

        map.CheckCharactersPosition();


    }

}
