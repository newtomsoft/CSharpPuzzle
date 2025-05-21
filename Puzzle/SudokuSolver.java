import com.microsoft.z3.*;
import java.util.stream.IntStream;

public class SudokuSolver {
    private static final int GridSize = 9;
    private static final int BlockSize = 3;
    private static final int MinValue = 1;
    private static final int MaxValue = GridSize;

    private final Context ctx;
    private final int[][] puzzle;
    private final Solver solver;
    private final int[][] solution;
    private final IntExpr[][] matrixExpr;

    public SudokuSolver(int[][] puzzle) {
        this.ctx = new Context();
        this.puzzle = puzzle;
        this.solver = ctx.mkSolver();
        this.matrixExpr = new IntExpr[GridSize][GridSize];
        this.solution = new int[GridSize][GridSize];
    }

    public int[][] SolveSudoku() {
        SetInitialPuzzleValues();
        AddValueRangeConstraints();
        AddDistinctRowConstraint();
        AddDistinctColumnConstraint();
        AddDistinctBlockConstraints();
        EvaluateSolverAndExtractSolution();
        return solution;
    }

    private void EvaluateSolverAndExtractSolution() {
        Status status = solver.check();
        if (status != Status.SATISFIABLE) return;
        Model model = solver.getModel();

        for (int i = 0; i < GridSize; i++) {
            for (int j = 0; j < GridSize; j++) {
                solution[i][j] = ((IntNum) model.evaluate(matrixExpr[i][j], true)).getInt();
            }
        }
    }

    private void SetInitialPuzzleValues() {
        for (int i = 0; i < GridSize; i++) {
            for (int j = 0; j < GridSize; j++) {
                matrixExpr[i][j] = ctx.mkIntConst("cell_" + i + "_" + j);
            }
        }

        for (int i = 0; i < GridSize; i++) {
            for (int j = 0; j < GridSize; j++) {
                if (puzzle[i][j] != 0) {
                    solver.add(ctx.mkEq(matrixExpr[i][j], ctx.mkInt(puzzle[i][j])));
                }
            }
        }
    }

    private void AddValueRangeConstraints() {
        for (IntExpr[] row : matrixExpr) {
            for (IntExpr cellExpr : row) {
                BoolExpr minConstraint = ctx.mkGe(cellExpr, ctx.mkInt(MinValue));
                BoolExpr maxConstraint = ctx.mkLe(cellExpr, ctx.mkInt(MaxValue));
                solver.add(minConstraint, maxConstraint);
            }
        }
    }

    private void AddDistinctRowConstraint() {
        for (int rowIndex = 0; rowIndex < GridSize; rowIndex++) {
            Expr[] rowValues = IntStream.range(0, GridSize)
                .mapToObj(columnIndex -> matrixExpr[rowIndex][columnIndex])
                .toArray(Expr[]::new);
            solver.add(ctx.mkDistinct(rowValues));
        }
    }

    private void AddDistinctColumnConstraint() {
        for (int columnIndex = 0; columnIndex < GridSize; columnIndex++) {
            Expr[] columnValues = IntStream.range(0, GridSize)
                .mapToObj(rowIndex -> matrixExpr[rowIndex][columnIndex])
                .toArray(Expr[]::new);
            solver.add(ctx.mkDistinct(columnValues));
        }
    }

    private void AddDistinctBlockConstraints() {
        for (int blockRow = 0; blockRow < BlockSize; blockRow++) {
            for (int blockCol = 0; blockCol < BlockSize; blockCol++) {
                Expr[] blockValues = new Expr[GridSize];
                int index = 0;

                for (int i = 0; i < BlockSize; i++) {
                    for (int j = 0; j < BlockSize; j++) {
                        blockValues[index++] = matrixExpr[blockRow * 3 + i][blockCol * 3 + j];
                    }
                }
                solver.add(ctx.mkDistinct(blockValues));
            }
        }
    }

    public void PrintPuzzle() {
        PrintMatrix(puzzle);
    }

    public void PrintSolution() {
        PrintMatrix(solution);
    }

    private static void PrintMatrix(int[][] matrix) {
        for (int i = 0; i < GridSize; i++) {
            if (i % BlockSize == 0 && i != 0) System.out.println("------+-------+------");
            for (int j = 0; j < GridSize; j++) {
                if (j % BlockSize == 0 && j != 0) System.out.print("| ");
                int value = matrix[i][j];
                if (value == 0) System.out.print(". ");
                else System.out.print(value + " ");
            }
            System.out.println();
        }
    }
}