using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

public class UpdateManager : MonoBehaviour
{
    #region Variables

    public static UpdateManager Instance; //Instance of the Manager that can be called from other scripts.

    [Header("Game Components")]
    [field: SerializeField] public SO_GameSettings gameSettings { get; private set; }
    [field: SerializeField] public SO_BrickSpawn spawnData { get; private set; }
    [SerializeField] private SO_SoundLibrary soundLibrary;

    [Header("Parallax")]
    [SerializeField] private Transform[] parallaxLayers;

    [Header("Game boundaries")]
    [SerializeField] private Transform rightWall;
    [SerializeField] private Transform leftWall;
    [SerializeField] private Transform topWall;
    [SerializeField] private Transform bottomWall;


    [Space(10)]

    [SerializeField] private Transform paddle;
    [SerializeField] private Transform ballSpawnPoint;

    [Header("Colors")]
    [SerializeField] private Color paddleColor;
    [SerializeField] private Color paddleSpeedColor;
    [SerializeField] private Color wallsColor;
    [SerializeField] private Color ballColor;

    [Header("Debug")]
    [SerializeField] private bool brickGridGizmos;

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
    private BallPool ballPool;
    private BrickPool brickPool;
    private AudioPool audioPool;
    public List<IPowerUp> powerUpList = new List<IPowerUp>();

    //Game Status
    private bool isGameDone;
    private bool isGamePaused;
    private bool ballIsActive;
    private int currentLives;
    private int paddleHits;

    //Other
    private float deltaTime;
    private MaterialPropertyBlock propertyBlock;
    private Vector3 workSpace;
    private float softLockTimer;

    #endregion

    #region Core Functions

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
        audioPool = new AudioPool(gameSettings.audioSourcesStartAmount);

        SetGameBoundaries();
        SetPlayer();
        StartGame();
        SpawnBricks();

