using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{ 

    // Singleton 패턴.
    private static MapManager _instance;

    public static MapManager Instance
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

    [SerializeField]
    private Player player = null;

    [SerializeField]
    private List<Enemy> enemies = new List<Enemy>();
    public List<Enemy> Enemies
    {
        get { return enemies; }
    }

    public const int ENEMY_COUNT = 3;

    [SerializeField]
    private InputField MapHeightInput = null, MapWidthInput = null;
    [SerializeField]
    private InputField WallCountInput = null;
    [SerializeField]
    private Sprite tileSprite = null;
    [SerializeField]
    private TileSelector tileSelector = null;

    private const int MAP_MAX_ROW = 16, MAP_MAX_COL = 16;
    private GameObject mapTiles;
    private GameObject uiTiles;

    public Map field;

    public enum TILE { NULL, EMPTY, WALL, ENEMY, PLAYER }

    public class Map
    {
        private TILE[,] map = new TILE[MAP_MAX_ROW, MAP_MAX_COL];
        private int row, col;

        public int Rows
        {
            get { return row; }
        }
        public int Cols
        {
            get { return col; }
        }
        public TILE[,] Tile
        {
            get { return map; }
        }
        
        public TILE getTile(Vector2 pos)
        {
            return map[(int)pos.x, (int)pos.y];
        }

        public Map(int row, int col)
        {
            if( row > MAP_MAX_ROW || col > MAP_MAX_COL)
                return;

            this.row = row;
            this.col = col;
            for (var i = 0; i < row; i++)
                for (var j = 0; j < col; j++)
                    map[i, j] = TILE.EMPTY;
        }
        
        // 특정한 타입의 타일 중 무작위로 하나를 고른다.
        // 적과 플레이어, 벽을 생성할 때 사용.
        public Vector2 GetRandomTilePos(TILE type)
        {
            List<Vector2> posArray = new List<Vector2>();
            for (var i = 0; i < row; i++)
            {
                for (var j = 0; j < col; j++)
                {
                    if (map[i, j] == type)
                    {
                        posArray.Add(new Vector2(i, j));
                    }
                }
            }
            int randPos = Random.Range(0, posArray.Count);
            return posArray[randPos];
        }
 
        public void Clear()
        {
            for (var i = 0; i < MAP_MAX_ROW; i++)
                for (var j = 0; j < MAP_MAX_COL; j++)
                    map[i, j] = TILE.NULL;
        }
        public void Empty()
        {
            for (var i = 0; i < row; i++)
                for (var j = 0; j < col; j++)
                    map[i, j] = TILE.EMPTY;
        }
    }

    public Player Player
    {
        get { return player; }
        set { player = value; }
    }
       
    public void CreateMap()
    {
        var row = int.Parse(MapHeightInput.text);
        var col = int.Parse(MapWidthInput.text);

        // 그린 맵 지우기
        EraseMap(field);

        // 비어 있는 맵 생성
        field = new Map(row, col);
        field.Empty();

        // 통과할 수 없는 벽 생성
        PlaceWall(int.Parse(WallCountInput.text), field);

        // Player, Enemy 초기 위치 설정
        player.Position = field.GetRandomTilePos(TILE.EMPTY);
        foreach(Enemy e in enemies)
        {
            Vector2 enemyPos = field.GetRandomTilePos(TILE.EMPTY);
            e.Position = enemyPos;
        }

        // Gameobject로 맵 그리기
        DrawMap(field);

        // Map 위에 UI 그리기
        DrawTileSelector();
    }

    private void EraseMap(Map field)
    {
        if (field != null)
        {
            field.Clear();
            Destroy(mapTiles);
        }
    }

    public void RefreshMap(Map field)
    {
        Destroy(mapTiles);
        DrawMap(field);
        DrawTileSelector();
    }

    private void PlaceWall(int count, Map field)
    {
        Vector2 newWallPos;
        for (var i = 0; i < count; i++)
        {
            newWallPos = field.GetRandomTilePos(TILE.EMPTY);
            field.Tile[(int)newWallPos.x, (int)newWallPos.y] = TILE.WALL;
        }
    }


    // A* Search Algorithm.
    public PathFinder pathFinder = new PathFinder();

    public class PathFinder
    {
        class Node
        {
            private Map field = MapManager.Instance.field;
            public Node parent;

            public int g;
            public int h;
            public int f
            {
                get { return g + h; }
            }
            public bool Walkable
            {
                get
                {
                    switch(field.Tile[(int)pos.x, (int)pos.y])
                    {
                        case TILE.EMPTY:
                        case TILE.PLAYER:
                        case TILE.ENEMY:
                            return true;

                        default:
                            return false;
                    }
                }
            }
            public Vector2 pos;

            public Node(Vector2 pos)
            {
                this.pos = pos;
            }

            public static int GetDistance(Node a, Node b)
            {
                int distX = Mathf.Abs((int)a.pos.x - (int)b.pos.x);
                int distY = Mathf.Abs((int)a.pos.y - (int)b.pos.y);

                /*
                if (distX > distY)
                    return 14 * distY + 10 * (distX - distY);
                return 14 * distX + 10 * (distY - distX);
                */
                return distX + distY;
            }

            public List<Node> GetNeighbors()
            {
                List<Node> neighbors = new List<Node>();
                for(int x=-1; x<=1; x++)
                {
                    for(int y=-1; y<=1; y++)
                    {
                        if (Mathf.Abs(x + y) != 1)
                            continue;

                        int checkX = (int)pos.x + x;
                        int checkY = (int)pos.y + y;

                        if (checkX >= 0 && checkY >= 0 && checkX < field.Rows && checkY < field.Cols)
                            neighbors.Add(new Node(new Vector2(checkX, checkY)));

                    }
                }
                return neighbors;
            }
        }

        public List<Vector2> FindPath(Vector2 startPos, Vector2 targetPos)
        {
            Node startNode = new Node(startPos);
            Node targetNode = new Node(targetPos);

            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(startNode);

            // 무한 루프 방지.
            int temp = 0;
            while (openSet.Count > 0)
            {
                Node node = openSet[0];
                for (var i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].f <= node.f)
                        if (openSet[i].h < node.h)
                            node = openSet[i];
                }

                openSet.Remove(node);
                closedSet.Add(node);

                if (node.pos == targetNode.pos)
                {
                    targetNode.parent = node.parent;
                    return RetracePath(startNode, targetNode);
                }

                foreach (Node neighbor in node.GetNeighbors())
                {
                    if (!neighbor.Walkable || closedSet.Contains(neighbor))
                        continue;

                    int newCostToNeighbor = node.g + Node.GetDistance(node, neighbor);
                    if (newCostToNeighbor < neighbor.g || !openSet.Contains(neighbor))
                    {
                        neighbor.g = newCostToNeighbor;
                        neighbor.h = Node.GetDistance(neighbor, targetNode);
                        neighbor.parent = node;
                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }

                }
                // 무한 루프 방지.
                temp++;
                if (temp >= 300)
                {
                    break;
                }
            }
            Debug.Log("Can't find Path in A* pathfinder here");
            // 경로를 못 찾으면 제자리에.
            return new List<Vector2>() { startPos };
        }

        List<Vector2> RetracePath(Node startNode, Node endNode)
        {
            List<Vector2> path = new List<Vector2>();
            Node currentNode = endNode;
            while (currentNode.pos != startNode.pos)
            {
                path.Add(currentNode.pos);
                currentNode = currentNode.parent;
            }
            path.Reverse();
            return path;
        }
    }

    public void CheckCharactersPosition()
    {
        // 여기서 Player, Enemy가 같은 위치에 있는 지 확인.
        for (var i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].Position == player.Position)
            {
                Debug.Log($"Battle at {enemies[i].Position} against {enemies[i].name} ");
                BattleSimulator.Instance.Player = this.player;
                BattleSimulator.Instance.Enemies.Add(enemies[i]);
                BattleSimulator.Instance.StartBattle();
            }
        }
    }

    private void DrawMap(Map field)
    {
        mapTiles = new GameObject("MapTiles");

        DrawCharacter(player);
        field.Tile[(int)player.Position.x, (int)player.Position.y] = TILE.PLAYER;
        foreach (Enemy e in enemies)
        {
            field.Tile[(int)e.Position.x, (int)e.Position.y] = TILE.ENEMY;
            DrawCharacter(e);
        }

        for (var i = 0; i < field.Rows; i++)
            for (var j = 0; j < field.Cols; j++)
                DrawTile(new Vector2 ( i, j ), field.Tile[i, j]);
    }

    private void DrawTile(Vector2 pos, TILE type)
    {
        if (type == TILE.NULL)
            return;

        GameObject tile = new GameObject($"Tile {pos.ToString()}");
        var spriteRenderer = tile.AddComponent<SpriteRenderer>();

        tile.transform.position = new Vector3(pos.x, pos.y, 5);
        tile.transform.localScale = new Vector3(0.9f, 0.9f);

        switch (type)
        {
            case TILE.EMPTY:
            case TILE.ENEMY:
            case TILE.PLAYER:
                spriteRenderer.sprite = tileSprite;
                spriteRenderer.color = new Color(1, 1, 1);
                tile.transform.parent = mapTiles.transform;
                break;

            case TILE.WALL:
                spriteRenderer.sprite = tileSprite;
                spriteRenderer.color = new Color(0.1f, 0.1f, 0.1f);
                tile.transform.parent = mapTiles.transform;
                break;

            default:
                break;
        }
    }

    private void DrawCharacter(Character character)
    {
        GameObject tile = new GameObject($"Tile ({character.name})");
        var spriteRenderer = tile.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = tileSprite;

        tile.transform.position = character.Position;
        tile.transform.localScale = new Vector3(0.8f, 0.8f);
        tile.transform.parent = mapTiles.transform;

        if (character is Player)
            spriteRenderer.color = new Color(0, 1, 0);
        else if (character is Enemy)
            spriteRenderer.color = new Color(1, 0, 0);
    }

    // 플레이어가 이동 가능한 타일을 표시한다.
    private void DrawTileSelector()
    {
        if (uiTiles != null)
            Destroy(uiTiles);

        uiTiles = new GameObject("UITiles");

        TileSelector selector;
        Vector2 playerPos = player.Position;
        int dist;

        for (var i = 0; i < field.Rows; i++)
        {
            for (var j = 0; j < field.Cols; j++)
            {
                dist = (int)(Mathf.Abs(playerPos.x - i) + Mathf.Abs(playerPos.y - j));
                if (this.player.MoveDist >= dist && field.Tile[i, j] == TILE.EMPTY)
                {

                    selector = Instantiate<TileSelector>(tileSelector, new Vector3(i, j, -5f), Quaternion.identity);
                    selector.transform.parent = uiTiles.transform;
                    selector.Position = (new Vector2(i, j));
                }
            }
        }
    }
}
