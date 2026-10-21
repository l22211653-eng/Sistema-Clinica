let tablaData;
const controlador = "Doctor";

document.addEventListener("DOMContentLoaded", function (event) {

    tablaData = $('#tbData').DataTable({
        responsive: true,
        scrollX: true,
        "ajax": {
            // ⚠️ CAMBIO: Ahora llamamos al endpoint enviando el orden por defecto
            "url": `/${controlador}/ListaCitasAtendidas?orden=recientes`,
            "type": "GET",
            "datatype": "json",
            "dataSrc": function (json) {
                if (json && json.error) {
                    Swal.fire("Error", json.error, "error");
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
                    return data ? `${data.nombre} ${data.apellido}` : '';
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

    // ⚠️ EVENTO NUEVO: Recargar la tabla cuando se cambia el combo de orden
    $("#cboOrdenFecha").change(function () {
        let orden = $(this).val();
        tablaData.ajax.url(`/${controlador}/ListaCitasAtendidas?orden=${orden}`).load();
    });

});