# 🏥 Hospital Alert System with RabbitMQ

This project simulates a hospital alert system using **RabbitMQ** for messaging between producers and consumers. It implements a real-world case where different types of alerts are sent from multiple services (maintenance, emergency, nursing).

---

## 🗂️ Project Structure

HospitalAlertSystem/
├── Domain/
│ ├── AlertEvent.cs // Shared data model
│ └── Constants.cs // Shared constants (exchanges, queues, routing keys)
├── Producer/
│ ├── Program.cs // Produces 3 types of alerts
│ └── Producer.csproj
├── Consumers/
│ ├── EmergenciaConsumer/
│ │ ├── Program.cs
│ │ └── EmergenciaConsumer.csproj
│ ├── EnfermeriaConsumer/
│ │ ├── Program.cs
│ │ └── EnfermeriaConsumer.csproj
│ └── MantenimientoConsumer/
│ ├── Program.cs
│ └── MantenimientoConsumer.csproj
├── AlertMonitorUI/
│ ├── Form1.cs // Windows Forms consumer alert UI
│ └── AlertMonitorUI.csproj
└── HospitalAlertSystem.sln // Visual Studio Solution file

---

## ✨ Key Features

- **Shared domain model**: `AlertEvent` includes type, severity, message, location, and timestamp.
- **Alert producer**: Randomly generates alerts with varying types and severities.
- **Specialized consumers**: Each consumer handles a specific type of alert with custom logic.
- **Direct exchange**: Routes specific alerts to their dedicated consumers.
- **Fanout exchange**: Broadcasts critical alerts to all consumers, regardless of type.
- **Simulated responses**: Each consumer simulates a different reaction based on alert type and severity.

---

## 📋 Requirements

- .NET 7.0 or higher  
- RabbitMQ Server  
- Visual Studio 2022 or Visual Studio Code

---


## 🐳 Docker - RabbitMQ Setup

If you don't have RabbitMQ installed, you can run it with Docker:

```bash
docker run -d --hostname rabbitmq-host --name hospital-rabbitmq -p 5672:5672 -p 15672:15672 -e RABBITMQ_DEFAULT_USER=guest -e RABBITMQ_DEFAULT_PASS=guest rabbitmq:3-management

---

## ⚙️ RabbitMQ Configuration

The project is set to connect to a local RabbitMQ server using default credentials:
- Host: `localhost`  
- Port: `5672`  
- Username: `guest`  
- Password: `guest`

If you wish to connect to a different RabbitMQ server, update the connection settings in each `Program.cs` file.

---

## ▶️ Running the Project

1. Ensure RabbitMQ Server is running.


```bash
2. Start the consumers (each in a separate terminal):
dotnet run --project Consumers/EmergenciaConsumer/EmergenciaConsumer.csproj
dotnet run --project Consumers/EnfermeriaConsumer/EnfermeriaConsumer.csproj
dotnet run --project Consumers/MantenimientoConsumer/MantenimientoConsumer.csproj


Start the producer:
dotnet run --project Producer/Producer.csproj


Launch the GUI Alert Monitor:
dotnet run --project AlertMonitorUI


🔄 Message Flow
The producer generates random alerts and publishes them to the direct exchange with the appropriate routing key.

Critical alerts are also published to a fanout exchange and distributed to all consumers.

Each consumer processes the alerts according to its type and severity, simulating a response.

🚨 Alert Types
Emergency: Related to urgent medical attention.

Nursing: Related to nursing care and medication administration.

Maintenance: Related to hospital infrastructure and equipment.

📶 Severities
Low: Needs attention but not urgent.

Medium: Should be addressed within a reasonable time.

High: Requires priority attention.

Critical: Needs immediate attention and is broadcasted to all services.
