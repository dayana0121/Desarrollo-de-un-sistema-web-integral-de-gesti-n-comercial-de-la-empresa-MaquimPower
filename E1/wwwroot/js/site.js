// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
/* =========================================
   MODULO TERCEROS - CLIENTES
========================================= */

/* =========================================
   MODULO TERCEROS - CLIENTES
========================================= */

let tablaClientes = null;

$(document).ready(function () {
    inicializarClientes();
    inicializarRestriccionesCliente();
});

function inicializarClientes() {
    if ($("#tblClientes").length === 0) return;

    tablaClientes = $("#tblClientes").DataTable({
        ajax: {
            url: "/Terceros/ListarClientes",
            type: "GET",
            dataSrc: "data"
        },
        initComplete: function () {
            this.api().column(5).search("^Activo$", true, false).draw();
        },
        columns: [
            {
                data: null,
                render: function (data) {
                    return data.tipo_documento + " - " + data.numero_documento;
                }
            },
            { data: "nombre_completo" },
            { data: "telefono", defaultContent: "" },
            { data: "email", defaultContent: "" },
            { data: "direccion", defaultContent: "" },
            {
                data: "estado",
                render: function (estado, type) {
                    if (type === 'display') {
                        return estado
                            ? '<span class="badge-activo">Activo</span>'
                            : '<span class="badge-inactivo">Inactivo</span>';
                    }
                    return estado ? 'Activo' : 'Inactivo';
                }
            },
            {
                data: null,
                orderable: false,
                searchable: false,
                render: function (data) {
                    return `
                        <button type="button" class="btn-edit" onclick='editarCliente(${JSON.stringify(data)})'>
                            <i class="fa-solid fa-pen"></i>
                        </button>

                        <button type="button" class="btn-delete" onclick="eliminarCliente(${data.id_cliente})">
                            <i class="fa-solid fa-trash"></i>
                        </button>
                    `;
                }
            }
        ]
    });

    $("#formCliente").on("submit", function (e) {
        e.preventDefault();

        if (!validarCliente()) return;

        $.ajax({
            url: "/Terceros/GuardarCliente",
            type: "POST",
            data: $(this).serialize(),
            success: function (res) {
                $("#modalCliente").modal("hide");
                tablaClientes.ajax.reload(null, false);
                showAlert(res.mensaje || "Cliente guardado correctamente", "success");
            },
            error: function () {
                showAlert("Error al guardar el cliente", "error");
            }
        });
    });
}

function inicializarRestriccionesCliente() {
    $("#tipo_documento").on("change", function () {
        const tipo = $(this).val();
        const doc = $("#numero_documento");

        doc.val("");

        if (tipo === "DNI") {
            doc.attr("maxlength", "8");
            doc.attr("minlength", "8");
            doc.attr("placeholder", "Ingrese 8 dígitos");
        } else if (tipo === "RUC") {
            doc.attr("maxlength", "11");
            doc.attr("minlength", "11");
            doc.attr("placeholder", "Ingrese 11 dígitos");
        } else {
            doc.attr("maxlength", "11");
            doc.removeAttr("minlength");
            doc.attr("placeholder", "Ingrese documento");
        }
    });

    $("#numero_documento").on("input", function () {
        this.value = this.value.replace(/\D/g, "");
    });

    $("#telefono").on("input", function () {
        this.value = this.value.replace(/\D/g, "").slice(0, 9);
    });
}

function validarCliente() {
    const tipo = $("#tipo_documento").val();
    const documento = $("#numero_documento").val().trim();
    const nombre = $("#nombre_completo").val().trim();
    const telefono = $("#telefono").val().trim();
    const email = $("#email").val().trim();

    if (!tipo) {
        showAlert("Seleccione el tipo de documento.", "error");
        return false;
    }

    if (tipo === "DNI" && documento.length !== 8) {
        showAlert("El DNI debe tener exactamente 8 dígitos.", "error");
        return false;
    }

    if (tipo === "RUC" && documento.length !== 11) {
        showAlert("El RUC debe tener exactamente 11 dígitos.", "error");
        return false;
    }

    if (nombre.length < 3) {
        showAlert("El nombre o razón social debe tener al menos 3 caracteres.", "error");
        return false;
    }

    if (tipo === "DNI" && /\d/.test(nombre)) {
        showAlert("El nombre de una persona no debe contener números.", "error");
        return false;
    }

    if (telefono && telefono.length !== 9) {
        showAlert("El celular debe tener exactamente 9 dígitos.", "error");
        return false;
    }

    if (telefono && !telefono.startsWith("9")) {
        showAlert("El celular debe iniciar con 9.", "error");
        return false;
    }

    if (email) {
        const correoValido = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        if (!correoValido.test(email)) {
            showAlert("Ingrese un correo válido.", "error");
            return false;
        }
    }

    return true;
}

