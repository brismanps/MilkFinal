using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
using static Granja.Tests.Entorno;

namespace Granja.Tests;

public class GranjaTests
{
    [Fact]
    public async Task Produccion_diaria_es_n_por_l_y_no_se_registra_dos_veces_en_la_misma_fecha()
    {
        using var e = new Entorno();
        var r = await e.Produccion.RegistrarProduccionAsync(new(D(1), null));

        Assert.Equal(150m, r.LitrosProducidos);   // 10 vacas × 15 L
        await Assert.ThrowsAsync<ConflictoException>(() => e.Produccion.RegistrarProduccionAsync(new(D(1), null)));
    }

    [Fact]
    public async Task No_se_pueden_transformar_mas_litros_que_los_disponibles_del_dia()
    {
        using var e = new Entorno();
        await e.ProducirAsync(1, (TipoProducto.Queso, 100));

        await Assert.ThrowsAsync<LecheInsuficienteException>(
            () => e.Produccion.TransformarAsync(new(TipoProducto.Yogurt, 60, D(1))));   // quedan 50 L
    }

    [Fact]
    public async Task Venta_toma_primero_del_lote_mas_antiguo()
    {
        using var e = new Entorno();
        await e.ProducirAsync(1, (TipoProducto.Queso, 100));   // Q1: 10 kg
        await e.ProducirAsync(2, (TipoProducto.Queso, 150));   // Q2: 15 kg

        var venta = await e.Ventas.RegistrarVentaAsync(new(TipoProducto.Queso, 5, D(3)));

        var detalle = Assert.Single(venta.Detalle);
        Assert.Equal(D(1), detalle.FechaProduccion);
        Assert.Equal(5m, detalle.Cantidad);
    }

    [Fact]
    public async Task Venta_que_excede_un_lote_consume_el_siguiente_y_genera_varios_detalles()
    {
        using var e = new Entorno();
        await e.ProducirAsync(1, (TipoProducto.Queso, 100));
        await e.ProducirAsync(2, (TipoProducto.Queso, 150));

        var venta = await e.Ventas.RegistrarVentaAsync(new(TipoProducto.Queso, 12, D(3)));

        Assert.Equal(2, venta.Detalle.Count);
        Assert.Equal(10m, venta.Detalle[0].Cantidad);
        Assert.Equal(2m, venta.Detalle[1].Cantidad);
        Assert.Equal(28000m, venta.PrecioUnitario);
        Assert.Equal(336000m, venta.Total);
    }

    [Fact]
    public async Task Lote_vencido_nunca_se_vende_aunque_sea_el_mas_antiguo()
    {
        using var e = new Entorno();
        await e.ProducirAsync(1, (TipoProducto.Yogurt, 50));    // vence el día 16
        await e.ProducirAsync(20, (TipoProducto.Yogurt, 50));   // vence el 4 de enero

        var venta = await e.Ventas.RegistrarVentaAsync(new(TipoProducto.Yogurt, 5, D(20)));

        var detalle = Assert.Single(venta.Detalle);
        Assert.Equal(D(20), detalle.FechaProduccion);   // el lote del día 1 estaba vencido
        // y si piden más de lo que hay en el lote vigente, se rechaza aunque el vencido tenga existencias
        await Assert.ThrowsAsync<InventarioInsuficienteException>(
            () => e.Ventas.RegistrarVentaAsync(new(TipoProducto.Yogurt, 51, D(20))));
    }

    [Fact]
    public async Task Venta_con_existencias_insuficientes_no_modifica_el_inventario()
    {
        using var e = new Entorno();
        await e.ProducirAsync(1, (TipoProducto.Queso, 50));   // 5 kg

        await Assert.ThrowsAsync<InventarioInsuficienteException>(
            () => e.Ventas.RegistrarVentaAsync(new(TipoProducto.Queso, 10, D(3))));

        var inv = await e.Inventario.ObtenerInventarioAsync(D(3));
        Assert.Equal(5m, Assert.Single(inv.Lotes).CantidadDisponible);
        Assert.Empty(await e.Ventas.ListarAsync(null, null, null));
    }

