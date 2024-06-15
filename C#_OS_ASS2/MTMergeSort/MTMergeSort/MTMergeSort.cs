using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

class MTMergeSort
{
    public List<string> MergeSort(string[] strList, int nMin = 2)
    {
        if (strList == null || strList.Length == 0)
        {
            return new List<string>();
        }

        // Call the recursive multi-threaded merge sort function
        string[] sortedArray = MultiThreadedMergeSort(strList, nMin);

        // Return the sorted list
        return new List<string>(sortedArray);
    }

    private string[] MultiThreadedMergeSort(string[] array, int nMin)
    {
        if (array.Length <= nMin)
        {
            Array.Sort(array, StringComparer.Ordinal);
            return array;
        }

        int mid = array.Length / 2;
        string[] left = new string[mid];
        string[] right = new string[array.Length - mid];

        Array.Copy(array, 0, left, 0, mid);
        Array.Copy(array, mid, right, 0, array.Length - mid);

        string[] sortedLeft = null;
        string[] sortedRight = null;

        // Create threads to sort the left and right halves concurrently
        Thread leftThread = new Thread(() => sortedLeft = MultiThreadedMergeSort(left, nMin));
        Thread rightThread = new Thread(() => sortedRight = MultiThreadedMergeSort(right, nMin));

        leftThread.Start();
        rightThread.Start();

        leftThread.Join();
        rightThread.Join();

        return Merge(sortedLeft, sortedRight);
    }

    private string[] Merge(string[] left, string[] right)
    {
        string[] result = new string[left.Length + right.Length];
        int i = 0, j = 0, k = 0;

        while (i < left.Length && j < right.Length)
        {
            if (string.Compare(left[i], right[j], StringComparison.Ordinal) <= 0)
                result[k++] = left[i++];
            else
                result[k++] = right[j++];
        }

        while (i < left.Length)
            result[k++] = left[i++];

        while (j < right.Length)
            result[k++] = right[j++];

        // Finally, return the result
        return result;
    }
}

class Program
{
    static void Main()
    {
        // Generate a large list of unsorted strings for testing
        List<string> strList = new List<string>();
        Random rand = new Random();
        for (int i = 0; i < 100000; i++)
        {
            strList.Add(new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ", 10)
                                            .Select(s => s[rand.Next(s.Length)]).ToArray()));
        }

        MTMergeSort sorter = new MTMergeSort();

        // Sort the list using multi-threaded merge sort
        List<string> sortedList = sorter.MergeSort(strList.ToArray(), 1000);

        // Output part of the sorted list to verify correctness
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(sortedList[i]);
        }

        Console.WriteLine("Sorting completed.");
    }
}
