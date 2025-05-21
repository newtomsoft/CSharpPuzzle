#include <z3++.h>
#include <vector>
#include <iostream>
using namespace z3;

class SudokuSolver {
private:
    static const int GridSize = 9;
    static const int BlockSize = 3;
    static const int MinValue = 1;
    static const int MaxValue = GridSize;

    context ctx;
    const std::vector<std::vector<int>> puzzle;
    solver solver;
    std::vector<std::vector<int>> solution;
    std::vector<std::vector<expr>> matrixExpr;

public:
    SudokuSolver(const std::vector<std::vector<int>>& puzzle)
        : puzzle(puzzle), solver(ctx), solution(GridSize, std::vector<int>(GridSize, 0)),
          matrixExpr(GridSize, std::vector<expr>(GridSize, ctx.int_const("default"))) {}

    std::vector<std::vector<int>> SolveSudoku() {
        SetInitialPuzzleValues();
        AddValueRangeConstraints();
        AddDistinctRowConstraint();
        AddDistinctColumnConstraint();
        AddDistinctBlockConstraints();
        EvaluateSolverAndExtractSolution();
        return solution;
    }

private:
    void EvaluateSolverAndExtractSolution() {
        check_result status = solver.check();
        if (status != sat) return;
        model model = solver.get_model();

        for (int i = 0; i < GridSize; i++) {
            for (int j = 0; j < GridSize; j++) {
                solution[i][j] = model.eval(matrixExpr[i][j]).get_numeral_int();
            }
        }
    }

    void SetInitialPuzzleValues() {
        for (int i = 0; i < GridSize; i++) {
            for (int j = 0; j < GridSize; j++) {
                matrixExpr[i][j] = ctx.int_const(("cell_" + std::to_string(i) + "_" + std::to_string(j)).c_str());
            }
        }

        for (int i = 0; i < GridSize; i++) {
            for (int j = 0; j < GridSize; j++) {
                if (puzzle[i][j] != 0) {
                    solver.add(matrixExpr[i][j] == puzzle[i][j]);
                }
            }
        }
    }

    void AddValueRangeConstraints() {
        for (const auto& row : matrixExpr) {
            for (const auto& cellExpr : row) {
                solver.add(cellExpr >= MinValue && cellExpr <= MaxValue);
            }
        }
    }

    void AddDistinctRowConstraint() {
        for (int rowIndex = 0; rowIndex < GridSize; rowIndex++) {
            expr_vector rowValues(ctx);
            for (int columnIndex = 0; columnIndex < GridSize; columnIndex++) {
                rowValues.push_back(matrixExpr[rowIndex][columnIndex]);
            }
            solver.add(distinct(rowValues));
        }
    }

    void AddDistinctColumnConstraint() {
        for (int columnIndex = 0; columnIndex < GridSize; columnIndex++) {
            expr_vector columnValues(ctx);
            for (int rowIndex = 0; rowIndex < GridSize; rowIndex++) {
                columnValues.push_back(matrixExpr[rowIndex][columnIndex]);
            }
            solver.add(distinct(columnValues));
        }
    }

    void AddDistinctBlockConstraints() {
        for (int blockRow = 0; blockRow < BlockSize; blockRow++) {
            for (int blockCol = 0; blockCol < BlockSize; blockCol++) {
                expr_vector blockValues(ctx);
                for (int i = 0; i < BlockSize; i++) {
                    for (int j = 0; j < BlockSize; j++) {
                        blockValues.push_back(matrixExpr[blockRow * 3 + i][blockCol * 3 + j]);
                    }
                }
                solver.add(distinct(blockValues));
            }
        }
    }

public:
    void PrintPuzzle() {
        PrintMatrix(puzzle);
    }

    void PrintSolution() {
        PrintMatrix(solution);
    }

private:
    static void PrintMatrix(const std::vector<std::vector<int>>& matrix) {
        for (int i = 0; i < GridSize; i++) {
            if (i % BlockSize == 0 && i != 0) std::cout << "------+-------+------\n";
            for (int j = 0; j < GridSize; j++) {
                if (j % BlockSize == 0 && j != 0) std::cout << "| ";
                int value = matrix[i][j];
                if (value == 0) std::cout << ". ";
                else std::cout << value << " ";
            }
            std::cout << "\n";
        }
    }
};