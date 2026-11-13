using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResQ.API.Building_Management.Interfaces.ACL;
using ResQ.API.Monitoring.Interfaces.REST;
using ResQ.API.Risk_Detection.Domain.Model.Aggregates;
using ResQ.API.Risk_Detection.Domain.Model.Commands;
using ResQ.API.Risk_Detection.Domain.Model.ValueObjects;
using ResQ.API.Risk_Detection.Domain.Repositories;
using ResQ.API.Risk_Detection.Domain.Services;
using Xunit;

namespace ResQ.API.Tests;

public sealed class ZoneMonitoringProfileControllerTests
{
    private static readonly Guid OrganizationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ZoneId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task InvalidRuleDoesNotDeactivateExistingProfile()
    {
        var existing = Rule("GAS_ADC", EComparisonOperator.GreaterThan);
        var repository = new FakeRuleRepository(existing);
        var commands = new FakeRuleCommandService(existing);
        var controller = Controller(repository, commands);
        var resource = new ZoneMonitoringProfileResource([
            new ZoneMonitoringRuleResource("GAS_LEAK", "Warning", "GAS_ADC", "NOT_AN_OPERATOR", 500)
        ]);

        var result = await controller.Put(ZoneId, resource);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.True(existing.IsActive());
        Assert.Empty(commands.StatusCommands);
        Assert.Empty(commands.ConfigureCommands);
    }

    [Theory]
    [InlineData("GREATER_THAN", EComparisonOperator.GreaterThan)]
    [InlineData("GREATER_THAN_OR_EQUAL", EComparisonOperator.GreaterThanOrEqual)]
    [InlineData("LESS_THAN", EComparisonOperator.LessThan)]
    [InlineData("LESS_THAN_OR_EQUAL", EComparisonOperator.LessThanOrEqual)]
    [InlineData("EQUAL", EComparisonOperator.Equal)]
    [InlineData("GreaterThanOrEqual", EComparisonOperator.GreaterThanOrEqual)]
    public async Task ValidatesAllSupportedOperatorRepresentations(
        string operatorCode,
        EComparisonOperator expected)
    {
        var existing = Rule("GAS_ADC", EComparisonOperator.GreaterThan);
        var repository = new FakeRuleRepository(existing);
        var commands = new FakeRuleCommandService(existing);
        var controller = Controller(repository, commands);
        var resource = new ZoneMonitoringProfileResource([
            new ZoneMonitoringRuleResource("GAS_LEAK", "Warning", "GAS_ADC", operatorCode, 500)
        ]);

        var result = await controller.Put(ZoneId, resource);

        Assert.IsType<OkObjectResult>(result);
        Assert.Single(commands.StatusCommands);
        Assert.Single(commands.ConfigureCommands);
        Assert.Equal(expected, commands.ConfigureCommands[0].Operator);
    }

    private static ZoneMonitoringProfilesController Controller(
        IDetectionRuleRepository repository,
        IDetectionRuleCommandService commands)
    {
        var controller = new ZoneMonitoringProfilesController(
            repository,
            commands,
            new FakeBuildingsContextFacade())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        controller.HttpContext.Items["OrganizationId"] = OrganizationId;
        return controller;
    }

    private static DetectionRule Rule(string variableType, EComparisonOperator comparisonOperator) =>
        DetectionRule.Configure(
            OrganizationId,
            "GAS_LEAK",
            "Warning",
            DetectionCondition.Create(variableType, comparisonOperator, 500),
            ZoneId);

    private sealed class FakeBuildingsContextFacade : IBuildingsContextFacade
    {
        public Task<bool> ValidateAssignmentAsync(Guid organizationId, Guid buildingId, Guid? zoneId) =>
            Task.FromResult(true);

        public Task<bool> ZoneExistsAsync(Guid organizationId, Guid zoneId) => Task.FromResult(true);
    }

    private sealed class FakeRuleCommandService(DetectionRule existing) : IDetectionRuleCommandService
    {
        public List<ConfigureDetectionRuleCommand> ConfigureCommands { get; } = [];
        public List<ChangeDetectionRuleStatusCommand> StatusCommands { get; } = [];

        public Task<DetectionRule> Handle(ConfigureDetectionRuleCommand command)
        {
            ConfigureCommands.Add(command);
            return Task.FromResult(DetectionRule.Configure(
                command.OrganizationId,
                command.RiskTypeCode,
                command.SeverityCode,
                DetectionCondition.Create(command.VariableType, command.Operator, command.Threshold),
                command.ZoneId));
        }

        public Task<DetectionRule?> Handle(ChangeDetectionRuleStatusCommand command)
        {
            StatusCommands.Add(command);
            if (command.RuleId == existing.Id && !command.Active) existing.Deactivate();
            return Task.FromResult<DetectionRule?>(existing);
        }
    }

    private sealed class FakeRuleRepository(params DetectionRule[] rules) : IDetectionRuleRepository
    {
        public Task<DetectionRule?> FindByIdAsync(Guid organizationId, Guid ruleId) =>
            Task.FromResult(rules.FirstOrDefault(rule => rule.Id == ruleId));

        public Task<IEnumerable<DetectionRule>> FindActiveByVariableTypeAsync(
            Guid organizationId, string variableType, Guid? zoneId) =>
            Task.FromResult<IEnumerable<DetectionRule>>(rules.Where(rule => rule.IsActive()));

        public Task<IEnumerable<DetectionRule>> FindByZoneAsync(Guid organizationId, Guid zoneId) =>
            Task.FromResult<IEnumerable<DetectionRule>>(rules);

        public Task<IEnumerable<DetectionRule>> FindAllAsync(
            Guid organizationId, EDetectionRuleStatus? status, string? variableType) =>
            Task.FromResult<IEnumerable<DetectionRule>>(rules);

        public Task AddAsync(DetectionRule entity) => Task.CompletedTask;
        public Task<DetectionRule?> FindByIdAsync(int id) => Task.FromResult<DetectionRule?>(null);
        public void Update(DetectionRule entity) { }
        public void Remove(DetectionRule entity) { }
        public Task<IEnumerable<DetectionRule>> ListAsync() => Task.FromResult<IEnumerable<DetectionRule>>(rules);
    }
}
