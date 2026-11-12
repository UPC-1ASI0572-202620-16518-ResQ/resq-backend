namespace ResQ.API.Alert_Management.Domain.Model.Aggregates;

public class ResponsePolicy
{
    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Guid? ZoneId { get; private set; }
    public string RiskTypeCode { get; private set; } = string.Empty;
    public string SeverityCode { get; private set; } = string.Empty;
    public string ActionsJson { get; private set; } = "[]";
    public bool Active { get; private set; }
    public int UpdatedByUserId { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    protected ResponsePolicy() { }

    public static ResponsePolicy Create(
        Guid organizationId, Guid? zoneId, string riskTypeCode, string severityCode,
        string actionsJson, bool active, int userId)
    {
        var policy = new ResponsePolicy { Id = Guid.NewGuid(), OrganizationId = organizationId, ZoneId = zoneId };
        policy.Update(riskTypeCode, severityCode, actionsJson, active, userId);
        return policy;
    }

    public void Update(string riskTypeCode, string severityCode, string actionsJson, bool active, int userId)
    {
        if (string.IsNullOrWhiteSpace(riskTypeCode) || string.IsNullOrWhiteSpace(severityCode))
            throw new ArgumentException("Risk type and severity are required.");
        RiskTypeCode = riskTypeCode.Trim().ToUpperInvariant();
        SeverityCode = severityCode.Trim().ToUpperInvariant();
        ActionsJson = actionsJson;
        Active = active;
        UpdatedByUserId = userId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
