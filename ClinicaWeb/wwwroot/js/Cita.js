let tablaData;
let idEditar = 0;
const controlador = "Citas";
const modal = "mdData";
const preguntaEliminar = "¿Desea cancelar su cita?";
const confirmaEliminar = "Su cita fue cancelada.";

document.addEventListener("DOMContentLoaded", function (event) {

    tablaData = $('#tbData').DataTable({
        responsive: true,
        scrollX: true,
        "ajax": {
            "url": `/${controlador}/ListaCitasPendiente`,
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
            { title: "Fecha Cita", "data": "fechaCita", width: "150px" },
            { title: "Hora Cita", "data": "horaCita", width: "150px" },
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
                title: "Acciones", "data": "idCita", width: "120px", render: function (data, type, row) {
                    return `<button type="button" class="btn btn-sm btn-outline-danger btn-cancelar"><i class="fas fa-times"></i> Cancelar</button>`
                }
            }
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        },
    });
});

$("#btnEmergencia").click(async function () {
    Swal.fire({ title: 'Cargando...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

    try {
        const response = await fetch('/Especialidad/Lista');
        const data = await response.json();

        let opcionesEspecialidades = {};
        data.data.forEach(esp => {
            opcionesEspecialidades[esp.idEspecialidad] = esp.nombre;
        });

        // ⚠️ PASO 1: Seleccionar especialidad
        const { value: idEspecialidad } = await Swal.fire({
            title: '¡Emergencia Médica!',
            text: 'Selecciona la especialidad que necesitas.',
            input: 'select',
            inputOptions: opcionesEspecialidades,
            inputPlaceholder: 'Selecciona una especialidad',
            showCancelButton: true,
            confirmButtonText: 'Siguiente',
            cancelButtonText: 'Cancelar'
        });

        if (idEspecialidad) {
            // ⚠️ PASO 2: Pedir el motivo
            const { value: motivo } = await Swal.fire({
                title: 'Describe tu emergencia',
                input: 'textarea',
                inputPlaceholder: 'Ej: Me duele mucho el pecho y no puedo respirar...',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                confirmButtonText: 'Solicitar Emergencia',
                cancelButtonText: 'Cancelar',
                inputValidator: (value) => {
                    if (!value) return '¡Necesitas escribir el motivo!';
                }
            });

            if (motivo) {
                Swal.fire({ title: 'Buscando doctor...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

                const res = await fetch(`/Citas/SolicitarEmergencia?idEspecialidad=${idEspecialidad}&motivo=${encodeURIComponent(motivo)}`, { method: 'POST' });
                const resultado = await res.json();

                if (resultado.data === "") {
                    Swal.fire('¡Atendido!', 'Un doctor revisará tu emergencia en breve.', 'success');
                } else {
                    Swal.fire('Lo sentimos', resultado.data, 'error');
                }
            }
        }
    } catch (error) {
        Swal.fire('Error', 'No se pudieron cargar las especialidades.', 'error');
    }
});

$("#tbData tbody").on("click", ".btn-cancelar", function () {
    let filaSeleccionada = $(this).closest('tr');
    let data = tablaData.row(filaSeleccionada).data();

    Swal.fire({
        text: `${preguntaEliminar}`,
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Si, continuar",
        cancelButtonText: "No, volver"
    }).then((result) => {
        if (result.isConfirmed) {
            fetch(`/${controlador}/Cancelar?Id=${data.idCita}`, {
                method: "DELETE",
                headers: { 'Content-Type': 'application/json;charset=utf-8' }
            }).then(response => {
                // Si la respuesta HTTP no es OK, lanzamos el error para que lo atrape el catch
                if (!response.ok) {
                    return response.json().then(err => { throw new Error(err.data || "Error desconocido"); });
                }
                return response.json();
            }).then(responseJson => {
                // Si "data" está vacío (""), se canceló correctamente
                if (responseJson.data === "") {
                    Swal.fire({
                        title: "¡Listo!",
                        text: confirmaEliminar,
                        icon: "success"
                    });
                    tablaData.ajax.reload();
                } else {
                    // Si hay texto, mostramos el error real de la BD
                    Swal.fire({
                        title: "Atención",
                        text: responseJson.data,
                        icon: "warning"
                    });
                }
            }).catch((error) => {
                // Errores de red o excepciones del servidor
                Swal.fire({
                    title: "Error del Servidor",
                    text: error.message,
                    icon: "error"
                });
            });
        }
    });
});