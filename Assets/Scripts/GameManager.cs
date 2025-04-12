using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private List<Ball> balls = new List<Ball>();
    [SerializeField] private GameObject ballPrefab;

    [SerializeField] public Transform paddle;
    [SerializeField] private Transform ballSpawnPoint;
    private Paddle player;

    public BallPool ballPool = new BallPool(3);

    [Header("Game Settings")]
    [SerializeField] public float leftLimit = -14.8f;
    [SerializeField] public float rightLimit = 14.8f;
    [SerializeField] public float topLimit = 18.8f;
    [SerializeField] public float bottomLimit = -5f;

    [SerializeField] private float ballSpeed = 5f;
    [SerializeField] private float paddleSpeed = 5f;

    private float xDir = 0f;

    public void Awake()
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

        player = new Paddle(paddleSpeed, leftLimit, rightLimit);

        ballPool.InitializePool();

        SpawnBallOnPaddle();

        player.Awake(paddle);
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        for (int i = 0; i < balls.Count; i++)
        {
            if (balls[i] != null && balls[i].IsActive) balls[i].Update(deltaTime, paddle);
        }
        
        xDir = Input.GetAxisRaw("Horizontal");
        if (MathF.Abs(xDir) > 0.1)
        {
            player.Update(deltaTime, xDir);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            player.UnbindBall();
            balls[balls.Count-1].LaunchBall(ballSpeed, GenerateRandomLaunchDirection());
        }
    }

    private Vector3 GenerateRandomLaunchDirection()
    {
        Vector3 launchDirection = Vector3.zero;

        float xDir = UnityEngine.Random.Range(-0.75f, 0.75f);

        launchDirection = new Vector3(xDir, 1, 0);

        return launchDirection;
    }

    public void DestroyBall(Ball ball, Transform transform)
    {
        Destroy(transform.gameObject);
        balls.Remove(ball);
        SpawnBallOnPaddle();
    }

    public void SpawnBallOnPaddle()
    {
        Ball ball = ballPool.GetBall();
        ball.Awake();
        Transform ballPos = ball.GetTransform();
        ballPos.position = ballSpawnPoint.position;
        balls.Add(ball);
        player.SetBall(ballPos);
    }

    public GameObject SpawnBall()
    {
        GameObject ballGameObject = Instantiate(ballPrefab, ballSpawnPoint.position, Quaternion.identity);
        return ballGameObject;
    }

    public void ReturnBallToPool(Ball ball)
    {
        ballPool.ReturnBall(ball);
        balls.Remove(ball);
        SpawnBallOnPaddle();
    }

    public void DisableBall(Transform ball)
    {
        ball.gameObject.SetActive(false);
    }
}