function abrirModalNuevoCliente() {
    $("#formCliente")[0].reset();

    $("#id_cliente").val(0);
    $("#estado").val("true");
    $("#tipo_documento").val("");

    $("#numero_documento")
        .val("")
        .attr("maxlength", "11")
        .removeAttr("minlength")
        .attr("placeholder", "Ingrese documento");

    $("#tituloModalCliente").text("Nuevo Cliente");
    $("#modalCliente").modal("show");
}

function editarCliente(c) {
    $("#tituloModalCliente").text("Editar Cliente");

    $("#id_cliente").val(c.id_cliente);
    $("#tipo_documento").val(c.tipo_documento);
    $("#numero_documento").val(c.numero_documento);
    $("#nombre_completo").val(c.nombre_completo);
    $("#telefono").val(c.telefono || "");
    $("#email").val(c.email || "");
    $("#direccion").val(c.direccion || "");
    $("#estado").val(c.estado ? "true" : "false");

    $("#tipo_documento").trigger("change");
    $("#numero_documento").val(c.numero_documento);

    $("#modalCliente").modal("show");
}

function eliminarCliente(id) {
    if (!confirm("¿Seguro que deseas eliminar este cliente?")) return;

    $.ajax({
        url: "/Terceros/EliminarCliente",
        type: "POST",
        data: { id_cliente: id },
        success: function (res) {
            tablaClientes.ajax.reload(null, false);
            showAlert(res.mensaje || "Cliente eliminado correctamente", "success");
        },
        error: function () {
            showAlert("Error al eliminar el cliente", "error");
        }
    });
}

/* =========================================
   MODULO TERCEROS - PROVEEDORES
========================================= */

let tablaProveedores = null;

$(document).ready(function () {
    inicializarProveedores();
    inicializarRestriccionesProveedor();
});

function inicializarProveedores() {
    if ($("#tblProveedores").length === 0) return;

    tablaProveedores = $("#tblProveedores").DataTable({
        ajax: {
            url: "/Terceros/ListarProveedores",
            type: "GET",
            dataSrc: "data"
        },
        initComplete: function () {
            this.api().column(6).search("^Activo$", true, false).draw();
        },
        columns: [
            { data: "ruc" },
            { data: "razon_social" },
            { data: "nombre_contacto", defaultContent: "" },
            { data: "telefono", defaultContent: "" },
            { data: "email", defaultContent: "" },
            { data: "direccion", defaultContent: "" },
            {
                data: "estado",
                render: function (estado, type) {
                    if (type === 'display') {
                        return estado
                            ? '<span class="badge-activo">Activo</span>'
                            : '<span class="badge-inactivo">Inactivo</span>';
                    }
                    return estado ? 'Activo' : 'Inactivo';
                }
            },
            {
                data: null,
                orderable: false,
                searchable: false,
                render: function (data) {
                    return `
                        <button type="button" class="btn-edit" onclick='editarProveedor(${JSON.stringify(data)})'>
                            <i class="fa-solid fa-pen"></i>
                        </button>

                        <button type="button" class="btn-delete" onclick="eliminarProveedor(${data.id_proveedor})">
                            <i class="fa-solid fa-trash"></i>
                        </button>
                    `;
                }
            }
        ]
    });

    $("#formProveedor").on("submit", function (e) {
        e.preventDefault();

        if (!validarProveedor()) return;

        $.ajax({
            url: "/Terceros/GuardarProveedor",
            type: "POST",
            data: $(this).serialize(),
            success: function (res) {
                $("#modalProveedor").modal("hide");
                tablaProveedores.ajax.reload(null, false);
                showAlert(res.mensaje || "Proveedor guardado correctamente", "success");
            },
            error: function () {
                showAlert("Error al guardar el proveedor", "error");
            }
        });
    });
}

function inicializarRestriccionesProveedor() {
    $("#ruc").on("input", function () {
        this.value = this.value.replace(/\D/g, "").slice(0, 11);
    });

    $("#telefono_proveedor").on("input", function () {
        this.value = this.value.replace(/\D/g, "").slice(0, 9);
    });
}