    [Fact]
    public async Task Al_procesar_vencimientos_lo_disponible_pasa_a_danado_y_el_reporte_lo_refleja()
    {
        using var e = new Entorno();
        await e.ProducirAsync(1, (TipoProducto.Yogurt, 50));
        await e.Ventas.RegistrarVentaAsync(new(TipoProducto.Yogurt, 20, D(3)));

        var venc = await e.Inventario.ProcesarVencimientosAsync(D(17));

        Assert.Equal(30m, venc.TotalDanado);
        var reporte = await e.Reportes.ProductoDanadoAsync(null, null);
        var lote = Assert.Single(reporte.Lotes);
        Assert.Equal(30m, lote.CantidadDanada);
        Assert.Equal(D(16), lote.FechaVencimiento);
        Assert.Equal(30m, Assert.Single(reporte.PorTipo).CantidadDanada);
    }

    [Fact]
    public async Task Escenario_de_referencia_del_enunciado()
    {
        using var e = new Entorno();
        await e.ProducirAsync(1, (TipoProducto.Queso, 100), (TipoProducto.Yogurt, 50));
        await e.ProducirAsync(2, (TipoProducto.Queso, 150));

        var vq = await e.Ventas.RegistrarVentaAsync(new(TipoProducto.Queso, 12, D(3)));
        Assert.Equal(2, vq.Detalle.Count);
        await e.Ventas.RegistrarVentaAsync(new(TipoProducto.Yogurt, 20, D(3)));

        await e.Inventario.ProcesarVencimientosAsync(D(17));
        await Assert.ThrowsAsync<InventarioInsuficienteException>(
            () => e.Ventas.RegistrarVentaAsync(new(TipoProducto.Yogurt, 5, D(17))));

        var total = await e.Reportes.ProductoTotalAsync();
        var queso = total.Single(t => t.Tipo == TipoProducto.Queso);
        var yogurt = total.Single(t => t.Tipo == TipoProducto.Yogurt);
        Assert.Equal((25m, 12m, 13m, 0m), (queso.Producido, queso.Vendido, queso.Disponible, queso.Danado));
        Assert.Equal((50m, 20m, 0m, 30m), (yogurt.Producido, yogurt.Vendido, yogurt.Disponible, yogurt.Danado));

        var leche = await e.Reportes.LecheDiariaAsync(null, null);
        Assert.Equal(2, leche.Count);
        Assert.All(leche, l => Assert.Equal(150m, l.LitrosProducidos));
        Assert.Equal(0m, leche[0].LitrosNoAprovechados);

        var ventas = await e.Reportes.VentasAsync(null, null, null);
        Assert.Equal(12m * 28000 + 20m * 8000, ventas.TotalGeneral);
    }

    [Fact]
    public async Task Reporte_de_vacas_identifica_mayor_menor_y_por_debajo_del_promedio()
    {
        using var e = new Entorno();
        var vacas = await e.Granja.ListarVacasAsync();
        var req = new Granja.Services.Dtos.ProduccionRequest(D(1), new()
        {
            new(vacas[0].Id, 25),   // la que más produce
            new(vacas[1].Id, 5)     // la que menos produce
        });
        await e.Produccion.RegistrarProduccionAsync(req);

        var r = await e.Reportes.VacasAsync(null, null);

        Assert.Equal(vacas[0].Id, r.MasProduce!.VacaId);
        Assert.Equal(vacas[1].Id, r.MenosProduce!.VacaId);
        Assert.Contains(r.PorDebajoDelPromedio, v => v.VacaId == vacas[1].Id);
        Assert.DoesNotContain(r.PorDebajoDelPromedio, v => v.VacaId == vacas[0].Id);
    }
}
