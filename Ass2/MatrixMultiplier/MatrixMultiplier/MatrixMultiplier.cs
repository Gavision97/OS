using System;
using System.Threading;




class MatrixMultiplier
{
    public static void Main(string[] args)
    {
        int[,] matrixA = new int[,]
        {
            {1, 2, 3 },
            {1, 2, 3 },
            {1, 2, 3 }

        };
        int[,] matrixB = new int[,]
        {
            {3,2,1 }, {3,2,2}, {3,2,3}
        };
        int[,] resultMatrix = new int[3,3];
        int rowA = 3;
        int rowB = 3;
        int colsA = 3;
        int colsB = 3;
        int numThreads = 9;
        MultiplyMatricesConcurrently(matrixA, matrixB, resultMatrix, rowA, rowB, colsA, colsB);
        Console.WriteLine(resultMatrix.ToString());
    }
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
            int endRow = currentRow + rowsPerThread - 1;

            if (i < remainingRows)
                endRow += 1;
       
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