function validarProveedor() {
    const ruc = $("#ruc").val().trim();
    const razonSocial = $("#razon_social").val().trim();
    const contacto = $("#nombre_contacto").val().trim();
    const telefono = $("#telefono_proveedor").val().trim();
    const email = $("#email_proveedor").val().trim();

    if (ruc.length !== 11) {
        showAlert("El RUC debe tener exactamente 11 dígitos.", "error");
        return false;
    }

    if (razonSocial.length < 3) {
        showAlert("La razón social debe tener al menos 3 caracteres.", "error");
        return false;
    }

    if (contacto && contacto.length < 3) {
        showAlert("El nombre de contacto debe tener al menos 3 caracteres.", "error");
        return false;
    }

    if (telefono && telefono.length !== 9) {
        showAlert("El celular debe tener exactamente 9 dígitos.", "error");
        return false;
    }

    if (telefono && !telefono.startsWith("9")) {
        showAlert("El celular debe iniciar con 9.", "error");
        return false;
    }

    if (email) {
        const correoValido = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        if (!correoValido.test(email)) {
            showAlert("Ingrese un correo válido.", "error");
            return false;
        }
    }

    return true;
}

function abrirModalNuevoProveedor() {
    $("#formProveedor")[0].reset();

    $("#id_proveedor").val(0);
    $("#estado_proveedor").val("true");
    $("#tituloModalProveedor").text("Nuevo Proveedor");

    $("#modalProveedor").modal("show");
}

function editarProveedor(p) {
    $("#tituloModalProveedor").text("Editar Proveedor");

    $("#id_proveedor").val(p.id_proveedor);
    $("#ruc").val(p.ruc);
    $("#razon_social").val(p.razon_social);
    $("#nombre_contacto").val(p.nombre_contacto || "");
    $("#telefono_proveedor").val(p.telefono || "");
    $("#email_proveedor").val(p.email || "");
    $("#direccion_proveedor").val(p.direccion || "");
    $("#estado_proveedor").val(p.estado ? "true" : "false");

    $("#modalProveedor").modal("show");
}

function eliminarProveedor(id) {
    if (!confirm("¿Seguro que deseas eliminar este proveedor?")) return;

    $.ajax({
        url: "/Terceros/EliminarProveedor",
        type: "POST",
        data: { id_proveedor: id },
        success: function (res) {
            tablaProveedores.ajax.reload(null, false);
            showAlert(res.mensaje || "Proveedor eliminado correctamente", "success");
        },
        error: function () {
            showAlert("Error al eliminar el proveedor", "error");
        }
    });
}

function filtrarProveedores() {
    const texto = $("#buscarProveedor").val();
    tablaProveedores.search(texto).draw();
}

function limpiarFiltrosProveedores() {
    $("#buscarProveedor").val("");
    $("#filtroEstadoProveedor").val("");

    tablaProveedores.search("").draw();
}


/* =========================================
FILTROS CLIENTES
========================================= */

function filtrarClientes() {

    if (!tablaClientes) return;

    const texto = $("#buscarCliente").val();
    const tipo = $("#filtroTipoDocumento").val();
    const estado = $("#filtroEstado").val();

    tablaClientes.search(texto);

    tablaClientes.column(0).search(tipo);
    
    const searchVal = estado ? '^' + estado + '$' : '';
    tablaClientes.column(5).search(searchVal, true, false);

    tablaClientes.draw();
}

function limpiarFiltrosClientes() {

    if (!tablaClientes) return;

    $("#buscarCliente").val("");
    $("#filtroTipoDocumento").val("");
    $("#filtroEstado").val("Activo");

    tablaClientes.search("");
    tablaClientes.column(0).search("");
    tablaClientes.column(5).search("^Activo$", true, false);

    tablaClientes.draw();
}

/* =========================================
   FILTROS PROVEEDORES
========================================= */

function filtrarProveedores() {

    if (!tablaProveedores) return;

    const texto = $("#buscarProveedor").val();
    const estado = $("#filtroEstadoProveedor").val();

    tablaProveedores.search(texto);
    
    const searchVal = estado ? '^' + estado + '$' : '';
    tablaProveedores.column(6).search(searchVal, true, false);

    tablaProveedores.draw();
}

