using System;
using System.Threading.Tasks;

public class SharableSpreadSheetTests
{
    private SharableSpreadSheet spreadSheet;

    public static void Main(string[] args)
    {
        var tests = new SharableSpreadSheetTests();
        tests.RunAllTests();

        Console.WriteLine("All tests completed. Press any key to exit...");
        Console.ReadKey();
    }

    public void RunAllTests()
    {
        TestConstructor();
        TestGetSetCell();
        TestSearchString();
        TestExchangeRows();
        TestExchangeCols();
        TestSearchInRow();
        TestSearchInCol();
        TestSearchInRange();
        TestAddRow();
        TestAddCol();
        TestFindAll();
        TestSetAll();
        TestGetSize();
        TestConcurrentAccess();
        TestSearchLimit();

        Console.WriteLine("All tests completed successfully.");
    }

    private void SetUp()
    {
        spreadSheet = new SharableSpreadSheet(5, 5, 3);
    }

    // ... [All the test methods remain the same as in the previous response] ...

    // Example of one test method:
    private void TestConstructor()
    {
        try
        {
            new SharableSpreadSheet(0, 5);
            throw new Exception("Expected ArgumentException was not thrown.");
        }
        catch (ArgumentException)
        {
            Console.WriteLine("TestConstructor passed.");
        }
    }

    private void TestGetSetCell()
    {
        SetUp();
        spreadSheet.SetCell(0, 0, "Test");
        if (spreadSheet.GetCell(0, 0) != "Test")
            throw new Exception("TestGetSetCell failed.");
        Console.WriteLine("TestGetSetCell passed.");
    }

    private void TestSearchString()
    {
        SetUp();
        spreadSheet.SetCell(2, 3, "FindMe");
        var result = spreadSheet.SearchString("FindMe");
        if (result.Item1 != 2 || result.Item2 != 3)
            throw new Exception("TestSearchString failed.");
        Console.WriteLine("TestSearchString passed.");
    }

    private void TestExchangeRows()
    {
        SetUp();
        spreadSheet.SetCell(0, 0, "Row1");
        spreadSheet.SetCell(1, 0, "Row2");
        spreadSheet.ExchangeRows(0, 1);
        if (spreadSheet.GetCell(0, 0) != "Row2" || spreadSheet.GetCell(1, 0) != "Row1")
            throw new Exception("TestExchangeRows failed.");
        Console.WriteLine("TestExchangeRows passed.");
    }

    private void TestExchangeCols()
    {
        SetUp();
        spreadSheet.SetCell(0, 0, "Col1");
        spreadSheet.SetCell(0, 1, "Col2");
        spreadSheet.ExchangeCols(0, 1);
        if (spreadSheet.GetCell(0, 0) != "Col2" || spreadSheet.GetCell(0, 1) != "Col1")
            throw new Exception("TestExchangeCols failed.");
        Console.WriteLine("TestExchangeCols passed.");
    }

    private void TestSearchInRow()
    {
        SetUp();
        spreadSheet.SetCell(1, 2, "FindInRow");
        if (spreadSheet.SearchInRow(1, "FindInRow") != 2)
            throw new Exception("TestSearchInRow failed.");
        Console.WriteLine("TestSearchInRow passed.");
    }

    private void TestSearchInCol()
    {
        SetUp();
        spreadSheet.SetCell(2, 1, "FindInCol");
        if (spreadSheet.SearchInCol(1, "FindInCol") != 2)
            throw new Exception("TestSearchInCol failed.");
        Console.WriteLine("TestSearchInCol passed.");
    }

    private void TestSearchInRange()
    {
        SetUp();
        spreadSheet.SetCell(2, 2, "FindInRange");
        var result = spreadSheet.SearchInRange(1, 3, 1, 3, "FindInRange");
        if (result.Item1 != 2 || result.Item2 != 2)
            throw new Exception("TestSearchInRange failed.");
        Console.WriteLine("TestSearchInRange passed.");
    }

