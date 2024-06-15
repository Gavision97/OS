using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace ImageSearch
{
    class Program
    {
        static void Main(string[] args)
        {
            // Check if the correct number of arguments are provided
            if (args.Length < 4)
            {
                Console.WriteLine("Usage: ImageSearch <image1> <image2> <nThreads> <algorithm>");
                Console.WriteLine("<image1>: Larger image file (jpg or gif)");
                Console.WriteLine("<image2>: Smaller image file (jpg or gif)");
                Console.WriteLine("<nThreads>: Number of threads (1 or greater)");
                Console.WriteLine("<algorithm>: 'exact' or 'euclidean'");
                return;
            }

            // Parse command-line arguments
            string image1Path = args[0];
            string image2Path = args[1];
            if (!int.TryParse(args[2], out int nThreads) || nThreads < 1)
            {
                Console.WriteLine("Error: Number of threads must be 1 or greater.");
                return;
            }
            string algorithm = args[3];

            // Validate algorithm parameter
            if (algorithm != "exact" && algorithm != "euclidean")
            {
                Console.WriteLine("Error: Invalid algorithm specified. Use 'exact' or 'euclidean'.");
                return;
            }

            // Load images from provided file paths
            Bitmap image1;
            Bitmap image2;
            try
            {
                image1 = new Bitmap(image1Path);
                image2 = new Bitmap(image2Path);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error loading images: {e.Message}");
                return;
            }

            // Convert images to 2D arrays of Color objects
            Color[,] largeImage = ImageToColorArray(image1);
            Color[,] smallImage = ImageToColorArray(image2);

            // Get dimensions of the images
            int largeWidth = largeImage.GetLength(0);
            int largeHeight = largeImage.GetLength(1);
            int smallWidth = smallImage.GetLength(0);
            int smallHeight = smallImage.GetLength(1);

            // Initialize a list to store matching coordinates
            List<Point> matches = new List<Point>();

            // Calculate chunk size for dividing the work among threads
            int chunkSize = largeHeight / nThreads;

            // Create and start threads
            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < nThreads; i++)
            {
                int startRow = i * chunkSize;
                int endRow = (i == nThreads - 1) ? largeHeight : (i + 1) * chunkSize;

                Thread thread = new Thread(() =>
                {
                    SearchInChunk(largeImage, smallImage, startRow, endRow, largeWidth, largeHeight, smallWidth, smallHeight, algorithm, matches);
                });

                threads.Add(thread);
                thread.Start();
            }

            // Wait for all threads to complete
            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            // Output results (matching coordinates)
            if (matches.Count > 0)
            {
                foreach (Point match in matches)
                {
                    Console.WriteLine($"{match.X},{match.Y}");
                }
            }
            else
            {
                Console.WriteLine("No matches found.");
            }

            // Wait for user input before closing (to keep the console window open)
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        // Convert a Bitmap image to a 2D array of Color objects
        static Color[,] ImageToColorArray(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;
            Color[,] result = new Color[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    result[x, y] = image.GetPixel(x, y);
                }
            }

            return result;
        }

        // Search for the small image in a specific chunk of the large image
        static void SearchInChunk(Color[,] largeImage, Color[,] smallImage, int startRow, int endRow,
            int largeWidth, int largeHeight, int smallWidth, int smallHeight, string algorithm, List<Point> matches)
        {
            for (int x = 0; x <= largeWidth - smallWidth; x++)
            {
                for (int y = startRow; y <= endRow - smallHeight; y++)
                {
                    if (IsMatch(largeImage, smallImage, x, y, smallWidth, smallHeight, algorithm))
                    {
                        lock (matches) // Ensure thread safety
                        {
                            matches.Add(new Point(x, y));
                        }
                    }
                }
            }
        }

        // Check if smallImage matches largeImage at position (startX, startY)
        static bool IsMatch(Color[,] largeImage, Color[,] smallImage, int startX, int startY,
            int smallWidth, int smallHeight, string algorithm)
        {
            for (int x = 0; x < smallWidth; x++)
            {
                for (int y = 0; y < smallHeight; y++)
                {
                    Color largePixel = largeImage[startX + x, startY + y];
                    Color smallPixel = smallImage[x, y];

                    if (algorithm == "exact")
                    {
                        if (!largePixel.Equals(smallPixel))
                        {
                            return false;
                        }
                    }
                    else if (algorithm == "euclidean")
                    {
                        double distance = Math.Sqrt(
                            Math.Pow(largePixel.R - smallPixel.R, 2) +
                            Math.Pow(largePixel.G - smallPixel.G, 2) +
                            Math.Pow(largePixel.B - smallPixel.B, 2)
                        );
                        if (distance != 0)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
