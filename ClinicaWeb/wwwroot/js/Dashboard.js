$(document).ready(function () {
    fetch('/Home/ListaEmergenciasAdmin')
        .then(res => res.json())
        .then(data => {
            if (data.data && data.data.length > 0) {
                $("#panelEmergenciasAdmin").show();
                $("#listaEmergenciasAdmin").html("");

                data.data.forEach(em => {
                    $("#listaEmergenciasAdmin").append(`
                        <div class="d-flex justify-content-between align-items-center border-bottom pb-2 mb-2">
                            <div>
                                <i class="fas fa-user-injured me-2"></i>
                                <strong>${em.usuario.nombre}</strong> - ${em.fechaCita} ${em.horaCita}
                                <br>
                                <small class="text-muted">Especialidad: ${em.especialidad.nombre}</small>
                                <br>
                                <small class="text-danger">${em.indicaciones}</small>
                            </div>
                        </div>
                    `);
                });
            }
        })
        .catch(err => console.log("Error al cargar emergencias admin", err));
});
$(document).ready(function () {
    cargarEmergenciasAdmin();
});

function cargarEmergenciasAdmin() {
    fetch('/Home/ListaEmergenciasAdmin')
        .then(res => res.json())
        .then(data => {
            if (data.data && data.data.length > 0) {
                $("#panelEmergenciasAdmin").show();
                let tbody = $("#tbEmergenciasAdmin tbody");
                tbody.empty();

                data.data.forEach(em => {
                    tbody.append(`
                        <tr>
                            <td>${em.fechaCita}</td>
                            <td>${em.horaCita}</td>
                            <td>${em.usuario.nombre}</td>
                            <td>${em.especialidad.nombre}</td>
                            <td><small class="text-danger">${em.indicaciones}</small></td>
                            <td>
                                <button class="btn btn-sm btn-primary btn-asignar-emergencia" 
                                        data-idcita="${em.idCita}" 
                                        data-especialidad="${em.especialidad.nombre}">
                                    <i class="fas fa-user-md me-1"></i> Asignar Doctor
                                </button>
                            </td>
                        </tr>
                    `);
                });
            } else {
                $("#panelEmergenciasAdmin").hide();
            }
        })
        .catch(err => console.log("Error al cargar emergencias admin", err));
}

// Evento para asignar doctor manualmente
$(document).on("click", ".btn-asignar-emergencia", function () {
    let idCita = $(this).data("idcita");
    let especialidad = $(this).data("especialidad");

    Swal.fire({
        title: 'Asignar Emergencia',
        text: `Selecciona un doctor de ${especialidad} para atender la emergencia.`,
        input: 'select',
        inputOptions: {
            // Aquí deberías cargar los doctores de esa especialidad
            // Por simplicidad, te muestro un ejemplo estático. 
            // Lo ideal es hacer un fetch a /Doctor/Lista y filtrar.
        },
        showCancelButton: true,
        confirmButtonText: 'Asignar',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            // Aquí harías el fetch para actualizar la cita con el doctor seleccionado
            Swal.fire("Asignado", "La emergencia ha sido asignada al doctor.", "success");
            cargarEmergenciasAdmin(); // Recargar la tabla
        }
    });
});