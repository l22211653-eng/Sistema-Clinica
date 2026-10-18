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
            "datatype": "json"
        },
        "columns": [
            { title: "Fecha Cita", "data": "fechaCita", width: "120px" },
            {
                title: "Especialidad", "data": "especialidad", render: function (data, type, row) {
                    return data.nombre
                }
            },
            {
                title: "Doctor", "data": "doctor", render: function (data, type, row) {
                    return `${data.nombres} ${data.apellidos}`
                }
            },
            {

                title: "Acciones", "data": null, width: "250px", render: function (data, type, row) {
                    // Validación segura por si el doctor o el id no existen
                    let idDoctor = (row.doctor && row.doctor.idDoctor) ? row.doctor.idDoctor : 0;

                    return `
                        <button type="button" class="btn btn-sm btn-outline-primary me-1 btn-indicaciones" data-id="${row.idCita}">Indicaciones</button>
                        <button type="button" class="btn btn-sm btn-outline-info me-1 btn-receta" data-id="${row.idCita}">Receta</button>
                        <button type="button" class="btn btn-sm btn-outline-warning btn-calificar" data-idcita="${row.idCita}" data-iddoctor="${idDoctor}">Calificar</button>
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

    $("#txtIndicaciones").val(data.indicaciones)
    $(`#${modal}`).modal('show');
    $("#txtIndicaciones").prop('disabled', true);
});

// ==========================================
// 2. EVENTO: CALIFICAR (Estrellas)
// ==========================================
let puntuacionSeleccionada = 0;

// Efecto visual de las estrellas
$("#contenedorEstrellas i").hover(function () {
    let val = $(this).data("val");
    $("#contenedorEstrellas i").each(function (index) {
        if (index < val) {
            $(this).removeClass("far").addClass("fas"); // Estrella llena
        } else {
            $(this).removeClass("fas").addClass("far"); // Estrella vacía
        }
    });
}, function () {
    // Al quitar el mouse, volvemos a la puntuación seleccionada
    $("#contenedorEstrellas i").each(function (index) {
        if (index < puntuacionSeleccionada) {
            $(this).removeClass("far").addClass("fas");
        } else {
            $(this).removeClass("fas").addClass("far");
        }
    });
});

// Al hacer clic en una estrella
$("#contenedorEstrellas i").click(function () {
    puntuacionSeleccionada = $(this).data("val");
    $("#txtPuntuacion").val(puntuacionSeleccionada);
});

// Abrir modal de calificar
$("#tbData tbody").on("click", ".btn-calificar", function () {
    puntuacionSeleccionada = 0; // Reiniciar
    $("#txtPuntuacion").val(0);
    $("#txtComentario").val("");
    $("#contenedorEstrellas i").removeClass("fas").addClass("far"); // Resetear estrellas

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
        IdUsuario: 0, // Se llena automáticamente en el backend
        Puntuacion: puntuacion,
        Comentario: $("#txtComentario").val()
    };

    // Mostrar carga
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

    // Mostrar carga
    Swal.fire({ title: 'Buscando receta...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

    fetch(`/HistorialCitas/ListaRecetasCita?idCita=${idCita}`, {
        method: 'GET'
    })
        .then(response => response.json())
        .then(data => {
            Swal.close(); // Cerrar alerta de carga
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
                // No hay recetas
                $("#tbReceta").addClass("d-none");
                $("#mensajeSinReceta").removeClass("d-none");
            }

            $("#mdReceta").modal('show');
        })
        .catch(error => {
            Swal.fire("Error", "No se pudo obtener la receta.", "error");
        });
});