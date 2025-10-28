namespace tochka_internship;

using System;
using System.Collections.Generic;
using System.Linq;

internal static class Run
{
    private class State
    {
        public readonly Dictionary<(int x, int y), int> Occupied;
        public int Energy;
        
        public State()
        {
            Occupied = new();
            Energy = 0;
        }
        
        public State(State other)
        {
            Occupied = new(other.Occupied);
            Energy = other.Energy;
        }
        
        public string GetKey()
        {
            var parts = Occupied.OrderBy(kv => kv.Key)
                .Select(kv => $"{kv.Key.x},{kv.Key.y}:{kv.Value}")
                .ToList();
            return string.Join("|", parts);
        }
        
        public bool Equals(State other)
        {
            if (Occupied.Count != other.Occupied.Count) return false;
            foreach (var kv in Occupied)
            {
                if (!other.Occupied.TryGetValue(kv.Key, out var value) || value != kv.Value)
                    return false;
            }
            return true;
        }
    }

    private static int depth;
    private static readonly int[] Cost = [1, 10, 100, 1000];
    private static readonly int[] TargetX = [2, 4, 6, 8];
    private static readonly int[] ForbiddenX = [2, 4, 6, 8];

    private static int Solve(List<string> lines)
    {
        depth = lines.Count - 3;
        var state = new State();
        
        for (var lineIdx = 0; lineIdx < lines.Count; lineIdx++)
        {
            var line = lines[lineIdx];
            
            if (lineIdx == 1)
            {
                for (var charIdx = 1; charIdx < line.Length - 1; charIdx++)
                {
                    var c = line[charIdx];
                    if (c is >= 'A' and <= 'D')
                    {
                        var type = c - 'A';
                        var x = charIdx - 1;
                        state.Occupied[(x, 0)] = type;
                    }
                }
            }
            else if (lineIdx > 1 && lineIdx < lines.Count - 1)
            {
                var trimmed = line.TrimStart();
                
                for (var i = 1; i < trimmed.Length; i += 2)
                {
                    if (i >= trimmed.Length) break;
                    var c = trimmed[i];
                    if (c is >= 'A' and <= 'D')
                    {
                        var type = c - 'A';
                        var x = (i + 1) / 2 * 2;
                        var y = lineIdx - 1;
                        state.Occupied[(x, y)] = type;
                    }
                }
            }
        }
        
        var queue = new List<(State state, int energy)> { (state, 0) };

        var visited = new HashSet<string> { state.GetKey() };

        while (queue.Count > 0)
        {
            var minIdx = 0;
            for (var i = 1; i < queue.Count; i++)
            {
                if (queue[i].energy < queue[minIdx].energy)
                    minIdx = i;
            }
            
            var current = queue[minIdx];
            queue.RemoveAt(minIdx);

            if (IsGoal(current.state))
            {
                return current.energy;
            }

            queue.AddRange(from nextState in GetNextStates(current.state) let key = nextState.GetKey() where visited.Add(key) select (nextState, nextState.Energy));
        }
        
        return 0;
    }

    private static bool IsGoal(State state)
    {
        foreach (var kv in state.Occupied)
        {
            var x = kv.Key.x;
            var y = kv.Key.y;
            var type = kv.Value;
            
            if (y == 0)
                return false;
            
            if (x != TargetX[type])
                return false;
            
            if (y < 1 || y > depth)
                return false;
        }
        
        for (var type = 0; type < 4; type++)
        {
            var count = state.Occupied.Count(kv => kv.Key.x == TargetX[type] && kv.Value == type);
            if (count != depth)
                return false;
        }
        
        return true;
    }

    private static List<State> GetNextStates(State state)
    {
        var nextStates = new List<State>();
        
        foreach (var pos in state.Occupied.ToList())
        {
            var x = pos.Key.x;
            var y = pos.Key.y;
            var type = pos.Value;
            
            if (y == 0)
            {
                if (!CanMoveToTargetRoom(state, type, x))
                    continue;
                
                var targetX = TargetX[type];
                if (!CanMoveHorizontally(state, x, targetX))
                    continue;
                
                var targetY = GetTargetRoomY(state, type);
                if (targetY == -1)
                    continue;
                
                var newState = new State(state);
                newState.Occupied.Remove((x, y));
                newState.Occupied[(targetX, targetY)] = type;
                
                var steps = Math.Abs(x - targetX) + y + targetY;
                newState.Energy += steps * Cost[type];
                
                nextStates.Add(newState);
            }
            else
            {
                if (y > 0 && !CanExitRoom(state, x, y))
                    continue;
                
                var roomX = x;
                var stepsOut = y;
                var hallwayY = 0;
                
                for (var hallwayX = 0; hallwayX < 11; hallwayX++)
                {
                    if (ForbiddenX.Contains(hallwayX))
                        continue;
                    
                    var totalSteps = stepsOut + Math.Abs(hallwayX - roomX);
                    
                    if (CanMoveToHallway(state, roomX, hallwayX))
                    {
                        var newState = new State(state);
                        newState.Occupied.Remove((x, y));
                        newState.Occupied[(hallwayX, hallwayY)] = type;
                        newState.Energy += totalSteps * Cost[type];
                        
                        nextStates.Add(newState);
                    }
                }
            }
        }
        
        return nextStates;
    }

    private static bool CanExitRoom(State state, int x, int y)
    {
        for (var checkY = 1; checkY < y; checkY++)
        {
            if (state.Occupied.ContainsKey((x, checkY)))
                return false;
        }
        return true;
    }

    private static bool CanMoveToHallway(State state, int fromX, int toX)
    {
        var step = fromX < toX ? 1 : -1;
        for (var x = fromX; x != toX + step; x += step)
        {
            if (state.Occupied.ContainsKey((x, 0)))
                return false;
        }
        return true;
    }

    private static bool CanMoveHorizontally(State state, int fromX, int toX)
    {
        var step = fromX < toX ? 1 : -1;
        for (var x = fromX + step; x != toX + step; x += step)
        {
            if (state.Occupied.ContainsKey((x, 0)))
                return false;
        }
        return true;
    }

    private static bool CanMoveToTargetRoom(State state, int type, int fromX)
    {
        var targetX = TargetX[type];
        var roomY = GetTargetRoomY(state, type);
        
        if (roomY == -1)
            return false;
        
        if (!CanMoveHorizontally(state, fromX, targetX))
            return false;
        
        for (var y = 1; y <= depth; y++)
        {
            if (state.Occupied.TryGetValue((targetX, y), out var existingType) && existingType != type)
                return false;
        }
        
        return true;
    }

    private static int GetTargetRoomY(State state, int type)
    {
        var targetX = TargetX[type];
        var deepestFree = -1;
        
        for (var y = depth; y >= 1; y--)
        {
            if (state.Occupied.ContainsKey((targetX, y)))
            {
                if (state.Occupied[(targetX, y)] != type)
                    return -1;
            }
            else
            {
                deepestFree = y;
                break;
            }
        }
        
        return deepestFree;
    }

    private static void Main()
    {
        var lines = new List<string>();

        while (Console.ReadLine() is { } line)
        {
            if (line == string.Empty)
                break;
            lines.Add(line);
        }

        var result = Solve(lines);
        Console.WriteLine(result);
    }
}
