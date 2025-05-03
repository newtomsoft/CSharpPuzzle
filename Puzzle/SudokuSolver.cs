using Microsoft.Z3;

namespace Puzzle;

public class SudokuSolver
{
    private const int GridSize = 9;
    private const int BlockSize = 3;
    private const int MinValue = 1;
    private const int MaxValue = 9;
    private readonly Context _ctx;
    private readonly int[,] _puzzle;
    private readonly Solver _solver;
    private int[,]? _solution;

    public SudokuSolver(int[,] puzzle)
    {
        _ctx = new Context();
        _puzzle = puzzle;
        _solver = _ctx.MkSolver();
    }
    public int[,] SolveSudoku()
    {
        var matrix = new IntExpr[GridSize, GridSize];
        for (var i = 0; i < GridSize; i++)
        for (var j = 0; j < GridSize; j++)
            matrix[i, j] = _ctx.MkIntConst($"cell_{i}_{j}");

        AddValueRangeConstraints(matrix);

        for (var rowIndex = 0; rowIndex < GridSize; rowIndex++)
        {
            var rowValues = Enumerable.Range(0, GridSize).Select(col => matrix[rowIndex, col]).ToArray<Expr>();
            _solver.Add(_ctx.MkDistinct(rowValues));
        }

        for (var columnIndex = 0; columnIndex < GridSize; columnIndex++)
        {
            var columnValues = Enumerable.Range(0, GridSize).Select(i => matrix[i, columnIndex]).ToArray<Expr>();
            _solver.Add(_ctx.MkDistinct(columnValues));
        }

        for (var blockRow = 0; blockRow < BlockSize; blockRow++)
        {
            for (var blockCol = 0; blockCol < BlockSize; blockCol++)
            {
                var blockValues = new Expr[GridSize];
                var index = 0;

                for (var i = 0; i < BlockSize; i++)
                for (var j = 0; j < BlockSize; j++)
                    blockValues[index++] = matrix[blockRow * 3 + i, blockCol * 3 + j];

                _solver.Add(_ctx.MkDistinct(blockValues));
            }
        }

        for (var i = 0; i < GridSize; i++)
        for (var j = 0; j < GridSize; j++)
            if (_puzzle[i, j] != 0)
                _solver.Add(_ctx.MkEq(matrix[i, j], _ctx.MkInt(_puzzle[i, j])));

        var status = _solver.Check();
        if (status != Status.SATISFIABLE) return new int[GridSize, GridSize];
        {
            var model = _solver.Model;
            var solution = new int[GridSize, GridSize];

            for (var i = 0; i < GridSize; i++)
            for (var j = 0; j < GridSize; j++)
                solution[i, j] = ((IntNum)model.Evaluate(matrix[i, j])).Int;

            _solution = solution;
            return solution;
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

    private void AddValueRangeConstraints(IntExpr[,] matrix)
    {
        for (var row = 0; row < GridSize; row++)
        {
            for (var col = 0; col < GridSize; col++)
            {
                var cell = matrix[row, col];
                var minConstraint = _ctx.MkGe(cell, _ctx.MkInt(MinValue));
                var maxConstraint = _ctx.MkLe(cell, _ctx.MkInt(MaxValue));
                _solver.Add(_ctx.MkAnd(minConstraint, maxConstraint));
            }
        }
    }
}