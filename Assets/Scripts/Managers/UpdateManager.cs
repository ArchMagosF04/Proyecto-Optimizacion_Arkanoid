using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class UpdateManager : MonoBehaviour
{
    #region Variables

    public static UpdateManager Instance; //instance of Manager that can be called from other scrits

    public enum GameStates { MainMenu, Game, Win, Lose }
    public GameStates CurrentGameState = GameStates.MainMenu;

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
    //Game Screens
    [SerializeField] private GameObject mainMenuScreen;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;

    [Space(10)]

    [SerializeField] private Transform paddle;
    [SerializeField] private Transform ballSpawnPoint;
    [SerializeField] private Transform brickSpawnPoint;

    [SerializeField] private Material[] brickColors;
    
    [Header("Game Variables")]
    [SerializeField] private float paddleSpeed = 10f;

    [SerializeField] private float ballSpeed = 20f;

    [SerializeField] private int ballLives = 5;

    [Space(10)]

    [SerializeField, Range(1, 6)] private int brickRows = 4;
    private int brickColums = 7;

    //Game-Area Limits
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
    private List<Brick> activeBricksList = new List<Brick>();
    private Brick[,] brickGrid;
    public List<IPowerUp> powerUpList = new List<IPowerUp>();

    //Game Status
    private bool ballIsActive;
    private int ballsUsed;

    //Other
    private float deltaTime;

    #endregion
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

        ChangeGameState(GameStates.MainMenu);

        SetGameBoundaries();
        SetPlayer();
        SpawnBricks(1.2f, 3.2f);

        StartCoroutine(OnUpdate());
    }

    #region Update
    private IEnumerator OnUpdate()
    {
        deltaTime = Time.deltaTime;

        switch (CurrentGameState)
        {
            case GameStates.MainMenu:
                MenuUpdate();
                break;
            case GameStates.Game:
                GameUpdate(deltaTime);
                break;
            case GameStates.Win:
                WinUpdate();
                break;
            case GameStates.Lose:
                LoseUpdate();
                break;
        }

        //if (pool != null) Debug.Log(pool.ballsInUse.Count);

        yield return null;

        StartCoroutine(OnUpdate());
    }

    private void MenuUpdate()
    {
        MenuInputs();
    }  
    
    private void MenuInputs()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeGameState(GameStates.Game);
        }
    }

    private void GameUpdate(float deltaTime)
    {
        GameInputs();

        player.Update(deltaTime, xMoveInput);

        for (int i = 0; i < pool.ballsInUse.Count; i++)
        {
            pool.ballsInUse[i].Update(deltaTime, activeBricksList);
        }

        for (int i = 0; i < powerUpList.Count; i++)
        {
            if (powerUpList[i] != null) powerUpList[i].Update(deltaTime, paddle);
        }
    }
    private void GameInputs() //detectes inputs
    {
        xMoveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && !ballIsActive)
        {
            LaunchBall();
            ballIsActive = true;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ChangeGameState(GameStates.MainMenu);
        }
    }

    private void WinUpdate()
    {
        EndScreensInput();
    }

    private void LoseUpdate()
    {
        EndScreensInput();
    }

    private void EndScreensInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ChangeGameState(GameStates.MainMenu);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeGameState(GameStates.Game);
        }
    }

    #endregion

    #region OnStartGame
    private void InitializeGame() //set somethings at the start of the game
    {
        ballIsActive = false;
        ballsUsed = 0;
        paddle.transform.position = new Vector3(0f, paddle.transform.position.y, 0f);

        if (pool == null)
        {
            pool = new Pool(5);
            pool.Initialize();
        }
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

    private void SpawnBricks(float rowSeparation, float columnSeparation)
    {
        brickGrid = new Brick[brickRows, brickColums];

        for (int i = 0; i < brickRows; i++)
        {
            for (int j = 0; j < brickColums; j++)
            {
                GameObject newBrick = Instantiate(brickPrefab, brickSpawnPoint.position, Quaternion.identity);
                newBrick.transform.position += new Vector3(columnSeparation * j, -rowSeparation * i, 0);

                MeshRenderer mesh = newBrick.GetComponent<MeshRenderer>();
                mesh.material = brickColors[j];

                Brick brick = new Brick(newBrick.transform);

                brickGrid[i, j] = brick;
            }
        }
    }


    #endregion

    #region Ball
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

    public void OnBallDeath(Ball ball)//Gets called when a ball exits the screen
    {
        pool.ReturnBall(ball);
        if (pool.ballsInUse.Count == 0)
        {
            ballIsActive = false;

            for (int i = 0; i < powerUpList.Count; i++)
            {
                powerUpList[0].DestroyPowerUp();
            }
        }

        if (!ballIsActive && CurrentGameState == GameStates.Game)
        {
            ballsUsed++;
            Debug.Log(ballsUsed);
            if (ballsUsed < ballLives) //If the player has remaining lives then a new ball spawns ready to be launched
            {
                GetBallToLaunch();
            }
            else // If the player runs out of lives then the game ends.
            {
                ChangeGameState(GameStates.Lose);
            }
        }
    }

    #endregion

    #region Bricks & PowerUps
    public GameObject SpawnPowerUp(Vector3 spawnPoint)
    {
        GameObject powerUp = Instantiate(multiballPrefab);
        powerUp.transform.position = spawnPoint;
        return powerUp;
    }

    private Vector2[] SetPowerUpCoordinates(int rows, int colums)
    {
        Vector2[] coordinates = new Vector2[rows];

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

        for (int i = 0; i < rows; i++)
        {
            Vector2 pwLocation = new Vector2(Random.Range(0, availableRows.Count), Random.Range(0, availableColums.Count));
            availableRows.RemoveAt((int)pwLocation.x);
            availableColums.RemoveAt((int)pwLocation.y);
            coordinates[i] = pwLocation;

            Debug.Log(pwLocation);
        }

        return coordinates;
    }

    private void InitializeBricks() //Spawns the bricks across the screen
    {
        Vector2[] powerUpsPos = SetPowerUpCoordinates(brickRows, brickColums);

        for (int i = 0; i < brickRows; i++)
        {
            for (int j = 0; j < brickColums; j++)
            {
                Brick brick = brickGrid[i,j];

                brick.transform.gameObject.SetActive(true);

                if (PowerUpCheck(powerUpsPos, i, j))
                {
                    brick.SetPowerUp(Brick.PowerUpType.MultiBall);
                }
                else
                {
                    brick.SetPowerUp(Brick.PowerUpType.None);
                }

                activeBricksList.Add(brick);
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
        brick.transform.gameObject.SetActive(false);
        activeBricksList.Remove(brick);

        if (activeBricksList.Count == 0)
        {
            ChangeGameState(GameStates.Win);
        }
    }

    #endregion

    #region ChangeGameState

    private void GoToMenu()
    {

    }

    private void StartGame()
    {
        InitializeGame();
        InitializeBricks();
        GetBallToLaunch();
    }
    private void WinGame()
    {
        pool.ReturnAll();

        Debug.Log("YOU WIN");
    }
    private void LoseGame()
    {
        Debug.Log("YOU LOSE");
    }

    private void ChangeGameState(GameStates state)
    {
        mainMenuScreen.SetActive(false);
        winScreen.SetActive(false);
        loseScreen.SetActive(false);

        CurrentGameState = state;

        switch (state)
        {
            case GameStates.MainMenu:
                mainMenuScreen.SetActive(true);
                GoToMenu();
                break;
            case GameStates.Game:
                StartGame();
                break;
            case GameStates.Win:
                winScreen.SetActive(true);
                WinGame();
                break;
            case GameStates.Lose:
                loseScreen.SetActive(true);
                LoseGame();
                break;
        }
    }

    #endregion
    public void DestroyGameObject(GameObject go)
    {
        Destroy(go);
    }

}
