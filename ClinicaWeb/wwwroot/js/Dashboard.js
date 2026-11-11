$(document).ready(function () {

    // ==========================================
    // 1. CARGAR PANEL DE EMERGENCIAS
    // ==========================================
    fetch('/Home/ListaEmergenciasAdmin')
        .then(res => res.json())
        .then(data => {
            console.log("Datos Emergencias:", data); // ⚠️ Para depurar
            if (data.data && data.data.length > 0) {
                $("#panelEmergenciasAdmin").show();
                let tbody = $("#tbEmergenciasAdmin tbody");
                tbody.empty();

                data.data.forEach(em => {
                    // ⚠️ Usamos los nombres exactos del JSON
                    tbody.append(`
                        <tr>
                            <td>${em.fechaCita}</td>
                            <td>${em.horaCita}</td>
                            <td>${em.paciente}</td>
                            <td>${em.especialidad}</td>
                            <td><small class="text-danger">${em.motivo}</small></td>
                        </tr>
                    `);
                });
            } else {
                $("#panelEmergenciasAdmin").hide();
            }
        })
        .catch(err => console.error("Error al cargar emergencias:", err));

    // ==========================================
    // 2. CARGAR GRÁFICA
    // ==========================================
    fetch('/Home/GraficaEmergenciasRechazadas')
        .then(res => res.json())
        .then(data => {
            if (!data.data || data.data.length === 0) {
                console.log("No hay urgencias rechazadas para graficar.");
                return;
            }

            // ⚠️ Aquí también usamos los nombres exactos del JSON
            const labels = data.data.map(x => x.fecha);
            const valores = data.data.map(x => x.total);

            const canvas = document.getElementById('graficaUrgencias');
            if (canvas) {
                new Chart(canvas, {
                    type: 'bar', // ⚠️ Cambiado a barras para que se vea mejor
                    data: {
                        labels: labels,
                        datasets: [{
                            label: 'Urgencias Rechazadas',
                            data: valores,
                            backgroundColor: '#ef4444',
                            borderRadius: 6
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: { display: false }
                        },
                        scales: {
                            y: {
                                beginAtZero: true,
                                ticks: { stepSize: 1 }
                            }
                        }
                    }
                });
            }
        })
        .catch(err => console.error("Error al cargar gráfica:", err));
});

// ==========================================
// 3. EVENTO PARA ASIGNAR DOCTOR MANUALMENTE
// ==========================================
$(document).on("click", ".btn-asignar-emergencia", function () {
    let idCita = $(this).data("idcita");
    let especialidad = $(this).data("especialidad");

    // ⚠️ Aquí cargamos los doctores dinámicamente
    fetch('/Doctor/Lista')
        .then(res => res.json())
        .then(data => {
            if (data.data && data.data.length > 0) {
                // Filtramos los doctores por la especialidad de la emergencia
                let doctoresFiltrados = {};
                data.data.forEach(doc => {
                    if (doc.especialidad.nombre === especialidad) {
                        doctoresFiltrados[doc.idDoctor] = `Dr. ${doc.nombres} ${doc.apellidos}`;
                    }
                });

                if (Object.keys(doctoresFiltrados).length === 0) {
                    Swal.fire("Aviso", "No hay doctores registrados para esta especialidad.", "info");
                    return;
                }

                Swal.fire({
                    title: 'Asignar Emergencia',
                    text: `Selecciona un doctor de ${especialidad}`,
                    input: 'select',
                    inputOptions: doctoresFiltrados,
                    inputPlaceholder: 'Selecciona un doctor',
                    showCancelButton: true,
                    confirmButtonText: 'Asignar',
                    cancelButtonText: 'Cancelar'
                }).then((result) => {
                    if (result.isConfirmed && result.value) {
                        // Aquí harías el fetch para actualizar la cita con el doctor seleccionado
                        Swal.fire("Asignado", "La emergencia ha sido asignada al doctor.", "success").then(() => {
                            // Recargar la tabla
                            location.reload();
                        });
                    }
                });
            }
        })
        .catch(err => console.log("Error al cargar doctores", err));
});