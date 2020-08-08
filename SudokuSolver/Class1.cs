using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolver
{
    public interface Sudoku
    {
        IEnumerable<int> Alphabet { get; }
        IEnumerable<string> Cells { get; }
        IEnumerable<Condition> Conditions { get; }
    }

    public class SudokuPuzzle
    {
        public Sudoku Sudoku { get; set; }
        public IReadOnlyDictionary<string, int> Setup { get; set; }
    }

    public interface Solver
    {
        IEnumerable<IReadOnlyDictionary<string, int>> Solve(SudokuPuzzle puzzle);
    }

    public class NaiveSolver : Solver
    {
        private class Cell
        {
            public Cell(string key, IEnumerable<int> candidates, bool visited = false)
            {
                Key = key;
                Candidates = candidates.ToList();
                Visited = visited;
            }

            public string Key { get; }
            public IReadOnlyList<int> Candidates { get; }
            public bool Visited { get; }
        }

        private class State : IEnumerable<Cell>
        {
            private readonly IReadOnlyDictionary<string, Cell> store;

            private State(IEnumerable<Cell> cells)
            {
                store = cells.ToDictionary(c => c.Key);
            }

            public State(SudokuPuzzle puzzle)
            : this(puzzle.Sudoku.Cells.Select(c => new Cell(c, puzzle.Setup.TryGetValue(c, out var value) ? new List<int> { value } : puzzle.Sudoku.Alphabet)))
            { }

            public State Update(Cell cell)
            {
                return new State(store.Values.Select(c => c.Key == cell.Key ? cell : c));
            }

            public Cell this[string cell] => store[cell];

            public IEnumerator<Cell> GetEnumerator() => store.Values.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private IEnumerable<State> Visit(SudokuPuzzle puzzle, State state, string cell)
        {
            var neighbours = puzzle.Sudoku.Conditions.Where(x => x.Cells.Contains(cell))
                .SelectMany(x => x.Cells)
                .Where(x => x != cell && !state[x].Visited)
                .Distinct()
                .ToList();

            foreach (var candidate in state[cell].Candidates)
            {
                yield return neighbours.Aggregate(
                    state.Update(new Cell(cell, new[] { candidate }, true)),
                    (s, n) => s.Update(new Cell(n, s[n].Candidates.Where(x => x != candidate)))
                );
            }
        }

        public IEnumerable<IReadOnlyDictionary<string, int>> Solve(SudokuPuzzle puzzle)
        {
            var state = puzzle.Setup.Aggregate(new State(puzzle), (s, kvp) => Visit(puzzle, s, kvp.Key).Single());

            return Solve(puzzle, state)
                .Select(s => s.ToDictionary(c => c.Key, c => c.Candidates.Single()));
        }

        private IEnumerable<State> Solve(SudokuPuzzle puzzle, State state)
        {
            var next = state.Where(s => !s.Visited).OrderBy(s => s.Candidates.Count).FirstOrDefault();

            return next == null ? new[] { state } : Visit(puzzle, state, next.Key).SelectMany(s => Solve(puzzle, s));
        }
    }

    public class Condition
    {
        public Condition(IEnumerable<string> cells, IEnumerable<int> symbols)
        {
            Cells = cells.ToList();
            Symbols = symbols.ToList();

            if (Cells.Count() != Symbols.Count())
                throw new ArgumentException("Invalid condition, cell count does not match symbol count");
        }

        public IEnumerable<string> Cells { get; }
        public IEnumerable<int> Symbols { get; }
    }

    public class ClassicSudoku : Sudoku
    {
        public IEnumerable<int> Alphabet { get; }
        public IEnumerable<string> Cells { get; }
        public IEnumerable<Condition> Conditions { get; }

        public ClassicSudoku()
        {
            Alphabet = Enumerable.Range(1, 9).ToList();
            Cells = Enumerable.Range(0, 81).Select(i => $"{i / 9}{i % 9}").ToList();
            Conditions = Rows.Concat(Cols).Concat(Blocks);
        }

        private IEnumerable<Condition> Rows => Enumerable.Range(0, 9).Select(i =>
                Enumerable.Range(0, 9).Select(j => $"{i}{j}"))
            .Select(row => new Condition(row, Alphabet));

        private IEnumerable<Condition> Cols => Enumerable.Range(0, 9).Select(i =>
                Enumerable.Range(0, 9).Select(j => $"{j}{i}"))
            .Select(col => new Condition(col, Alphabet));

        private IEnumerable<Condition> Blocks => Enumerable.Range(0, 9).Select(i =>
                Enumerable.Range(0, 3).SelectMany(x => Enumerable.Range(0, 3).Select(y => $"{3 * (i % 3) + x}{3 * (i / 3) + y}"))
            ).Select(block => new Condition(block, Alphabet));
    }

    public class ClassicSudokuParser
    {
        public SudokuPuzzle Parse(IEnumerable<string> puzzle)
        {
            return new SudokuPuzzle
            {
                Sudoku = new ClassicSudoku(),
                Setup = puzzle.SelectMany(ParseRow).ToDictionary(x => x.Key, x => x.Value)
            };
        }

        private IEnumerable<KeyValuePair<string, int>> ParseRow(string r, int i)
        {
            for(var j = 0;j < r.Length;j++)
                if (int.TryParse($"{r[j]}", out var value))
                    yield return new KeyValuePair<string, int>($"{i}{j}", value);
        }
    }

    public class RainbowCircleSudoku : Sudoku
    {
        public IEnumerable<int> Alphabet { get; } 

        public IEnumerable<string> Cells { get; }

        public IEnumerable<Condition> Conditions { get; }

        public RainbowCircleSudoku()
        {
            Alphabet = Enumerable.Range(1, 12);
            Cells = Angles.SelectMany(t => Radii.Select(r => $"{t}{r}"));
            Conditions = Rings.Concat(Wedges).Concat(Across);
        }

        private IEnumerable<Condition> Rings =>
            Radii.Select(r => new Condition(Angles.Select(t => $"{t}{r}"), Alphabet));

        private IEnumerable<Condition> Wedges =>
            Enumerable.Range(0, 6).Select(i => Angles.Where((_, x) => x / 2 == i))
                .Select(angles => angles.SelectMany(t => Radii.Select(r => $"{t}{r}")))
                .Select(xs => new Condition(xs, Alphabet));
        
        private IEnumerable<Condition> Across =>
            Enumerable.Range(0, 6).Select(i => Angles.Where((_, x) => x % 6 == i))
                .Select(angles => angles.SelectMany(t => Radii.Select(r => $"{t}{r}")))
                .Select(xs => new Condition(xs, Alphabet));

        private static IEnumerable<string> Angles { get; } = Enumerable.Range(0, 12).Select(t => ((char) ('A' + t)).ToString()).ToList();

        private static IEnumerable<int> Radii { get; } = Enumerable.Range(0, 6).ToList();
    }

    public class RainbowCircleSudokuParser
    {
        public SudokuPuzzle Parse(IEnumerable<string> puzzle)
        {
            return new SudokuPuzzle
            {
                Sudoku = new RainbowCircleSudoku(),
                Setup = puzzle.SelectMany(ParseRow).ToDictionary(x => x.Key, x => x.Value)
            };
        }

        private IEnumerable<KeyValuePair<string, int>> ParseRow(string r, int i)
        {
            var entries = r.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).ToList();
            for (var j = 0; j < entries.Count; j++)
                if (int.TryParse($"{entries[j]}", out var value))
                    yield return new KeyValuePair<string, int>($"{(char) ('A' + i)}{j}", value);
        }
    }
}
