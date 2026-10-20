let tablaData;
const controlador = "Doctor";
const modal = "mdData";

document.addEventListener("DOMContentLoaded", function (event) {

    tablaData = $('#tbData').DataTable({
        responsive: true,
        scrollX: true,
        "ajax": {
            "url": `/${controlador}/ListaCitasAtendidas?IdEstadoCita=2`, // 2 = Atendido
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
});