function limpiarFiltrosProveedores() {

    if (!tablaProveedores) return;

    $("#buscarProveedor").val("");
    $("#filtroEstadoProveedor").val("Activo");

    tablaProveedores.search("");
    tablaProveedores.column(6).search("^Activo$", true, false);

    tablaProveedores.draw();
}

/* =========================================
MODULO COMPRAS
========================================= */

let tablaCompras = null;
let productosCompra = [];
let detalleCompra = [];

$(document).ready(function () {
    inicializarCompras();
});

function inicializarCompras() {
    if ($("#tblCompras").length === 0) return;

    cargarCombosCompra();

    tablaCompras = $("#tblCompras").DataTable({
        ajax: {
            url: "/Compras/ListarCompras",
            type: "GET",
            dataSrc: "data"
        },
        columns: [
            { data: "numero_compra" },
            { data: "proveedor" },
            { data: "almacen" },
            { data: "fecha_compra" },
            {
                data: "subtotal",
                render: d => "S/ " + Number(d).toFixed(2)
            },
            {
                data: "igv",
                render: d => "S/ " + Number(d).toFixed(2)
            },
            {
                data: "total",
                render: d => "S/ " + Number(d).toFixed(2)
            },
            {
                data: "estado",
                render: function (estado) {
                    if (estado === "Pendiente")
                        return '<span class="badge-activo">Pendiente</span>';

                    if (estado === "Recepcionado")
                        return '<span class="badge-activo">Recepcionado</span>';

                    if (estado === "Anulado")
                        return '<span class="badge-inactivo">Anulado</span>';

                    return estado;
                }
            },
            {
                data: null,
                orderable: false,
                searchable: false,
                render: function (data) {
                    let botones = "";

                    if (data.estado === "Pendiente") {
                        botones += `
                            <button type="button" class="btn-edit" onclick="recepcionarCompra(${data.id_compra})" title="Recepcionar">
                                <i class="fa-solid fa-box"></i>
                            </button>

                            <button type="button" class="btn-delete" onclick="anularCompra(${data.id_compra})" title="Anular">
                                <i class="fa-solid fa-ban"></i>
                            </button>
                        `;
                    }

                    return botones;
                }
            }
        ]
    });

    $("#formCompra").on("submit", function (e) {
        e.preventDefault();
        guardarCompra();
    });
}

function cargarCombosCompra() {
    $.get("/Compras/ListarProveedores", function (data) {
        const select = $("#id_proveedor");
        select.empty();
        select.append('<option value="">Seleccione proveedor</option>');

        data.forEach(p => {
            select.append(`<option value="${p.id_proveedor}">${p.razon_social}</option>`);
        });
    });

    $.get("/Compras/ListarAlmacenes", function (data) {
        const select = $("#id_almacen");
        select.empty();
        select.append('<option value="">Seleccione almacén</option>');

        data.forEach(a => {
            select.append(`<option value="${a.id_almacen}">${a.nombre}</option>`);
        });
    });

    $.get("/Compras/ListarProductos", function (data) {
        productosCompra = data;

        const select = $("#productoCompra");
        select.empty();
        select.append('<option value="">Seleccione producto</option>');

        data.forEach(p => {
            select.append(`
                <option value="${p.id_producto}" data-precio="${p.precio_costo}">
                    ${p.nombre} - Stock: ${p.stock_actual}
                </option>
            `);
        });
    });
}

$("#productoCompra").on("change", function () {
    const precio = $("#productoCompra option:selected").data("precio") || 0;
    $("#precioCostoCompra").val(Number(precio).toFixed(2));
});

function abrirModalNuevaCompra() {
    $("#formCompra")[0].reset();
    $("#id_compra").val(0);

    detalleCompra = [];
    renderDetalleCompra();

    $("#tituloModalCompra").text("Nueva Orden de Compra");
    $("#modalCompra").modal("show");
}

function agregarProductoCompra() {
    const idProducto = parseInt($("#productoCompra").val());
    const cantidad = parseInt($("#cantidadCompra").val());
    const precio = parseFloat($("#precioCostoCompra").val());

    if (!idProducto) {
        showAlert("Seleccione un producto.", "error");
        return;
    }

    if (!cantidad || cantidad <= 0) {
        showAlert("Ingrese una cantidad válida.", "error");
        return;
    }

    if (isNaN(precio) || precio <= 0) {
        showAlert("Ingrese un precio de costo válido.", "error");
        return;
    }

    const producto = productosCompra.find(p => p.id_producto === idProducto);

    if (!producto) {
        showAlert("Producto no encontrado.", "error");
        return;
    }

    const existente = detalleCompra.find(d => d.id_producto === idProducto);

    if (existente) {
        existente.cantidad += cantidad;
        existente.precio_unitario = precio;
        existente.subtotal = existente.cantidad * precio;
    } else {
        detalleCompra.push({
            id_producto: idProducto,
            nombre: producto.nombre,
            cantidad: cantidad,
            precio_unitario: precio,
            subtotal: cantidad * precio
        });
    }

    $("#productoCompra").val("");
    $("#cantidadCompra").val("");
    $("#precioCostoCompra").val("");

    renderDetalleCompra();
}

