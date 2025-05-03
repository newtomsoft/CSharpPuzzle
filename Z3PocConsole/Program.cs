using Microsoft.Z3;

namespace Z3PocConsole;

class Program
{
    private static void AddSingleRegionsConstraints(Context ctx, Solver solver, IntExpr[][] matrix,
        IntExpr[][][] steps, List<int> regionsSize, int n, int m)
    {
        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j < m; j++)
            {
                solver.Add(ctx.MkGe(matrix[i][j], ctx.MkInt(1)));
                solver.Add(ctx.MkLe(matrix[i][j], ctx.MkInt(steps.Length)));
            }
        }

        for (var index = 0; index < steps.Length; index++)
        {
            var regionId = index + 1;
            AddConnectedCellsInRegionConstraints(ctx, solver, matrix, steps[index],
                regionsSize[index], n, m, regionId);
        }
    }

    private static void AddConnectedCellsInRegionConstraints(Context ctx, Solver solver, IntExpr[][] matrix, IntExpr[][] step, int regionSize, int n, int m, int regionId)
    {
        var cellsInRegion = new BoolExpr[n * m];
        var idx = 0;
        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j < m; j++) cellsInRegion[idx++] = ctx.MkEq(matrix[i][j], ctx.MkInt(regionId));
        }

        solver.Add(ctx.MkEq(ctx.MkAdd(cellsInRegion.Select(b => (ArithExpr)ctx.MkITE(b, ctx.MkInt(1), ctx.MkInt(0))).ToArray()), ctx.MkInt(regionSize)));

        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j < m; j++)
            {
                solver.Add(ctx.MkEq(step[i][j],
                    ctx.MkITE(
                        ctx.MkEq(matrix[i][j], ctx.MkInt(regionId)),
                        step[i][j],
                        ctx.MkInt(0)
                    )
                ));
            }
        }

        var roots = new List<BoolExpr>();
        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j < m; j++)
            {
                roots.Add(ctx.MkAnd(
                    ctx.MkEq(matrix[i][j], ctx.MkInt(regionId)),
                    ctx.MkEq(step[i][j], ctx.MkInt(1))
                ));
            }
        }

        solver.Add(ctx.MkOr(roots.ToArray()));

        for (var i = 0; i < roots.Count; i++)
        for (var j = i + 1; j < roots.Count; j++)
            solver.Add(ctx.MkNot(ctx.MkAnd(roots[i], roots[j])));

        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j < m; j++)
            {
                var currentStep = step[i][j];
                var adjacent = new List<BoolExpr>();

                if (i > 0)
                {
                    adjacent.Add(ctx.MkAnd(
                        ctx.MkEq(matrix[i - 1][j], ctx.MkInt(regionId)),
                        ctx.MkEq(step[i - 1][j], ctx.MkSub(currentStep, ctx.MkInt(1)))
                    ));
                }

                if (i < n - 1)
                {
                    adjacent.Add(ctx.MkAnd(
                        ctx.MkEq(matrix[i + 1][j], ctx.MkInt(regionId)),
                        ctx.MkEq(step[i + 1][j], ctx.MkSub(currentStep, ctx.MkInt(1)))
                    ));
                }

                if (j > 0)
                {
                    adjacent.Add(ctx.MkAnd(
                        ctx.MkEq(matrix[i][j - 1], ctx.MkInt(regionId)),
                        ctx.MkEq(step[i][j - 1], ctx.MkSub(currentStep, ctx.MkInt(1)))
                    ));
                }

                if (j < m - 1)
                {
                    adjacent.Add(ctx.MkAnd(
                        ctx.MkEq(matrix[i][j + 1], ctx.MkInt(regionId)),
                        ctx.MkEq(step[i][j + 1], ctx.MkSub(currentStep, ctx.MkInt(1)))
                    ));
                }

                solver.Add(ctx.MkImplies(
                    ctx.MkAnd(
                        ctx.MkEq(matrix[i][j], ctx.MkInt(regionId)),
                        ctx.MkGt(currentStep, ctx.MkInt(1))
                    ),
                    ctx.MkOr(adjacent.ToArray())
                ));
            }
        }
    }

    private static void PrintSolved(int[][] matrix, int n, int m)
    {
        for (var i = 0; i < n; i++)
        {
            var row = new string[m];
            for (var j = 0; j < m; j++)
                row[j] = matrix[i][j] > 0 ? matrix[i][j].ToString() : ".";

            Console.WriteLine(string.Join(" ", row));
        }

        Console.WriteLine();
    }

    static void PrintStepSolved(int[][] matrix, int n, int m)
    {
        for (var i = 0; i < n; i++)
        {
            var row = new string[m];
            for (var j = 0; j < m; j++) row[j] = matrix[i][j] > 0 ? matrix[i][j].ToString() : ".";

            Console.WriteLine(string.Join(" ", row));
        }

        Console.WriteLine();
    }

    private static void Exclude(Context ctx, Solver solver, IntExpr[][] matrix, int[][] matrixSolved, int n, int m)
    {
        var conditions = new BoolExpr[n * m];
        var idx = 0;
        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j < m; j++)
                conditions[idx++] = ctx.MkEq(matrix[i][j], ctx.MkInt(matrixSolved[i][j]));
        }

        solver.Add(ctx.MkNot(ctx.MkAnd(conditions)));
    }

    private static void SolveConnectedRegion(int n, int m)
    {
        using var ctx = new Context();
        using var solver = ctx.MkSolver();

        var matrix = new IntExpr[n][];
        for (var i = 0; i < n; i++)
        {
            matrix[i] = new IntExpr[m];
            for (var j = 0; j < m; j++)
                matrix[i][j] = ctx.MkIntConst($"cell_{i}_{j}");
        }

        const int stepsNumber = 2;
        var steps = new IntExpr[stepsNumber][][];
        for (var s = 0; s < stepsNumber; s++)
        {
            steps[s] = new IntExpr[n][];
            for (var i = 0; i < n; i++)
            {
                steps[s][i] = new IntExpr[m];
                for (var j = 0; j < m; j++)
                    steps[s][i][j] = ctx.MkIntConst($"step{s}_{i}_{j}");
            }
        }

        var sizes = new List<int> { 7, 9 };

        AddSingleRegionsConstraints(ctx, solver, matrix, steps, sizes, n, m);

        var count = 0;
        while (solver.Check() == Status.SATISFIABLE)
        {
            count++;
            var model = solver.Model;
            var matrixSolved = new int[n][];
            for (var i = 0; i < n; i++)
            {
                matrixSolved[i] = new int[m];
                for (var j = 0; j < m; j++)
                    matrixSolved[i][j] = ((IntNum)model.Evaluate(matrix[i][j])).Int;
            }

            PrintSolved(matrixSolved, n, m);
            Exclude(ctx, solver, matrix, matrixSolved, n, m);
            // Console.WriteLine("Appuyez sur ENTRÉE pour continuer...");
            // Console.ReadLine();
        }

        Console.WriteLine("solutions count " + count);
    }

    private static void Main(string[] args)
    {
        SolveConnectedRegion(4, 4);
    }
}