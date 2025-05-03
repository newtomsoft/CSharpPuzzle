using Microsoft.Z3;



using var ctx = new Context();
using var solver = ctx.MkSolver();

var x = ctx.MkIntConst("x");
var y = ctx.MkIntConst("y");

var constraint1 = ctx.MkGt(x, ctx.MkInt(0)); // x > 0
var constraint2 = ctx.MkGt(y, ctx.MkInt(0)); // y > 0
var constraint3 = ctx.MkEq(ctx.MkAdd(x, y), ctx.MkInt(10)); // x + y == 10

solver.Assert(constraint1);
solver.Assert(constraint2);
solver.Assert(constraint3);

if (solver.Check() == Status.SATISFIABLE)
{
    var model = solver.Model;
    Console.WriteLine("x = " + model.Evaluate(x));
    Console.WriteLine("y = " + model.Evaluate(y));
}
else
{
    Console.WriteLine("No solution found.");
}