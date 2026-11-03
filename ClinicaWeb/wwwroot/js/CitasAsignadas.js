let tablaData;
const controlador = "Doctor";
const modal = "mdData";
let recetasTemporales = [];

document.addEventListener("DOMContentLoaded", function (event) {
    cargarTabla();
    cargarEspecialidad();
    revisarEmergencias();
    setInterval(revisarEmergencias, 30000); // Revisar cada 30 seg
});



function cargarTabla() {
    if ($.fn.DataTable.isDataTable('#tbData')) {
        $('#tbData').DataTable().destroy();
    }

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
            { title: "Paciente", "data": "usuario", render: function (data) { return data ? `${data.nombre} ${data.apellido}` : ''; } },
            { title: "Estado", "data": "estadoCita", render: function (data) { return data ? data.nombre : ''; } },
            {
                title: "Acciones", "data": null, width: "150px", render: function (data, type, row) {
                    return `<button type="button" class="btn btn-sm btn-primary btn-indicaciones" data-id="${row.idCita}"><i class="fas fa-notes-medical"></i> Atender</button>`;
                }
            }
        ],
        language: { url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json" }
    });
}

// ==========================================
// 1. REVISAR EMERGENCIAS
// ==========================================
function revisarEmergencias() {
    fetch('/Doctor/ListaEmergencias')
        .then(res => res.json())
        .then(data => {
            if (data.data && data.data.length > 0) {
                $("#panelEmergencias").removeClass("d-none");
                $("#listaEmergencias").html("");

                data.data.forEach(em => {
                    $("#listaEmergencias").append(`
                        <div class="d-flex justify-content-between align-items-center border-bottom pb-2 mb-2">
                            <div>
                                <i class="fas fa-user-injured me-2"></i>
                                <strong>${em.usuario.nombre}</strong> - Hoy a las ${em.horaCita}
                            </div>
                            <div>
                                <button class="btn btn-sm btn-outline-primary btn-aceptar-emergencia me-2" data-id="${em.idCita}">
                                    <i class="fas fa-check me-1"></i> Aceptar
                                </button>
                                <button class="btn btn-sm btn-outline-danger btn-descartar-emergencia" data-id="${em.idCita}">
                                    <i class="fas fa-times me-1"></i> Descartar
                                </button>
                            </div>
                        </div>
                    `);
                });
            } else {
                $("#panelEmergencias").addClass("d-none");
            }
        })
        .catch(error => console.log("Error al revisar emergencias", error));
}

// ==========================================
// 2. ACEPTAR EMERGENCIA (Doctor)
// ==========================================
$(document).on("click", ".btn-aceptar-emergencia", function () {
    let idSolicitud = $(this).data("id");

    Swal.fire({
        title: "¿Aceptar emergencia?",
        text: "Se asignará el turno más próximo disponible hoy.",
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Sí, aceptar",
        cancelButtonText: "Cancelar"
    }).then((result) => {
        if (result.isConfirmed) {
            Swal.fire({ title: 'Aceptando...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

            fetch(`/Doctor/AceptarEmergencia?idSolicitud=${idSolicitud}`, { method: "POST" })
                .then(res => res.json())
                .then(data => {
                    if (data.data === "") {
                        Swal.fire("Aceptada", "El paciente ha sido notificado.", "success");
                        revisarEmergencias(); // Recargar panel
                        cargarTabla(); // Recargar tabla de pendientes
                    } else {
                        Swal.fire("Error", data.data, "error");
                    }
                })
                .catch(() => Swal.fire("Error", "No se pudo aceptar.", "error"));
        }
    });
});

// ==========================================
// 3. DESCARTAR EMERGENCIA
// ==========================================
$(document).on("click", ".btn-descartar-emergencia", function () {
    let idCita = $(this).data("id");
    let boton = $(this);

    Swal.fire({
        title: "¿Descartar emergencia?",
        text: "Esto eliminará la alerta de emergencia del panel. Úsalo si ya atendiste al paciente en consultorio.",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#d33",
        cancelButtonColor: "#6c757d",
        confirmButtonText: "Sí, descartar",
        cancelButtonText: "Cancelar"
    }).then((result) => {
        if (result.isConfirmed) {
            fetch(`/Doctor/DescartarEmergencia?idCita=${idCita}`, { method: "POST" })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        Swal.fire("Descartada", "La emergencia ha sido removida.", "success");
                        boton.closest('.d-flex').remove();
                        if ($("#listaEmergencias").children().length === 0) {
                            $("#panelEmergencias").addClass("d-none");
                        }
                    } else {
                        Swal.fire("Error", "No se pudo descartar la emergencia.", "error");
                    }
                });
        }
    });
});