function renderDetalleCompra() {
    const tbody = $("#tblDetalleCompra tbody");
    tbody.empty();

    if (detalleCompra.length === 0) {
        tbody.append(`
            <tr id="filaSinProductos">
                <td colspan="5" class="text-center text-muted">
                    No hay productos agregados.
                </td>
            </tr>
        `);
    } else {
        detalleCompra.forEach((item, index) => {
            tbody.append(`
                <tr>
                    <td>${item.nombre}</td>
                    <td>${item.cantidad}</td>
                    <td>S/ ${Number(item.precio_unitario).toFixed(2)}</td>
                    <td>S/ ${Number(item.subtotal).toFixed(2)}</td>
                    <td>
                        <button type="button" class="btn-delete" onclick="quitarProductoCompra(${index})">
                            <i class="fa-solid fa-trash"></i>
                        </button>
                    </td>
                </tr>
            `);
        });
    }

    calcularTotalesCompra();
}

function quitarProductoCompra(index) {
    detalleCompra.splice(index, 1);
    renderDetalleCompra();
}

function calcularTotalesCompra() {
    const subtotal = detalleCompra.reduce((acc, item) => acc + item.subtotal, 0);
    const igv = subtotal * 0.18;
    const total = subtotal + igv;

    $("#subtotalCompra").text("S/ " + subtotal.toFixed(2));
    $("#igvCompra").text("S/ " + igv.toFixed(2));
    $("#totalCompra").text("S/ " + total.toFixed(2));
}

function guardarCompra() {
    const idProveedor = parseInt($("#id_proveedor").val());
    const idAlmacen = parseInt($("#id_almacen").val());
    const observacion = $("#observacion").val();

    if (!idProveedor) {
        showAlert("Seleccione un proveedor.", "error");
        return;
    }

    if (!idAlmacen) {
        showAlert("Seleccione un almacén.", "error");
        return;
    }

    if (detalleCompra.length === 0) {
        showAlert("Debe agregar al menos un producto.", "error");
        return;
    }

    const request = {
        id_proveedor: idProveedor,
        id_almacen: idAlmacen,
        id_usuario: 1,
        observacion: observacion,
        detalles: detalleCompra.map(d => ({
            id_producto: d.id_producto,
            cantidad: d.cantidad,
            precio_unitario: d.precio_unitario
        }))
    };

    $.ajax({
        url: "/Compras/GuardarCompra",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(request),
        success: function (res) {
            if (res.success) {
                $("#modalCompra").modal("hide");
                tablaCompras.ajax.reload(null, false);
                showAlert(res.mensaje || "Compra guardada correctamente.", "success");
            } else {
                showAlert(res.mensaje || "No se pudo guardar la compra.", "error");
            }
        },
        error: function () {
            showAlert("Error al guardar la compra.", "error");
        }
    });
}

function recepcionarCompra(id) {
    if (!confirm("¿Deseas recepcionar esta compra? Esto aumentará el stock.")) return;

    $.ajax({
        url: "/Compras/RecepcionarCompra",
        type: "POST",
        data: { id_compra: id },
        success: function (res) {
            showAlert(res.mensaje, res.success ? "success" : "error");

            if (res.success) {
                tablaCompras.ajax.reload(null, false);
            }
        },
        error: function () {
            showAlert("Error al recepcionar la compra.", "error");
        }
    });
}

function anularCompra(id) {
    if (!confirm("¿Deseas anular esta compra?")) return;

    $.ajax({
        url: "/Compras/AnularCompra",
        type: "POST",
        data: { id_compra: id },
        success: function (res) {
            showAlert(res.mensaje, res.success ? "success" : "error");

            if (res.success) {
                tablaCompras.ajax.reload(null, false);
            }
        },
        error: function () {
            showAlert("Error al anular la compra.", "error");
        }
    });
}

