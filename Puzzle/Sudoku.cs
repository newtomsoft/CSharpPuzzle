using Microsoft.Z3;

namespace Puzzle;

public static class Sudoku
{
    public static int[,] SolveSudoku(int[,] puzzle)
    {
        const int gridSize = 9;
        using var ctx = new Context();

        var matrix = new IntExpr[9, 9];
        for (var i = 0; i < 9; i++)
        for (var j = 0; j < 9; j++)
            matrix[i, j] = ctx.MkIntConst($"cell_{i}_{j}");

        var solver = ctx.MkSolver();

        for (var i = 0; i < 9; i++)
        for (var j = 0; j < 9; j++)
            solver.Add(ctx.MkAnd(ctx.MkGe(matrix[i, j], ctx.MkInt(1)), ctx.MkLe(matrix[i, j], ctx.MkInt(9))));


        for (var rowIndex = 0; rowIndex < gridSize; rowIndex++)
        {
            Expr[] rowValues = Enumerable.Range(0, gridSize).Select(col => matrix[rowIndex, col]).ToArray();
            solver.Add(ctx.MkDistinct(rowValues));
        }

        for (var columnIndex = 0; columnIndex < gridSize; columnIndex++)
        {
            Expr[] columnValues = Enumerable.Range(0, gridSize).Select(i => matrix[i, columnIndex]).ToArray();
            solver.Add(ctx.MkDistinct(columnValues));
        }

        for (var blockRow = 0; blockRow < 3; blockRow++)
        {
            for (var blockCol = 0; blockCol < 3; blockCol++)
            {
                var blockValues = new Expr[gridSize];
                var index = 0;

                for (var i = 0; i < 3; i++)
                for (var j = 0; j < 3; j++)
                    blockValues[index++] = matrix[blockRow * 3 + i, blockCol * 3 + j];

                solver.Add(ctx.MkDistinct(blockValues));
            }
        }

        for (var i = 0; i < 9; i++)
        for (var j = 0; j < 9; j++)
            if (puzzle[i, j] != 0)
                solver.Add(ctx.MkEq(matrix[i, j], ctx.MkInt(puzzle[i, j])));

        var status = solver.Check();
        if (status != Status.SATISFIABLE) return new int[9, 9];
        {
            var model = solver.Model;
            var solution = new int[9, 9];

            for (var i = 0; i < 9; i++)
            for (var j = 0; j < 9; j++)
                solution[i, j] = ((IntNum)model.Evaluate(matrix[i, j])).Int;

            return solution;
        }
    }

    public static void PrintSudoku(int[,] grid)
    {
        for (var i = 0; i < 9; i++)
        {
            if (i % 3 == 0 && i != 0) Console.WriteLine("------+-------+------");
            for (var j = 0; j < 9; j++)
            {
                if (j % 3 == 0 && j != 0) Console.Write("| ");
                var value = grid[i, j];
                if (value == 0) Console.Write(". ");
                else Console.Write(value + " ");
            }
            Console.WriteLine();
        }
    }
}