namespace tochka_internship;

using System;
using System.Collections.Generic;
using System.Linq;

internal static class Run2
{
    private static bool IsGateway(string s) => s.Length > 0 && char.IsUpper(s[0]);

    private static Dictionary<string, HashSet<string>> BuildGraph(List<(string, string)> edges)
    {
        var g = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        foreach (var e in edges)
        {
            if (!g.ContainsKey(e.Item1)) g[e.Item1] = new HashSet<string>(StringComparer.Ordinal);
            if (!g.ContainsKey(e.Item2)) g[e.Item2] = new HashSet<string>(StringComparer.Ordinal);
            g[e.Item1].Add(e.Item2);
            g[e.Item2].Add(e.Item1);
        }
        if (!g.ContainsKey("a")) g["a"] = new HashSet<string>(StringComparer.Ordinal);
        return g;
    }

    private static Dictionary<string, int> Bfs(string start, Dictionary<string, HashSet<string>> g)
    {
        var dist = new Dictionary<string, int>(StringComparer.Ordinal);
        if (!g.ContainsKey(start)) return dist;
        var q = new Queue<string>();
        dist[start] = 0;
        q.Enqueue(start);
        while (q.Count > 0)
        {
            var v = q.Dequeue();
            var dv = dist[v] + 1;
            if (!g.TryGetValue(v, out var neigh)) continue;
            foreach (var u in neigh)
            {
                if (!dist.ContainsKey(u))
                {
                    dist[u] = dv;
                    q.Enqueue(u);
                }
            }
        }
        return dist;
    }

    private static string? CutWhenAdjacent(Dictionary<string, HashSet<string>> g, string v)
    {
        if (!g.TryGetValue(v, out var neigh)) return null;
        var cands = neigh.Where(IsGateway).OrderBy(x => x, StringComparer.Ordinal).ToList();
        if (cands.Count == 0) return null;
        return cands[0] + "-" + v;
    }

    private static string CutLexSmallest(Dictionary<string, HashSet<string>> g)
    {
        string? best = null;
        foreach (var gw in g.Keys.Where(IsGateway))
        {
            if (!g.TryGetValue(gw, out var neigh)) continue;
            foreach (var u in neigh)
            {
                if (IsGateway(u)) continue;
                var s = gw + "-" + u;
                if (best == null || string.CompareOrdinal(s, best) < 0) best = s;
            }
        }
        return best!;
    }

    private static void ApplyCut(Dictionary<string, HashSet<string>> g, string cut)
    {
        var p = cut.Split('-');
        var gw = p[0];
        var u = p[1];
        if (g.TryGetValue(gw, out var a)) a.Remove(u);
        if (g.TryGetValue(u, out var b)) b.Remove(gw);
    }

    private static string? ChooseGateway(Dictionary<string, HashSet<string>> g, string v)
    {
        var d = Bfs(v, g);
        var cands = new List<(string gw, int d)>();
        foreach (var gw in g.Keys.Where(IsGateway))
        {
            if (d.TryGetValue(gw, out var dist)) cands.Add((gw, dist));
        }
        if (cands.Count == 0) return null;
        var md = cands.Min(x => x.d);
        return cands.Where(x => x.d == md).Select(x => x.gw).OrderBy(x => x, StringComparer.Ordinal).First();
    }

    private static string Step(Dictionary<string, HashSet<string>> g, string v)
    {
        var gw = ChooseGateway(g, v);
        if (gw == null) return v;
        var d = Bfs(gw, g);
        if (!d.TryGetValue(v, out var dv)) return v;
        if (!g.TryGetValue(v, out var neigh)) return v;
        var next = neigh.Where(u => !IsGateway(u) && d.TryGetValue(u, out var du) && du == dv - 1).OrderBy(x => x, StringComparer.Ordinal).FirstOrDefault();
        return string.IsNullOrEmpty(next) ? v : next;
    }

    public static List<string> Solve(List<(string, string)> edges)
    {
        var g = BuildGraph(edges);
        var v = "a";
        var ans = new List<string>();
        while (true)
        {
            var reach = g.Keys.Where(IsGateway).Any(gw => Bfs(v, g).ContainsKey(gw));
            if (!reach) break;
            var cut = CutWhenAdjacent(g, v) ?? CutLexSmallest(g);
            ApplyCut(g, cut);
            ans.Add(cut);
            reach = g.Keys.Where(IsGateway).Any(gw => Bfs(v, g).ContainsKey(gw));
            if (!reach) break;
            v = Step(g, v);
        }
        return ans;
    }

    public static void Main2()
    {
        var edges = new List<(string, string)>();
        string? line;
        while ((line = Console.ReadLine()) != null)
        {
            line = line.Trim();
            if (line.Length == 0) continue;
            var parts = line.Split('-');
            if (parts.Length == 2) edges.Add((parts[0], parts[1]));
        }
        var res = Solve(edges);
        foreach (var s in res) Console.WriteLine(s);
    }
}
