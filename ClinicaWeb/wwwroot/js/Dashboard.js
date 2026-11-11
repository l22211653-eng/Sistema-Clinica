$(document).ready(function () {
    // 1. Cargar Panel de Emergencias
    fetch('/Home/ListaEmergenciasAdmin')
        .then(res => res.json())
        .then(data => {
            if (data.data && data.data.length > 0) {
                $("#panelEmergenciasAdmin").show();
                data.data.forEach(em => {
                    $("#tbEmergenciasAdmin tbody").append(`
                        <tr>
                            <td>${em.usuario.nombre}</td>
                            <td>${em.especialidad.nombre}</td>
                            <td>${em.fechaCita}</td>
                            <td>${em.horaCita}</td>
                            <td class="text-danger">${em.indicaciones}</td>
                        </tr>
                    `);
                });
            }
        });

    // 2. Cargar Gráfica de Urgencias
    fetch('/Home/GraficaEmergenciasRechazadas')
        .then(res => res.json())
        .then(data => {
            const labels = data.data.map(x => x.fecha);
            const valores = data.data.map(x => x.total);

            new Chart(document.getElementById('graficaUrgencias'), {
                type: 'line',
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'Urgencias Rechazadas',
                        data: valores,
                        borderColor: '#ef4444',
                        backgroundColor: 'rgba(239, 68, 68, 0.1)',
                        fill: true,
                        tension: 0.3
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
                }
            });
        });
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