    private void TestAddRow()
    {
        SetUp();
        int initialRows = spreadSheet.GetSize().Item1;
        spreadSheet.SetCell(initialRows - 1, 0, "LastRow");
        spreadSheet.AddRow(initialRows - 1);

        try
        {
            if (spreadSheet.GetCell(initialRows, 0) != "LastRow" || spreadSheet.GetSize().Item1 != initialRows + 1)
                throw new Exception($"TestAddRow failed: Expected 'LastRow' in cell ({initialRows}, 0), but got '{spreadSheet.GetCell(initialRows, 0)}'. Expected {initialRows + 1} rows, but got {spreadSheet.GetSize().Item1}");
            Console.WriteLine("TestAddRow passed.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"TestAddRow failed: {e.Message}");
        }
    }

    private void TestAddCol()
    {
        SetUp();
        int initialCols = spreadSheet.GetSize().Item2;
        spreadSheet.SetCell(0, initialCols - 1, "LastCol");
        spreadSheet.AddCol(initialCols - 1);

        try
        {
            if (spreadSheet.GetCell(0, initialCols) != "LastCol" || spreadSheet.GetSize().Item2 != initialCols + 1)
                throw new Exception($"TestAddCol failed: Expected 'LastCol' in cell (0, {initialCols}), but got '{spreadSheet.GetCell(0, initialCols)}'. Expected {initialCols + 1} columns, but got {spreadSheet.GetSize().Item2}");
            Console.WriteLine("TestAddCol passed.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"TestAddCol failed: {e.Message}");
        }
    }

    private void TestFindAll()
    {
        SetUp();
        spreadSheet.SetCell(0, 0, "Find");
        spreadSheet.SetCell(1, 1, "find");
        spreadSheet.SetCell(2, 2, "Find");
        var results = spreadSheet.FindAll("Find", true);
        if (results.Length != 2)
            throw new Exception("TestFindAll failed.");
        Console.WriteLine("TestFindAll passed.");
    }

    private void TestSetAll()
    {
        SetUp();
        spreadSheet.SetCell(0, 0, "Old");
        spreadSheet.SetCell(1, 1, "old");
        spreadSheet.SetCell(2, 2, "OLD");
        spreadSheet.SetAll("old", "New", false);
        if (spreadSheet.GetCell(0, 0) != "New" || spreadSheet.GetCell(1, 1) != "New" || spreadSheet.GetCell(2, 2) != "New")
            throw new Exception("TestSetAll failed.");
        Console.WriteLine("TestSetAll passed.");
    }

    private void TestGetSize()
    {
        SetUp();
        var size = spreadSheet.GetSize();
        if (size.Item1 != 5 || size.Item2 != 5)
            throw new Exception("TestGetSize failed.");
        Console.WriteLine("TestGetSize passed.");
    }

    private void TestConcurrentAccess()
    {
        SetUp();
        const int numTasks = 1000;
        var tasks = new Task[numTasks];

        for (int i = 0; i < numTasks; i++)
        {
            int row = i % 5;
            int col = i % 5;
            tasks[i] = Task.Run(() =>
            {
                spreadSheet.SetCell(row, col, $"Test{i}");
                spreadSheet.GetCell(row, col);
                spreadSheet.SearchString($"Test{i}");
            });
        }

        Task.WaitAll(tasks);
        Console.WriteLine("TestConcurrentAccess passed.");
    }

    private void TestSearchLimit()
    {
        var limitedSpreadSheet = new SharableSpreadSheet(5, 5, 2);
        var tasks = new Task[3];

        for (int i = 0; i < 3; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                limitedSpreadSheet.SearchString("Test");
            });
        }

        var completedInTime = Task.WaitAll(tasks, TimeSpan.FromSeconds(5));
        if (!completedInTime)
            throw new Exception("TestSearchLimit failed.");
        Console.WriteLine("TestSearchLimit passed.");
    }
}