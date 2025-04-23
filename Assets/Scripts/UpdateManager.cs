using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class UpdateManager : MonoBehaviour
{
    public static UpdateManager Instance; //instance of Manager that can be called from other scrits

    [Header("Game Components")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private GameObject brickPrefab;
    [SerializeField] private GameObject multiballPrefab;

    [Space(10)]
    //Game boundaries
    [SerializeField] private Transform rightWall;
    [SerializeField] private Transform leftWall;
    [SerializeField] private Transform topWall;
    [SerializeField] private Transform bottomWall;

    [Space(10)]
    
    [SerializeField] private Transform paddle;
    [SerializeField] private Transform ballSpawnPoint;
    [SerializeField] private Transform brickSpawnPoint;

    [SerializeField] private Material[] brickColors;
    
    [Header("Game Variables")]
    [SerializeField] private float paddleSpeed = 10f;

    [SerializeField] private float ballSpeed = 20f;

    [SerializeField] private int ballLives = 5;

    //Game-field Limits
    public float rightLimit {  get; private set; }
    public float leftLimit {  get; private set; }
    public float topLimit { get; private set; }
    public float bottomLimit { get; private set; }

    //Input variables
    private float xMoveInput;

    //Class references
    private PlayerPaddle player;
    private Ball ballToLaunch;
    private Pool pool;
    private List<Brick> brickList = new List<Brick>();
    public List<IPowerUp> powerUpList = new List<IPowerUp>();

    //Game Status
    private bool ballIsActive;
    private int ballsUsed;

    //Other
    private float deltaTime;

    PU_Multiball powerup;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        SetGameBoundaries();
        SetPlayer();
        InitializeGame();
        SpawnBricks(4, 7, 1.2f, 3.2f);
        GetBallToLaunch();

        StartCoroutine(GameUpdate());
    }

    //private void Update()
    //{
    //    deltaTime = Time.deltaTime;

    //    Inputs();

    //    player.Update(deltaTime, xMoveInput);

    //    for (int i = 0; i < pool.ballsInUse.Count; i++)
    //    {
    //        pool.ballsInUse[i].Update(deltaTime, brickList);
    //    }

    //    for (int i = 0; i < powerUpList.Count; i++)
    //    {
    //        if (powerUpList[i] != null) powerUpList[i].Update(deltaTime, paddle);
    //    }
    //}

    private IEnumerator GameUpdate()
    {
        deltaTime = Time.deltaTime;

        Inputs();

        player.Update(deltaTime, xMoveInput);

        for (int i = 0; i < pool.ballsInUse.Count; i++)
        {
            pool.ballsInUse[i].Update(deltaTime, brickList);
        }

        for (int i = 0; i < powerUpList.Count; i++)
        {
            if (powerUpList[i] != null) powerUpList[i].Update(deltaTime, paddle);
        }

        yield return null;

        StartCoroutine(GameUpdate());
    }

    private void Inputs() //detectes inputs
    {
        xMoveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && !ballIsActive)
        {
            LaunchBall();
            ballIsActive = true;
        }
    }

    public void LaunchBall() //causes the ball to start moving with a random direction upward
    {
        Vector3 launchDirection = Vector3.zero;

        float xDir = Random.Range(0.25f, 0.6f);

        if (Random.value < 0.5f)
        {
            xDir *= -1;
        }

        launchDirection = new Vector3(xDir, 1, 0);

        ballToLaunch.LauchBall(launchDirection);
    }

    private void InitializeGame() //set somethings at the start of the game
    {
        ballIsActive = false;
        pool = new Pool(5);
        pool.Initialize();
    }

    public void GetBallToLaunch() //get a ball from the pool and set in the padel
    {
        ballToLaunch = pool.GetBall();
        ballToLaunch.Initialize(ballSpawnPoint);
    }

    public Ball SpawnBall() //Instantiates a new ball
    {
        GameObject newBall = Instantiate(ballPrefab, ballSpawnPoint.position, Quaternion.identity);
        Ball ball = new Ball(newBall.transform, ballSpeed, leftLimit, rightLimit, topLimit, bottomLimit, player);

        return ball;
    }

    public GameObject SpawnPowerUp(Vector3 spawnPoint)
    {
        GameObject powerUp = Instantiate(multiballPrefab);
        powerUp.transform.position = spawnPoint;
        return powerUp;
    }

    private void SetGameBoundaries() //calculates the cordinates of the game boundaries
    {
        rightLimit = rightWall.position.x - rightWall.lossyScale.x / 2;
        leftLimit = leftWall.position.x + leftWall.lossyScale.x / 2;
        topLimit = topWall.position.y - topWall.lossyScale.y / 2;
        bottomLimit = bottomWall.position.y + bottomWall.lossyScale.y / 2;
    }

    private void SetPlayer()//Create the player paddle
    {
        player = new PlayerPaddle(paddle, paddleSpeed, leftLimit, rightLimit);
    }

    private Vector2[] SetPowerUpCoordinates(int rows, int colums)
    {
        Vector2[] coordinates = new Vector2[4];

        List<int> availableRows = new List<int>();
        List<int> availableColums = new List<int>();

        for (int i = 0; i < rows; i++)
        {
            availableRows.Add(i);
        }
        for (int i = 0; i < colums; i++)
        {
            availableColums.Add(i);
        }

        for (int i = 0; i < 4; i++)
        {
            Vector2 pwLocation = new Vector2(Random.Range(0, availableRows.Count), Random.Range(0, availableColums.Count));
            availableRows.RemoveAt((int)pwLocation.x);
            availableColums.RemoveAt((int)pwLocation.y);
            coordinates[i] = pwLocation;
        }

        return coordinates;
    }

    private void SpawnBricks(int rows, int colums, float rowSeparation, float columnSeparation) //Spawns the bricks across the screen
    {
        Vector2[] powerUpsPos = SetPowerUpCoordinates(rows, colums);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < colums; j++)
            {
                GameObject newBrick = Instantiate(brickPrefab, brickSpawnPoint.position, Quaternion.identity);
                newBrick.transform.position += new Vector3(columnSeparation * j, - rowSeparation * i, 0);

                MeshRenderer mesh = newBrick.GetComponent<MeshRenderer>();
                mesh.material = brickColors[j];

                Brick brick = null;


                if (PowerUpCheck(powerUpsPos, i, j))
                {
                    brick = new Brick(newBrick.transform, Brick.PowerUpType.MultiBall);
                }
                else
                {
                    brick = new Brick(newBrick.transform, Brick.PowerUpType.None);
                }

                brickList.Add(brick);
            }
        }
    }

    private bool PowerUpCheck(Vector2[] powerUpsPos, int posX, int posY)
    {
        for (int i = 0; i < powerUpsPos.Length; i++)
        {
            if (posX == (int)powerUpsPos[i].x && posY == (int)powerUpsPos[i].y)
            {
                return true;
            }
        }

        return false;
    }

    public void OnBrickDestruction(Brick brick) //Destroy the given brick
    {
        Destroy(brick.transform.gameObject);
        brickList.Remove(brick);

        if (brickList.Count == 0)
        {
            WinGame();
        }
    }

    public void OnBallDeath(Ball ball)//Gets called when a ball exits the screen
    {
        pool.ReturnBall(ball);
        if (pool.ballsInUse.Count == 0)
        {
            ballIsActive = false;

            for (int i = 0; i < powerUpList.Count; i++)
            {
                powerUpList[i].DestroyPowerUp();
            }
        }

        if (!ballIsActive)
        {
            ballsUsed++;
            if (ballsUsed < ballLives) //If the player has remaining lives then a new ball spawns ready to be launched
            {
                GetBallToLaunch();
            }
            else // If the player runs out of lives then the game ends.
            {
                LoseGame();
            }
        }
    }

    public void DestroyGameObject(GameObject go)
    {
        Destroy(go);
    }

    private void WinGame()
    {
        Debug.Log("YOU WIN");
    }

    private void LoseGame()
    {
        Debug.Log("YOU LOSE");
    }
}
