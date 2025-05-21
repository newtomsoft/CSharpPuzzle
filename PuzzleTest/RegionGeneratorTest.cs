using Puzzle;
using Shouldly;

namespace PuzzleTest;

public class RegionGeneratorTest
{
    [Fact]
    public void Test()
    {
        var count = RegionGenerator.SolveConnectedRegion(5, 4);
        count.ShouldBe(12932);
    }
}