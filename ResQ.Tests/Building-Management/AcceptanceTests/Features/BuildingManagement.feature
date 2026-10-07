# language: es

Característica: Gestión de edificaciones y zonas

  Como administrador de una organización
  quiero gestionar las edificaciones y sus zonas
  para organizar la infraestructura que será monitoreada por ResQ.


  @US21 @AC1
  Escenario: Registrar una edificación con datos válidos

    Dado el usuario de pruebas está autenticado

    Cuando registro una edificación con los siguientes datos
      """json
      {
        "buildingCode": "BLD-ACCEPTANCE-01",
        "name": "Edificio Central de Operaciones",
        "description": "Sede principal corporativa con centro de monitoreo",
        "address": {
          "streetAddress": "Av. Javier Prado Este 456",
          "district": "San Isidro",
          "city": "Lima",
          "countryCode": "PE"
        }
      }
      """

    Entonces la respuesta HTTP debe ser 201
    Y la edificación creada pertenece a la organización de pruebas
    Y la edificación creada está activa

    Cuando consulto la lista de edificaciones

    Entonces la respuesta HTTP debe ser 200
    Y la lista contiene la edificación creada

    Cuando consulto la edificación creada

    Entonces la respuesta HTTP debe ser 200
    Y la edificación consultada tiene el código registrado


  @US21 @AC1
  Escenario: Actualizar y cambiar el estado de una edificación

    Dado el usuario de pruebas está autenticado
    Y existe una edificación de prueba

    Cuando actualizo los detalles de la edificación

    Entonces la respuesta HTTP debe ser 200
    Y la edificación actualizada tiene el nombre "Edificio Acceptance Actualizado"

    Cuando desactivo la edificación

    Entonces la respuesta HTTP debe ser 200
    Y la edificación está inactiva


  @US21 @AC2
  Escenario: Rechazar el registro de una edificación con datos inválidos

    Dado el usuario de pruebas está autenticado

    Cuando registro una edificación con los siguientes datos
      """json
      {
        "buildingCode": "bld central 01!",
        "name": "",
        "description": "Descripción inválida",
        "address": {
          "streetAddress": "",
          "district": "San Isidro",
          "city": "Lima",
          "countryCode": "PER"
        }
      }
      """

    Entonces la respuesta HTTP debe ser 400
    Y la edificación inválida no fue creada


  @US22 @AC1
  Escenario: Registrar y consultar una zona dentro de una edificación

    Dado el usuario de pruebas está autenticado
    Y existe una edificación de prueba

    Cuando registro una zona con los siguientes datos
      """json
      {
        "zoneCode": "ZON-ACCEPTANCE-01",
        "name": "Sala Principal de Servidores",
        "description": "Data center principal",
        "floorLabel": "Sótano 1"
      }
      """

    Entonces la respuesta HTTP debe ser 201
    Y la zona creada pertenece a la edificación de prueba

    Cuando consulto la lista de zonas de la edificación

    Entonces la respuesta HTTP debe ser 200
    Y la lista contiene la zona creada

    Cuando consulto la zona creada

    Entonces la respuesta HTTP debe ser 200
    Y la zona consultada pertenece a la edificación de prueba


  @US22 @AC2
  Escenario: Actualizar una zona manteniendo su relación con la edificación

    Dado el usuario de pruebas está autenticado
    Y existe una edificación de prueba con una zona

    Cuando actualizo los detalles de la zona

    Entonces la respuesta HTTP debe ser 200
    Y la zona actualizada conserva la edificación de prueba

    Cuando desactivo la zona

    Entonces la respuesta HTTP debe ser 200
    Y la zona está inactiva