function filtrarCompras() {
    if (!tablaCompras) return;

    const texto = $("#buscarCompra").val();
    const estado = $("#filtroEstadoCompra").val();

    tablaCompras.search(texto);
    tablaCompras.column(7).search(estado);
    tablaCompras.draw();
}

function limpiarFiltrosCompras() {
    if (!tablaCompras) return;

    $("#buscarCompra").val("");
    $("#filtroEstadoCompra").val("");

    tablaCompras.search("");
    tablaCompras.column(7).search("");
    tablaCompras.draw();
}

/* =========================================
   INVENTARIO - ALMACENES
========================================= */

let tablaAlmacenes = null;

$(document).ready(function () {

    if ($("#tblAlmacenes").length > 0) {
        inicializarAlmacenes();
    }

    if ($("#tblKardex").length > 0) {
        inicializarKardex();
    }

    if ($("#tblTraslados").length > 0) {
        inicializarTraslados();
    }

});

function inicializarAlmacenes() {

    tablaAlmacenes = $("#tblAlmacenes").DataTable({
        ajax: {
            url: "/Inventario/ListarAlmacenes",
            type: "GET",
            dataSrc: "data"
        },
        initComplete: function () {
            this.api().column(3).search("^Activo$", true, false).draw();
        },
        columns: [
            { data: "codigo" },
            { data: "nombre" },
            { data: "ubicacion" },
            {
                data: "estado",
                render: function (estado, type) {
                    if (type === 'display') {
                        return estado
                            ? '<span class="badge-activo">Activo</span>'
                            : '<span class="badge-inactivo">Inactivo</span>';
                    }
                    return estado ? 'Activo' : 'Inactivo';
                }
            },
            {
                data: null,
                render: function (data) {
                    return `
                        <button class="btn-edit"
                            onclick='editarAlmacen(${JSON.stringify(data)})'>
                            <i class="fa-solid fa-pen"></i>
                        </button>

                        <button class="btn-delete"
                            onclick='eliminarAlmacen(${data.id_almacen})'>
                            <i class="fa-solid fa-trash"></i>
                        </button>
                    `;
                }
            }
        ]
    });

    $("#formAlmacen").on("submit", function (e) {

        e.preventDefault();

        $.ajax({
            url: "/Inventario/GuardarAlmacen",
            type: "POST",
            data: $(this).serialize(),
            success: function (res) {

                showAlert(
                    res.mensaje,
                    res.success ? "success" : "error"
                );

                if (res.success) {

                    $("#modalAlmacen").modal("hide");

                    tablaAlmacenes.ajax.reload(
                        null,
                        false
                    );
                }
            }
        });
    });
}

function abrirModalNuevoAlmacen() {

    $("#formAlmacen")[0].reset();

    $("#id_almacen").val(0);

    $("#estado_almacen").val("true");

    $("#tituloModalAlmacen")
        .text("Nuevo Almacén");

    $("#modalAlmacen").modal("show");
}

function editarAlmacen(a) {

    $("#id_almacen").val(a.id_almacen);
    $("#codigo").val(a.codigo);
    $("#nombre").val(a.nombre);
    $("#ubicacion").val(a.ubicacion);

    $("#estado_almacen")
        .val(a.estado ? "true" : "false");

    $("#tituloModalAlmacen")
        .text("Editar Almacén");

    $("#modalAlmacen").modal("show");
}

function eliminarAlmacen(id) {

    if (!confirm("¿Eliminar almacén?"))
        return;

    $.post(
        "/Inventario/EliminarAlmacen",
        { id_almacen: id },
        function (res) {

            showAlert(
                res.mensaje,
                res.success ? "success" : "error"
            );

            if (res.success) {

                tablaAlmacenes.ajax.reload(
                    null,
                    false
                );
            }
        }
    );
}

function filtrarAlmacenes() {
    if (!tablaAlmacenes) return;

    const texto = $("#buscarAlmacen").val();
    const estado = $("#filtroEstadoAlmacen").val();

    tablaAlmacenes.search(texto);
    
    const searchVal = estado ? '^' + estado + '$' : '';
    tablaAlmacenes.column(3).search(searchVal, true, false);
    tablaAlmacenes.draw();
}

function limpiarFiltrosAlmacenes() {
    if (!tablaAlmacenes) return;

    $("#buscarAlmacen").val("");
    $("#filtroEstadoAlmacen").val("Activo");

    tablaAlmacenes.search("");
    tablaAlmacenes.column(3).search("^Activo$", true, false);
    tablaAlmacenes.draw();
}

