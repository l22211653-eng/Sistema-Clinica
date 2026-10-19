let tablaData;
const controlador = "HistorialCitas";
const modal = "mdData";

document.addEventListener("DOMContentLoaded", function (event) {

    tablaData = $('#tbData').DataTable({
        responsive: true,
        scrollX: true,
        "ajax": {
            "url": `/${controlador}/ListaHistorialCitas`,
            "type": "GET",
            "datatype": "json",
            "dataSrc": function (json) {
                if (json && json.error) {
                    Swal.fire("Error del Servidor", json.error, "error");
                    return [];
                }
                return json || [];
            }
        },
        "columns": [
            { title: "Fecha Cita", "data": "fechaCita", width: "120px" },
            {
                title: "Especialidad", "data": "especialidad", render: function (data, type, row) {
                    return data ? data.nombre : '';
                }
            },
            {
                title: "Doctor", "data": "doctor", render: function (data, type, row) {
                    return data ? `${data.nombres} ${data.apellidos}` : '';
                }
            },
            {
                title: "Acciones", "data": null, width: "250px", render: function (data, type, row) {
                    let idDoctor = (row.doctor && row.doctor.idDoctor) ? row.doctor.idDoctor : 0;

                    return `
                   <button type="button" class="btn btn-outline-primary btn-sm me-1 btn-indicaciones" data-id="${row.idCita}" title="Ver Indicaciones">
                   <i class="fas fa-eye"></i> Indicaciones
                   </button>
                   <button type="button" class="btn btn-outline-info btn-sm me-1 btn-receta" data-id="${row.idCita}" title="Ver Receta">
                   <i class="fas fa-prescription"></i> Receta
                   </button>
                   <button type="button" class="btn btn-outline-warning btn-sm btn-calificar" data-idcita="${row.idCita}" data-iddoctor="${idDoctor}" title="Calificar Doctor">
                   <i class="fas fa-star"></i> Calificar
                   </button>
                    `;
                }
            }
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        },
    });
});

// ==========================================
// 1. EVENTO: VER INDICACIONES
// ==========================================
$("#tbData tbody").on("click", ".btn-indicaciones", function () {
    const filaSeleccionada = $(this).closest('tr');
    const data = tablaData.row(filaSeleccionada).data();

    $("#txtIndicaciones").val(data.indicaciones);
    $(`#${modal}`).modal('show');
    $("#txtIndicaciones").prop('disabled', true);
});

// ==========================================
// 2. EVENTO: CALIFICAR (Estrellas)
// ==========================================
let puntuacionSeleccionada = 0;

function pintarEstrellas(valor) {
    $("#contenedorEstrellas .estrella").each(function (index) {
        if (index < valor) {
            $(this).css("color", "#f59e0b"); // Amarillo
        } else {
            $(this).css("color", "#cbd5e1"); // Gris
        }
    });
}

// Efecto al pasar el mouse
$(document).on("mouseenter", "#contenedorEstrellas .estrella", function () {
    let val = parseInt($(this).attr("data-val"));
    pintarEstrellas(val);
});

// Al quitar el mouse
$(document).on("mouseleave", "#contenedorEstrellas", function () {
    pintarEstrellas(puntuacionSeleccionada);
});

// Al hacer clic en una estrella
$(document).on("click", "#contenedorEstrellas .estrella", function () {
    puntuacionSeleccionada = parseInt($(this).attr("data-val"));
    $("#txtPuntuacion").val(puntuacionSeleccionada);
    pintarEstrellas(puntuacionSeleccionada);
});

// Abrir modal de calificar
$(document).on("click", ".btn-calificar", function () {
    puntuacionSeleccionada = 0;
    $("#txtPuntuacion").val(0);
    $("#txtComentario").val("");

    // ⚠️ Pequeño truco: reseteamos las estrellas justo después de que el modal se muestre
    // para asegurarnos de que se pinten bien visualmente.
    setTimeout(() => {
        pintarEstrellas(0);
    }, 100);

    $("#txtIdCitaResena").val($(this).data("idcita"));
    $("#txtIdDoctorResena").val($(this).data("iddoctor"));
    $("#mdResena").modal('show');
});

// Guardar la reseña
$("#btnGuardarResena").click(function () {
    let puntuacion = parseInt($("#txtPuntuacion").val());

    if (puntuacion === 0) {
        Swal.fire("Atención", "Por favor, selecciona al menos 1 estrella.", "warning");
        return;
    }

    let objetoResena = {
        IdCita: parseInt($("#txtIdCitaResena").val()),
        IdDoctor: parseInt($("#txtIdDoctorResena").val()),
        IdUsuario: 0,
        Puntuacion: puntuacion,
        Comentario: $("#txtComentario").val()
    };

    Swal.fire({ title: 'Guardando...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

    fetch(`/HistorialCitas/GuardarResena`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(objetoResena)
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                Swal.fire("¡Gracias!", "Tu reseña ha sido guardada.", "success");
                $("#mdResena").modal('hide');
            } else {
                Swal.fire("Error", data.message, "error");
            }
        })
        .catch(error => {
            Swal.fire("Error", "No se pudo guardar la reseña.", "error");
        });
});

// ==========================================
// 3. EVENTO: VER RECETA MÉDICA
// ==========================================
$("#tbData tbody").on("click", ".btn-receta", function () {
    let idCita = $(this).data("id");

    Swal.fire({ title: 'Buscando receta...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

    fetch(`/HistorialCitas/ListaRecetasCita?idCita=${idCita}`, {
        method: 'GET'
    })
        .then(response => response.json())
        .then(data => {
            Swal.close();
            let tbody = $("#tbReceta tbody");
            tbody.empty();

            if (data && data.length > 0) {
                $("#mensajeSinReceta").addClass("d-none");
                $("#tbReceta").removeClass("d-none");

                data.forEach(receta => {
                    tbody.append(`
                <tr>
                    <td>${receta.medicamento}</td>
                    <td>${receta.dosis}</td>
                    <td>${receta.indicaciones}</td>
                </tr>
            `);
                });
            } else {
                $("#tbReceta").addClass("d-none");
                $("#mensajeSinReceta").removeClass("d-none");
            }

            $("#mdReceta").modal('show');
        })
        .catch(error => {
            Swal.fire("Error", "No se pudo obtener la receta.", "error");
        });
});