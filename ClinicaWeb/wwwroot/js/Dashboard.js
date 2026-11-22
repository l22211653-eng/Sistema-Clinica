$(document).ready(function () {

    // ==========================================
    // 1. CARGAR PANEL DE EMERGENCIAS
    // ==========================================
    fetch('/Home/ListaEmergenciasAdmin')
        .then(res => res.json())
        .then(data => {
            console.log("Datos Emergencias:", data);
            if (data.data && data.data.length > 0) {
                $("#panelEmergenciasAdmin").show();
                let tbody = $("#tbEmergenciasAdmin tbody");
                tbody.empty();

                data.data.forEach(em => {
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

            const labels = data.data.map(x => x.fecha);
            const valores = data.data.map(x => x.total);

            const canvas = document.getElementById('graficaUrgencias');
            if (canvas) {
                new Chart(canvas, {
                    type: 'bar',
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

$(document).on("click", ".btn-asignar-emergencia", function () {
    let idCita = $(this).data("idcita");
    let especialidad = $(this).data("especialidad");

    // ⚠️ Cargar TODOS los doctores, sin filtrar por especialidad
    fetch('/Doctor/Lista')
        .then(res => res.json())
        .then(data => {
            let doctores = {};
            data.data.forEach(doc => {
                // Mostramos el nombre y su especialidad (para que el admin sepa)
                doctores[doc.idDoctor] = `Dr. ${doc.nombres} ${doc.apellidos} (${doc.especialidad.nombre})`;
            });

            if (Object.keys(doctores).length === 0) {
                Swal.fire("Aviso", "No hay doctores registrados en el sistema.", "info");
                return;
            }

            Swal.fire({
                title: 'Asignar Emergencia',
                text: `Selecciona un doctor para atender la emergencia (Especialidad original: ${especialidad})`,
                input: 'select',
                inputOptions: doctores,
                inputPlaceholder: 'Selecciona un doctor',
                showCancelButton: true,
                confirmButtonText: 'Asignar',
                cancelButtonText: 'Cancelar'
            }).then((result) => {
                if (result.isConfirmed && result.value) {
                    fetch(`/Home/AsignarEmergenciaAdmin?idCita=${idCita}&idDoctor=${result.value}`, { method: "POST" })
                        .then(res => res.json())
                        .then(data => {
                            if (data.success) {
                                Swal.fire("Asignado", "La emergencia ha sido asignada al doctor.", "success");
                                location.reload();
                            } else {
                                Swal.fire("Error", data.message, "error");
                            }
                        });
                }
            });
        })
        .catch(err => console.log("Error al cargar doctores", err));
});