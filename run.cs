namespace tochka_internship;

using System;
using System.Collections.Generic;
using System.Linq;

internal static class Run
{
    private static readonly int[] RoomEntrance = [2, 4, 6, 8];
    private static readonly int[] HallwayStops = [0, 1, 3, 5, 7, 9, 10];
    private static readonly int[] Cost = [1, 10, 100, 1000];

    private static void Main()
    {
        var lines = new List<string>();
        while (Console.ReadLine() is { } line)
        {
            if (line.Length == 0)
                continue;
            lines.Add(line);
        }
        if (lines.Count < 5)
            return;
        var depth = lines.Count - 3;
        var totalCells = 11 + 4 * depth;
        var stateChars = new char[totalCells];
        var hallway = lines[1].Substring(1, 11);
        for (var i = 0; i < 11; i++)
            stateChars[i] = hallway[i];
        for (var y = 0; y < depth; y++)
        {
            var letters = lines[2 + y].Where(c => c is >= 'A' and <= 'D').ToArray();
            for (var room = 0; room < 4; room++)
                stateChars[11 + room * depth + y] = letters[room];
        }
        var start = new string(stateChars);
        var goalChars = new char[totalCells];
        for (var i = 0; i < 11; i++)
            goalChars[i] = '.';
        for (var room = 0; room < 4; room++)
        {
            for (var y = 0; y < depth; y++)
                goalChars[11 + room * depth + y] = (char)('A' + room);
        }
        var goal = new string(goalChars);
        var result = Solve(start, goal, depth);
        Console.WriteLine(result);
    }

    private static int Solve(string start, string goal, int depth)
    {
        var best = new Dictionary<string, int> { [start] = 0 };
        var queue = new PriorityQueue<string, int>();
        queue.Enqueue(start, 0);
        while (queue.TryDequeue(out var state, out var energy))
        {
            if (best.TryGetValue(state, out var current) && energy > current)
                continue;
            if (state == goal)
                return energy;
            foreach (var (nextState, moveCost) in GenerateMoves(state, depth))
            {
                var nextEnergy = energy + moveCost;
                if (!best.TryGetValue(nextState, out var existing) || nextEnergy < existing)
                {
                    best[nextState] = nextEnergy;
                    queue.Enqueue(nextState, nextEnergy);
                }
            }
        }
        return -1;
    }

    private static IEnumerable<(string state, int cost)> GenerateMoves(string state, int depth)
    {
        var chars = state.ToCharArray();
        for (var h = 0; h < 11; h++)
        {
            var occupant = chars[h];
            if (occupant == '.')
                continue;
            var target = occupant - 'A';
            var entrance = RoomEntrance[target];
            if (!PathClear(chars, h, entrance))
                continue;
            var roomStart = 11 + target * depth;
            var acceptable = true;
            for (var i = 0; i < depth; i++)
            {
                var c = chars[roomStart + i];
                if (c != '.' && c != occupant)
                {
                    acceptable = false;
                    break;
                }
            }
            if (!acceptable)
                continue;
            var insertIndex = -1;
            for (var i = depth - 1; i >= 0; i--)
            {
                var idx = roomStart + i;
                if (chars[idx] == '.')
                {
                    insertIndex = idx;
                    break;
                }
            }
            if (insertIndex == -1)
                continue;
            var steps = Math.Abs(h - entrance) + (insertIndex - roomStart + 1);
            var cost = steps * Cost[target];
            var next = (char[])chars.Clone();
            next[h] = '.';
            next[insertIndex] = occupant;
            yield return (new string(next), cost);
        }
        for (var room = 0; room < 4; room++)
        {
            var roomStart = 11 + room * depth;
            var targetChar = (char)('A' + room);
            var allCorrect = true;
            for (var i = 0; i < depth; i++)
            {
                var c = chars[roomStart + i];
                if (c != '.' && c != targetChar)
                {
                    allCorrect = false;
                    break;
                }
            }
            if (allCorrect)
                continue;
            var occupantIndex = -1;
            char occupant = '.';
            for (var i = 0; i < depth; i++)
            {
                var c = chars[roomStart + i];
                if (c == '.')
                    continue;
                occupantIndex = roomStart + i;
                occupant = c;
                break;
            }
            if (occupantIndex == -1)
                continue;
            var entrance = RoomEntrance[room];
            if (chars[entrance] != '.')
                continue;
            var stepsOut = occupantIndex - roomStart + 1;
            var typeIndex = occupant - 'A';
            foreach (var h in HallwayStops)
            {
                if (!PathClear(chars, entrance, h))
                    continue;
                var steps = stepsOut + Math.Abs(h - entrance);
                var cost = steps * Cost[typeIndex];
                var next = (char[])chars.Clone();
                next[occupantIndex] = '.';
                next[h] = occupant;
                yield return (new string(next), cost);
            }
        }
    }

    private static bool PathClear(char[] state, int from, int to)
    {
        if (from < to)
        {
            for (var x = from + 1; x <= to; x++)
            {
                if (state[x] != '.')
                    return false;
            }
            return true;
        }
        for (var x = from - 1; x >= to; x--)
        {
            if (state[x] != '.')
                return false;
        }
        return true;
    }
}
