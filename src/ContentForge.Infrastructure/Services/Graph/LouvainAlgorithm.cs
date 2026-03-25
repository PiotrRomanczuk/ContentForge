namespace ContentForge.Infrastructure.Services.Graph;

// Louvain community detection algorithm — finds clusters in a weighted graph.
//
// For JS devs: imagine you have a social network graph. Louvain finds groups of people
// who interact with each other more than with outsiders. It works by:
// 1. Starting with each node in its own community
// 2. Moving nodes to neighboring communities if it improves "modularity" (cluster quality)
// 3. Repeating until no more improvements can be made
//
// In ContentForge, nodes = content items, edges = shared entities weighted by salience.
// The algorithm groups content about similar topics into clusters.
//
// Reference: Blondel et al., "Fast unfolding of communities in large networks" (2008)
public static class LouvainAlgorithm
{
    /// <summary>
    /// Run Louvain community detection on a weighted undirected graph.
    /// Input: adjacency list (node → { neighbor → weight }).
    /// Output: dictionary of communityId → set of node IDs in that community.
    /// </summary>
    public static Dictionary<int, HashSet<Guid>> Detect(
        Dictionary<Guid, Dictionary<Guid, double>> graph)
    {
        if (graph.Count == 0)
            return new Dictionary<int, HashSet<Guid>>();

        // Initialize: each node starts in its own community.
        // Like: const community = new Map(); nodes.forEach((n, i) => community.set(n, i));
        var nodeList = graph.Keys.ToList();
        var nodeToCommunity = new Dictionary<Guid, int>();
        for (int i = 0; i < nodeList.Count; i++)
            nodeToCommunity[nodeList[i]] = i;

        // Total weight of all edges in the graph (needed for modularity calculation).
        // m = sum of all edge weights / 2 (since each edge is counted twice in undirected graph).
        var totalWeight = graph.Values.SelectMany(n => n.Values).Sum() / 2.0;
        if (totalWeight == 0) totalWeight = 1;

        // Phase 1: Local optimization — repeatedly move nodes to maximize modularity.
        // Like a gradient descent: keep making small improvements until convergence.
        bool improved;
        int iteration = 0;
        const int maxIterations = 100; // Prevent infinite loops on pathological graphs.

        do
        {
            improved = false;
            iteration++;

            foreach (var node in nodeList)
            {
                var currentCommunity = nodeToCommunity[node];
                var bestCommunity = currentCommunity;
                var bestGain = 0.0;

                // Calculate the modularity gain of moving this node to each neighbor's community.
                if (!graph.TryGetValue(node, out var neighbors)) continue;

                // k_i = degree (total weight) of node i
                var ki = neighbors.Values.Sum();

                // Find neighboring communities and their connection weights.
                var neighborCommunities = new Dictionary<int, double>();
                foreach (var (neighbor, weight) in neighbors)
                {
                    var neighborComm = nodeToCommunity[neighbor];
                    neighborCommunities.TryGetValue(neighborComm, out var existing);
                    neighborCommunities[neighborComm] = existing + weight;
                }

                // Try moving to each neighboring community.
                foreach (var (targetCommunity, ki_in) in neighborCommunities)
                {
                    if (targetCommunity == currentCommunity) continue;

                    // Modularity gain formula (ΔQ):
                    // ΔQ = [Σ_in + k_{i,in}] / (2m) - [(Σ_tot + k_i) / (2m)]²
                    //    - [Σ_in / (2m) - (Σ_tot / (2m))² - (k_i / (2m))²]
                    //
                    // Simplified: ΔQ ≈ ki_in / m - ki * sigma_tot / (2 * m²)
                    // where sigma_tot = total weight of edges to nodes in target community.
                    var sigmaTot = GetCommunityWeight(graph, nodeToCommunity, targetCommunity);
                    var gain = ki_in / totalWeight - (sigmaTot * ki) / (2.0 * totalWeight * totalWeight);

                    if (gain > bestGain)
                    {
                        bestGain = gain;
                        bestCommunity = targetCommunity;
                    }
                }

                // Move node to the best community if there's a positive gain.
                if (bestCommunity != currentCommunity)
                {
                    nodeToCommunity[node] = bestCommunity;
                    improved = true;
                }
            }
        } while (improved && iteration < maxIterations);

        // Phase 2: Aggregate — group nodes by their final community assignment.
        var communities = new Dictionary<int, HashSet<Guid>>();
        foreach (var (node, community) in nodeToCommunity)
        {
            if (!communities.ContainsKey(community))
                communities[community] = new HashSet<Guid>();
            communities[community].Add(node);
        }

        // Re-number communities sequentially (0, 1, 2, ...) for clean output.
        var renumbered = new Dictionary<int, HashSet<Guid>>();
        int newId = 0;
        foreach (var (_, members) in communities.OrderByDescending(c => c.Value.Count))
        {
            renumbered[newId++] = members;
        }

        return renumbered;
    }

    // Calculate total weight of edges connected to nodes in a specific community.
    // sigma_tot in the Louvain modularity formula.
    private static double GetCommunityWeight(
        Dictionary<Guid, Dictionary<Guid, double>> graph,
        Dictionary<Guid, int> nodeToCommunity,
        int communityId)
    {
        double total = 0;
        foreach (var (node, community) in nodeToCommunity)
        {
            if (community != communityId) continue;
            if (!graph.TryGetValue(node, out var neighbors)) continue;
            total += neighbors.Values.Sum();
        }
        return total;
    }
}
