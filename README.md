# FastFix API

FastFix es una API desarrollada para administrar las solicitudes de reparación de electrodomésticos a domicilio.

El sistema permite manejar clientes, técnicos y solicitudes de servicio, además de controlar la asignación de técnicos y los estados de cada solicitud.

## Tecnologías utilizadas

- C#
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- Swagger
- Azure App Service

## Arquitectura

El proyecto utiliza una arquitectura monolítica con organización MVC.

Se eligió esta arquitectura porque FastFix es un negocio pequeño, trabaja en una sola ciudad y cuenta con cuatro técnicos.

## Funcionalidades principales

La API permite:

- Crear y consultar clientes.
- Crear y consultar técnicos.
- Listar técnicos disponibles.
- Crear solicitudes de servicio.
- Listar solicitudes.
- Filtrar solicitudes por estado.
- Asignar un técnico a una solicitud.
- Cambiar una solicitud a estado En Proceso.
- Marcar una solicitud como Completada.
- Modificar solicitudes.
- Eliminar solicitudes que no estén completadas.

## Reglas de negocio

El sistema valida las siguientes reglas:

- El nombre del cliente es obligatorio.
- El teléfono debe contener exactamente 8 dígitos.
- La descripción del problema debe tener al menos 10 caracteres.
- Solo se puede asignar un técnico que esté disponible.
- Un técnico puede tener como máximo 3 solicitudes activas.
- Una solicitud no puede completarse si no tiene un técnico asignado.
- Una solicitud completada no puede modificarse.
- Una solicitud completada no puede eliminarse.

## Ejecutar el proyecto localmente

El proyecto utiliza .NET 8.

Primero se deben restaurar las dependencias:

```bash
dotnet restore
