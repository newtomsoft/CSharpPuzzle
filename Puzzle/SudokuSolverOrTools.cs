using System.Diagnostics;
using Google.OrTools.Sat;

namespace Puzzle;

public static class SudokuSolverOrTools
{
    public static int[][] SolveSudoku(int[][] initialGrid)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var model = new CpModel();

            var lineSize = initialGrid.Length;
            var cellSize = (int)Math.Sqrt(lineSize);

            // Print initial grid
            Console.WriteLine("initial grid:");
            for (var i = 0; i < lineSize; i++)
            {
                Console.WriteLine("[" + string.Join(", ", initialGrid[i]) + "]");
            }

            // Create variables
            var grid = new Dictionary<(int, int), IntVar>();
            for (var i = 0; i < lineSize; i++)
            {
                for (var j = 0; j < lineSize; j++)
                {
                    grid[(i, j)] = model.NewIntVar(1, lineSize, $"grid {i} {j}");
                }
            }

            // Add row constraints
            for (var i = 0; i < lineSize; i++)
            {
                var rowVars = new List<IntVar>();
                for (var j = 0; j < lineSize; j++)
                {
                    rowVars.Add(grid[(i, j)]);
                }
                model.AddAllDifferent(rowVars);
            }

            // Add column constraints
            for (var j = 0; j < lineSize; j++)
            {
                var colVars = new List<IntVar>();
                for (var i = 0; i < lineSize; i++)
                {
                    colVars.Add(grid[(i, j)]);
                }
                model.AddAllDifferent(colVars);
            }

            // Add cell constraints
            for (var i = 0; i < cellSize; i++)
            {
                for (var j = 0; j < cellSize; j++)
                {
                    var oneCell = new List<IntVar>();
                    for (var di = 0; di < cellSize; di++)
                    {
                        for (var dj = 0; dj < cellSize; dj++)
                        {
                            oneCell.Add(grid[(i * cellSize + di, j * cellSize + dj)]);
                        }
                    }
                    model.AddAllDifferent(oneCell);
                }
            }

            // Add initial values constraints
            for (var i = 0; i < lineSize; i++)
            {
                for (var j = 0; j < lineSize; j++)
                {
                    if (initialGrid[i][j] != 0)
                    {
                        model.Add(grid[(i, j)] == initialGrid[i][j]);
                    }
                }
            }

            // Solve the model
            var solver = new CpSolver();
            var status = solver.Solve(model);

            if (status != CpSolverStatus.Optimal && status != CpSolverStatus.Feasible)
            {
                Console.WriteLine("No solution found.");
                return [];
            }

            stopwatch.Stop();
            Console.WriteLine($"solution ortools in {stopwatch.ElapsedMilliseconds / 1000.0}s :");

            var solution = new int[lineSize][]; 
            for (var i = 0; i < lineSize; i++)
            {
                var row = new List<int>();
                for (var j = 0; j < lineSize; j++)
                {
                    row.Add((int)solver.Value(grid[(i, j)]));
                }
                solution[i] = row.ToArray();
                Console.WriteLine("[" + string.Join(", ", row) + "]");
            }

            return solution;
        }
}