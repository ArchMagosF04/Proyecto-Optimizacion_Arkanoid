using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSpawnData", menuName = "Data/Game/SpawnData", order = 0)]
public class SO_BrickSpawn : ScriptableObject
{
    [field: SerializeField] public int AssetGroup;
    [field: SerializeField] public BrickSpawn[] spawnList;

    [field: SerializeField] public bool isRandom = false;


    [Header("Brick Amount Range")]
    [SerializeField, Range(2f, 10f)] private int maxRows;
    [SerializeField, Range(2f, 7f)] private int maxColumns;

    [Header("Position")]
    [SerializeField, Range(10f, 22f)] private float startRowHeight = 22f;
    [SerializeField, Range(0.2f, 0.8f)] private float maxRowPadding;
    [SerializeField, Range(0.1f, 1.5f)] private float maxColumnPadding;

    public void GenerateRandomLevel()
    {
        int rows = Random.Range(3, maxRows + 1);
        int columns = Random.Range(3, maxColumns + 1);

        float rowPadding = Random.Range(0.2f, maxRowPadding);
        float columnPadding = Random.Range(0.1f, maxColumnPadding);

        spawnList = new BrickSpawn[rows * columns];

        float[] rowPos = new float[rows];
        float[] columnPos = new float[columns];

        for (int i = 0; i < rowPos.Length; i++)
        {
            rowPos[i] = startRowHeight - (i * (rowPadding + 1f));
        }

        for (int i = 0; i < columnPos.Length; i++)
        {
            float startColumnPos = 0;
            float columsHalf = Mathf.FloorToInt(columns / 2);

            if (columns % 2 == 0)
            {
                float spacing = (3 + columnPadding);

                startColumnPos = (spacing * columsHalf) - (spacing / 2);
            }
            else
            {
                startColumnPos = (3 + columnPadding) * Mathf.FloorToInt(columns / 2);
            }

            columnPos[i] = startColumnPos - (i * (3 + columnPadding));
        }

        int rowIndex = 0;
        int columnIndex = 0;

        for (int i = 0; i < spawnList.Length; i++)
        {
            float xPos = 0f;
            float yPos = 0f;

            xPos = columnPos[columnIndex];
            yPos = rowPos[rowIndex];

            spawnList[i].SpawnPoint = new Vector2(xPos, yPos);
            spawnList[i].Type = (BrickType)Random.Range(0, 4);

            columnIndex++;
            if (columnIndex >= columnPos.Length)
            {
                columnIndex = 0;
                rowIndex++;
            }
        }
    }
}
