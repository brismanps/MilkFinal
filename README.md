# Granja lechera – API Web (.NET 10, arquitectura en capas)

Taller de Lenguajes de Programación – Universidad Santiago de Cali.

## Estructura
```
Granja.slnx
├── Granja.Domain          Entidades, reglas de lote, excepciones, interfaces (sin dependencias)
├── Granja.Infrastructure  EF Core (SQLite), configuraciones, repositorios, UnitOfWork, SystemClock
├── Granja.Services        Servicios (Granja, Produccion, Inventario, Venta, Reporte), DTOs, AutoMapper, FIFO
├── Granja.Web             Controllers, Program.cs (composición), manejo global de errores, Swagger
└── Granja.Tests           Pruebas unitarias xUnit
```
Dependencias: Web → Services → Domain, Infrastructure → Domain; Web referencia Infrastructure
solo para registrar dependencias en `Program.cs`. Los controllers no usan el DbContext.

> La entidad "Granja" se llama `GranjaLechera` para no chocar con el namespace `Granja`.

## Cómo ejecutar
Requisitos: SDK de .NET 10.

```bash
dotnet restore
# Migraciones (una sola vez)
dotnet tool install --global dotnet-ef
dotnet ef migrations add Inicial -p Granja.Infrastructure -s Granja.Web -o Data/Migrations
# Correr la API (aplica migraciones y siembra n=10, l=15 automáticamente)
dotnet run --project Granja.Web
```
Swagger: http://localhost:5080/swagger  ·  Colección de pruebas: `Granja.Web/Granja.http`

> Si prefieres archivo `.sln`: `dotnet sln Granja.slnx migrate`.

## Pruebas
```bash
dotnet test
```

## Endpoints
| Método | Ruta | Descripción |
|---|---|---|
| GET/PUT | /api/granja | Configuración (n, l, rendimiento, vida útil, precio) |
| GET | /api/granja/vacas | Lista de vacas |
| POST | /api/produccion | Registra producción del día (n × l), una vez por fecha |
| GET | /api/produccion/{fecha} | Leche producida / transformada / no aprovechada |
| POST | /api/produccion/transformar | Convierte leche en lote de queso o yogurt |
| GET | /api/inventario | Lotes con existencias y resumen |
| POST | /api/inventario/procesar-vencimientos | Vencido no vendido → dañado |
| GET | /api/inventario/proximos-a-vencer | Alerta de vencimiento (extensión) |
| POST/GET | /api/ventas, /api/ventas/{id} | Venta FIFO atómica (409 si no alcanza) |
| GET | /api/reportes/ventas, leche-diaria, productodiario, producto-total, productodanado | Reportes del enunciado |
| GET | /api/reportes/vacas | Promedio, la que más/menos produce, por debajo del promedio |

Errores: `ProblemDetails` con 400 (validación), 404 (no existe) y 409 (leche/inventario insuficiente, producción repetida).
