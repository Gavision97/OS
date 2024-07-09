using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace SharableSpreadSheet
{
    public class SharableSpreadSheet
    {
        private ConcurrentDictionary<(int, int), string> spreadSheet;
        private int nRows;
        private int nCols;
        private readonly SemaphoreSlim searchSemaphore;
        private readonly ReaderWriterLockSlim structureLock = new ReaderWriterLockSlim();

        public SharableSpreadSheet(int nRows, int nCols, int nUsers = -1)
        {
            if (nRows <= 0 || nCols <= 0)
                throw new ArgumentException("Rows and columns must be positive.");

            this.nRows = nRows;
            this.nCols = nCols;
            spreadSheet = new ConcurrentDictionary<(int, int), string>();
            searchSemaphore = nUsers > 0 ? new SemaphoreSlim(nUsers, nUsers) : null;
        }

        public string GetCell(int row, int col)
        {
            ValidateCell(row, col);
            return spreadSheet.TryGetValue((row, col), out var value) ? value : string.Empty;
        }

        public void SetCell(int row, int col, string str)
        {
            ValidateCell(row, col);
            spreadSheet[(row, col)] = str;
        }

        public Tuple<int, int> SearchString(string str)
        {
            return SearchInRange(0, nCols - 1, 0, nRows - 1, str);
        }

        public void ExchangeRows(int row1, int row2)
        {
            ValidateRow(row1);
            ValidateRow(row2);
            if (row1 == row2) return;

            structureLock.EnterWriteLock();
            try
            {
                for (int col = 0; col < nCols; col++)
                {
                    var temp = GetCell(row1, col);
                    SetCell(row1, col, GetCell(row2, col));
                    SetCell(row2, col, temp);
                }
            }
            finally
            {
                structureLock.ExitWriteLock();
            }
        }

        public void ExchangeCols(int col1, int col2)
        {
            ValidateColumn(col1);
            ValidateColumn(col2);
            if (col1 == col2) return;

            structureLock.EnterWriteLock();
            try
            {
                for (int row = 0; row < nRows; row++)
                {
                    var temp = GetCell(row, col1);
                    SetCell(row, col1, GetCell(row, col2));
                    SetCell(row, col2, temp);
                }
            }
            finally
            {
                structureLock.ExitWriteLock();
            }
        }

        public int SearchInRow(int row, string str)
        {
            ValidateRow(row);
            if (searchSemaphore != null) searchSemaphore.Wait();
            try
            {
                for (int col = 0; col < nCols; col++)
                {
                    if (GetCell(row, col) == str)
                        return col;
                }
                return -1;
            }
            finally
            {
                if (searchSemaphore != null) searchSemaphore.Release();
            }
        }

        public int SearchInCol(int col, string str)
        {
            ValidateColumn(col);
            if (searchSemaphore != null) searchSemaphore.Wait();
            try
            {
                for (int row = 0; row < nRows; row++)
                {
                    if (GetCell(row, col) == str)
                        return row;
                }
                return -1;
            }
            finally
            {
                if (searchSemaphore != null) searchSemaphore.Release();
            }
        }

        public Tuple<int, int> SearchInRange(int col1, int col2, int row1, int row2, string str)
        {
            ValidateCell(row1, col1);
            ValidateCell(row2, col2);
            if (row2 < row1 || col2 < col1)
                throw new ArgumentException("Invalid range specified.");

            if (searchSemaphore != null) searchSemaphore.Wait();
            try
            {
                for (int row = row1; row <= row2; row++)
                {
                    for (int col = col1; col <= col2; col++)
                    {
                        if (GetCell(row, col) == str)
                            return Tuple.Create(row, col);
                    }
                }
                return null;
            }
            finally
            {
                if (searchSemaphore != null) searchSemaphore.Release();
            }
        }

        public void AddRow(int row1)
        {
            ValidateRow(row1);
            structureLock.EnterWriteLock();
            try
            {
                nRows++;
                var newCells = new Dictionary<(int, int), string>();
                foreach (var kvp in spreadSheet)
                {
                    var (row, col) = kvp.Key;
                    if (row >= row1)
                        newCells[(row + 1, col)] = kvp.Value;
                }
                foreach (var newCell in newCells)
                {
                    spreadSheet[newCell.Key] = newCell.Value;
                }
            }
            finally
            {
                structureLock.ExitWriteLock();
            }
        }

        public void AddCol(int col1)
        {
            ValidateColumn(col1);
            structureLock.EnterWriteLock();
            try
            {
                nCols++;
                var newCells = new Dictionary<(int, int), string>();
                foreach (var kvp in spreadSheet)
                {
                    var (row, col) = kvp.Key;
                    if (col >= col1)
                        newCells[(row, col + 1)] = kvp.Value;
                }
                foreach (var newCell in newCells)
                {
                    spreadSheet[newCell.Key] = newCell.Value;
                }
            }
            finally
            {
                structureLock.ExitWriteLock();
            }
        }

        public Tuple<int, int>[] FindAll(string str, bool caseSensitive)
        {
            var result = new List<Tuple<int, int>>();
            if (searchSemaphore != null) searchSemaphore.Wait();
            try
            {
                StringComparison comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                foreach (var kvp in spreadSheet)
                {
                    if (string.Equals(kvp.Value, str, comparison))
                    {
                        result.Add(Tuple.Create(kvp.Key.Item1, kvp.Key.Item2));
                    }
                }
            }
            finally
            {
                if (searchSemaphore != null) searchSemaphore.Release();
            }
            return result.ToArray();
        }

        public void SetAll(string oldStr, string newStr, bool caseSensitive)
        {
            StringComparison comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            foreach (var kvp in spreadSheet)
            {
                if (string.Equals(kvp.Value, oldStr, comparison))
                {
                    spreadSheet.TryUpdate(kvp.Key, newStr, kvp.Value);
                }
            }
        }

        public Tuple<int, int> GetSize()
        {
            return Tuple.Create(nRows, nCols);
        }

        public void Print()
        {
            structureLock.EnterReadLock();
            try
            {
                for (int row = 0; row < nRows; row++)
                {
                    for (int col = 0; col < nCols; col++)
                    {
                        var value = GetCell(row, col);
                        Console.Write(value);
                        if (col < nCols - 1)
                        {
                            Console.Write(", ");
                        }
                    }
                    Console.WriteLine();
                }
            }
            finally
            {
                structureLock.ExitReadLock();
            }
        }


        private void ValidateCell(int row, int col)
        {
            if (row < 0 || row >= nRows || col < 0 || col >= nCols)
                throw new ArgumentException("Invalid cell coordinates.");
        }

        private void ValidateRow(int row)
        {
            if (row < 0 || row >= nRows)
                throw new ArgumentException("Invalid row index.");
        }

        private void ValidateColumn(int col)
        {
            if (col < 0 || col >= nCols)
                throw new ArgumentException("Invalid column index.");
        }

    }
}
