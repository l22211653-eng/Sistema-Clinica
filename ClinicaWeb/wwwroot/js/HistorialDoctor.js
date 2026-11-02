let tablaData;
const controlador = "Doctor";

document.addEventListener("DOMContentLoaded", function (event) {

    tablaData = $('#tbData').DataTable({
        responsive: true,
        scrollX: true,
        "ajax": {
            "url": `/${controlador}/ListaCitasAsignadas`,
            "type": "GET",
            "datatype": "json",
            "dataSrc": function (json) {
                if (json && json.error) {
                    Swal.fire("Error del Servidor", json.error, "error");
                    return [];
                }
                return json.data || [];
            }
        },
        "columns": [
            { title: "Fecha Cita", "data": "fechaCita", width: "120px" },
            { title: "Hora Cita", "data": "horaCita", width: "100px" },
            {
                title: "Paciente", "data": "usuario", render: function (data, type, row) {
                    // ⚠️ Validamos si data existe y tiene las propiedades
                    if (data && data.nombre) {
                        return `${data.nombre} ${data.apellido}`;
                    }
                    return '<span class="text-muted">Sin nombre</span>';
                }
            },
            {
                title: "Indicaciones", "data": "indicaciones", render: function (data) {
                    return data ? data : '<span class="text-muted">Sin indicaciones</span>';
                }
            }
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        },
    });

    // Evento para el filtro de orden
    $("#cboOrdenFecha").change(function () {
        let orden = $(this).val();
        tablaData.ajax.url(`/${controlador}/ListaCitasAtendidas?orden=${orden}`).load();
    });
});