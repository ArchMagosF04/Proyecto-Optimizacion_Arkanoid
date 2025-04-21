using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class UpdateManager : MonoBehaviour
{
    public static UpdateManager Instance;

    [Header("Game Components")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private GameObject brickPrefab;

    [Space(10)]

    [SerializeField] private Transform rightWall;
    [SerializeField] private Transform leftWall;
    [SerializeField] private Transform topWall;
    [SerializeField] private Transform bottomWall;

    [Space(10)]

    [SerializeField] private Transform paddle;
    [SerializeField] private Transform ballSpawnPoint;
    [SerializeField] private Transform brickSpawnPoint;

    [Header("Game Variables")]
    [SerializeField] private float paddleSpeed = 10f;

    [SerializeField] private float ballSpeed = 20f;

    [SerializeField] private int ballLives = 5;

    //Game-field Limits
    private float rightLimit;
    private float leftLimit;
    private float topLimit;
    private float bottomLimit;

    //Input variables
    private float xMoveInput;

    //Class references
    private PlayerPaddle player;
    private GameBall ballToLaunch;
    private Pool pool;
    private List<Brick> brickList = new List<Brick>();

    //Game Status
    private bool ballIsActive;
    private int ballsUsed;

    //Other
    private float deltaTime;

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
        SpawnBricks(4, 7, 1.5f, 4f);
        GetBallToLaunch();
    }

    private void Update()
    {
        
        deltaTime = Time.deltaTime;

        Inputs();

        player.Update(deltaTime, xMoveInput);

        for (int i = 0; i < pool.ballsInUse.Count; i++)
        {
            pool.ballsInUse[i].Update(deltaTime, brickList);
        }
    }

    private void Inputs()
    {
        xMoveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && !ballIsActive)
        {
            LaunchBall();
            ballIsActive = true;
        }
    }

    private void LaunchBall()
    {
        Vector3 launchDirection = Vector3.zero;

        float xDir = Random.Range(-0.75f, 0.75f);

        launchDirection = new Vector3(xDir, 1, 0);

        ballToLaunch.LauchBall(launchDirection);
    }

    private void InitializeGame()
    {
        ballIsActive = false;
        pool = new Pool(5);
        pool.Initialize();
    }

    private void GetBallToLaunch()
    {
        ballToLaunch = pool.GetBall();
        ballToLaunch.Initialize(ballSpawnPoint);
    }

    public GameBall SpawnBall()
    {
        GameObject newBall = Instantiate(ballPrefab, ballSpawnPoint.position, Quaternion.identity);
        GameBall ball = new GameBall(newBall.transform, ballSpeed, leftLimit, rightLimit, topLimit, bottomLimit, player);

        return ball;
    }

    private void SetGameBoundaries()
    {
        rightLimit = rightWall.position.x - rightWall.lossyScale.x / 2;
        leftLimit = leftWall.position.x + leftWall.lossyScale.x / 2;
        topLimit = topWall.position.y - topWall.lossyScale.y / 2;
        bottomLimit = bottomWall.position.y + bottomWall.lossyScale.y / 2;
    }

    private void SetPlayer()
    {
        player = new PlayerPaddle(paddle, paddleSpeed, leftLimit, rightLimit);
    }

    private void SpawnBricks(int rows, int colums, float rowSeparation, float columnSeparation)
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < colums; j++)
            {
                GameObject newBrick = Instantiate(brickPrefab, brickSpawnPoint.position, Quaternion.identity);
                newBrick.transform.position += new Vector3(columnSeparation * j, - rowSeparation * i, 0);

                Brick brick = new Brick(newBrick.transform);
                brickList.Add(brick);
            }
        }
    }

    public void OnBrickDestruction(Brick brick)
    {
        Destroy(brick.transform.gameObject);
        brickList.Remove(brick);
    }

    public void OnBallDeath(GameBall ball)
    {
        ballsUsed++;

        pool.ReturnBall(ball);
        if (pool.ballsInUse.Count == 0)
        {
            ballIsActive = false;
        }

        if (!ballIsActive)
        {
            GetBallToLaunch();
        }
    }
}
