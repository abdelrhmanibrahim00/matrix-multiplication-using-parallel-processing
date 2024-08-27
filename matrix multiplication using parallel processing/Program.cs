using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

class Program
{

    static void Main()
    {
        int count = 0;
        string filepath = "vector_data2.txt";
        Console.Write("Enter the degree of parallelism: ");
        if (int.TryParse(Console.ReadLine(), out int degreeOfParallelism) && degreeOfParallelism > 0)
        {
            Stopwatch totalStopwatch = Stopwatch.StartNew();

            // Call the ReadMatrixData method to read matrix data from a file
            if (ReadMatrixData(filepath, out int[,] matrixA, out int[,] matrixB))
            {

                // Iterate through each line
                foreach (string line in ReadAllLines(filepath))
                {
                    count++;
                    // Split the line into two parts using ';'
                    string[] parts = line.Split(';');

                    // Create matrices
                    matrixA = ParseMatrix(parts[0]);
                    matrixB = ParseMatrix(parts[1]);

                    Console.WriteLine("\nMatrix A:");
                    PrintMatrix(matrixA);

                    Console.WriteLine("\nMatrix B:");
                    PrintMatrix(matrixB);

                    Stopwatch stopwatch = Stopwatch.StartNew();
                    // Perform matrix multiplication
                    int[,] resultMatrix = MultiplyMatrices(matrixA, matrixB, degreeOfParallelism);
                    stopwatch.Stop();

                    Console.WriteLine("\nResult Matrix:");
                    PrintMatrix(resultMatrix);
                }
            }
            else
            {
                Console.WriteLine("Failed to read matrix data from the file.");
            }

            totalStopwatch.Stop();
            Console.WriteLine($"\nTotal time taken for processing: {totalStopwatch.ElapsedMilliseconds} ms");
        }
        else
        {
            Console.WriteLine("Invalid input for the degree of parallelism.");
        }
        Console.WriteLine("The number of multplication process = {0} ", count);
        Console.WriteLine("The number of input Matrices = {0} ", count * 2);
        Console.WriteLine("The number of result  Matrices = {0} ", count);


    }


    static bool ReadMatrixData(string filePath, out int[,] matrixA, out int[,] matrixB)
    {
        matrixA = null;
        matrixB = null;

        try
        {
            // Read all lines from the file
            foreach (string line in ReadAllLines(filePath))
            {
                // Split the line into two parts using ';'
                string[] parts = line.Split(';');

                // Create matrices
                matrixA = ParseMatrix(parts[0]);
                matrixB = ParseMatrix(parts[1]);
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading matrix data: {ex.Message}");
            return false;
        }
    }

    static IEnumerable<string> ReadAllLines(string filePath)
    {
        using (var reader = new StreamReader(filePath))
        {
            while (!reader.EndOfStream)
            {
                yield return reader.ReadLine();
            }
        }
    }

    static int[,] ParseMatrix(string line)
    {
        var values = line.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        int rows = values.Length / 3; // Assuming each vector has 3 components (change if needed)
        int cols = 3; // Assuming each vector has 3 components (change if needed)

        int[,] matrix = new int[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                matrix[i, j] = int.Parse(values[i * cols + j]);
            }
        }

        return matrix;
    }

    static int[,] MultiplyMatrices(int[,] matrixA, int[,] matrixB, int degreeOfParallelism)
    {
        int rowsA = matrixA.GetLength(0);
        int colsA = matrixA.GetLength(1);
        int colsB = matrixB.GetLength(1);

        if (colsA != matrixB.GetLength(0))
        {
            throw new InvalidOperationException("Matrices cannot be multiplied. Number of columns in Matrix A must be equal to the number of rows in Matrix B.");
        }

        int[,] resultMatrix = new int[rowsA, colsB];

        IEnumerable<int> Data(int start, int count) => Enumerable.Range(start, count);

        var query = from i in Data(0, rowsA)
                    from j in Data(0, colsB)
                    select new
                    {
                        i,
                        j,
                        value = Data(0, colsA)
                            .AsParallel()
                            .WithDegreeOfParallelism(degreeOfParallelism)
                            .Sum(k => matrixA[i, k] * matrixB[k, j])
                    };

        foreach (var item in query)
        {
            resultMatrix[item.i, item.j] = item.value;
        }

        return resultMatrix;
    }

    static void PrintMatrix(int[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Console.Write($"{matrix[i, j]} ");
            }
            Console.WriteLine();
        }
    }
}