// ==========================================
// 4. ATENDER CITA NORMAL
// ==========================================
$("#tbData tbody").on("click", ".btn-indicaciones", function () {
    const filaSeleccionada = $(this).closest('tr');
    const data = tablaData.row(filaSeleccionada).data();

    $("#txtIdCita").val(data.idCita);
    $("#txtIndicaciones").val("");

    recetasTemporales = [];
    $("#tbRecetaTemporal tbody").empty();
    $("#txtMedicamento").val("");
    $("#txtDosis").val("");

    $(`#${modal}`).modal('show');
});

// ==========================================
// 5. AGREGAR MEDICAMENTO A LA RECETA
// ==========================================
$("#btnAgregarMedicamento").click(function () {
    let medicamento = $("#txtMedicamento").val().trim();
    let dosis = $("#txtDosis").val().trim();
    let indicacionesExtra = $("#txtIndicaciones").val().trim();

    if (medicamento === "" || dosis === "") {
        Swal.fire("Atención", "Ingrese el medicamento y la dosis.", "warning");
        return;
    }

    recetasTemporales.push({ Medicamento: medicamento, Dosis: dosis, Indicaciones: indicacionesExtra });

    let fila = `
        <tr>
            <td>${medicamento}</td>
            <td>${dosis}</td>
            <td><button type="button" class="btn btn-sm btn-danger btn-eliminar-medicamento" data-med="${medicamento}"><i class="fas fa-trash"></i></button></td>
        </tr>
    `;
    $("#tbRecetaTemporal tbody").append(fila);
    $("#txtMedicamento").val("");
    $("#txtDosis").val("");
});

$("#tbRecetaTemporal").on("click", ".btn-eliminar-medicamento", function () {
    let med = $(this).data("med");
    recetasTemporales = recetasTemporales.filter(r => r.Medicamento !== med);
    $(this).closest("tr").remove();
});

// ==========================================
// 6. TERMINAR CITA (Guardar todo)
// ==========================================
$("#btnTerminarCita").click(function () {
    let idCita = parseInt($("#txtIdCita").val());
    let indicaciones = $("#txtIndicaciones").val().trim();

    if (indicaciones === "") {
        Swal.fire("Atención", "Por favor, ingrese las indicaciones médicas.", "warning");
        return;
    }

    Swal.fire({ title: 'Guardando...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

    let promesasRecetas = recetasTemporales.map(receta => {
        let objetoReceta = { IdCita: idCita, Medicamento: receta.Medicamento, Dosis: receta.Dosis, Indicaciones: receta.Indicaciones };
        return fetch(`/${controlador}/GuardarReceta`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(objetoReceta)
        });
    });

    Promise.all(promesasRecetas).then(() => {
        let objetoCita = { IdCita: idCita, EstadoCita: { IdEstadoCita: 2 }, Indicaciones: indicaciones };
        return fetch(`/${controlador}/CambiarEstado`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(objetoCita)
        });
    })
        .then(response => response.json())
        .then(data => {
            if (data.data === "") {
                Swal.fire("¡Listo!", "La cita ha sido atendida.", "success");
                $("#mdData").modal('hide');
                tablaData.ajax.reload();
            } else {
                Swal.fire("Error", data.data, "error");
            }
        })
        .catch(() => {
            Swal.fire("Error", "Ocurrió un error al guardar.", "error");
        });
});