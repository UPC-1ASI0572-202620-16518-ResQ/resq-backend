using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Reqnroll;
using Xunit;

namespace ResQ.Tests.Subscriptions.AcceptanceTests.Steps;

[Binding]
public class SubscriptionManagementSteps
{
    private readonly HttpClient _client;

    private HttpResponseMessage? _response;

    private Guid _subscriptionId;

    private bool _subscriptionIsActive;


    // ========================================================================
    // SETUP
    // ========================================================================

    public SubscriptionManagementSteps()
    {
        var baseUrl =
            Environment.GetEnvironmentVariable(
                "RESQ_API_BASE_URL");

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException(
                "RESQ_API_BASE_URL no está configurada.");
        }

        _client = new HttpClient
        {
            BaseAddress = new Uri(
                baseUrl.EndsWith("/")
                    ? baseUrl
                    : baseUrl + "/")
        };

        _client.Timeout = TimeSpan.FromSeconds(30);
    }


    // ========================================================================
    // AUTHENTICATION
    // ========================================================================

    [Given("el usuario de pruebas de suscripciones está autenticado")]
    public async Task GivenSubscriptionTestUserIsAuthenticated()
    {
        var username =
            Environment.GetEnvironmentVariable(
                "RESQ_TEST_LOGIN_IDENTIFIER");

        var password =
            Environment.GetEnvironmentVariable(
                "RESQ_TEST_CREDENTIAL_SECRET");

        Assert.False(
            string.IsNullOrWhiteSpace(username),
            "RESQ_TEST_LOGIN_IDENTIFIER no está configurada.");

        Assert.False(
            string.IsNullOrWhiteSpace(password),
            "RESQ_TEST_CREDENTIAL_SECRET no está configurada.");

        var loginPayload = new
        {
            username,
            password
        };

        using var response = await _client.PostAsync(
            "api/v1/Authentication/sign-in",
            CreateJsonContent(loginPayload));

        var responseBody =
            await response.Content.ReadAsStringAsync();

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        using var json =
            JsonDocument.Parse(responseBody);

        var token =
            GetProperty(
                json.RootElement,
                "token").GetString();

        Assert.False(
            string.IsNullOrWhiteSpace(token),
            "La respuesta de autenticación no contiene un JWT válido.");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);
    }


    // ========================================================================
    // SUBSCRIPTION - CREATE
    // ========================================================================

    [When("creo una suscripción válida")]
    public async Task WhenCreateValidSubscription()
    {
        var payload = new
        {
            startDate = DateTime.UtcNow.AddDays(-1),
            endDate = DateTime.UtcNow.AddDays(365)
        };

        _response = await _client.PostAsync(
            "api/v1/subscriptions",
            CreateJsonContent(payload));

        if (_response.IsSuccessStatusCode)
        {
            var responseBody =
                await _response.Content.ReadAsStringAsync();

            using var json =
                JsonDocument.Parse(responseBody);

            _subscriptionId =
                GetProperty(
                    json.RootElement,
                    "id").GetGuid();

            _subscriptionIsActive = true;
        }
    }


    [Given("existe una suscripción activa de prueba")]
    public async Task GivenActiveTestSubscriptionExists()
    {
        /*
         * Primero intentamos consultar la suscripción existente.
         * Esto evita crear una segunda suscripción si la organización
         * ya tiene una activa.
         */
        using var existingResponse =
            await _client.GetAsync(
                "api/v1/subscriptions");

        if (existingResponse.StatusCode == HttpStatusCode.OK)
        {
            var existingBody =
                await existingResponse.Content.ReadAsStringAsync();

            using var existingJson =
                JsonDocument.Parse(existingBody);

            _subscriptionId =
                GetProperty(
                    existingJson.RootElement,
                    "id").GetGuid();

            var status =
                GetProperty(
                    existingJson.RootElement,
                    "status").GetString();

            if (string.Equals(
                    status,
                    "active",
                    StringComparison.OrdinalIgnoreCase))
            {
                _subscriptionIsActive = true;
                return;
            }
        }

        /*
         * Si no existe una suscripción activa, creamos una.
         */
        await CreateTestSubscriptionAsync();

        Assert.Equal(
            HttpStatusCode.Created,
            _response!.StatusCode);

        _subscriptionId =
            await ExtractSubscriptionIdAsync(_response);

        _subscriptionIsActive = true;
    }


    private async Task CreateTestSubscriptionAsync()
    {
        var payload = new
        {
            startDate = DateTime.UtcNow.AddDays(-1),
            endDate = DateTime.UtcNow.AddDays(365)
        };

        _response = await _client.PostAsync(
            "api/v1/subscriptions",
            CreateJsonContent(payload));
    }


    // ========================================================================
    // SUBSCRIPTION - DUPLICATE
    // ========================================================================

    [When("intento crear otra suscripción")]
    public async Task WhenTryCreateAnotherSubscription()
    {
        var payload = new
        {
            startDate = DateTime.UtcNow.AddDays(-1),
            endDate = DateTime.UtcNow.AddDays(365)
        };

        _response = await _client.PostAsync(
            "api/v1/subscriptions",
            CreateJsonContent(payload));
    }


    // ========================================================================
    // SUBSCRIPTION - GET
    // ========================================================================

    [When("consulto la suscripción de la organización")]
    public async Task WhenGetOrganizationSubscription()
    {
        _response =
            await _client.GetAsync(
                "api/v1/subscriptions");
    }


    [When("consulto la suscripción creada")]
    public async Task WhenGetCreatedSubscription()
    {
        Assert.NotEqual(
            Guid.Empty,
            _subscriptionId);

        _response =
            await _client.GetAsync(
                $"api/v1/subscriptions/{_subscriptionId}");
    }


    // ========================================================================
    // SUBSCRIPTION - CANCEL
    // ========================================================================

    [When("cancelo la suscripción")]
    public async Task WhenCancelSubscription()
    {
        Assert.NotEqual(
            Guid.Empty,
            _subscriptionId);

        _response =
            await _client.PutAsync(
                $"api/v1/subscriptions/{_subscriptionId}/cancel",
                null);

        if (_response.IsSuccessStatusCode)
        {
            _subscriptionIsActive = false;
        }
    }


    // ========================================================================
    // SUBSCRIPTION - RENEW
    // ========================================================================

    [When("intento renovar la suscripción activa")]
    public async Task WhenTryRenewActiveSubscription()
    {
        Assert.NotEqual(
            Guid.Empty,
            _subscriptionId);

        var payload = new
        {
            newEndDate = DateTime.UtcNow.AddYears(2)
        };

        _response =
            await _client.PutAsync(
                $"api/v1/subscriptions/{_subscriptionId}/renew",
                CreateJsonContent(payload));
    }


    // ========================================================================
    // SUBSCRIPTION - EXPIRE
    // ========================================================================

    [When("intento expirar la suscripción activa")]
    public async Task WhenTryExpireActiveSubscription()
    {
        Assert.NotEqual(
            Guid.Empty,
            _subscriptionId);

        _response =
            await _client.PutAsync(
                $"api/v1/subscriptions/{_subscriptionId}/expire",
                null);
    }


    // ========================================================================
    // SUBSCRIPTION - INVALID CREATE
    // ========================================================================

    [When("creo una suscripción con fechas inválidas")]
    public async Task WhenCreateSubscriptionWithInvalidDates()
    {
        var payload = new
        {
            startDate = DateTime.UtcNow.AddDays(10),
            endDate = DateTime.UtcNow.AddDays(5)
        };

        _response =
            await _client.PostAsync(
                "api/v1/subscriptions",
                CreateJsonContent(payload));
    }


    // ========================================================================
    // THEN - HTTP STATUS
    // ========================================================================

    [Then("la respuesta HTTP de suscripciones debe ser {int}")]
    public void ThenSubscriptionResponseStatusMustBe(
        int expectedStatus)
    {
        Assert.NotNull(_response);

        Assert.Equal(
            expectedStatus,
            (int)_response!.StatusCode);
    }


    // ========================================================================
    // THEN - CREATE
    // ========================================================================

    [Then("la suscripción creada está activa")]
    public async Task ThenCreatedSubscriptionIsActive()
    {
        Assert.NotNull(_response);

        var responseBody =
            await _response!.Content.ReadAsStringAsync();

        using var json =
            JsonDocument.Parse(responseBody);

        var status =
            GetProperty(
                json.RootElement,
                "status").GetString();

        Assert.Equal(
            "active",
            status,
            ignoreCase: true);
    }


    [Then("la suscripción creada pertenece a la organización de pruebas")]
    public async Task ThenSubscriptionBelongsToTestOrganization()
    {
        Assert.NotNull(_response);

        var responseBody =
            await _response!.Content.ReadAsStringAsync();

        using var json =
            JsonDocument.Parse(responseBody);

        var organizationId =
            GetProperty(
                json.RootElement,
                "organizationId").GetGuid();

        var expectedOrganizationId =
            GetTestOrganizationId();

        Assert.Equal(
            expectedOrganizationId,
            organizationId);
    }


    // ========================================================================
    // THEN - GET
    // ========================================================================

    [Then("la suscripción consultada corresponde a la suscripción creada")]
    public async Task ThenRetrievedSubscriptionMatchesCreatedSubscription()
    {
        Assert.NotNull(_response);

        var responseBody =
            await _response!.Content.ReadAsStringAsync();

        using var json =
            JsonDocument.Parse(responseBody);

        var id =
            GetProperty(
                json.RootElement,
                "id").GetGuid();

        Assert.Equal(
            _subscriptionId,
            id);
    }


    // ========================================================================
    // THEN - CANCEL
    // ========================================================================

    [Then("la suscripción queda cancelada")]
    public async Task ThenSubscriptionIsCancelled()
    {
        Assert.NotNull(_response);

        var responseBody =
            await _response!.Content.ReadAsStringAsync();

        using var json =
            JsonDocument.Parse(responseBody);

        var status =
            GetProperty(
                json.RootElement,
                "status").GetString();

        Assert.Equal(
            "cancelled",
            status,
            ignoreCase: true);
    }


    // ========================================================================
    // HELPERS
    // ========================================================================

    private static StringContent CreateJsonContent(
        object payload)
    {
        return new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");
    }


    private static JsonElement GetProperty(
        JsonElement element,
        string propertyName)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(
                    property.Name,
                    propertyName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return property.Value;
            }
        }

        throw new KeyNotFoundException(
            $"No se encontró la propiedad '{propertyName}' en la respuesta JSON.");
    }


    private static async Task<Guid> ExtractSubscriptionIdAsync(
        HttpResponseMessage response)
    {
        var responseBody =
            await response.Content.ReadAsStringAsync();

        using var json =
            JsonDocument.Parse(responseBody);

        return GetProperty(
            json.RootElement,
            "id").GetGuid();
    }


    private static Guid GetTestOrganizationId()
    {
        var value =
            Environment.GetEnvironmentVariable(
                "RESQ_TEST_ORGANIZATION_ID");

        Assert.False(
            string.IsNullOrWhiteSpace(value),
            "RESQ_TEST_ORGANIZATION_ID no está configurada.");

        Assert.True(
            Guid.TryParse(
                value,
                out var organizationId),
            "RESQ_TEST_ORGANIZATION_ID no contiene un GUID válido.");

        return organizationId;
    }


    // ========================================================================
    // CLEANUP
    // ========================================================================

    [AfterScenario]
    public async Task CleanupSubscription()
    {
        if (!_subscriptionIsActive ||
            _subscriptionId == Guid.Empty)
        {
            return;
        }

        try
        {
            using var response =
                await _client.PutAsync(
                    $"api/v1/subscriptions/{_subscriptionId}/cancel",
                    null);

            _subscriptionIsActive = false;
        }
        catch
        {
            // El cleanup no debe ocultar el resultado del escenario.
        }
    }
}