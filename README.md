# Hospital Alert System with RabbitMQ

Este proyecto simula un sistema de alertas hospitalarias utilizando RabbitMQ para la mensajería entre productores y consumidores. El sistema implementa un caso real donde se emiten distintos tipos de alertas desde diferentes servicios (mantenimiento, emergencias, enfermería).

## Estructura del Proyecto

```
HospitalAlertSystem/
│
├── Domain/
│   ├── AlertEvent.cs           // Modelo de datos compartido
│   └── Constants.cs            // Constantes compartidas (exchanges, queues, routing keys)
│
├── Producer/
│   ├── Program.cs              // Produce 3 tipos de alertas diferentes
│   └── Producer.csproj
│
├── Consumers/
│   ├── EmergenciaConsumer/     // Consumidor especializado en alertas de emergencia
│   │   ├── Program.cs
│   │   └── EmergenciaConsumer.csproj
│   ├── EnfermeriaConsumer/     // Consumidor especializado en alertas de enfermería
│   │   ├── Program.cs
│   │   └── EnfermeriaConsumer.csproj
│   ├── MantenimientoConsumer/  // Consumidor especializado en alertas de mantenimiento
│   │   ├── Program.cs
│   │   └── MantenimientoConsumer.csproj
│
└── HospitalAlertSystem.sln     // Solución de Visual Studio
```

## Características Principales

- **Modelo de dominio compartido**: `AlertEvent` con tipo de alerta, severidad, mensaje, ubicación y timestamp.
- **Productor de alertas**: Genera aleatoriamente alertas de diferentes tipos y severidades.
- **Consumidores especializados**: Cada uno maneja un tipo específico de alerta con lógica personalizada.
- **Exchange directo**: Enruta alertas específicas a sus respectivos consumidores.
- **Exchange fanout**: Distribuye alertas críticas a todos los consumidores, independientemente del tipo.
- **Simulación de respuestas**: Cada consumidor simula diferentes respuestas según el tipo y severidad de la alerta.

## Requisitos

- .NET 7.0 o superior
- RabbitMQ Server
- Visual Studio 2022 o VS Code

## Configuración de RabbitMQ

El proyecto está configurado para conectarse a un servidor RabbitMQ local con la configuración por defecto:
- Host: localhost
- Puerto: 5672
- Usuario: guest
- Contraseña: guest

Para utilizar un servidor RabbitMQ diferente, modifique la configuración en los archivos `Program.cs` de cada proyecto.

## Ejecución del Proyecto

1. Asegúrese de que el servidor RabbitMQ esté en ejecución.
2. Inicie los consumidores (ejecute cada uno en una terminal separada):
   ```
   dotnet run --project Consumers/EmergenciaConsumer/EmergenciaConsumer.csproj
   dotnet run --project Consumers/EnfermeriaConsumer/EnfermeriaConsumer.csproj
   dotnet run --project Consumers/MantenimientoConsumer/MantenimientoConsumer.csproj
   ```
3. Inicie el productor:
   ```
   dotnet run --project Producer/Producer.csproj
   ```

## Flujo de Mensajes

1. El productor genera alertas aleatorias y las publica en el exchange directo con la routing key apropiada.
2. Las alertas críticas también se publican en el exchange fanout para ser distribuidas a todos los consumidores.
3. Cada consumidor procesa las alertas según su tipo y severidad, simulando diferentes respuestas.

## Tipos de Alertas

- **Emergencia**: Alertas relacionadas con la atención médica urgente de pacientes.
- **Enfermería**: Alertas relacionadas con cuidados de enfermería y administración de medicamentos.
- **Mantenimiento**: Alertas relacionadas con la infraestructura y equipamiento del hospital.

## Severidades

- **Baja**: Requiere atención pero no es urgente.
- **Media**: Requiere atención en un tiempo razonable.
- **Alta**: Requiere atención prioritaria.
- **Crítica**: Requiere atención inmediata y se distribuye a todos los servicios.