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