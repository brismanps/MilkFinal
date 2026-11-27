# Granja Lechera – API Web

**Taller de Lenguajes de Programación – Universidad Santiago de Cali**

API REST desarrollada en C# y .NET 10 para gestionar una granja lechera mediante arquitectura en capas.

## Estructura del proyecto

```text
Granja.slnx
│
├── Granja.Domain
│   ├── Entidades
│   ├── Reglas de negocio
│   ├── Excepciones
│   └── Interfaces
│
├── Granja.Infrastructure
│   ├── Entity Framework Core
│   ├── SQLite
│   ├── Configuraciones
│   ├── Repositorios
│   └── Data/Migrations
│
├── Granja.Services
│   ├── Servicios
│   ├── DTOs
│   ├── AutoMapper
│   └── Lógica FIFO
│
├── Granja.Web
│   ├── Controllers
│   ├── Program.cs
│   └── Swagger
│
└── Granja.Tests
    └── Pruebas unitarias xUnit
```

## Tecnologías utilizadas

- C#
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- Swagger / OpenAPI
- AutoMapper
- xUnit

## Arquitectura

El proyecto utiliza arquitectura en capas:

- **Domain:** entidades, reglas de negocio e interfaces.
- **Infrastructure:** persistencia de datos y repositorios.
- **Services:** lógica de negocio, servicios y DTOs.
- **Web:** controladores, configuración e inyección de dependencias.
- **Tests:** pruebas unitarias.

Los controladores no acceden directamente al DbContext.

## Cómo ejecutar el proyecto

### Requisitos

- SDK de .NET 10.
- Visual Studio 2022 compatible con .NET 10.
- Herramienta de línea de comandos `dotnet-ef`.

### 1. Clonar el repositorio

```bash
git clone https://github.com/brismanps/MilkFinal.git
```

### 2. Entrar en la carpeta del proyecto

```bash
cd MilkFinal
```

### 3. Restaurar dependencias

```bash
dotnet restore
```

### 4. Instalar la herramienta de migraciones

Si todavía no está instalada:

```bash
dotnet tool install --global dotnet-ef
```

### 5. Aplicar las migraciones existentes

```bash
dotnet ef database update --project Granja.Infrastructure --startup-project Granja.Web
```

Este comando aplica las migraciones incluidas en el repositorio y actualiza la base de datos. No es necesario crear una migración nueva.

### 6. Ejecutar la API

```bash
dotnet run --project Granja.Web
```

## Swagger

Una vez iniciada la API, acceder a:

`http://localhost:5080/swagger`

La dirección y el puerto pueden variar según la configuración de ejecución.

## Endpoints principales

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/granja` | Consultar configuración |
| PUT | `/api/granja` | Actualizar configuración |
| GET | `/api/granja/vacas` | Listar vacas |
| POST | `/api/produccion` | Registrar producción |
| GET | `/api/produccion/{fecha}` | Consultar producción |
| POST | `/api/produccion/transformar` | Transformar leche |
| GET | `/api/inventario` | Consultar inventario |
| POST | `/api/inventario/procesar-vencimientos` | Procesar vencimientos |
| GET | `/api/inventario/proximos-a-vencer` | Consultar alertas |
| POST | `/api/ventas` | Registrar venta |
| GET | `/api/ventas/{id}` | Consultar venta |

### Reportes

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/reportes/ventas` | Reporte de ventas |
| GET | `/api/reportes/leche-diaria` | Producción diaria de leche |
| GET | `/api/reportes/producto-diario` | Productos elaborados por día |
| GET | `/api/reportes/producto-total` | Resumen de productos |
| GET | `/api/reportes/producto-danado` | Productos dañados |
| GET | `/api/reportes/vacas` | Estadísticas de producción de vacas |

## Reglas de negocio

- Producción diaria calculada según el número de vacas y litros por vaca.
- Transformación de leche en queso o yogurt.
- Control de inventario mediante lotes.
- Control de fechas de vencimiento.
- Ventas sin límite diario, restringidas por existencias.
- Despacho FIFO: primero sale el producto más antiguo.
- Ventas atómicas: si no existe suficiente inventario, se rechaza la operación.
- Registro de productos dañados.

## Pruebas

Para ejecutar las pruebas unitarias:

```bash
dotnet test
```

El proyecto cuenta con pruebas unitarias desarrolladas con xUnit.

## Proyecto académico

**Asignatura:** Lenguajes de Programación  
**Universidad:** Universidad Santiago de Cali  
**Proyecto:** Sistema de gestión de una granja lechera  
**Lenguaje:** C# / .NET 10
