🏥 Cambios realizados en la rama justingomezcoello – HospitalAlertSystem
---
🚀 Conexión a Azure Service Bus
---
En esta rama se ha implementado la integración del sistema con Azure Service Bus, utilizando dos colas principales para la gestión de eventos críticos en el sistema de alertas hospitalarias:

Cola Alerts: configurada como la cola principal de procesamiento en Azure.

Cola sendAlertEvent: con permisos limitados, diseñada para enviar mensajes específicos.

🔑 Uso de la cadena de conexión principal
---
Se utilizó la cadena de conexión principal de Azure Service Bus para establecer la conexión al namespace, asegurando la autenticación adecuada y la autorización para trabajar con las colas.

```
private const string ConnectionString = "Endpoint=sb://<tu-namespace>.servicebus.windows.net/;SharedAccessKeyName=<KeyName>;SharedAccessKey=<Key>";
```

La cadena de conexión se define en la clase Program.cs dentro de cada consumer del sistema, por ejemplo:
```
EmergenciaConsumer/Program.cs
EnfermeriaConsumer/Program.cs
MantenimientoConsumer/Program.cs
```
---
📬 Envío de mensajes
---
La lógica implementada permite enviar múltiples mensajes a la cola sendAlertEvent. En la prueba realizada, se enviaron 15 mensajes de prueba, lo que confirma el correcto funcionamiento del flujo de envío de datos al Service Bus.


📂 Archivos modificados
---
Program.cs en cada carpeta de consumer (Emergencia, Enfermería, Mantenimiento):
```
Definición de la cadena de conexión.
Definición del QueueName (sendAlertEvent).
Implementación del cliente ServiceBusClient y el procesador ServiceBusProcessor.
```
🛠️ Permisos configurados en Azure
---
Cola sendAlertEvent: configurada solo para envío de mensajes, asegurando un control de acceso seguro.

Namespace HospitalAlertSystem en Azure Service Bus: configurado con la cadena de conexión principal para autenticación y acceso a recursos.

📈 Resultado de pruebas
---
✔️ Se envían correctamente los mensajes a la cola sendAlertEvent.

✔️ La integración con Azure Service Bus está funcional.

✔️ El sistema ahora es capaz de interactuar con Azure para el envío de eventos críticos.
