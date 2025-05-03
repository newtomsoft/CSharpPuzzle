using Microsoft.Z3;

namespace Puzzle;

public static class Sudoku
{
    private const int GridSize = 9;
    private const int BlockSize = 3;
    public static int[,] SolveSudoku(int[,] puzzle)
    {
        using var ctx = new Context();

        var matrix = new IntExpr[GridSize, GridSize];
        for (var i = 0; i < GridSize; i++)
        for (var j = 0; j < GridSize; j++)
            matrix[i, j] = ctx.MkIntConst($"cell_{i}_{j}");

        var solver = ctx.MkSolver();

        for (var i = 0; i < GridSize; i++)
        for (var j = 0; j < GridSize; j++)
            solver.Add(ctx.MkAnd(ctx.MkGe(matrix[i, j], ctx.MkInt(1)), ctx.MkLe(matrix[i, j], ctx.MkInt(9))));


        for (var rowIndex = 0; rowIndex < GridSize; rowIndex++)
        {
            var rowValues = Enumerable.Range(0, GridSize).Select(col => matrix[rowIndex, col]).ToArray<Expr>();
            solver.Add(ctx.MkDistinct(rowValues));
        }

        for (var columnIndex = 0; columnIndex < GridSize; columnIndex++)
        {
            var columnValues = Enumerable.Range(0, GridSize).Select(i => matrix[i, columnIndex]).ToArray<Expr>();
            solver.Add(ctx.MkDistinct(columnValues));
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

                solver.Add(ctx.MkDistinct(blockValues));
            }
        }

        for (var i = 0; i < GridSize; i++)
        for (var j = 0; j < GridSize; j++)
            if (puzzle[i, j] != 0)
                solver.Add(ctx.MkEq(matrix[i, j], ctx.MkInt(puzzle[i, j])));

        var status = solver.Check();
        if (status != Status.SATISFIABLE) return new int[GridSize, GridSize];
        {
            var model = solver.Model;
            var solution = new int[GridSize, GridSize];

            for (var i = 0; i < GridSize; i++)
            for (var j = 0; j < GridSize; j++)
                solution[i, j] = ((IntNum)model.Evaluate(matrix[i, j])).Int;

            return solution;
        }
    }

    public static void PrintSudoku(int[,] grid)
    {
        for (var i = 0; i < GridSize; i++)
        {
            if (i % BlockSize == 0 && i != 0) Console.WriteLine("------+-------+------");
            for (var j = 0; j < GridSize; j++)
            {
                if (j % BlockSize == 0 && j != 0) Console.Write("| ");
                var value = grid[i, j];
                if (value == 0) Console.Write(". ");
                else Console.Write(value + " ");
            }
            Console.WriteLine();
        }
    }
}