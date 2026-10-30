let tablaData;
const controlador = "Doctor";
const modal = "mdData";
let recetasTemporales = [];

document.addEventListener("DOMContentLoaded", function (event) {
    cargarTabla();
});

fetch('/Doctor/ObtenerEspecialidad')
    .then(response => response.text())
    .then(data => {
        // Limpiamos las comillas dobles que puedan venir en la respuesta
        let especialidadLimpia = data.replace(/['"]+/g, '');
        $("#txtEspecialidadBadge").text(especialidadLimpia || "Sin especialidad");
    })
    .catch(() => {
        $("#txtEspecialidadBadge").text("Sin especialidad");
    });

function cargarTabla() {
    if ($.fn.DataTable.isDataTable('#tbData')) {
        $('#tbData').DataTable().destroy();
    }

    tablaData = $('#tbData').DataTable({
        responsive: true,
        scrollX: true,
        "ajax": {
            "url": `/${controlador}/ListaCitasAsignadas`, // ⚠️ Sin parámetros, solo pendientes
            "type": "GET",
            "datatype": "json",
            "dataSrc": function (json) {
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
function revisarEmergencias() {
    fetch('/Doctor/ListaEmergencias')
        .then(res => res.json())
        .then(data => {
            if (data.data && data.data.length > 0) {
                $("#panelEmergencias").removeClass("d-none");
                $("#listaEmergencias").html("");
                data.data.forEach(em => {
                    $("#listaEmergencias").append(`<p><strong>${em.usuario.nombre}</strong> - ${em.horaCita} (Hoy)</p>`);
                });
            } else {
                $("#panelEmergencias").addClass("d-none");
            }
        });
}

// Llamar a la función al cargar y luego cada 30 segundos
$(document).ready(function () {
    revisarEmergencias();
    setInterval(revisarEmergencias, 30000);
});

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
// ==========================================
// REVISAR EMERGENCIAS CADA 30 SEGUNDOS
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
                                <button class="btn btn-sm btn-outline-primary btn-atender-emergencia me-2" data-id="${em.idCita}">
                                    <i class="fas fa-notes-medical me-1"></i> Atender
                                </button>
                                <!-- ⚠️ NUEVO BOTÓN PARA DESCARTAR -->
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

// Iniciar la revisión al cargar la página y luego cada 30 segundos
$(document).ready(function () {
    revisarEmergencias();
    setInterval(revisarEmergencias, 30000);
});

// Evento para atender la emergencia desde el panel
$(document).on("click", ".btn-atender-emergencia", function () {
    let idCita = $(this).data("id");
    // Abrimos el modal de indicaciones y receta que ya tienes, pasándole el ID
    $("#txtIdCita").val(idCita);
    $("#txtIndicaciones").val("");
    recetasTemporales = [];
    $("#tbRecetaTemporal tbody").empty();
    $("#mdData").modal('show');
});
// ⚠️ EVENTO PARA DESCARTAR EMERGENCIA
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
            // Llamamos al endpoint para eliminar o cambiar estado
            fetch(`/Doctor/DescartarEmergencia?idCita=${idCita}`, { method: "POST" })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        Swal.fire("Descartada", "La emergencia ha sido removida del panel.", "success");
                        boton.closest('.d-flex').remove(); // Removemos visualmente
                        // Si ya no hay emergencias, ocultamos el panel
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