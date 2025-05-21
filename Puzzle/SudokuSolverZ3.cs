using Microsoft.Z3;

namespace Puzzle;

public class SudokuSolverZ3
{
    private static int _gridSize;
    private static int _blockSize;
    private const int MinValue = 1;
    private static int _maxValue = _gridSize;

    private readonly Context _ctx;
    private readonly int[,] _puzzle;
    private readonly Solver _solver;
    private readonly int[,] _solution;
    private readonly IntExpr[,] _matrixExpr;

    public SudokuSolverZ3(int[,] puzzle)
    {
        _ctx = new Context();
        _puzzle = puzzle;
        _solver = _ctx.MkSolver();
        _gridSize = puzzle.GetLength(0);
        _blockSize = Convert.ToInt32(Math.Sqrt(_gridSize));
        _maxValue = _gridSize;
        _matrixExpr = new IntExpr[_gridSize, _gridSize];
        _solution = new int[_gridSize, _gridSize];
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

        for (var i = 0; i < _gridSize; i++)
        for (var j = 0; j < _gridSize; j++)
            _solution[i, j] = ((IntNum)model.Evaluate(_matrixExpr[i, j])).Int;
    }

    private void SetInitialPuzzleValues()
    {
        for (var i = 0; i < _gridSize; i++)
        for (var j = 0; j < _gridSize; j++)
            _matrixExpr[i, j] = _ctx.MkIntConst($"cell_{i}_{j}");

        for (var i = 0; i < _gridSize; i++)
        for (var j = 0; j < _gridSize; j++)
            if (_puzzle[i, j] != 0)
                _solver.Add(_ctx.MkEq(_matrixExpr[i, j], _ctx.MkInt(_puzzle[i, j])));
    }

    private void AddValueRangeConstraints()
    {
        foreach (var cellExpr in _matrixExpr)
        {
            var minConstraint = _ctx.MkGe(cellExpr, _ctx.MkInt(MinValue));
            var maxConstraint = _ctx.MkLe(cellExpr, _ctx.MkInt(_maxValue));
            _solver.Add(minConstraint, maxConstraint);
        }
    }

    private void AddDistinctRowConstraint()
    {
        for (var rowIndex = 0; rowIndex < _gridSize; rowIndex++)
        {
            var rowValues = Enumerable.Range(0, _gridSize).Select(columnIndex => _matrixExpr[rowIndex, columnIndex]).ToArray<Expr>();
            _solver.Add(_ctx.MkDistinct(rowValues));
        }
    }

    private void AddDistinctColumnConstraint()
    {
        for (var columnIndex = 0; columnIndex < _gridSize; columnIndex++)
        {
            var columnValues = Enumerable.Range(0, _gridSize).Select(rowIndex => _matrixExpr[rowIndex, columnIndex]).ToArray<Expr>();
            _solver.Add(_ctx.MkDistinct(columnValues));
        }
    }

    private void AddDistinctBlockConstraints()
    {
        for (var blockRow = 0; blockRow < _blockSize; blockRow++)
        {
            for (var blockCol = 0; blockCol < _blockSize; blockCol++)
            {
                var blockValues = new Expr[_gridSize];
                var index = 0;

                for (var i = 0; i < _blockSize; i++)
                for (var j = 0; j < _blockSize; j++)
                    blockValues[index++] = _matrixExpr[blockRow * _blockSize + i, blockCol * _blockSize + j];

                _solver.Add(_ctx.MkDistinct(blockValues));
            }
        }
    }

    public void PrintPuzzle() => PrintMatrix(_puzzle);
    public void PrintSolution() => PrintMatrix(_solution);

    private static void PrintMatrix(int[,] matrix)
    {
        for (var i = 0; i < _gridSize; i++)
        {
            if (i % _blockSize == 0 && i != 0) Console.WriteLine("------+-------+------");
            for (var j = 0; j < _gridSize; j++)
            {
                if (j % _blockSize == 0 && j != 0) Console.Write("| ");
                var value = matrix[i, j];
                if (value == 0) Console.Write(". ");
                else Console.Write(value + " ");
            }

            Console.WriteLine();
        }
    }
}