using System;
using System.Threading;

namespace SharableSpreadSheet.Simulator
{
    internal class Simulator
    {
        public static void Main(string[] args)
        {
            if (args.Length != 5)
            {
                Console.WriteLine("Usage: Simulator <rows> <cols> <nThreads> <nOperations> <mssleep>");
                return;
            }

            int nRows = Int32.Parse(args[0]);
            int nCols = Int32.Parse(args[1]);
            int nThreads = Int32.Parse(args[2]);
            int nOperations = Int32.Parse(args[3]);
            int mssleep = Int32.Parse(args[4]);

            SharableSpreadSheet spreadSheet = new SharableSpreadSheet(nRows, nCols, nThreads);
            var waitHandles = new ManualResetEvent[nThreads];
            for (int i = 0; i < waitHandles.Length; i++) waitHandles[i] = new ManualResetEvent(false);

            for (int i = 0; i < nRows; i++)
            {
                for (int j = 0; j < nCols; j++)
                    spreadSheet.setCell(i, j, String.Format("Grade99Cell{0}{1}", i, j));
            }

            for (int i = 0; i < nThreads; i++)
            {
                new Thread((waitHandle) =>
                {
                    for (int k = 0; k < nOperations; k++)
                    {
                        doRandomOperation(spreadSheet, nRows, nCols);
                        Thread.Sleep(mssleep);
                    }
                    (waitHandle as ManualResetEvent).Set();
                }).Start(waitHandles[i]);
            }
            if (!WaitHandle.WaitAll(waitHandles, TimeSpan.FromSeconds(30)))
            {
                // timeout
            }
        }

        static private void doRandomOperation(SharableSpreadSheet spreadSheet, int nRows, int nCols)
        {
            Random rnd = new Random();
            int randomNum = rnd.Next(1, 12);
            int row = rnd.Next(nRows);
            int col = rnd.Next(nCols);
            int threadId = Thread.CurrentThread.ManagedThreadId;

            switch (randomNum)
            {
                case 1:
                    String cell = spreadSheet.getCell(row, col);
                    Console.WriteLine(String.Format("User [{0}]: string '{1}' found in cell[{2},{3}]", threadId, cell, row, col));
                    break;
                case 2:
                    spreadSheet.setCell(row, col, "Grade 100");
                    Console.WriteLine(String.Format("User [{0}]: string 'Grade 100' inserted to cell[{1},{2}].", threadId, row, col));
                    break;
                case 3:
                    Tuple<int, int> result = spreadSheet.searchString("Grade 100");
                    if (result == null)
                        Console.WriteLine(String.Format("User [{0}]: String 'Grade 100' not in spreadsheet.", threadId));
                    else
                        Console.WriteLine(String.Format("User[{0}]: String 'Grade 100' found in cell[{1},{2}].", threadId, result.Item1, result.Item2));
                    break;
                case 4:
                    int row1 = rnd.Next(nRows);
                    if (row1 != row)
                        spreadSheet.exchangeRows(row, row1);
                    else if (row1 > 0)
                        spreadSheet.exchangeRows(row, row1 - 1);
                    else
                        spreadSheet.exchangeRows(row, row1 + 1);
                    Console.WriteLine(String.Format("User [{0}]: rows {1} and {2} exchanged successfully.", threadId, row, row1));
                    break;
                case 5:
                    int col1 = rnd.Next(nCols);
                    if (col != col1)
                        spreadSheet.exchangeCols(col, col1);
                    else if (0 < col1)
                        spreadSheet.exchangeCols(col, col1 - 1);
                    else
                        spreadSheet.exchangeCols(col, col1 + 1);
                    Console.WriteLine(String.Format("User [{0}]: columns {1} and {2} exchanged successfully.", threadId, col, col1));
                    break;
                case 6:
                    int rowResult = spreadSheet.searchInRow(row, "Grade 100");
                    if (rowResult != -1)
                        Console.WriteLine(String.Format("User[{0}]: String 'Grade 100' found in cell[{1},{2}].", threadId, row, rowResult));
                    else
                        Console.WriteLine(String.Format("User[{0}]: String 'Grade 100' wasn't found in row {1}.", threadId, row));
                    break;
                case 7:
                    int colResult = spreadSheet.searchInCol(col, "Grade 100");
                    if (colResult != -1)
                        Console.WriteLine(String.Format("User[{0}]: String 'Grade 100' found in cell[{1},{2}].", threadId, colResult, col));
                    else
                        Console.WriteLine(String.Format("User[{0}]: String 'Grade 100' wasn't found in column {1}.", threadId, col));
                    break;
                case 8:
                    int row2 = rnd.Next(row, nRows);
                    int col2 = rnd.Next(col, nCols);
                    Tuple<int, int> rangeResult = spreadSheet.searchInRange(col, col2, row, row2, "Grade 100");
                    if (rangeResult != null)
                        Console.WriteLine(String.Format("User[{0}]: String 'Grade 100' found in cell[{1},{2}].", threadId, rangeResult.Item1, rangeResult.Item2));
                    break;
                case 9:
                    spreadSheet.addRow(row);
                    Console.WriteLine(String.Format("User[{0}]: a new row added after row {1}.", threadId, row));
                    break;
                case 10:
                    bool caseSen = rnd.Next(2) == 1;
                    Tuple<int, int>[] findAllResult = spreadSheet.findAll("Grade 100", caseSen);
                    Console.WriteLine(String.Format("User[{0}]:The string 'Grade 100' appear {1} times in the spreadsheet", threadId, findAllResult.Length));
                    break;
                case 11:
                    bool caseSensitive = rnd.Next(2) == 1;
                    spreadSheet.setAll("Grade 100", "Grade 110", caseSensitive);
                    Console.WriteLine(String.Format("User[{0}]:The string 'Grade 100' changed successfully to 'Grade 110'.", threadId));
                    break;
                default:
                    Tuple<int, int> size = spreadSheet.getSize();
                    Console.WriteLine(String.Format("User[{0}]: Size of the spreadsheet is {1} rows and {2} columns.", threadId, size.Item1, size.Item2));
                    break;
            }
        }
    }
}