/* =========================================
   INVENTARIO - KARDEX
========================================= */

let tablaKardex = null;

function inicializarKardex() {

    tablaKardex = $("#tblKardex").DataTable({
        ajax: {
            url: "/Inventario/ListarMovimientos",
            type: "GET",
            dataSrc: "data"
        },
        columns: [
            { data: "fecha" },
            { data: "producto" },
            { data: "almacen" },
            { data: "tipo_movimiento" },
            { data: "tipo_referencia" },
            { data: "cantidad" },
            { data: "stock_anterior" },
            { data: "stock_resultante" },
            { data: "observacion" }
        ]
    });
}

function filtrarKardex() {

    if (!tablaKardex) return;

    const texto =
        $("#buscarKardex").val();

    tablaKardex.search(texto).draw();
}

function limpiarFiltrosKardex() {

    if (!tablaKardex) return;

    $("#buscarKardex").val("");

    tablaKardex.search("").draw();
}

/* =========================================
   INVENTARIO - TRASLADOS
========================================= */

let tablaTraslados = null;
let detalleTraslado = [];
let productosTraslado = [];

function inicializarTraslados() {

    cargarCombosTraslado();

    tablaTraslados = $("#tblTraslados").DataTable({
        ajax: {
            url: "/Inventario/ListarTraslados",
            type: "GET",
            dataSrc: "data"
        },
        columns: [
            { data: "numero_traslado" },
            { data: "origen" },
            { data: "destino" },
            { data: "fecha" },
            { data: "estado" },
            { data: "observacion" },
            {
                data: null,
                render: function (data) {

                    let botones = `
                        <button class="btn-edit"
                            onclick="verDetalleTraslado(${data.id_traslado})">
                            <i class="fa-solid fa-eye"></i>
                        </button>
                    `;

                    if (data.estado === "Pendiente") {

                        botones += `
                            <button class="btn-edit"
                                onclick="confirmarTraslado(${data.id_traslado})">
                                <i class="fa-solid fa-check"></i>
                            </button>

                            <button class="btn-delete"
                                onclick="anularTraslado(${data.id_traslado})">
                                <i class="fa-solid fa-ban"></i>
                            </button>
                        `;
                    }

                    return botones;
                }
            }
        ]
    });

    $("#formTraslado").on("submit", function (e) {

        e.preventDefault();

        guardarTraslado();
    });
}

function cargarCombosTraslado() {

    $.get(
        "/Inventario/ListarAlmacenesActivos",
        function (data) {

            $("#id_almacen_origen").empty();
            $("#id_almacen_destino").empty();

            $("#id_almacen_origen")
                .append('<option value="">Seleccione</option>');

            $("#id_almacen_destino")
                .append('<option value="">Seleccione</option>');

            data.forEach(a => {

                $("#id_almacen_origen")
                    .append(`<option value="${a.id_almacen}">${a.nombre}</option>`);

                $("#id_almacen_destino")
                    .append(`<option value="${a.id_almacen}">${a.nombre}</option>`);
            });
        }
    );

    $.get(
        "/Inventario/ListarProductosActivos",
        function (data) {

            productosTraslado = data;

            $("#productoTraslado").empty();

            $("#productoTraslado")
                .append('<option value="">Seleccione producto</option>');

            data.forEach(p => {

                $("#productoTraslado")
                    .append(`<option value="${p.id_producto}">${p.nombre}</option>`);
            });
        }
    );
}

function abrirModalNuevoTraslado() {

    $("#formTraslado")[0].reset();

    detalleTraslado = [];

    renderDetalleTraslado();

    $("#modalTraslado").modal("show");
}

function agregarProductoTraslado() {

    const idProducto =
        parseInt($("#productoTraslado").val());

    const cantidad =
        parseInt($("#cantidadTraslado").val());

    if (!idProducto || !cantidad)
        return;

    const producto =
        productosTraslado.find(
            x => x.id_producto === idProducto
        );

    detalleTraslado.push({
        id_producto: idProducto,
        producto: producto.nombre,
        cantidad: cantidad
    });

    renderDetalleTraslado();
}

