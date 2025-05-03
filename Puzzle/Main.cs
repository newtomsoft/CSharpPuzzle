using Puzzle;

Console.WriteLine("Sudoku Solver avec Z3");
Console.WriteLine("----------------------\n");

int[,] puzzle =
{
    { 5, 3, 0, 0, 7, 0, 0, 0, 0 },
    { 6, 0, 0, 1, 9, 5, 0, 0, 0 },
    { 0, 9, 8, 0, 0, 0, 0, 6, 0 },
    { 8, 0, 0, 0, 6, 0, 0, 0, 3 },
    { 4, 0, 0, 8, 0, 3, 0, 0, 1 },
    { 7, 0, 0, 0, 2, 0, 0, 0, 6 },
    { 0, 6, 0, 0, 0, 0, 2, 8, 0 },
    { 0, 0, 0, 4, 1, 9, 0, 0, 5 },
    { 0, 0, 0, 0, 8, 0, 0, 7, 9 }
};

Console.WriteLine("Grille initiale :");

SudokuSolver solver = new(puzzle);
solver.PrintPuzzle();

var solution = solver.SolveSudoku();
var isEmpty = solution.Cast<int>().SequenceEqual(new int[9, 9].Cast<int>());
if (isEmpty)
{
    Console.WriteLine("\nPas de solution trouvée !");
    return;
}

Console.WriteLine("\nSolution :");
solver.PrintSolution();