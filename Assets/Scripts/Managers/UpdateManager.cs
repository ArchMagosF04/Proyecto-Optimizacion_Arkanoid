using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.U2D;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

public class UpdateManager : MonoBehaviour
{
    #region Variables

    public static UpdateManager Instance; //Instance of the Manager that can be called from other scripts.

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
    [SerializeField] private Transform backgroundWall;

    [Space(10)]
    //Game UI
    [SerializeField] private GameObject mainMenuScreen;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;

    [SerializeField] private GameObject[] livesCounter;

    [Space(10)]

    [SerializeField] private Transform paddle;
    [SerializeField] private Transform ballSpawnPoint;
    [SerializeField] private Transform brickSpawnPoint;

    [Header("Colors")]
    [SerializeField] private Color paddleColor;
    [SerializeField] private Color paddleSpeedColor;
    [SerializeField] private Color wallsColor;
    [SerializeField] private Color ballColor;
    
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
    private MaterialPropertyBlock propertyBlock;

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

        propertyBlock = new MaterialPropertyBlock();

        ChangeGameState(GameStates.MainMenu); //The game starts in the main menu state.

        SetGameBoundaries();
        SetPlayer();
        SpawnBricks(1.2f, 3.2f);

        StartCoroutine(OnUpdate()); //When the awake is done it begins the game loop that replaces Unity Update.
    }

    #region Update

    private IEnumerator OnUpdate() //Calls a different method depending on the current game state.
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

        yield return null;

        StartCoroutine(OnUpdate()); //At the end the coroutine calls itself again to continue the update loop.
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

        player.Update(deltaTime, xMoveInput); //Updates the player, and sends it the movement input so it knows when to move.

        for (int i = 0; i < pool.ballsInUse.Count; i++) //Updates all the balls that are in an active state from the pool.
        {
            pool.ballsInUse[i].Update(deltaTime, activeBricksList);
        }

        for (int i = 0; i < powerUpList.Count; i++) //Updates all active powerup items.
        {
            if (powerUpList[i] != null) powerUpList[i].Update(deltaTime, paddle);
        }
    }

    private void GameInputs() //Detectes inputs
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

    private void InitializeGame() //Sets variables to their default values at the beggining of the game level.
    {
        ballIsActive = false;
        ballsUsed = 0;
        foreach (GameObject ball in livesCounter) { ball.SetActive(true); }

        paddle.transform.position = new Vector3(0f, paddle.transform.position.y, 0f);

        if (pool == null)
        {
            pool = new Pool(5);
            pool.Initialize();
        }
    }

    private void SetGameBoundaries() //Calculates the coordinates of the game boundaries
    {
        Transform[] wallMeshes = new Transform[4];

        rightLimit = rightWall.position.x - rightWall.lossyScale.x / 2;
        wallMeshes[0] = rightWall;

        leftLimit = leftWall.position.x + leftWall.lossyScale.x / 2;
        wallMeshes[1] = leftWall;

        topLimit = topWall.position.y - topWall.lossyScale.y / 2;
        wallMeshes[2] = topWall;

        bottomLimit = bottomWall.position.y + bottomWall.lossyScale.y / 2;
        wallMeshes[3] = bottomWall;

        for (int i = 0; i < wallMeshes.Length; i++)
        {
            MeshRenderer mesh = wallMeshes[i].GetComponent<MeshRenderer>();
            mesh.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_Color", wallsColor);
            mesh.SetPropertyBlock(propertyBlock);
        }

        MeshRenderer backMesh = backgroundWall.GetComponent<MeshRenderer>();
        backMesh.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_Color", Color.black);
        backMesh.SetPropertyBlock(propertyBlock);
    }

    private void SetPlayer() //Create the player paddle
    {
        player = new PlayerPaddle(paddle, paddleSpeed, leftLimit, rightLimit, propertyBlock, paddleColor, paddleSpeedColor);

        MeshRenderer mesh = paddle.GetComponent<MeshRenderer>();
        mesh.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_Color", paddleColor);
        mesh.SetPropertyBlock(propertyBlock);
    }

    private void SpawnBricks(float rowSeparation, float columnSeparation) //Instantiates all the bricks.
    {
        brickGrid = new Brick[brickRows, brickColums];

        for (int i = 0; i < brickRows; i++)
        {
            for (int j = 0; j < brickColums; j++)
            {
                GameObject newBrick = Instantiate(brickPrefab, brickSpawnPoint.position, Quaternion.identity);
                newBrick.transform.position += new Vector3(columnSeparation * j, -rowSeparation * i, 0);

                Brick brick = new Brick(newBrick.transform, propertyBlock);

                brickGrid[i, j] = brick;
            }
        }
    }

    #endregion

    #region Ball

    public void LaunchBall() //Causes the ball to start moving upward in a slight random angle.
    {
        Vector3 launchDirection = Vector3.zero;

        float xDir = Random.Range(0.25f, 0.6f); //Gets and angle in the range of the arc. 

        if (Random.value < 0.5f) //Then based on a coin flip, it determines if it launches it to the left or to the right of the paddle. This is done to avoid right angles.
        {
            xDir *= -1;
        }

        launchDirection = new Vector3(xDir, 1, 0);

        ballToLaunch.LauchBall(launchDirection); //Once it gets the launch vector it tells the ball to start moving in that direction.
    }

    public void GetBallToLaunch() //Get a ball from the pool and set in the paddel.
    {
        ballToLaunch = pool.GetBall();
        ballToLaunch.Initialize(ballSpawnPoint);
    }

    public Ball SpawnBall() //Instantiates a new ball.
    {
        GameObject newBall = Instantiate(ballPrefab, ballSpawnPoint.position, Quaternion.identity);
        Ball ball = new Ball(newBall.transform, ballSpeed, leftLimit, rightLimit, topLimit, bottomLimit, player);

        MeshRenderer mesh = newBall.GetComponent<MeshRenderer>();
        mesh.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_Color", ballColor);
        mesh.SetPropertyBlock(propertyBlock);

        return ball;
    }

    public void OnBallDeath(Ball ball) //Gets called when a ball exits the screen.
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

        if (!ballIsActive && CurrentGameState == GameStates.Game) //If there are no more balls active, then the player loses a life.
        {
            livesCounter[ballsUsed].SetActive(false);
            ballsUsed++;
            
            //Debug.Log(ballsUsed);
            if (ballsUsed < ballLives) //If the player has remaining lives then a new ball spawns ready to be launched
            {
                GetBallToLaunch();
            }
            else //If the player runs out of lives then the game ends.
            {
                ChangeGameState(GameStates.Lose);
            }
        }
    }

    #endregion

    #region Bricks & PowerUps
    public GameObject SpawnPowerUp(Vector3 spawnPoint) //Instantiates the powerup object.
    {
        GameObject powerUp = Instantiate(multiballPrefab);
        powerUp.transform.position = spawnPoint;
        return powerUp;
    }

    private Vector2[] SetPowerUpCoordinates(int rows, int colums) //When setting up the bricks this method, choses a positions at random on where to hide the powerups.
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
            int randX = Random.Range(0, availableRows.Count);
            int randY = Random.Range(0, availableColums.Count);

            Vector2 pwLocation = new Vector2(availableRows[randX], availableColums[randY]);
            availableRows.RemoveAt(randX);
            availableColums.RemoveAt(randY);
            coordinates[i] = pwLocation;

            //Debug.Log(pwLocation);
        }

        return coordinates;
    }

    private void InitializeBricks() //Spawns the bricks across the screen
    {
        Vector2[] powerUpsPos = SetPowerUpCoordinates(brickRows, brickColums);

        for (int i = 0; i < brickRows; i++)
        {
            float r = Random.Range(0f, 256) / 255f;
            float g = Random.Range(0f, 256) / 255f;
            float b = Random.Range(0f, 256) / 255f;

            for (int j = 0; j < brickColums; j++)
            {
                Brick brick = brickGrid[i,j];

                brick.transform.gameObject.SetActive(true);

                Color color = new Color(r, g, b);

                MeshRenderer mesh = brick.transform.GetComponent<MeshRenderer>();
                mesh.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor("_Color", color);
                mesh.SetPropertyBlock(propertyBlock);

                if (PowerUpCheck(powerUpsPos, i, j)) //If the current position is set to have a powerup, then the block recives a powerup to spawn on death.
                {
                    if (Random.value < 0.5f)
                    {
                        brick.SetPowerUp(Brick.PowerUpType.MultiBall);
                        //Debug.Log("Multi");
                    }
                    else
                    {
                        brick.SetPowerUp(Brick.PowerUpType.FastPaddle);
                        //Debug.Log("Speed");
                    }
                }
                else
                {
                    brick.SetPowerUp(Brick.PowerUpType.None);
                }

                activeBricksList.Add(brick);
            }
        }
    }

    private bool PowerUpCheck(Vector2[] powerUpsPos, int posX, int posY) //Checks if the current brick should have a powerup.
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

    public void ActivateSpeedPowerUp()
    {
        player.ToggleSpeedPowerUp(true);
    }

    #endregion

    #region ChangeGameState

    private void StartGame()
    {
        activeBricksList.Clear();
        if (pool != null) { pool.ReturnAll(); }

        for (int i = 0; i < powerUpList.Count; i++)
        {
            powerUpList[0].DestroyPowerUp();
        }

        InitializeGame();
        InitializeBricks();
        GetBallToLaunch();
    }
    private void WinGame()
    {
        pool.ReturnAll();

        //Debug.Log("YOU WIN");
    }

    private void ChangeGameState(GameStates state) //Used to switch between game states.
    {
        mainMenuScreen.SetActive(false);
        winScreen.SetActive(false);
        loseScreen.SetActive(false);

        CurrentGameState = state;

        switch (state)
        {
            case GameStates.MainMenu:
                mainMenuScreen.SetActive(true);
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
                break;
        }
    }

    #endregion

    public void DestroyGameObject(GameObject go)
    {
        Destroy(go);
    }

}
