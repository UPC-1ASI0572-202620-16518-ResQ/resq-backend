# Demo IoT ResQ — despliegue seguro (sin login de usuarios)

Esta funcionalidad es una **vista de demostración con telemetría públicamente visible**, independiente del registro de dispositivos del dominio Monitoring. No crea alertas ni incidentes persistentes.

## Backend
- Endpoint: `GET /api/v1/prototype/telemetry/latest`.
- Por defecto devuelve 404 (funcionalidad desactivada).
- Solo si `ThingSpeak__PublicDemoEnabled=true` se permite consultar lecturas recientes del canal configurado.
- El endpoint es `AllowAnonymous` **deliberadamente** para superar el bloqueo temporal del registro/login. Si los datos fueran sensibles, no habilitar el modo público.
- La Read API Key solo vive en el entorno del servidor y nunca se devuelve al navegador.
- Cache compartida en memoria del proceso durante 25 segundos; en múltiples réplicas cada réplica usa su caché.
- La respuesta incluye `channelId`, `entryId`, `measuredAt`, `gasAdc`, `temperatureC`, `wifiRssiDbm`, `alertFlag`, `motion` y `humidityPercent`. Los campos ausentes son null.
- La bandera `alertFlag` es el campo enviado por el firmware, **no** una alerta persistida de Risk Detection.

## Variables para Render
| Nombre | Valor |
|---|---|
| `ThingSpeak__ReadApiKey` | clave privada de lectura ya configurada |
| `ThingSpeak__ChannelId` | `3577021` (predeterminado) |
| `ThingSpeak__PublicDemoEnabled` | `true` **solo cuando se quiera publicar esta telemetría** |
| `ThingSpeak__Enabled` | `false` (importador separado, todavía no habilitado) |

## Pruebas después de merge y despliegue
1. Confirmar `GET https://resq-api-79xy.onrender.com/api/v1/prototype/telemetry/latest` → JSON sin claves.
2. Si 404: habilitar `ThingSpeak__PublicDemoEnabled=true` y desplegar rama backend correcta.
3. Si 503: revisar variable `ThingSpeak__ReadApiKey` en Render, sin copiarla al navegador.
4. Si 502: revisar conectividad ThingSpeak/encabezado de lectura en los logs del backend (sin exponer secreto).
5. Abrir frontend `/prototype`; debe actualizarse cada 25 s.
6. Probar modo DEMO del ESP32 y confirmar cambio de lecturas.
7. Si se desactiva la demo: ajustar `ThingSpeak__PublicDemoEnabled=false` y redeploy.

## Limitaciones
- Esta vista no sustituye la integración final a Monitoring con GUIDs y autenticación funcional.
- MQ-2 ADC es señal cruda, no PPM.
- El firmware mezcla datos reales y DEMO en los mismos seis campos; la vista no puede distinguirlos sin cambios adicionales al protocolo.
- Esta rama contiene además un importador de Monitoring opt-in, **desactivado por defecto**. No activar `ThingSpeak__Enabled` sin GUIDs y revisión de idempotencia.
- Código todavía sin ejecutar con `dotnet build` ni tests en entorno real: revisar CI/deploy antes de fusionar.
