using Microsoft.AspNetCore.Mvc;
using ResQ.API.Building_Management.Interfaces.ACL;
using ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using ResQ.API.Risk_Detection.Domain.Model.Commands;
using ResQ.API.Risk_Detection.Domain.Model.ValueObjects;
using ResQ.API.Risk_Detection.Domain.Repositories;
using ResQ.API.Risk_Detection.Domain.Services;
using ResQ.API.Risk_Detection.Interfaces.REST.Transform;

namespace ResQ.API.Monitoring.Interfaces.REST;

[ApiController]
[Route("api/v1/monitoring/zones/{zoneId:guid}/profile")]
[Authorize]
public class ZoneMonitoringProfilesController(
    IDetectionRuleRepository ruleRepository,
    IDetectionRuleCommandService ruleCommandService,
    IBuildingsContextFacade buildingsContextFacade) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(Guid zoneId)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();
        if (!await buildingsContextFacade.ZoneExistsAsync(organizationId, zoneId)) return NotFound();

        var zoneRules = (await ruleRepository.FindByZoneAsync(organizationId, zoneId))
            .Where(rule => rule.IsActive()).ToList();
        var effective = zoneRules.Count > 0
            ? zoneRules
            : (await ruleRepository.FindAllAsync(organizationId, EDetectionRuleStatus.Active, null)).ToList();
        return Ok(new
        {
            zoneId,
            source = zoneRules.Count > 0 ? "ZONE_OVERRIDE" : "GLOBAL_FALLBACK",
            rules = effective.Select(DetectionRuleResourceFromEntityAssembler.ToResourceFromEntity)
        });
    }

    [HttpPut]
    public async Task<IActionResult> Put(Guid zoneId, [FromBody] ZoneMonitoringProfileResource resource)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();
        if (!await buildingsContextFacade.ZoneExistsAsync(organizationId, zoneId)) return NotFound();
        if (resource.Rules is null || resource.Rules.Count == 0)
            return BadRequest(new { message = "At least one effective rule is required." });

        foreach (var existing in await ruleRepository.FindByZoneAsync(organizationId, zoneId))
            await ruleCommandService.Handle(new ChangeDetectionRuleStatusCommand(organizationId, existing.Id, false));

        var created = new List<object>();
        foreach (var item in resource.Rules)
        {
            if (!Enum.TryParse<EComparisonOperator>(item.Operator, true, out var comparisonOperator))
                return BadRequest(new { message = $"Invalid operator '{item.Operator}'." });
            var rule = await ruleCommandService.Handle(new ConfigureDetectionRuleCommand(
                organizationId, item.RiskTypeCode, item.SeverityCode, item.VariableType,
                comparisonOperator, item.Threshold, zoneId));
            created.Add(DetectionRuleResourceFromEntityAssembler.ToResourceFromEntity(rule));
        }

        return Ok(new { zoneId, source = "ZONE_OVERRIDE", rules = created });
    }
}

public record ZoneMonitoringProfileResource(IReadOnlyCollection<ZoneMonitoringRuleResource> Rules);
public record ZoneMonitoringRuleResource(
    string RiskTypeCode, string SeverityCode, string VariableType, string Operator, decimal Threshold);
