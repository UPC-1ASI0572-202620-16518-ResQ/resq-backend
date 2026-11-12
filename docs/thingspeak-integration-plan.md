# ResQ — integración ThingSpeak → backend (plan de implementación)

## Estado
Rama de trabajo: `feature/thingspeak-integration`. Esta primera fase **prepara y audita** la integración, sin activar todavía sincronización ni modificar `main`.

## Origen
Canal ThingSpeak: **3577021** (privado). Escritura desde firmware ESP32 ResQ 2.0 ya verificada por el equipo mediante código HTTP 200 y gráficos.

| Field | Significado | Tipo propuesto en Monitoring | Unidad |
|---|---|---|---|
| 1 | Gas MQ-2 crudo (ADC 0..4095) | `GAS_ADC` | `ADC` |
| 2 | Temperatura | `TEMPERATURE_C` | `C` |
| 3 | Intensidad Wi-Fi RSSI | `WIFI_RSSI` | `DBM` |
| 4 | Bandera de alerta general | `ALERT_FLAG` | `BOOLEAN` |
| 5 | Movimiento PIR | `MOTION` | `BOOLEAN` |
| 6 | Humedad relativa | `HUMIDITY` | `PERCENT` |

El mapeo es **propuesto**, sujeto al catálogo de capacidades y reglas realmente registradas en la base de datos. No convertir `GAS_ADC` a `GAS_PPM` ni asignar unidad PPM a mediciones no calibradas.

## Contratos reutilizables
- API de ingreso existente: `POST /api/v1/monitoring/measurements` (`MonitoringController`).
- `RecordMeasurementResource`: `deviceId` (GUID), `variableType`, `value`, `unit`, `measuredAt` (UTC).
- `MonitoringCommandService` exige dispositivo activo y capacidad de medición compatible con tipo y unidad; guarda la muestra y la envía al motor Risk Detection.
- La UI Angular ya consulta medidas actuales e históricas mediante la API de Monitoring. Se recomienda conservar esa fuente de verdad y no colocar la Read API Key en frontend.

## Implementación siguiente
1. Configurar en servidor `ThingSpeak:ChannelId=3577021` y `ThingSpeak:ReadApiKey` como secreto de entorno. No subir la clave a GitHub ni mostrarla en logs.
2. Crear cliente HTTP tipado con `IHttpClientFactory`, validación de respuesta, timeout y cancelación. Consultar la API de feeds de ThingSpeak con la clave por el lado servidor.
3. Incorporar un worker/servicio que procese entradas nuevas utilizando `entry_id` como identificador externo; asegurar idempotencia persistente incluso tras reinicios y despliegues múltiples.
4. Resolver de manera **explícita** un dispositivo existente y su organización/edificio/zona para los campos. No asumir que se dispone de sus IDs ni inventar asociaciones.
5. Procesar cada campo válido a través del servicio de Monitoring (para usar evaluación de riesgos existente). Respetar timestamps `created_at` de ThingSpeak.
6. Verificar si el motor de riesgo soporta `GAS_ADC`, temperatura, humedad y movimiento. Ajustar reglas/capacidades donde sea necesario; no inferir que la bandera Field 4 por sí sola genera incidentes.
7. Validar con tests: datos completos, faltantes, duplicados, reconexión, respuestas 401/429/5xx, cambios de zona y eventos DEMO documentados.
8. Validar que frontend muestre nuevas medidas; agregar actualización periódica si la experiencia lo requiere.

## Información necesaria para configurar en el despliegue
- URL/ambiente del backend en ejecución y forma de configurar secretos.
- `organizationId`, `deviceId` y capacidades del dispositivo registrado que representará ResQ.
- Read API Key **solo** en secret store del backend; no es necesaria para esta fase preparatoria.

## Riesgos y límites
- ThingSpeak es un almacén externo; la sincronización no debe bloquear el API ni duplicar lecturas.
- Sin calibración, un dato de gas en ADC no representa concentración ppm.
- Los registros generados por el modo DEMO usan los mismos fields que los reales; conservar en la documentación de pruebas el intervalo temporal de las demostraciones.
- El firmware todavía puede reportar `Gas=0 ADC` aunque el envío sea exitoso; esto requiere diagnóstico físico independiente.
