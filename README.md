# PracticaRabbitMQ 🐰🚀

Sistema distribuido de gestión de dispositivos desarrollado con **.NET 8**, implementando una arquitectura de microservicios comunicados mediante **RabbitMQ** para el manejo de logs y eventos asíncronos.

## 🏗️ Estructura del Proyecto

El repositorio está organizado en las siguientes capas técnicas:

* **src/AppForSEII2526.API**: El núcleo del sistema. Una Web API que gestiona la lógica de negocio, persistencia de datos y la publicación de eventos hacia RabbitMQ.
* **src/LogViewer**: Servicio suscriptor que consume mensajes de RabbitMQ en tiempo real para el monitoreo del sistema.
* **src/AppForSEII2526.Web**: Interfaz de usuario basada en Blazor Web App para la gestión desde el navegador.
* **src/AppForSEII2526.Maui**: Aplicación multiplataforma (Android, iOS, Windows) desarrollada con .NET MAUI y Blazor Hybrid.
* **test/**: Suite de pruebas unitarias (UT) e integración de interfaz de usuario (UIT).

## 🛠️ Tecnologías Principales

* **.NET 8 (C#)**.
* **Entity Framework Core**: Para la gestión de la base de datos SQL Server.
* **RabbitMQ**: Message Broker para la comunicación desacoplada.
* **Docker**: Soporte para contenedores en la API y el LogViewer.

## 🚀 Instalación y Uso

### Requisitos previos
* Docker Desktop / RabbitMQ Server.
* SDK de .NET 8.
* SQL Server.

### Configuración
1.  **RabbitMQ**: Asegúrate de tener una instancia de RabbitMQ corriendo localmente en el puerto `5672`.
2.  **Base de Datos**: Ejecuta el script SQL ubicado en `src/AppForSEII2526.API/Data/scriptBaseDatos.sql` para preparar el entorno.
3.  **Ejecución**: Puedes usar el archivo `commands.bat` incluido para agilizar tareas comunes.

## 🧪 Pruebas
El proyecto incluye pruebas automatizadas para asegurar la calidad:
```bash
dotnet test test/AppForSEII2526.UT  # Pruebas Unitarias
