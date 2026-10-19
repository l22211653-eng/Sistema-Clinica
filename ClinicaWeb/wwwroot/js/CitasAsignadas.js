let tablaData;
const controlador = "Doctor";
const modal = "mdData";

document.addEventListener("DOMContentLoaded", function (event) {

    // 1. Inicializar la tabla llamando al controlador con el estado "Pendiente" (1)
    cargarTabla(1);

    // 2. Evento para cuando se cambia el combo de "Estado Cita"
    $("#cboEstadoCita").change(function () {
        let idEstado = $(this).val();
        cargarTabla(idEstado);
    });
});

function cargarTabla(idEstadoCita) {
    // Si la tabla ya existe, la destruimos para volver a crearla con el nuevo filtro
    if ($.fn.DataTable.isDataTable('#tbData')) {
        $('#tbData').DataTable().destroy();
    }

    tablaData = $('#tbData').DataTable({
        responsive: true,
        scrollX: true,
        "ajax": {
            "url": `/${controlador}/ListaCitasAsignadas?IdEstadoCita=${idEstadoCita}`,
            "type": "GET",
            "datatype": "json",
            // ⚠️ Importante: Tu controlador devuelve { data: [...] }, así que leemos "data"
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
                title: "Estado", "data": "estadoCita", render: function (data, type, row) {
                    return data ? data.nombre : '';
                }
            },
            {
                title: "Acciones", "data": null, width: "150px", render: function (data, type, row) {
                    if (row.estadoCita && row.estadoCita.nombre === "Pendiente") {
                        return `<button type="button" class="btn btn-sm btn-outline-warning btn-indicaciones" data-id="${row.idCita}">Indicaciones</button>`;
                    }
                    return ''; // Si ya está atendida, no mostramos el botón
                }
            }
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        },
    });
}

// ==========================================
// EVENTO: ABRIR MODAL DE INDICACIONES
// ==========================================
$("#tbData tbody").on("click", ".btn-indicaciones", function () {
    const filaSeleccionada = $(this).closest('tr');
    const data = tablaData.row(filaSeleccionada).data();

    // Asignamos el ID de la cita al input oculto del modal
    $("#txtIdCita").val(data.idCita);
    // Limpiamos el textarea
    $("#txtIndicaciones").val("");

    // Mostramos el modal
    $(`#${modal}`).modal('show');

});

// ==========================================
// EVENTO: TERMINAR CITA (Cambiar Estado a Atendido)
// ==========================================
$("#btnTerminarCita").click(function () {
    let idCita = parseInt($("#txtIdCita").val());
    let indicaciones = $("#txtIndicaciones").val().trim();

    if (indicaciones === "") {
        Swal.fire("Atención", "Por favor, ingrese las indicaciones médicas antes de terminar.", "warning");
        return;
    }

    let objeto = {
        IdCita: idCita,
        EstadoCita: { IdEstadoCita: 2 }, // 2 = Atendido
        Indicaciones: indicaciones
    };

    // Mostrar alerta de carga
    Swal.fire({ title: 'Guardando...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

    fetch(`/${controlador}/CambiarEstado`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(objeto)
    })
        .then(response => response.json())
        .then(data => {
            if (data.data === "") { // Éxito
                Swal.fire("¡Listo!", "La cita ha sido marcada como Atendida.", "success");
                $("#mdData").modal('hide');
                // Recargamos la tabla para que la cita desaparezca de "Pendientes"
                tablaData.ajax.reload();
            } else {
                Swal.fire("Error", data.data, "error");
            }
        })
        .catch(error => {
            Swal.fire("Error", "No se pudo terminar la cita.", "error");
        });
});