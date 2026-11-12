# Configuración ThingSpeak / ResQ en Render

## Ya disponible en la rama
- Cliente de feeds JSON privado con `THINGSPEAKAPIKEY` en el encabezado de la solicitud.
- Parser de `entry_id`, `created_at` y `field1..field6`.
- Mapeo tipado a variables y unidades de Monitoring; la lectura MQ-2 es **ADC**, nunca PPM sin calibración.
- Registro del cliente HTTP en el contexto Monitoring.

## Variable de entorno
En Render > servicio de backend > Environment:

| Key | Value |
|---|---|
| `ThingSpeak__ReadApiKey` | Clave privada de lectura (configurar en Render; no compartir ni versionar) |
| `ThingSpeak__ChannelId` | `3577021` (opcional: valor predeterminado del cliente) |

**No se debe activar un importador de fondo todavía**: la configuración de la clave no significa que el código de esta rama se haya desplegado. El código de `main` no contiene estos cambios.

## Prerrequisito para la ingesta
Un dispositivo físico ResQ existente en el backend, con estado **active**, asociado a una edificación y preferentemente a una zona, debe tener las capacidades **measurement** y unidades compatibles con los seis campos definidos en `ThingSpeakMeasurementMapper`.

Usar Swagger del servicio desplegado y autenticación del sistema:
1. `GET /api/v1/devices` para listar los equipos existentes.
2. `GET /api/v1/devices/{deviceId}` para verificar `id`, `administrativeStatus`, `assignment` y `capabilities`.
3. Si el dispositivo no existe, registrar mediante `POST /api/v1/devices` y asociarlo a un `buildingId` real. El backend crea dispositivos inicialmente inactivos; activarlos con la operación de estado.
4. Solo una vez confirmado el GUID del dispositivo, configurar el mapeo en la ingesta. No usar IDs de ejemplo.

## Próximo cambio de código
Desarrollar worker idempotente con cursor persistente por canal y control de concurrencia para despliegues múltiples. Cada entrada debe procesarse una sola vez o tener deduplicación por campo; reutilizar `IMonitoringCommandService` para mantener la evaluación de riesgos. No almacenar claves en código, body o logs.

## Despliegue
Abrir PR y ejecutar build/tests; configurar variables en Render antes de desplegar. Verificar el backend y la UI contra lecturas reales. No probar con la clave en parámetros de URL enviados al navegador.


## Worker incorporado (fase 2)
`ThingSpeakImportWorker` fue agregado y registrado en DI. **Desactivado por defecto**. Lee periódicamente las entradas (30 s por defecto), mapea seis campos y usa `IMonitoringCommandService` para escribir en Monitoring y evaluar riesgos. Comprueba si ya existe medición del mismo dispositivo, variable y fecha antes de agregarla.

Configurar estas variables **solo después de identificar los IDs reales y validar capacidades del dispositivo**:

| Variable Render | Valor |
|---|---|
| `ThingSpeak__Enabled` | `true` (activar solo tras la validación) |
| `ThingSpeak__OrganizationId` | GUID real de la organización |
| `ThingSpeak__DeviceId` | GUID real del dispositivo activo |
| `ThingSpeak__PollIntervalSeconds` | `30` |
| `ThingSpeak__ImportHistorical` | `false` (al iniciar toma la primera lectura como referencia y empieza con nuevas) |

**Advertencias antes de producción**:
- El control de duplicados se basa en el trío `DeviceId + VariableType + MeasuredAt`, no en una clave única transaccional. Este worker es para **una sola instancia**. Para varios procesos se necesita bloqueo distribuido + índice único/cursor persistente con `entry_id/field`.
- Si Monitoring guarda la medida pero Risk Detection falla, el siguiente sondeo detecta la medida existente y no reevaluará automáticamente ese error parcial. Debe corregirse antes de producción crítica.
- En reinicios se establece un nuevo baseline de entradas si `ImportHistorical=false`; pueden perderse entradas del intervalo de reinicio. Para procesamiento fiable durante caídas, usar cursor durable y recuperación de lotes.
- El cliente usa el encabezado `THINGSPEAKAPIKEY`. Validar conectividad/compatibilidad con el servidor real y tests de integración.
- No se ha ejecutado `dotnet build` ni prueba contra Render desde este entorno; **no se debe hacer merge a main o desplegar sin verificación**.
- Mientras no se arregle el registro/login de usuarios, no se han podido verificar `OrganizationId`, `DeviceId`, capacidades, autenticación o reglas reales de riesgos.
- Si el endpoint de registro del usuario sigue dando HTTP 500, esa incidencia es independiente del importador y debe resolverse antes de validar el recorrido en frontend.
