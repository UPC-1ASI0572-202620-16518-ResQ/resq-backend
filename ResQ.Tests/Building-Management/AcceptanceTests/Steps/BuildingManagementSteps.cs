using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Reqnroll;
using Xunit;

namespace ResQ.Tests.Building_Management.AcceptanceTests.Steps;

[Binding]
public class BuildingManagementSteps
{
    private readonly HttpClient _client;

    private HttpResponseMessage? _response;

    private Guid _buildingId;
    private Guid _zoneId;

    private string? _buildingCode;
    private string? _zoneCode;

    public BuildingManagementSteps()
    {
        var baseUrl = Environment.GetEnvironmentVariable("RESQ_API_BASE_URL");

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

    [Given("el usuario de pruebas está autenticado")]
    public async Task GivenTestUserIsAuthenticated()
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
    // BUILDING - CREATE
    // ========================================================================

    [When("registro una edificación con los siguientes datos")]
    public async Task WhenRegisterBuilding(string json)
    {
        var payload =
            JsonNode.Parse(json)?.AsObject()
            ?? throw new InvalidOperationException(
                "El JSON de la edificación no es válido.");

        var requestedCode =
            payload["buildingCode"]?.GetValue<string>();

        /*
         * Para escenarios válidos generamos un código único.
         * El escenario inválido mantiene deliberadamente
         * el código inválido enviado en el feature.
         */
        if (!string.Equals(
                requestedCode,
                "bld central 01!",
                StringComparison.OrdinalIgnoreCase))
        {
            _buildingCode =
                GenerateBuildingCode();

            payload["buildingCode"] =
                _buildingCode;
        }
        else
        {
            _buildingCode =
                requestedCode;
        }

        _response = await _client.PostAsync(
            "api/v1/buildings",
            CreateJsonContent(payload.ToJsonString()));

        /*
         * Si la creación fue exitosa, guardamos inmediatamente
         * el ID generado por el backend.
         */
        if (_response.IsSuccessStatusCode)
        {
            var responseBody =
                await _response.Content.ReadAsStringAsync();

            using var responseJson =
                JsonDocument.Parse(responseBody);

            _buildingId =
                GetProperty(
                    responseJson.RootElement,
                    "id").GetGuid();
        }
    }


    // ========================================================================
    // BUILDING - CREATE HELPER
    // ========================================================================

    [Given("existe una edificación de prueba")]
    public async Task GivenTestBuildingExists()
    {
        await CreateTestBuildingAsync();
    }


    private async Task CreateTestBuildingAsync()
    {
        _buildingCode =
            GenerateBuildingCode();

        var payload = new
        {
            buildingCode = _buildingCode,
            name = "Edificio Acceptance Test",
            description =
                "Edificación creada automáticamente para Acceptance Tests",
            address = new
            {
                streetAddress =
                    "Av. Javier Prado Este 456",
                district = "San Isidro",
                city = "Lima",
                countryCode = "PE"
            }
        };

        using var response =
            await _client.PostAsync(
                "api/v1/buildings",
                CreateJsonContent(payload));

        var responseBody =
            await response.Content.ReadAsStringAsync();

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        using var json =
            JsonDocument.Parse(responseBody);

        _buildingId =
            GetProperty(
                json.RootElement,
                "id").GetGuid();
    }


    // ========================================================================
    // BUILDING - STATUS
    // ========================================================================

    [Then("la respuesta HTTP debe ser (.*)")]
    public void ThenResponseStatus(string expectedStatus)
    {
        Assert.NotNull(_response);

        var expectedCode =
            int.Parse(expectedStatus);

        Assert.Equal(
            expectedCode,
            (int)_response!.StatusCode);
    }


    [Then("la edificación creada pertenece a la organización de pruebas")]
    public async Task ThenBuildingBelongsToTestOrganization()
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


    [Then("la edificación creada está activa")]
    public async Task ThenBuildingIsActive()
    {
        Assert.NotNull(_response);

        var responseBody =
            await _response!.Content.ReadAsStringAsync();

        using var json =
            JsonDocument.Parse(responseBody);

        var status =
            GetProperty(
                json.RootElement,
                "administrativeStatus").GetString();

        Assert.Equal(
            "active",
            status,
            ignoreCase: true);
    }


    // ========================================================================
    // BUILDING - LIST
    // ========================================================================

    [When("consulto la lista de edificaciones")]
    public async Task WhenGetBuildings()
    {
        _response =
            await _client.GetAsync(
                "api/v1/buildings?page=0&size=100");
    }


    [Then("la lista contiene la edificación creada")]
    public async Task ThenBuildingIsPresentInList()
    {
        Assert.NotNull(_response);

        var responseBody =
            await _response!.Content.ReadAsStringAsync();

        using var json =
            JsonDocument.Parse(responseBody);

        var items =
            GetProperty(
                json.RootElement,
                "items");

        var exists =
            items
                .EnumerateArray()
                .Any(item =>
                    string.Equals(
                        GetProperty(
                            item,
                            "buildingCode").GetString(),
                        _buildingCode,
                        StringComparison.OrdinalIgnoreCase));

        Assert.True(
            exists,
            $"No se encontró la edificación '{_buildingCode}' en la lista.");
    }


    // ========================================================================
    // BUILDING - GET BY ID
    // ========================================================================

    [When("consulto la edificación creada")]
    public async Task WhenGetCreatedBuilding()
    {
        Assert.NotEqual(
            Guid.Empty,
            _buildingId);

        _response =
            await _client.GetAsync(
                $"api/v1/buildings/{_buildingId}");
    }


    [Then("la edificación consultada tiene el código registrado")]
    public async Task ThenBuildingHasRegisteredCode()
    {
        Assert.NotNull(_response);

        var responseBody =
            await _response!.Content.ReadAsStringAsync();

        using var json =
            JsonDocument.Parse(responseBody);

        var code =
            GetProperty(
                json.RootElement,
                "buildingCode").GetString();

        Assert.Equal(
            _buildingCode,
            code);
    }


    // ========================================================================
    // BUILDING - UPDATE DETAILS
    // ========================================================================

    [When("actualizo los detalles de la edificación")]
    public async Task WhenUpdateBuildingDetails()
    {
        var payload = new
        {
            name =
                "Edificio Acceptance Actualizado",
            description =
                "Edificio actualizado mediante Acceptance Test",
            address = new
            {
                streetAddress =
                    "Av. Javier Prado Este 789",
                district = "San Isidro",
                city = "Lima",
                countryCode = "PE"
            }
        };

        _response =
            await _client.PutAsync(
                $"api/v1/buildings/{_buildingId}/details",
                CreateJsonContent(payload));
    }


    [Then("la edificación actualizada tiene el nombre {string}")]
    public async Task ThenBuildingHasUpdatedName(
        string expectedName)
    {
        Assert.NotNull(_response);

        var responseBody =
            await _response!.Content.ReadAsStringAsync();

        using var json =
            JsonDocument.Parse(responseBody);

        var name =
            GetProperty(
                json.RootElement,
                "name").GetString();

        Assert.Equal(
            expectedName,
            name);
    }


    // ========================================================================
    // BUILDING - ADMINISTRATIVE STATUS
    // ========================================================================

    [When("desactivo la edificación")]
    public async Task WhenDeactivateBuilding()
    {
        var payload = new
        {
            administrativeStatus = "inactive"
        };

        _response =
            await _client.PutAsync(
                $"api/v1/buildings/{_buildingId}/administrative-status",
                CreateJsonContent(payload));
    }


    [Then("la edificación está inactiva")]
    public async Task ThenBuildingIsInactive()
    {
        Assert.NotNull(_response);

        var responseBody =
            await _response!.Content.ReadAsStringAsync();

        using var json =
            JsonDocument.Parse(responseBody);

        var status =
            GetProperty(
                json.RootElement,
                "administrativeStatus").GetString();

        Assert.Equal(
            "inactive",
            status,
            ignoreCase: true);
    }


    // ========================================================================
    // BUILDING - INVALID REGISTRATION
    // ========================================================================

    [Then("la edificación inválida no fue creada")]
    public async Task ThenInvalidBuildingWasNotCreated()
    {
        _response =
            await _client.GetAsync(
                "api/v1/buildings?page=0&size=100");

        Assert.Equal(
            HttpStatusCode.OK,
            _response.StatusCode);

        var responseBody =
            await _response.Content.ReadAsStringAsync();

        using var json =
            JsonDocument.Parse(responseBody);

        var items =
            GetProperty(
                json.RootElement,
                "items");

        var exists =
            items
                .EnumerateArray()
                .Any(item =>
                    string.Equals(
                        GetProperty(
                            item,
                            "buildingCode").GetString(),
                        _buildingCode,
                        StringComparison.OrdinalIgnoreCase));

        Assert.False(
            exists,
            $"La edificación inválida '{_buildingCode}' no debería existir.");
    }


    // ========================================================================
    // ZONE - CREATE
    // ========================================================================

    [Given("existe una edificación de prueba con una zona")]
    public async Task GivenTestBuildingWithZone()
    {
        await CreateTestBuildingAsync();
        await CreateTestZoneAsync();
    }


    [When("registro una zona con los siguientes datos")]
    public async Task WhenRegisterZone(string json)
    {
        var payload =
            JsonNode.Parse(json)?.AsObject()
            ?? throw new InvalidOperationException(
                "El JSON de la zona no es válido.");

        _zoneCode =
            GenerateZoneCode();

        payload["zoneCode"] =
            _zoneCode;

        _response =
            await _client.PostAsync(
                $"api/v1/buildings/{_buildingId}/zones",
                CreateJsonContent(payload.ToJsonString()));

        /*
         * AddZoneToBuilding devuelve BuildingResource,
         * por lo que obtenemos la zona creada desde
         * la colección zones del Building.
         */
        if (_response.IsSuccessStatusCode)
        {
            var responseBody =
                await _response.Content.ReadAsStringAsync();

            using var jsonResponse =
                JsonDocument.Parse(responseBody);

            var zones =
                GetProperty(
                    jsonResponse.RootElement,
                    "zones");

            var zone =
                zones
                    .EnumerateArray()
                    .First(z =>
                        string.Equals(
                            GetProperty(
                                z,
                                "zoneCode").GetString(),
                            _zoneCode,
                            StringComparison.OrdinalIgnoreCase));

            _zoneId =
                GetProperty(
                    zone,
                    "id").GetGuid();
        }
    }


    private async Task CreateTestZoneAsync()
    {
        _zoneCode =
            GenerateZoneCode();

        var payload = new
        {
            zoneCode = _zoneCode,
            name = "Zona Acceptance Test",
            description =
                "Zona creada automáticamente para Acceptance Tests",
            floorLabel = "Piso 1"
        };

        _response =
            await _client.PostAsync(
                $"api/v1/buildings/{_buildingId}/zones",
                CreateJsonContent(payload));

        var responseBody =
            await _response.Content.ReadAsStringAsync();

        Assert.Equal(
            HttpStatusCode.Created,
            _response.StatusCode);

        using var json =
            JsonDocument.Parse(responseBody);

        var zones =
            GetProperty(
                json.RootElement,
                "zones");

        var zone =
            zones
                .EnumerateArray()
                .First(z =>
                    string.Equals(
                        GetProperty(
                            z,
                            "zoneCode").GetString(),
                        _zoneCode,
                        StringComparison.OrdinalIgnoreCase));

        _zoneId =
            GetProperty(
                zone,
                "id").GetGuid();
    }


    // ========================================================================
    // ZONE - ASSERTIONS
    // ========================================================================

    [Then("la zona creada pertenece a la edificación de prueba")]
    public async Task ThenCreatedZoneBelongsToBuilding()
    {
        Assert.NotNull(_response);

        var responseBody =
            await _response!.Content.ReadAsStringAsync();

        using var json =
            JsonDocument.Parse(responseBody);

        var zones =
            GetProperty(
                json.RootElement,
                "zones");

        var zone =
            zones
                .EnumerateArray()
                .First(z =>
                    string.Equals(
                        GetProperty(
                            z,
                            "zoneCode").GetString(),
                        _zoneCode,
                        StringComparison.OrdinalIgnoreCase));

        var buildingId =
            GetProperty(
                zone,
                "buildingId").GetGuid();

        Assert.Equal(
            _buildingId,
            buildingId);
    }


    // ========================================================================
    // ZONE - LIST
    // ========================================================================

    [When("consulto la lista de zonas de la edificación")]
    public async Task WhenGetZones()
    {
        _response =
            await _client.GetAsync(
                $"api/v1/buildings/{_buildingId}/zones?page=0&size=100");
    }


    [Then("la lista contiene la zona creada")]
    public async Task ThenZoneIsPresentInList()
    {
        Assert.NotNull(_response);

        var responseBody =
            await _response!.Content.ReadAsStringAsync();

        using var json =
            JsonDocument.Parse(responseBody);

        var items =
            GetProperty(
                json.RootElement,
                "items");

        var exists =
            items
                .EnumerateArray()
                .Any(zone =>
                    string.Equals(
                        GetProperty(
                            zone,
                            "zoneCode").GetString(),
                        _zoneCode,
                        StringComparison.OrdinalIgnoreCase));

        Assert.True(
            exists,
            $"No se encontró la zona '{_zoneCode}' en la lista.");
    }


    // ========================================================================
    // ZONE - GET BY ID
    // ========================================================================

    [When("consulto la zona creada")]
    public async Task WhenGetCreatedZone()
    {
        Assert.NotEqual(
            Guid.Empty,
            _buildingId);

        Assert.NotEqual(
            Guid.Empty,
            _zoneId);

        _response =
            await _client.GetAsync(
                $"api/v1/buildings/{_buildingId}/zones/{_zoneId}");
    }


    [Then("la zona consultada pertenece a la edificación de prueba")]
    public async Task ThenQueriedZoneBelongsToBuilding()
    {
        Assert.NotNull(_response);

        var responseBody =
            await _response!.Content.ReadAsStringAsync();

        using var json =
            JsonDocument.Parse(responseBody);

        var buildingId =
            GetProperty(
                json.RootElement,
                "buildingId").GetGuid();

        Assert.Equal(
            _buildingId,
            buildingId);
    }


    // ========================================================================
    // ZONE - UPDATE DETAILS
    // ========================================================================

    [When("actualizo los detalles de la zona")]
    public async Task WhenUpdateZoneDetails()
    {
        var payload = new
        {
            name =
                "Zona Acceptance Actualizada",
            description =
                "Zona actualizada mediante Acceptance Test",
            floorLabel = "Piso 2"
        };

        _response =
            await _client.PutAsync(
                $"api/v1/buildings/{_buildingId}/zones/{_zoneId}/details",
                CreateJsonContent(payload));
    }


    [Then("la zona actualizada conserva la edificación de prueba")]
    public async Task ThenUpdatedZoneKeepsBuilding()
    {
        Assert.NotNull(_response);

        var responseBody =
            await _response!.Content.ReadAsStringAsync();

        using var json =
            JsonDocument.Parse(responseBody);

        /*
         * UpdateZone devuelve BuildingResource,
         * por lo que la zona actualizada se encuentra
         * dentro de zones.
         */
        var zones =
            GetProperty(
                json.RootElement,
                "zones");

        var zone =
            zones
                .EnumerateArray()
                .First(z =>
                    GetProperty(
                        z,
                        "id").GetGuid() == _zoneId);

        var buildingId =
            GetProperty(
                zone,
                "buildingId").GetGuid();

        Assert.Equal(
            _buildingId,
            buildingId);
    }


    // ========================================================================
    // ZONE - ADMINISTRATIVE STATUS
    // ========================================================================

    [When("desactivo la zona")]
    public async Task WhenDeactivateZone()
    {
        var payload = new
        {
            administrativeStatus = "inactive"
        };

        _response =
            await _client.PutAsync(
                $"api/v1/buildings/{_buildingId}/zones/{_zoneId}/administrative-status",
                CreateJsonContent(payload));
    }


    [Then("la zona está inactiva")]
    public async Task ThenZoneIsInactive()
    {
        Assert.NotNull(_response);

        var responseBody =
            await _response!.Content.ReadAsStringAsync();

        using var json =
            JsonDocument.Parse(responseBody);

        var zones =
            GetProperty(
                json.RootElement,
                "zones");

        var zone =
            zones
                .EnumerateArray()
                .First(z =>
                    GetProperty(
                        z,
                        "id").GetGuid() == _zoneId);

        var status =
            GetProperty(
                zone,
                "administrativeStatus").GetString();

        Assert.Equal(
            "inactive",
            status,
            ignoreCase: true);
    }


    // ========================================================================
    // HELPERS
    // ========================================================================

    private static StringContent CreateJsonContent(
        object payload)
    {
        var json =
            payload is string text
                ? text
                : JsonSerializer.Serialize(payload);

        return new StringContent(
            json,
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


    private static string GenerateBuildingCode()
    {
        var suffix =
            Guid.NewGuid()
                .ToString("N")
                .Substring(0, 8)
                .ToUpperInvariant();

        return $"BLD-ACC-{suffix}";
    }


    private static string GenerateZoneCode()
    {
        var suffix =
            Guid.NewGuid()
                .ToString("N")
                .Substring(0, 8)
                .ToUpperInvariant();

        return $"ZON-ACC-{suffix}";
    }
}