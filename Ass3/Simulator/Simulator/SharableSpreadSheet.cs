using System;
using System.Collections.Generic;
using System.Threading;


namespace SharableSpreadSheet
{
    class SharableSpreadSheet
    {
        private string[,] spreadSheet;
        private ReaderWriterLockSlim[,] cellLocks;
        private int nRows;
        private int nCols;
        private int nUsers;
        private SemaphoreSlim searchSemaphore;

        public SharableSpreadSheet(int nRows, int nCols, int nUsers = -1)
        {
            if (nRows <= 0 || nCols <= 0)
                throw new ArgumentException("Rows and columns must be positive.");

            this.nRows = nRows;
            this.nCols = nCols;
            this.nUsers = nUsers;
            spreadSheet = new string[nRows, nCols];
            cellLocks = new ReaderWriterLockSlim[nRows, nCols];

            for (int i = 0; i < nRows; i++)
                for (int j = 0; j < nCols; j++)
                {
                    spreadSheet[i, j] = string.Empty;
                    cellLocks[i, j] = new ReaderWriterLockSlim();
                }

            searchSemaphore = nUsers > 0 ? new SemaphoreSlim(nUsers, nUsers) : null;
        }

        public String getCell(int row, int col)
        {
            ValidateCell(row, col);
            cellLocks[row, col].EnterReadLock();
            try
            {
                return spreadSheet[row, col];
            }
            finally
            {
                cellLocks[row, col].ExitReadLock();
            }
        }

        public void setCell(int row, int col, String str)
        {
            ValidateCell(row, col);
            cellLocks[row, col].EnterWriteLock();
            try
            {
                spreadSheet[row, col] = str;
            }
            finally
            {
                cellLocks[row, col].ExitWriteLock();
            }
        }

        public Tuple<int, int> searchString(String str)
        {
            return searchInRange(0, nCols - 1, 0, nRows - 1, str);
        }

        public void exchangeRows(int row1, int row2)
        {
            ValidateRow(row1);
            ValidateRow(row2);
            if (row1 == row2) return;

            int minRow = Math.Min(row1, row2);
            int maxRow = Math.Max(row1, row2);

            for (int col = 0; col < nCols; col++)
            {
                cellLocks[minRow, col].EnterWriteLock();
                cellLocks[maxRow, col].EnterWriteLock();
            }

            try
            {
                for (int col = 0; col < nCols; col++)
                {
                    var temp = spreadSheet[row1, col];
                    spreadSheet[row1, col] = spreadSheet[row2, col];
                    spreadSheet[row2, col] = temp;
                }
            }
            finally
            {
                for (int col = 0; col < nCols; col++)
                {
                    cellLocks[maxRow, col].ExitWriteLock();
                    cellLocks[minRow, col].ExitWriteLock();
                }
            }
        }

        public void exchangeCols(int col1, int col2)
        {
            ValidateColumn(col1);
            ValidateColumn(col2);
            if (col1 == col2) return;

            int minCol = Math.Min(col1, col2);
            int maxCol = Math.Max(col1, col2);

            for (int row = 0; row < nRows; row++)
            {
                cellLocks[row, minCol].EnterWriteLock();
                cellLocks[row, maxCol].EnterWriteLock();
            }

            try
            {
                for (int row = 0; row < nRows; row++)
                {
                    var temp = spreadSheet[row, col1];
                    spreadSheet[row, col1] = spreadSheet[row, col2];
                    spreadSheet[row, col2] = temp;
                }
            }
            finally
            {
                for (int row = 0; row < nRows; row++)
                {
                    cellLocks[row, maxCol].ExitWriteLock();
                    cellLocks[row, minCol].ExitWriteLock();
                }
            }
        }

        public int searchInRow(int row, String str)
        {
            ValidateRow(row);
            if (searchSemaphore != null) searchSemaphore.Wait();
            try
            {
                for (int col = 0; col < nCols; col++)
                {
                    cellLocks[row, col].EnterReadLock();
                    try
                    {
                        if (spreadSheet[row, col] == str)
                            return col;
                    }
                    finally
                    {
                        cellLocks[row, col].ExitReadLock();
                    }
                }
                return -1;
            }
            finally
            {
                if (searchSemaphore != null) searchSemaphore.Release();
            }
        }

        public int searchInCol(int col, String str)
        {
            ValidateColumn(col);
            if (searchSemaphore != null) searchSemaphore.Wait();
            try
            {
                for (int row = 0; row < nRows; row++)
                {
                    cellLocks[row, col].EnterReadLock();
                    try
                    {
                        if (spreadSheet[row, col] == str)
                            return row;
                    }
                    finally
                    {
                        cellLocks[row, col].ExitReadLock();
                    }
                }
                return -1;
            }
            finally
            {
                if (searchSemaphore != null) searchSemaphore.Release();
            }
        }

