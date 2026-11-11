using ResQ.API.Risk_Detection.Domain.Model.Aggregates;
using ResQ.API.Risk_Detection.Domain.Model.Entities;
using ResQ.API.Risk_Detection.Domain.Model.ValueObjects;

namespace ResQ.API.Risk_Detection.Domain.Services;

/// <summary>
/// Domain service that decides whether valid monitored evidence satisfies active detection rules.
/// </summary>
public static class RiskEvaluationService
{
    /// <summary>
    /// True when the rule is active, applies to the evidence variable and its condition is satisfied.
    /// </summary>
    public static bool Matches(DetectionRule rule, DetectionEvidence evidence)
    {
        return rule.IsActive() && rule.Condition.Matches(evidence.VariableType, evidence.Value);
    }

    /// <summary>
    /// For each risk type, returns the most severe satisfied rule
    /// (e.g. GAS_LEAK Warning &gt; 300 and GAS_LEAK Critical &gt; 600 both match 700 → Critical).
    /// </summary>
    public static IReadOnlyCollection<DetectionRule> SelectSatisfiedRules(IEnumerable<DetectionRule> rules, DetectionEvidence evidence)
    {
        return rules
            .Where(rule => Matches(rule, evidence))
            .GroupBy(rule => rule.RiskTypeCode)
            .Select(group => group
                .OrderByDescending(rule => RiskCodes.SeverityRank(rule.SeverityCode))
                .ThenBy(rule => rule.ConfiguredAt)
                .First())
            .ToList();
    }
}
