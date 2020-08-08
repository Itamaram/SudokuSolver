using System;
using System.Linq;
using NUnit.Framework;

namespace SudokuSolver.Tests
{
    public class Tests
    {
        [Test]
        public void Test1()
        {
            var puzzle = new ClassicSudokuParser().Parse(new[]
            {
                ".........",
                ".3....16.",
                ".67.35..4",
                "6.812.9..",
                ".9..8..3.",
                "..2.798.6",
                "8..69.35.",
                ".26....9.",
                "........."
            });

            var solution = new NaiveSolver().Solve(puzzle).Single();

            for (var i = 0; i < 9; i++)
            {
                for (var j = 0; j < 9; j++) Console.Write(solution[$"{i}{j}"]);
                Console.WriteLine();
            }
        }

        [Test]
        public void Test2()
        {
            var puzzle = new ClassicSudokuParser().Parse(new[]
            {
                "2.5......",
                "3.86..9..",
                "...1..4..",
                "....5..1.",
                "....9..2.",
                "87..2....",
                "....89..3",
                "..6..3..5",
                "5.4.....1"
            });

            var solution = new NaiveSolver().Solve(puzzle).Single();

            for (var i = 0; i < 9; i++)
            {
                for (var j = 0; j < 9; j++)
                {
                    Console.Write(solution[$"{i}{j}"]);
                    if (j % 3 == 2)
                        Console.Write(" ");
                }

                if (i % 3 == 2)
                    Console.WriteLine();
                Console.WriteLine();
            }
        }


        [Test]
        public void NonUnique()
        {
            var puzzle = new ClassicSudokuParser().Parse(new[]
            {
                "174832596",
                "593461278",
                "..2957..1",
                "..75..9..",
                ".197.36.5",
                "435.968.7",
                "3.16..759",
                "9.8.75.6.",
                "7563.9.82",
            });

            foreach (var solution in new NaiveSolver().Solve(puzzle))
                for (var i = 0; i < 9; i++)
                {
                    for (var j = 0; j < 9; j++)
                    {
                        Console.Write(solution[$"{i}{j}"]);
                        if (j % 3 == 2)
                            Console.Write(" ");
                    }

                    if (i % 3 == 2)
                        Console.WriteLine();
                    Console.WriteLine();
                }
        }

        [Test]
        public void RainbowTest()
        {
            var puzzle = new RainbowCircleSudokuParser().Parse(new[]
            {
                "1  5  .  .  .  .",
                ". 12  9  3  .  .",
                ".  . 10  4  .  .",
                ".  .  .  2  6  9",
                ".  .  .  . 12  1",
                "4  7  .  .  .  5",
                "9  6  .  .  .  .",
                ".  8  4  1  .  .",
                ".  . 12  6  .  .",
                ".  .  . 11  1  4",
                ".  .  .  .  7  2",
                "11 9  .  .  .  3",
            });

            var solution = new NaiveSolver().Solve(puzzle).Single();

            for (var i = 0; i < 12; i++)
            {
                for (var j = 0; j < 6; j++) Console.Write(solution[$"{(char) ('A' + i)}{j}"] + " ");
                Console.WriteLine();
            }
        }
    }
}