        public Tuple<int, int> searchInRange(int col1, int col2, int row1, int row2, String str)
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
                        cellLocks[row, col].EnterReadLock();
                        try
                        {
                            if (spreadSheet[row, col] == str)
                                return Tuple.Create(row, col);
                        }
                        finally
                        {
                            cellLocks[row, col].ExitReadLock();
                        }
                    }
                }
                return null;
            }
            finally
            {
                if (searchSemaphore != null) searchSemaphore.Release();
            }
        }

        public void addRow(int row1)
        {
            ValidateRow(row1);

            lock (this)
            {
                nRows++;
                var newSpreadSheet = new string[nRows, nCols];
                var newCellLocks = new ReaderWriterLockSlim[nRows, nCols];

                for (int row = 0; row <= row1; row++)
                {
                    for (int col = 0; col < nCols; col++)
                    {
                        newSpreadSheet[row, col] = spreadSheet[row, col];
                        newCellLocks[row, col] = cellLocks[row, col];
                    }
                }


                for (int col = 0; col < nCols; col++)
                {
                    newSpreadSheet[row1 + 1, col] = string.Empty;
                    newCellLocks[row1 + 1, col] = new ReaderWriterLockSlim();
                }

                for (int row = row1 + 1; row < nRows - 1; row++)
                {
                    for (int col = 0; col < nCols; col++)
                    {
                        newSpreadSheet[row + 1, col] = spreadSheet[row, col];
                        newCellLocks[row + 1, col] = cellLocks[row, col];
                    }
                }

                spreadSheet = newSpreadSheet;
                cellLocks = newCellLocks;
            }
        }

        public void addCol(int col1)
        {
            ValidateColumn(col1);

            lock (this)
            {
                nCols++;
                var newSpreadSheet = new string[nRows, nCols];
                var newCellLocks = new ReaderWriterLockSlim[nRows, nCols];

                for (int row = 0; row < nRows; row++)
                {
                    for (int col = 0; col <= col1; col++)
                    {
                        newSpreadSheet[row, col] = spreadSheet[row, col];
                        newCellLocks[row, col] = cellLocks[row, col];
                    }
                    newSpreadSheet[row, col1 + 1] = string.Empty;
                    newCellLocks[row, col1 + 1] = new ReaderWriterLockSlim();
                    for (int col = col1 + 1; col < nCols - 1; col++)
                    {
                        newSpreadSheet[row, col + 1] = spreadSheet[row, col];
                        newCellLocks[row, col + 1] = cellLocks[row, col];
                    }
                }

                spreadSheet = newSpreadSheet;
                cellLocks = newCellLocks;
            }
        }

        public Tuple<int, int>[] findAll(String str, bool caseSensitive)
        {
            var result = new List<Tuple<int, int>>();
            if (searchSemaphore != null) searchSemaphore.Wait();
            try
            {
                for (int row = 0; row < nRows; row++)
                {
                    for (int col = 0; col < nCols; col++)
                    {
                        cellLocks[row, col].EnterReadLock();
                        try
                        {
                            if (caseSensitive ? spreadSheet[row, col] == str :
                                                spreadSheet[row, col].Equals(str, StringComparison.OrdinalIgnoreCase))
                            {
                                result.Add(Tuple.Create(row, col));
                            }
                        }
                        finally
                        {
                            cellLocks[row, col].ExitReadLock();
                        }
                    }
                }
            }
            finally
            {
                if (searchSemaphore != null) searchSemaphore.Release();
            }
            return result.ToArray();
        }

        public void setAll(String oldStr, String newStr, bool caseSensitive)
        {
            for (int row = 0; row < nRows; row++)
            {
                for (int col = 0; col < nCols; col++)
                {
                    cellLocks[row, col].EnterUpgradeableReadLock();
                    try
                    {
                        if (caseSensitive ? spreadSheet[row, col] == oldStr :
                                            spreadSheet[row, col].Equals(oldStr, StringComparison.OrdinalIgnoreCase))
                        {
                            cellLocks[row, col].EnterWriteLock();
                            try
                            {
                                spreadSheet[row, col] = newStr;
                            }
                            finally
                            {
                                cellLocks[row, col].ExitWriteLock();
                            }
                        }
                    }
                    finally
                    {
                        cellLocks[row, col].ExitUpgradeableReadLock();
                    }
                }
            }
        }

        public Tuple<int, int> getSize()
        {
            return Tuple.Create(nRows, nCols);
        }

        public void print()
        {
            for (int row = 0; row < nRows; row++)
            {
                for (int col = 0; col < nCols; col++)
                {
                    cellLocks[row, col].EnterReadLock();
                    try
                    {
                        Console.WriteLine($"[{row},{col}] {spreadSheet[row, col]}");
                    }
                    finally
                    {
                        cellLocks[row, col].ExitReadLock();
                    }
                }
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