function renderDetalleTraslado() {

    const tbody =
        $("#tblDetalleTraslado tbody");

    tbody.empty();

    if (detalleTraslado.length === 0) {

        tbody.append(`
            <tr>
                <td colspan="3"
                    class="text-center">
                    Sin productos
                </td>
            </tr>
        `);

        return;
    }

    detalleTraslado.forEach((item, index) => {

        tbody.append(`
            <tr>
                <td>${item.producto}</td>
                <td>${item.cantidad}</td>
                <td>
                    <button class="btn-delete"
                        onclick="quitarProductoTraslado(${index})">
                        <i class="fa-solid fa-trash"></i>
                    </button>
                </td>
            </tr>
        `);
    });
}

function quitarProductoTraslado(index) {

    detalleTraslado.splice(index, 1);

    renderDetalleTraslado();
}
function guardarTraslado() {
    const idOrigen = parseInt($("#id_almacen_origen").val());
    const idDestino = parseInt($("#id_almacen_destino").val());
    const observacion = $("#observacion_traslado").val();

    if (!idOrigen) {
        showAlert("Seleccione el almacén origen.", "error");
        return;
    }

    if (!idDestino) {
        showAlert("Seleccione el almacén destino.", "error");
        return;
    }

    if (idOrigen === idDestino) {
        showAlert("El almacén origen y destino no pueden ser iguales.", "error");
        return;
    }

    if (detalleTraslado.length === 0) {
        showAlert("Agregue al menos un producto al traslado.", "error");
        return;
    }

    const request = {
        id_almacen_origen: idOrigen,
        id_almacen_destino: idDestino,
        id_usuario: 1,
        observacion: observacion,
        detalles: detalleTraslado.map(d => ({
            id_producto: d.id_producto,
            cantidad: d.cantidad
        }))
    };

    $.ajax({
        url: "/Inventario/GuardarTraslado",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(request),
        success: function (res) {
            showAlert(res.mensaje, res.success ? "success" : "error");

            if (res.success) {
                $("#modalTraslado").modal("hide");
                detalleTraslado = [];
                renderDetalleTraslado();
                tablaTraslados.ajax.reload(null, false);
            }
        },
        error: function () {
            showAlert("Error al guardar el traslado.", "error");
        }
    });
}

function verDetalleTraslado(id) {
    $.get("/Inventario/DetalleTraslado", { id_traslado: id }, function (res) {
        const tbody = $("#tblDetalleTrasladoVista tbody");
        tbody.empty();

        if (!res.data || res.data.length === 0) {
            tbody.append(`
                <tr>
                    <td colspan="2" class="text-center text-muted">
                        No hay productos en este traslado.
                    </td>
                </tr>
            `);
        } else {
            res.data.forEach(item => {
                tbody.append(`
                    <tr>
                        <td>${item.producto}</td>
                        <td>${item.cantidad}</td>
                    </tr>
                `);
            });
        }

        $("#modalDetalleTraslado").modal("show");
    });
}

function confirmarTraslado(id) {
    if (!confirm("¿Confirmar este traslado? Se registrará una salida y una entrada de stock.")) return;

    $.ajax({
        url: "/Inventario/ConfirmarTraslado",
        type: "POST",
        data: { id_traslado: id },
        success: function (res) {
            showAlert(res.mensaje, res.success ? "success" : "error");

            if (res.success) {
                tablaTraslados.ajax.reload(null, false);

                if (tablaKardex) {
                    tablaKardex.ajax.reload(null, false);
                }
            }
        },
        error: function () {
            showAlert("Error al confirmar el traslado.", "error");
        }
    });
}

function anularTraslado(id) {
    if (!confirm("¿Anular este traslado?")) return;

    $.ajax({
        url: "/Inventario/AnularTraslado",
        type: "POST",
        data: { id_traslado: id },
        success: function (res) {
            showAlert(res.mensaje, res.success ? "success" : "error");

            if (res.success) {
                tablaTraslados.ajax.reload(null, false);
            }
        },
        error: function () {
            showAlert("Error al anular el traslado.", "error");
        }
    });
}

function filtrarTraslados() {
    if (!tablaTraslados) return;

    const texto = $("#buscarTraslado").val();
    const estado = $("#filtroEstadoTraslado").val();

    tablaTraslados.search(texto);
    tablaTraslados.column(4).search(estado);
    tablaTraslados.draw();
}

function limpiarFiltrosTraslados() {
    if (!tablaTraslados) return;

    $("#buscarTraslado").val("");
    $("#filtroEstadoTraslado").val("");

    tablaTraslados.search("");
    tablaTraslados.column(4).search("");
    tablaTraslados.draw();
}