        StartCoroutine(OnUpdate()); //When the awake is done it begins the game loop that replaces Unity Update.
    }
    

    private IEnumerator OnUpdate() //Calls a different method depending on the current game state.
    {
        deltaTime = Time.deltaTime;

        if (!isGameDone)GameInputs();

        if (!isGamePaused) GameUpdate(deltaTime);

        yield return null;

        if (!isGameDone) StartCoroutine(OnUpdate()); //At the end the coroutine calls itself again to continue the update loop.
    }

    private void GameUpdate(float deltaTime)
    {
        if (isGameDone) return;

        player.Update(deltaTime, xMoveInput); //Updates the player, and sends it the movement input so it knows when to move.

        ParallaxControl();

        for (int i = 0; i < ballPool.ballsInUse.Count; i++) //Updates all the balls that are in an active state from the pool.
        {
            ballPool.ballsInUse[i].Update(deltaTime, brickPool.bricksInUse);
        }

        for (int i = 0; i < powerUpList.Count; i++) //Updates all active powerup items.
        {
            if (powerUpList[i] != null) powerUpList[i].Update(deltaTime, paddle);
        }
    }

    private void GameInputs() //Detectes inputs
    {
        if(!isGamePaused) xMoveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && !ballIsActive && !isGamePaused)
        {
            LaunchBall();
            ballIsActive = true;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isGamePaused = !isGamePaused;
            UIManager.Instance.ToggleSettingsScreen(isGamePaused);
        }
    }

    private void ParallaxControl()
    {
        workSpace.x = -xMoveInput;

        for (int i = 0; i < parallaxLayers.Length; i++)
        {
            parallaxLayers[i].position += workSpace * deltaTime * gameSettings.parallaxSpeed[i];
        }
    }

    #endregion

    #region OnStartGame

    private void StartGame()
    {
        if (ballPool != null) ballPool.ReturnAll();

        if (brickPool != null) brickPool.ReturnAll();

        for (int i = 0; i < powerUpList.Count; i++)
        {
            powerUpList[0].DestroyPowerUp();
        }

        InitializeGame();
        GetBallToLaunch();
    }

    private void InitializeGame() //Sets variables to their default values at the beggining of the game level.
    {
        isGameDone = false;
        isGamePaused = false;
        ballIsActive = false;
        currentLives = gameSettings.maxLives;
        paddleHits = 0;

        UIManager.Instance.UpdateLives(currentLives);
        UIManager.Instance.UpdatePaddleHits(paddleHits);
        UIManager.Instance.UpdateBrickAmount(spawnData.spawnList.Length);

        paddle.transform.position = new Vector3(0f, paddle.transform.position.y, 0f);

        if (ballPool == null)
        {
            ballPool = new BallPool(gameSettings.ballPoolStartAmount);
        }
        if (brickPool == null)
        {
            brickPool = new BrickPool(spawnData.spawnList.Length);
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
    }

    private void SetPlayer() //Create the player paddle
    {
        player = new PlayerPaddle(paddle, gameSettings.paddleSpeed, leftLimit, rightLimit, propertyBlock, paddleColor, paddleSpeedColor);

        MeshRenderer mesh = paddle.GetComponent<MeshRenderer>();
        mesh.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_Color", paddleColor);
        mesh.SetPropertyBlock(propertyBlock);
    }

    private void SpawnBricks()
    {
        int[] powerUpIndex = new int[gameSettings.powerUpQuantity];

        List<int> availableBricks = new List<int>();

        for (int i = 0; i < spawnData.spawnList.Length; i++)
        {
            availableBricks.Add(i);
        }

        for (int i = 0; i < gameSettings.powerUpQuantity; i++)
        {
            powerUpIndex[i] = availableBricks[Random.Range(0, availableBricks.Count)];
            availableBricks.Remove(powerUpIndex[i]);
        }

        for(int i = 0; i < spawnData.spawnList.Length; i++)
        {
            BrickSpawn spawn = spawnData.spawnList[i];

            Brick newBrick = brickPool.GetBrick();
            newBrick.Transform.position = spawn.SpawnPoint;

            BrickStats stats = gameSettings.brickStats[(int)spawn.Type];

            newBrick.SetBrickType(stats.hitPoints, stats.blockMaterial);

            if (!PowerUpCheck(powerUpIndex, i))
            {
                newBrick.SetPowerUp(PowerUpHeld.None);
            }
            else
            {
                newBrick.SetPowerUp(PowerUpHeld.Multiball);
            }
        }

        //foreach (int coordinate in powerUpIndex)
        //{
        //    Debug.Log(coordinate);
        //}
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
        ballToLaunch = ballPool.GetBall();
        ballToLaunch.Initialize(ballSpawnPoint);
    }

    public Ball SpawnBall() //Instantiates a new ball.
    {
        GameObject newBall = Instantiate(gameSettings.ballPrefab, ballSpawnPoint.position, Quaternion.identity);
        Ball ball = new Ball(newBall.transform, gameSettings.ballSpeed, leftLimit, rightLimit, topLimit, bottomLimit, player, soundLibrary.soundData[0], soundLibrary.soundData[1]);

        MeshRenderer mesh = newBall.GetComponent<MeshRenderer>();
        mesh.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_Color", ballColor);
        mesh.SetPropertyBlock(propertyBlock);

        return ball;
    }

    public void OnBallDeath(Ball ball) //Gets called when a ball exits the screen.
    {
        ballPool.ReturnBall(ball);
        if (ballPool.ballsInUse.Count == 0)
        {
            ballIsActive = false;

            for (int i = 0; i < powerUpList.Count; i++)
            {
                powerUpList[0].DestroyPowerUp();
            }
        }

        LifeCheck();
    }

    private void LifeCheck()
    {
        if (!ballIsActive) //If there are no more balls active, then the player loses a life.
        {
            currentLives--;
            UIManager.Instance.UpdateLives(currentLives);
            PlayAudioClip(soundLibrary.soundData[2]);

            //Debug.Log(ballsUsed);
            if (currentLives > 0) //If the player has remaining lives then a new ball spawns ready to be launched
            {
                GetBallToLaunch();
            }
            else //If the player runs out of lives then the game ends.
            {
                //EndGame Logic Goes HERE <------
                UIManager.Instance.LoseScreen();
                PlayAudioClip(soundLibrary.soundData[6]);
                ballPool.ReturnAll();
                isGameDone = true;
            }
        }
    }

    #endregion

    #region Bricks & PowerUps

    public Brick SpawnBrick() //Instantiates a new ball.
    {
        GameObject newBrick = Instantiate(gameSettings.brickPrefab);
        Brick brick = new Brick(newBrick.transform);

        return brick;
    }

    public void SpawnPowerUp(Transform spawnpoint, PowerUpHeld type)
    {
        Debug.Log("SpawnPowerUp");

        GameObject newPowerUp = Instantiate(gameSettings.powerUpPrefab);
        newPowerUp.transform.position = spawnpoint.position;

        IPowerUp newPU = null;

        switch(type)
        {
            case PowerUpHeld.None:
                Debug.LogError("Trying To Spawn a Null PowerUP");
                break;
            case PowerUpHeld.Multiball:
                newPU = new PU_Multiball(newPowerUp.transform, 15f, bottomLimit, soundLibrary.soundData[4]);
                break;
            //case PowerUpHeld.Speed:
            //    PU_Multiball newMultiBall = new PU_Multiball(newPowerUp.transform, 15f, bottomLimit);
            //    break;
        }

        newPU.Activate(true);
        powerUpList.Add(newPU);
    }

    public void OnBrickDeath(Brick brick)
    {
        brickPool.ReturnBrick(brick);

        UIManager.Instance.UpdateBrickAmount(brickPool.bricksInUse.Count);
        PlayAudioClip(soundLibrary.soundData[3]);

        if (brickPool.bricksInUse.Count == 0)
        {
            UIManager.Instance.WinScreen();
            PlayAudioClip(soundLibrary.soundData[5]);
            ballPool.ReturnAll();
            isGameDone = true;
            Debug.Log("YOU WIN");
        }
    }

    private bool PowerUpCheck(int[] powerUps, int currentBrick) //Checks if the current brick should have a powerup.
    {
        for (int i = 0; i < powerUps.Length; i++)
        {
            if (currentBrick == powerUps[i])
            {
                return true;
            }
        }

        return false;
    }

    public void ActivateSpeedPowerUp()
    {
        player.ToggleSpeedPowerUp(true);
    }

    #endregion

    #region ChangeGameState

    private void WinGame()
    {
        ballPool.ReturnAll();

        //Debug.Log("YOU WIN");
    }

    #endregion

    #region Other

    public void NegateXInput()
    {
        xMoveInput = 0f;
    }

    public void IncreasePaddleHits()
    {
        paddleHits++;
        UIManager.Instance.UpdatePaddleHits(paddleHits);
    }

    public void DestroyGameObject(GameObject go)
    {
        Destroy(go);
    }

    public AudioSource SpawnAudioSource() //Instantiates a new audio source.
    {
        AudioSource newSource = Instantiate(gameSettings.sourcePrefab,transform);

        return newSource;
    }

    public void PlayAudioClip(SoundData data)
    {
        AudioSource audioSource = audioPool.GetAudioSource();
        audioSource.clip = data.Clip;
        audioSource.outputAudioMixerGroup = data.MixerGroup;
        audioSource.volume = data.Volume;
        audioSource.pitch = 1;
        if (data.WithRandomPitch)
        {
            audioSource.pitch += Random.Range(gameSettings.minRandomPitch, gameSettings.maxRandomPitch);
        }
        audioSource.Play();
        StartCoroutine(WaitForSoundToEnd(audioSource));
    }

    private IEnumerator WaitForSoundToEnd(AudioSource source) //When the sound stops playing the source is returned to the pool.
    {
        yield return new WaitWhile(() => source.isPlaying);
        source.Stop();
        audioPool.ReturnSource(source);
    }

    private void OnDrawGizmos()
    {
        if (brickGridGizmos && spawnData != null)
        {
            foreach(BrickSpawn spawn in spawnData.spawnList)
            {
                switch (spawn.Type)
                {
                    case BrickType.Glass:
                        Gizmos.color = Color.blue;
                        break;
                    case BrickType.Wood:
                        Gizmos.color = Color.green;
                        break;
                    case BrickType.Stone:
                        Gizmos.color = Color.yellow;
                        break;
                    case BrickType.Metal:
                        Gizmos.color = Color.red;
                        break;
                }

                Gizmos.DrawWireCube(spawn.SpawnPoint, new Vector3(3f, 1f, 1f));
            }
        }
    }

    #endregion

}
