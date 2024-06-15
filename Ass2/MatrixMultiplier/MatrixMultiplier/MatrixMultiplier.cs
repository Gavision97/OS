using System;
using System.Threading;

class MatrixMultiplier
{
    public static void MultiplyMatricesConcurrently(int[,] matrixA, int[,] matrixB,
        int[,] resultMatrix, int rowsA, int colsA, int colsB, int numThreads)
    {
        Thread[] threads = new Thread[numThreads];

        int rowsPerThread = rowsA / numThreads;
        int remainingRows = rowsA % numThreads;

        // Initialize and start threads
        int currentRow = 0;
        for (int i = 0; i < numThreads; i++)
        {
            int startRow = currentRow;
            int endRow = currentRow + rowsPerThread + (i < remainingRows ? 1 : 0) - 1;

            threads[i] = new Thread(() => MultiplyMatrixSection(matrixA, matrixB, resultMatrix, startRow, endRow, colsA, colsB));
            threads[i].Start();

            currentRow = endRow + 1;
        }

        // Wait for all threads to complete
        for (int i = 0; i < numThreads; i++)
        {
            threads[i].Join();
        }
    }

    private static void MultiplyMatrixSection(int[,] matrixA, int[,] matrixB, int[,] resultMatrix, int startRow, int endRow, int colsA, int colsB)
    {
        for (int i = startRow; i <= endRow; i++)
        {
            for (int j = 0; j < colsB; j++)
            {
                resultMatrix[i, j] = 0;
                for (int k = 0; k < colsA; k++)
                {
                    resultMatrix[i, j] += matrixA[i, k] * matrixB[k, j];
                }
            }
        }
    }
}


