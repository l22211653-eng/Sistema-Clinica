$(document).ready(function () {

    // 1. CARGAR PANEL DE EMERGENCIAS
    fetch('/Home/ListaEmergenciasAdmin')
        .then(res => res.json())
        .then(data => {
            console.log("Datos Emergencias:", data); // ⚠️ Para depurar
            if (data.data && data.data.length > 0) {
                $("#panelEmergenciasAdmin").show();
                let tbody = $("#tbEmergenciasAdmin tbody");
                tbody.empty();

                data.data.forEach(em => {
                    tbody.append(`
                        <tr>
                            <td>${em.usuario.nombre}</td>
                            <td>${em.especialidad.nombre}</td>
                            <td>${em.fechaCita}</td>
                            <td>${em.horaCita}</td>
                            <td class="text-danger">${em.indicaciones}</td>
                        </tr>
                    `);
                });
            } else {
                $("#panelEmergenciasAdmin").hide();
            }
        })
        .catch(err => console.error("Error al cargar emergencias:", err));

    // 2. CARGAR GRÁFICA
    fetch('/Home/GraficaEmergenciasRechazadas')
        .then(res => res.json())
        .then(data => {
            console.log("Datos Gráfica:", data); // ⚠️ Para depurar
            if (!data.data || data.data.length === 0) {
                console.log("No hay urgencias rechazadas para graficar.");
                return;
            }

            const labels = data.data.map(x => x.fecha);
            const valores = data.data.map(x => x.total);

            const canvas = document.getElementById('graficaUrgencias');
            if (canvas) {
                new Chart(canvas, {
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