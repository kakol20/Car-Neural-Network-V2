using System.Collections.Generic;
using UnityEngine;

public enum ActivationFunction
{
    Tanh, Sigmoid, ReLU, LeakyReLU
}

public class Matrix
{
    public Matrix()
    {
        Cols = 0;
        Rows = 0;
    }

    public Matrix(int cols, int rows)
    {
        Cols = cols;
        Rows = rows;

        if (Array.Count == 0) Array.Clear();
        for (int x = 0; x < cols; x++)
        {
            List<float> yGrid = new List<float>();
            for (int y = 0; y < rows; y++)
            {
                yGrid.Add(0);
            }
            Array.Add(yGrid);
        }
    }

    public List<List<float>> Array { get; private set; } = new List<List<float>>();

    public int Cols { get; private set; }

    public int Rows { get; private set; }

    public void Activation(ActivationFunction activation)
    {
        for (int x = 0; x < Cols; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                switch (activation)
                {
                    case ActivationFunction.Tanh:
                        Array[x][y] = Mathf.Atan(Array[x][y]);
                        break;

                    case ActivationFunction.Sigmoid:
                        Array[x][y] = 1 / (1 + Mathf.Exp(-1 * Array[x][y]));
                        break;

                    case ActivationFunction.ReLU:
                        Array[x][y] = Mathf.Max(0, Array[x][y]);
                        break;

                    case ActivationFunction.LeakyReLU:
                        Array[x][y] = Mathf.Max(Array[x][y] * 0.1f, Array[x][y]);
                        break;

                    default:
                        break;
                }
            }
        }
    }

    /// <summary>
    /// utate matrix and clamp it to min and max
    /// </summary>
    /// <param name="mutationRate"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public void Mutate(float min, float max, float chance)
    {
        for (int x = 0; x < Cols; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                //float chance = Own.Random.Range();

                float change = Own.Random.Range(min, max);
                bool mutate = Own.Random.Range(0, 1) <= chance;

                Array[x][y] += mutate ? change : 0.0f;
            }
        }
    }
    public void Add(Matrix otherMatrix)
    {
        if (Cols == otherMatrix.Cols && Rows == otherMatrix.Rows)
        {
            for (int x = 0; x < Cols; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    Array[x][y] += otherMatrix.Array[x][y];
                }
            }
        }
    }

    public void Clamp(float min, float max)
    {
        for (int x = 0; x < Cols; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                Array[x][y] = Mathf.Clamp(Array[x][y], min, max);
            }
        }
    }

    public void Copy(Matrix copyMatrix)
    {
        if (copyMatrix != this)
        {
            Cols = copyMatrix.Cols;
            Rows = copyMatrix.Rows;

            Array.Clear();

            for (int x = 0; x < Cols; x++)
            {
                List<float> yGrid = new List<float>();
                for (int y = 0; y < Rows; y++)
                {
                    yGrid.Add(copyMatrix.Array[x][y]);
                }
                Array.Add(yGrid);
            }
        }
    }

    public void Copy(float[][] otherMatrix)
    {
        if (otherMatrix.Length > 0)
        {
            Cols = otherMatrix.Length;
            Rows = otherMatrix[0].Length;

            Array.Clear();

            for (int x = 0; x < Cols; x++)
            {
                List<float> yGrid = new List<float>();

                for (int y = 0; y < Rows; y++)
                {
                    yGrid.Add(otherMatrix[x][y]);
                }

                Array.Add(yGrid);
            }
        }
        else
        {
            Cols = 0;
            Rows = 0;

            Array.Clear();
        }
    }

    public void Copy(List<List<float>> otherMatrix)
    {
        if (otherMatrix.Count > 0)
        {
            Cols = otherMatrix.Count;
            Rows = otherMatrix[0].Count;

            Array.Clear();

            for (int x = 0; x < Cols; x++)
            {
                List<float> yGrid = new List<float>();

                for (int y = 0; y < Rows; y++)
                {
                    yGrid.Add(otherMatrix[x][y]);
                }

                Array.Add(yGrid);
            }
        }
        else
        {
            Cols = 0;
            Rows = 0;

            Array.Clear();
        }
    }
    public void Multiply(Matrix otherMatrix)
    {
        if (Cols != otherMatrix.Rows)
        {
            Cols = 0;
            Rows = 0;

            Array.Clear();
        }
        else
        {
            int newCols = otherMatrix.Cols;
            int newRows = Rows;

            List<List<float>> newMatrix = new List<List<float>>();

            for (int i = 0; i < otherMatrix.Cols; i++)
            {
                List<float> col = new List<float>();

                for (int j = 0; j < Rows; j++)
                {
                    float total = 0;

                    for (int k = 0; k < otherMatrix.Rows; k++)
                    {
                        total += Array[k][j] * otherMatrix.Array[i][k];
                    }
                    col.Add(total);
                }

                newMatrix.Add(col);
            }

            Cols = newCols;
            Rows = newRows;

            //Array = newMatrix;
            Array.Clear();

            for (int x = 0; x < Cols; x++)
            {
                List<float> col = new List<float>();
                for (int y = 0; y < Rows; y++)
                {
                    col.Add(newMatrix[x][y]);
                }

                Array.Add(col);
            }
        }
    }

    public void Randomise(float min, float max)
    {
        for (int x = 0; x < Cols; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                Array[x][y] = Own.Random.Range(min, max);
            }
        }
    }

    public void Scale(float scalar)
    {
        for (int x = 0; x < Cols; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                Array[x][y] *= scalar;
            }
        }
    }

    public void SetIdentity(int size)
    {
        Array.Clear();

        Cols = size;
        Rows = size;

        for (int x = 0; x < Cols; x++)
        {
            List<float> yGrid = new List<float>();

            for (int y = 0; y < Rows; y++)
            {
                if (x == y)
                {
                    yGrid.Add(1);
                }
                else
                {
                    yGrid.Add(0);
                }
            }
        }
    }
}