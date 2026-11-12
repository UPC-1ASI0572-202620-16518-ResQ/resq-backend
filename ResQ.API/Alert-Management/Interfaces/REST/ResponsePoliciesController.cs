using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Alert_Management.Interfaces.ACL;
using ResQ.API.IAM.Domain.Model.Aggregates;
using ResQ.API.IAM.Domain.Model.ValueObjects;
using ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace ResQ.API.Alert_Management.Interfaces.REST;

[ApiController]
[Route("api/v1/alerts/response-policies")]
[Authorize]
public class ResponsePoliciesController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid? zoneId = null)
    {
        if (!TryContext(out var organizationId, out _)) return Unauthorized();
        var policies = await context.Set<ResponsePolicy>()
            .Where(policy => policy.OrganizationId == organizationId && (!zoneId.HasValue || policy.ZoneId == zoneId))
            .OrderBy(policy => policy.RiskTypeCode).ThenBy(policy => policy.SeverityCode).ToListAsync();
        return Ok(policies.Select(ToResource));
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] ResponsePolicyResource resource)
    {
        if (!TryContext(out var organizationId, out var user) || user.Role != Roles.Volunteer) return Forbid();
        var risk = resource.RiskTypeCode.Trim().ToUpperInvariant();
        var severity = resource.SeverityCode.Trim().ToUpperInvariant();
        var policy = await context.Set<ResponsePolicy>().FirstOrDefaultAsync(candidate =>
            candidate.OrganizationId == organizationId && candidate.ZoneId == resource.ZoneId &&
            candidate.RiskTypeCode == risk && candidate.SeverityCode == severity);
        var json = JsonSerializer.Serialize(resource.Actions ?? []);
        if (policy is null)
        {
            policy = ResponsePolicy.Create(
                organizationId, resource.ZoneId, risk, severity, json, resource.Active, user.Id);
            await context.Set<ResponsePolicy>().AddAsync(policy);
        }
        else policy.Update(risk, severity, json, resource.Active, user.Id);
        await context.SaveChangesAsync();
        return Ok(ToResource(policy));
    }

    private bool TryContext(out Guid organizationId, out User user)
    {
        organizationId = HttpContext.Items["OrganizationId"] is Guid id ? id : Guid.Empty;
        user = HttpContext.Items["User"] as User ?? null!;
        return organizationId != Guid.Empty && user is not null;
    }

    private static object ToResource(ResponsePolicy policy) => new
    {
        policy.Id, policy.ZoneId, policy.RiskTypeCode, policy.SeverityCode,
        actions = JsonSerializer.Deserialize<List<ResponsePolicyAction>>(policy.ActionsJson) ?? [],
        policy.Active, policy.UpdatedByUserId, policy.UpdatedAt
    };
}

public record ResponsePolicyResource(
    Guid? ZoneId, string RiskTypeCode, string SeverityCode,
    IReadOnlyCollection<ResponsePolicyAction>? Actions, bool Active);
