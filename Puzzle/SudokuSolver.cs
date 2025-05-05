using Microsoft.Z3;

namespace Puzzle;

public class SudokuSolver
{
    private const int GridSize = 9;
    private const int BlockSize = 3;
    private const int MinValue = 1;
    private const int MaxValue = GridSize;

    private readonly Context _ctx;
    private readonly int[,] _puzzle;
    private readonly Solver _solver;
    private readonly int[,] _solution;
    private readonly IntExpr[,] _matrixExpr;

    public SudokuSolver(int[,] puzzle)
    {
        _ctx = new Context();
        _puzzle = puzzle;
        _solver = _ctx.MkSolver();
        _matrixExpr = new IntExpr[GridSize, GridSize];
        _solution = new int[GridSize, GridSize];
    }

    public int[,] SolveSudoku()
    {
        SetInitialPuzzleValues();
        AddValueRangeConstraints();
        AddDistinctRowConstraint();
        AddDistinctColumnConstraint();
        AddDistinctBlockConstraints();
        EvaluateSolverAndExtractSolution();
        return _solution;
    }

    private void EvaluateSolverAndExtractSolution()
    {
        var status = _solver.Check();
        if (status != Status.SATISFIABLE) return;
        var model = _solver.Model;

        for (var i = 0; i < GridSize; i++)
        for (var j = 0; j < GridSize; j++)
            _solution[i, j] = ((IntNum)model.Evaluate(_matrixExpr[i, j])).Int;
    }

    private void SetInitialPuzzleValues()
    {
        for (var i = 0; i < GridSize; i++)
        for (var j = 0; j < GridSize; j++)
            _matrixExpr[i, j] = _ctx.MkIntConst($"cell_{i}_{j}");

        for (var i = 0; i < GridSize; i++)
        for (var j = 0; j < GridSize; j++)
            if (_puzzle[i, j] != 0)
                _solver.Add(_ctx.MkEq(_matrixExpr[i, j], _ctx.MkInt(_puzzle[i, j])));
    }

    private void AddValueRangeConstraints()
    {
        foreach (var cellExpr in _matrixExpr)
        {
            var minConstraint = _ctx.MkGe(cellExpr, _ctx.MkInt(MinValue));
            var maxConstraint = _ctx.MkLe(cellExpr, _ctx.MkInt(MaxValue));
            _solver.Add(minConstraint, maxConstraint);
        }
    }

    private void AddDistinctRowConstraint()
    {
        for (var rowIndex = 0; rowIndex < GridSize; rowIndex++)
        {
            var rowValues = Enumerable.Range(0, GridSize).Select(columnIndex => _matrixExpr[rowIndex, columnIndex]).ToArray<Expr>();
            _solver.Add(_ctx.MkDistinct(rowValues));
        }
    }

    private void AddDistinctColumnConstraint()
    {
        for (var columnIndex = 0; columnIndex < GridSize; columnIndex++)
        {
            var columnValues = Enumerable.Range(0, GridSize).Select(rowIndex => _matrixExpr[rowIndex, columnIndex]).ToArray<Expr>();
            _solver.Add(_ctx.MkDistinct(columnValues));
        }
    }

    private void AddDistinctBlockConstraints()
    {
        for (var blockRow = 0; blockRow < BlockSize; blockRow++)
        {
            for (var blockCol = 0; blockCol < BlockSize; blockCol++)
            {
                var blockValues = new Expr[GridSize];
                var index = 0;

                for (var i = 0; i < BlockSize; i++)
                for (var j = 0; j < BlockSize; j++)
                    blockValues[index++] = _matrixExpr[blockRow * 3 + i, blockCol * 3 + j];

                _solver.Add(_ctx.MkDistinct(blockValues));
            }
        }
    }

    public void PrintPuzzle() => PrintMatrix(_puzzle);
    public void PrintSolution() => PrintMatrix(_solution);

    private static void PrintMatrix(int[,] matrix)
    {
        for (var i = 0; i < GridSize; i++)
        {
            if (i % BlockSize == 0 && i != 0) Console.WriteLine("------+-------+------");
            for (var j = 0; j < GridSize; j++)
            {
                if (j % BlockSize == 0 && j != 0) Console.Write("| ");
                var value = matrix[i, j];
                if (value == 0) Console.Write(". ");
                else Console.Write(value + " ");
            }

            Console.WriteLine();
        }
    }
}