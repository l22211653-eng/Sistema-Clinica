let tablaData;
const controlador = "Doctor";
const modal = "mdData";
let recetasTemporales = []; // Array para guardar los medicamentos antes de enviarlos

document.addEventListener("DOMContentLoaded", function (event) {
    cargarTabla(1); // Cargar Pendientes por defecto

    $("#cboEstadoCita").change(function ()
    {
        cargarTabla($(this).val());
    });
});

function cargarTabla(idEstadoCita) {

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
            "dataSrc": function (json)
            {
                return json.data || [];
            }
        },
        "columns": [
            { title: "Fecha Cita", "data": "fechaCita", width: "120px" },
            { title: "Hora Cita", "data": "horaCita", width: "100px" },
            { title: "Paciente", "data": "usuario", render: function (data) { return data ? `${data.nombre} ${data.apellido}` : ''; } },
            { title: "Estado", "data": "estadoCita", render: function (data) { return data ? data.nombre : ''; } },
            {

                title: "Acciones", "data": null, width: "150px", render: function (data, type, row) {
                    if (row.estadoCita && row.estadoCita.nombre === "Pendiente") {
                        // En lugar de: btn btn-outline-warning btn-indicaciones
                        return `<button type="button" class="btn btn-primary btn-sm btn-indicaciones" data-id="${row.idCita}"> <i class="fas fa-notes-medical"> </i> Atender </button>`;
                    }
                    return '';
                }
            }
        ],
        language: { url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json" }
    });
}

// ==========================================
// 1. ABRIR MODAL Y PREPARAR RECETA
// ==========================================
$("#tbData tbody").on("click", ".btn-indicaciones", function () {
    const filaSeleccionada = $(this).closest('tr');
    const data = tablaData.row(filaSeleccionada).data();


    $("#txtIdCita").val(data.idCita);

    $("#txtIndicaciones").val("");

    // Limpiamos la receta temporal
    recetasTemporales = [];
    $("#tbRecetaTemporal tbody").empty();
    $("#txtMedicamento").val("");
    $("#txtDosis").val("");

    $(`#${modal}`).modal('show');
});

// ==========================================
// 2. AGREGAR MEDICAMENTO A LA TABLA TEMPORAL
// ==========================================
$("#btnAgregarMedicamento").click(function () {
    let medicamento = $("#txtMedicamento").val().trim();
    let dosis = $("#txtDosis").val().trim();
    let indicacionesExtra = $("#txtIndicaciones").val().trim(); // Usamos las indicaciones generales para la receta

    if (medicamento === "" || dosis === "") {
        Swal.fire("Atención", "Ingrese el medicamento y la dosis.", "warning");
        return;
    }

    // Agregar al array
    recetasTemporales.push({
        Medicamento: medicamento,
        Dosis: dosis,
        Indicaciones: indicacionesExtra
    });

    // Agregar a la tabla visual
    let fila = `
        <tr>
            <td>${medicamento}</td>
            <td>${dosis}</td>
            <td><button type="button" class="btn btn-sm btn-danger btn-eliminar-medicamento" data-med="${medicamento}"><i class="fas fa-trash"></i></button></td>
        </tr>
    `;
    $("#tbRecetaTemporal tbody").append(fila);

    // Limpiar inputs
    $("#txtMedicamento").val("");
    $("#txtDosis").val("");
});

// Eliminar medicamento de la tabla temporal
$("#tbRecetaTemporal").on("click", ".btn-eliminar-medicamento", function () {
    let med = $(this).data("med");
    recetasTemporales = recetasTemporales.filter(r => r.Medicamento !== med);
    $(this).closest("tr").remove();
});

// ==========================================
// 3. TERMINAR CITA (Guardar todo)
// ==========================================
$("#btnTerminarCita").click(function () {
    let idCita = parseInt($("#txtIdCita").val());
    let indicaciones = $("#txtIndicaciones").val().trim();

    if (indicaciones === "") {
        Swal.fire("Atención", "Por favor, ingrese las indicaciones médicas.", "warning");
        return;
    }

    Swal.fire({ title: 'Guardando...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

    // 1. Primero guardamos la Receta (si hay medicamentos)
    let promesasRecetas = recetasTemporales.map(receta => {
        let objetoReceta = {
            IdCita: idCita,
            Medicamento: receta.Medicamento,
            Dosis: receta.Dosis,
            Indicaciones: receta.Indicaciones
        };
        return fetch(`/${controlador}/GuardarReceta`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(objetoReceta)
        });
    });

    // Esperamos a que todas las recetas se guarden
    Promise.all(promesasRecetas).then(() => {

        // 2. Luego cambiamos el estado de la cita a Atendido
        let objetoCita = {
            IdCita: idCita,
            EstadoCita: { IdEstadoCita: 2 }, // 2 = Atendido
            Indicaciones: indicaciones
        };

        return fetch(`/${controlador}/CambiarEstado`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(objetoCita)
        });
    })
        .then(response => response.json())
        .then(data => {
            if (data.data === "") {
                Swal.fire("¡Listo!", "La cita ha sido atendida y la receta guardada.", "success");
                $("#mdData").modal('hide');

                tablaData.ajax.reload();

            } else {
                Swal.fire("Error", data.data, "error");
            }
        })
        .catch(error => {
            Swal.fire("Error", "Ocurrió un error al guardar.", "error");
        });
});