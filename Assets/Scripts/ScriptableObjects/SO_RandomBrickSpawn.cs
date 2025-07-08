using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSpawnData", menuName = "Data/Game/RandomSpawnData", order = 1)]
public class SO_RandomBrickSpawn : SO_BrickSpawn
{
    [Header("Brick Amount Range")]
    [SerializeField, Range(2f, 10f)] private int maxRows;
    [SerializeField, Range(2f, 7f)] private int maxColumns;

    [Header("Position")]
    [SerializeField, Range(10f, 22f)] private float startRowHeight = 22f;
    [SerializeField, Range(0.2f, 2f)] private float rowPadding;
    [SerializeField, Range(0.2f, 2f)] private float columnPadding;

    public void GenerateRandomLevel()
    {
        int rows = Random.Range(2, maxRows + 1);
        int columns = Random.Range(2, maxColumns + 1);

        spawnList = new BrickSpawn[rows * columns];

        float[] rowPos = new float[rows];
        float[] columnPos = new float[columns];

        for (int i = 0; i < rowPos.Length; i++)
        {
            rowPos[i] = startRowHeight - (i * rowPadding);
        }

        for (int i = 0; i < columnPos.Length; i++)
        {
            float startColumnPos = 0;

            if (columns % 2 == 0)
            {
                float spacing = 3 + columnPadding;

                startColumnPos = (spacing * 3) - (spacing/2);
            }
            else
            {
                startColumnPos = (Mathf.FloorToInt(columns / 2) + columnPadding) * 3;
            }

            columnPos[i] = startColumnPos - (i * columnPadding);
        }

        int rowIndex = 0;
        int columnIndex = 0;

        for (int i = 0; i < spawnList.Length; i++)
        {
            float xPos = 0f;
            float yPos = 0f;

            xPos = rowPos[rowIndex];
            yPos = columnPos[columnIndex];

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
