# language: es

Característica: Gestión de suscripciones

  Como organización autenticada
  quiero gestionar mi suscripción
  para controlar su ciclo de vida dentro de ResQ


  @Subscriptions @Create
  Escenario: Crear una suscripción válida

    Dado el usuario de pruebas de suscripciones está autenticado

    Cuando creo una suscripción válida

    Entonces la respuesta HTTP de suscripciones debe ser 201
    Y la suscripción creada está activa
    Y la suscripción creada pertenece a la organización de pruebas


  @Subscriptions @Create
  Escenario: Rechazar la creación de una segunda suscripción

    Dado el usuario de pruebas de suscripciones está autenticado
    Y existe una suscripción activa de prueba

    Cuando intento crear otra suscripción

    Entonces la respuesta HTTP de suscripciones debe ser 409


  @Subscriptions @Create
  Escenario: Rechazar una suscripción con fechas inválidas

    Dado el usuario de pruebas de suscripciones está autenticado

    Cuando creo una suscripción con fechas inválidas

    Entonces la respuesta HTTP de suscripciones debe ser 400


  @Subscriptions @Query
  Escenario: Consultar la suscripción de la organización

    Dado el usuario de pruebas de suscripciones está autenticado
    Y existe una suscripción activa de prueba

    Cuando consulto la suscripción de la organización

    Entonces la respuesta HTTP de suscripciones debe ser 200


  @Subscriptions @Query
  Escenario: Consultar una suscripción específica

    Dado el usuario de pruebas de suscripciones está autenticado
    Y existe una suscripción activa de prueba

    Cuando consulto la suscripción creada

    Entonces la respuesta HTTP de suscripciones debe ser 200
    Y la suscripción consultada corresponde a la suscripción creada


  @Subscriptions @Cancel
  Escenario: Cancelar una suscripción activa

    Dado el usuario de pruebas de suscripciones está autenticado
    Y existe una suscripción activa de prueba

    Cuando cancelo la suscripción

    Entonces la respuesta HTTP de suscripciones debe ser 200
    Y la suscripción queda cancelada


  @Subscriptions @Renew
  Escenario: Rechazar la renovación de una suscripción activa

    Dado el usuario de pruebas de suscripciones está autenticado
    Y existe una suscripción activa de prueba

    Cuando intento renovar la suscripción activa

    Entonces la respuesta HTTP de suscripciones debe ser 409


  @Subscriptions @Expire
  Escenario: Rechazar la expiración de una suscripción que aún está activa

    Dado el usuario de pruebas de suscripciones está autenticado
    Y existe una suscripción activa de prueba

    Cuando intento expirar la suscripción activa

    Entonces la respuesta HTTP de suscripciones debe ser 409