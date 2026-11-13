using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ResQ.API.Alert_Management.Interfaces.ACL;
using ResQ.API.Incident_Management.Interfaces.ACL;
using ResQ.API.Risk_Detection.Application.Internal.CommandServices;
using ResQ.API.Risk_Detection.Application.Internal.OutboundServices;
using ResQ.API.Risk_Detection.Domain.Model.Aggregates;
using ResQ.API.Risk_Detection.Domain.Model.Commands;
using ResQ.API.Risk_Detection.Domain.Model.ValueObjects;
using ResQ.API.Risk_Detection.Domain.Repositories;
using ResQ.API.Shared.Domain.Repositories;
using Xunit;

namespace ResQ.API.Tests;

public sealed class RiskDetectionFlowTests
{
    private static readonly Guid OrganizationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid BuildingId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid ZoneId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid DeviceId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    [Fact]
    public async Task SatisfiedMonitoringMeasurementPersistsDetectionAndPublishesAlert()
    {
        var rule = DetectionRule.Configure(
            OrganizationId,
            "HIGH_TEMPERATURE",
            "Warning",
            DetectionCondition.Create("TEMPERATURE_C", EComparisonOperator.GreaterThanOrEqual, 26m),
            ZoneId);
        var detectionRepository = new FakeRiskDetectionRepository();
        var alerts = new FakeAlertsContextFacade();
        var publisher = new AlertManagementRiskDetectionEventPublisher(
            alerts,
            new FakeIncidentContextFacade(),
            NullLogger<AlertManagementRiskDetectionEventPublisher>.Instance);
        var service = new RiskDetectionCommandService(
            new FakeRuleRepository(rule),
            detectionRepository,
            new FakeRiskLocationResolver(),
            publisher,
            Options.Create(new RiskDetectionSettings { RaiseAlerts = true }),
            new FakeUnitOfWork());
        var measuredAt = DateTimeOffset.UtcNow;

        var result = await service.Handle(new EvaluateMeasurementCommand(
            OrganizationId,
            Guid.NewGuid(),
            DeviceId,
            "TEMPERATURE_C",
            26m,
            measuredAt));

        var detection = Assert.Single(detectionRepository.Added);
        Assert.Equal("HIGH_TEMPERATURE", detection.RiskTypeCode);
        Assert.Equal(26m, Assert.Single(detection.Evidence).Value);
        Assert.Equal(detection.Id.ToString(), Assert.Single(alerts.RiskDetectionIds));
        Assert.Single(result.GeneratedAlertIds);
        Assert.Empty(result.GeneratedIncidentIds);
    }

    private sealed class FakeRiskLocationResolver : IRiskLocationResolver
    {
        public Task<RiskLocation> ResolveAsync(Guid organizationId, Guid deviceId) =>
            Task.FromResult(RiskLocation.Resolved(BuildingId, ZoneId));
    }

    private sealed class FakeAlertsContextFacade : IAlertsContextFacade
    {
        public List<string> RiskDetectionIds { get; } = [];

        public Task<Guid> GenerateAlertAsync(
            Guid organizationId,
            string riskDetectionId,
            string riskTypeCode,
            string severityCode,
            Guid? buildingId,
            Guid? zoneId,
            DateTimeOffset detectedAt,
            IEnumerable<AlertRecipient> recipients)
        {
            RiskDetectionIds.Add(riskDetectionId);
            return Task.FromResult(Guid.NewGuid());
        }
    }

    private sealed class FakeIncidentContextFacade : IIncidentContextFacade
    {
        public Task<Guid> CreateIncident(Guid organizationId, Guid zoneId, string type, string level) =>
            Task.FromResult(Guid.Empty);

        public Task<Guid> CreateIncidentForDetection(
            Guid organizationId,
            Guid zoneId,
            string type,
            string level,
            Guid riskDetectionId,
            Guid? alertId) => Task.FromResult(Guid.Empty);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task CompleteAsync() => Task.CompletedTask;
    }

    private sealed class FakeRiskDetectionRepository : IRiskDetectionRepository
    {
        public List<RiskDetection> Added { get; } = [];

        public Task<RiskDetection?> FindByIdAsync(Guid organizationId, Guid riskDetectionId) =>
            Task.FromResult(Added.FirstOrDefault(item => item.Id == riskDetectionId));

        public Task<IEnumerable<RiskDetection>> FindAllAsync(
            Guid organizationId,
            Guid? buildingId,
            Guid? zoneId,
            string? riskTypeCode) => Task.FromResult<IEnumerable<RiskDetection>>(Added);

        public Task<RiskDetection?> FindOpenAsync(
            Guid organizationId,
            string riskTypeCode,
            RiskLocation location,
            DateTimeOffset since) => Task.FromResult<RiskDetection?>(null);

        public Task AddAsync(RiskDetection entity)
        {
            Added.Add(entity);
            return Task.CompletedTask;
        }

        public Task<RiskDetection?> FindByIdAsync(int id) => Task.FromResult<RiskDetection?>(null);
        public void Update(RiskDetection entity) { }
        public void Remove(RiskDetection entity) { }
        public Task<IEnumerable<RiskDetection>> ListAsync() =>
            Task.FromResult<IEnumerable<RiskDetection>>(Added);
    }

    private sealed class FakeRuleRepository(DetectionRule rule) : IDetectionRuleRepository
    {
        public Task<DetectionRule?> FindByIdAsync(Guid organizationId, Guid ruleId) =>
            Task.FromResult(rule.Id == ruleId ? rule : null);

        public Task<IEnumerable<DetectionRule>> FindActiveByVariableTypeAsync(
            Guid organizationId,
            string variableType,
            Guid? zoneId) => Task.FromResult<IEnumerable<DetectionRule>>([rule]);

        public Task<IEnumerable<DetectionRule>> FindByZoneAsync(Guid organizationId, Guid zoneId) =>
            Task.FromResult<IEnumerable<DetectionRule>>([rule]);

        public Task<IEnumerable<DetectionRule>> FindAllAsync(
            Guid organizationId,
            EDetectionRuleStatus? status,
            string? variableType) => Task.FromResult<IEnumerable<DetectionRule>>([rule]);

        public Task AddAsync(DetectionRule entity) => Task.CompletedTask;
        public Task<DetectionRule?> FindByIdAsync(int id) => Task.FromResult<DetectionRule?>(null);
        public void Update(DetectionRule entity) { }
        public void Remove(DetectionRule entity) { }
        public Task<IEnumerable<DetectionRule>> ListAsync() =>
            Task.FromResult<IEnumerable<DetectionRule>>([rule]);
    }
}
