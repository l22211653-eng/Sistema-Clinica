let tablaData;
let idEditar = 0;
let fechasSeleccionadas = [];
const controlador = "DoctorHorario";
const modal = "mdData";
const preguntaEliminar = "Desea eliminar el horario";
const confirmaEliminar = "El horario fue eliminado.";
const confirmaRegistro = "Horario registrado!";
const meses = [{ value: 1, text: "Enero" }, { value: 2, text: "Febrero" }, { value: 3, text: "Marzo" }, { value: 4, text: "Abril" },
{ value: 5, text: "Mayo" }, { value: 6, text: "Junio" }, { value: 7, text: "Julio" }, { value: 8, text: "Agosto" }, { value: 9, text: "Septiembre" }
    , { value: 10, text: "Octubre" }, { value: 11, text: "Noviembre" }, { value: 12, text: "Diciembre" }]
const mesActual = new Date().getMonth() + 1;

document.addEventListener("DOMContentLoaded", function (event) {

    tablaData = $('#tbData').DataTable({
        responsive: true,
        scrollX: true,
        "ajax": {
            "url": `/${controlador}/Lista`,
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                title: "Nro Documento", "data": "doctor", width: "150px", render: function (data, type, row) {
                    return data.numeroDocumentoIdentidad
                }
            },
            {
                title: "Doctor", "data": "doctor", render: function (data, type, row) {
                    return `${data.nombres} ${data.apellidos}`
                }
            },
            {
                title: "Mes", "data": "numeroMes", render: function (data, type, row) {
                    const mes = meses.find((m) => m.value == data);
                    return mes ? mes.text : '';
                }
            },
            { title: "Hora Inicio AM", "data": "horaInicioAM" },
            { title: "Hora Fin AM", "data": "horaFinAM" },
            { title: "Hora Inicio PM", "data": "horaInicioPM" },
            { title: "Hora Fin PM", "data": "horaFinPM" },
            { title: "Fecha Creacion", "data": "fechaCreacion" },
            {
                title: "", "data": "idDoctor", width: "100px", render: function (data, type, row) {
                    return `<button type="button" class="btn btn-sm btn-outline-danger me-1 btn-eliminar"><i class="fas fa-trash"></i></button>`
                }
            }
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        },
    });

    // Configuración de los Timepickers
    $('#txtHoraFinAM').timepicker({
        timeFormat: 'h:mm p', interval: 30, minTime: '08', maxTime: '11:30am',
        startTime: '08:00', dynamic: false, dropdown: true, scrollbar: true, zindex: 9999999
    });
    $('#txtHoraInicioAM').timepicker({
        timeFormat: 'h:mm p', interval: 30, minTime: '08', maxTime: '11:30am',
        startTime: '08:00', dynamic: false, dropdown: true, scrollbar: true, zindex: 9999999, change: MostrarHoraFinAM
    });
    $('#txtHoraFinPM').timepicker({
        timeFormat: 'h:mm p', interval: 30, minTime: '12', maxTime: '19:30PM',
        startTime: '12:00', dynamic: false, dropdown: true, scrollbar: true, zindex: 9999999
    });
    $('#txtHoraInicioPM').timepicker({
        timeFormat: 'h:mm p', interval: 30, minTime: '12', maxTime: '19:30PM',
        startTime: '12:00', dynamic: false, dropdown: true, scrollbar: true, zindex: 9999999, change: MostrarHoraFinPM
    });

    // Llenar combo de Doctores
    fetch(`/Doctor/Lista`, {
        method: "GET",
        headers: { 'Content-Type': 'application/json;charset=utf-8' }
    }).then(response => {
        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {
        if (responseJson.data.length > 0) {
            $("#cboDoctor").append($("<option>").val("").text(""));
            responseJson.data.forEach((item) => {
                $("#cboDoctor").append($("<option>").val(item.idDoctor).text(`${item.numeroDocumentoIdentidad} - ${item.nombres} ${item.apellidos}`));
            });
            $('#cboDoctor').select2({
                theme: 'bootstrap-5',
                dropdownParent: $('#mdData'),
                placeholder: "Seleccionar"
            });
        }
    }).catch((error) => {
        Swal.fire({ title: "Error!", text: "No se encontraron coincidencias.", icon: "warning" });
    });

    // Inicializar el calendario de jQuery UI
    $("#calendarioFechas").datepicker({
        dateFormat: 'dd/mm/yy',
        firstDay: 1,
        changeMonth: true,
        changeYear: true,
        minDate: 0,
        onSelect: function (dateText, inst) {
            let index = fechasSeleccionadas.indexOf(dateText);
            if (index > -1) {
                fechasSeleccionadas.splice(index, 1);
            } else {
                fechasSeleccionadas.push(dateText);
            }
            $("#txtFechasSeleccionadas").val(fechasSeleccionadas.join(","));
            $(this).datepicker('refresh');
        },
        beforeShowDay: function (date) {
            let string = jQuery.datepicker.formatDate('dd/mm/yy', date);
            let seleccionado = fechasSeleccionadas.indexOf(string) !== -1;

            if (seleccionado) {
                return [true, "dia-seleccionado", "Seleccionado"];
            } else {
                return [true, "", ""];
            }
        }
    });

});

function MostrarHoraFinAM() {
    const Hora = moment($("#txtHoraInicioAM").val(), "HH:mm");
    Hora.add(moment.duration("00:30"))
    $('#txtHoraFinAM').val('');
    $('#txtHoraFinAM').timepicker('option', 'minTime', Hora.format('HH:mm'))
    $('#txtHoraFinAM').timepicker('option', 'startTime', Hora.format('HH:mm'))
}

function MostrarHoraFinPM() {
    const Hora = moment($("#txtHoraInicioPM").val(), ['h:mm A']);
    Hora.add(moment.duration("00:30"))
    $('#txtHoraFinPM').val('');
    $('#txtHoraFinPM').timepicker('option', 'minTime', Hora.format('HH:mm'))
    $('#txtHoraFinPM').timepicker('option', 'startTime', Hora.format('HH:mm'))
}

$("#btnNuevo").on("click", function () {
    idEditar = 0;
    $("#cboDoctor").val("").trigger('change');
    $("#txtHoraInicioAM").val("")
    $("#txtHoraFinAM").val("")
    $("#txtHoraInicioPM").val("")
    $("#txtHoraFinPM").val("")

    fechasSeleccionadas = [];
    $("#txtFechasSeleccionadas").val("");
    $("#calendarioFechas").datepicker("setDate", null);
    $("#calendarioFechas").datepicker('refresh');

    $(`#${modal}`).modal('show');
})

$("#tbData tbody").on("click", ".btn-eliminar", function () {
    var filaSeleccionada = $(this).closest('tr');
    var data = tablaData.row(filaSeleccionada).data();

    Swal.fire({
        text: `${preguntaEliminar}?`,
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Si, continuar",
        cancelButtonText: "No, volver"
    }).then((result) => {
        if (result.isConfirmed) {
            fetch(`/${controlador}/Eliminar?Id=${data.idDoctorHorario}`, {
                method: "DELETE",
                headers: { 'Content-Type': 'application/json;charset=utf-8' }
            }).then(response => {
                return response.ok ? response.json() : Promise.reject(response);
            }).then(responseJson => {
                if (responseJson.data == "") {
                    Swal.fire({ title: "Eliminado!", text: confirmaEliminar, icon: "success" });
                    tablaData.ajax.reload();
                } else {
                    Swal.fire({ title: "Error!", text: "No se pudo eliminar.", icon: "warning" });
                }
            }).catch((error) => {
                Swal.fire({ title: "Error!", text: "No se pudo eliminar.", icon: "warning" });
            })
        }
    });
})

$("#btnGuardar").on("click", function () {

    if ($("#cboDoctor").val() == "" ||
        $("#txtFechasSeleccionadas").val() == "" ||
        $("#txtHoraInicioAM").val() == "" ||
        $("#txtHoraFinAM").val() == "" ||
        $("#txtHoraInicioPM").val() == "" ||
        $("#txtHoraFinPM").val() == ""
    ) {
        Swal.fire({ title: "Error!", text: "Falta completar datos o seleccionar fechas.", icon: "warning" });
        return;
    }

    const HoraInicioAM = moment($("#txtHoraInicioAM").val(), "HH:mm")
    const HoraFinAM = moment($("#txtHoraFinAM").val(), "HH:mm")
    const HoraInicioPM = moment($("#txtHoraInicioPM").val(), ['h:mm A'])
    const HoraFinPM = moment($("#txtHoraFinPM").val(), ['h:mm A'])

    let primerFecha = $("#txtFechasSeleccionadas").val().split(",")[0];
    let partes = primerFecha.split("/");
    let mes = parseInt(partes[1]);

    const objeto = {
        IdDoctorHorario: idEditar,
        Doctor: { IdDoctor: $("#cboDoctor").val() },
        NumeroMes: mes,
        HoraInicioAM: HoraInicioAM.format('HH:mm'),
        HoraFinAM: HoraFinAM.format('HH:mm'),
        HoraInicioPM: HoraInicioPM.format('HH:mm'),
        HoraFinPM: HoraFinPM.format('HH:mm'),
        DoctorHorarioDetalle: {
            Fecha: $("#txtFechasSeleccionadas").val()
        }
    }

    // ⚠️ AQUÍ ESTÁ EL CAMBIO: Ahora llama a /RegistrarHorario en lugar de /Guardar
    fetch(`/${controlador}/RegistrarHorario`, {
        method: "POST",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(objeto)
    }).then(async response => {
        if (response.ok) {
            return response.json();
        } else {
            const errorData = await response.json().catch(() => ({}));
            throw new Error(errorData.data || "Error en el servidor");
        }
    }).then(responseJson => {
        if (responseJson.data == "") {
            Swal.fire({ text: confirmaRegistro, icon: "success" });
            $(`#${modal}`).modal('hide');
            tablaData.ajax.reload();
        } else {
            Swal.fire({ title: "Error!", text: responseJson.data, icon: "warning" });
        }
    }).catch((error) => {
        // Muestra el error real si algo falla
        Swal.fire({ title: "Error al guardar", text: error.message, icon: "error" });
    })
});

// ==========================================
// GRÁFICAS Y TABLAS DE RENDIMIENTO
// ==========================================
$(document).ready(function () {
    $.ajax({
        url: "/DoctorHorario/ListaDoctoresSinHorario",
        type: "GET",
        success: function (response) {
            $("#tbAdvertencia tbody").empty();
            response.data.forEach((item) => {
                $("#tbAdvertencia tbody").append(
                    `<tr>
                        <td>${item.nombres} ${item.apellidos}</td>
                        <td>${item.especialidad.nombre}</td>
                    </tr>`
                )
            })
        }
    })
})

$.ajax({
    url: "/DoctorHorario/ListaRendimiento30Dias",
    type: "GET",
    success: function (response) {
        const datos = response.data;
        const agrupado = {};
        datos.forEach(item => {
            if (!agrupado[item.especialidad]) { agrupado[item.especialidad] = []; }
            agrupado[item.especialidad].push(item);
        });

        Object.keys(agrupado).forEach((especialidad, index) => {
            const canvasId = "grafica_" + index;
            $("#contenedorGraficas").append(`
                <div class="col-md-6 mb-4">
                    <div class="card shadow-sm h-100">
                        <div class="card-header bg-primary text-white">
                            <h5 class="mb-0">${especialidad}</h5>
                        </div>

                        <div class="card-body">
                            <canvas id="${canvasId}" height="180"></canvas>
                        </div>

                    </div>

                </div>

            `);
            const labels = agrupado[especialidad].map(x => x.doctor);
            const valores = agrupado[especialidad].map(x => x.totalAtendidas);
            const ctx = document.getElementById(canvasId);

            new Chart(ctx,
                {
                    type: 'bar',
                    data: {
                        labels: labels,
                        datasets: [{
                            label: 'Citas Atendidas',
                            data: valores,
                            borderWidth: 1,
                            borderRadius: 8,
                            backgroundColor: ['#4e73df', '#1cc88a', '#36b9cc', '#f6c23e', '#e74a3b', '#858796']
                        }]
                    },

                    options: {
                        responsive: true, maintainAspectRatio: false,
                        plugins: {
                            legend: { display: false },
                            title: { display: true, text: 'Pacientes atendidos últimos 30 días', font: { size: 16 } }
                        },
                        scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
                    }
                });
        });

    }
});

$.ajax({
    url: "/DoctorHorario/ListaDoctoresBajoRendimiento",
    type: "GET",
    success: function (response) {
        $("#tbBajoRendimiento tbody").empty();
        response.data.forEach(item => {
            $("#tbBajoRendimiento tbody").append(`
                <tr>
                    <td>${item.doctor}</td>
                    <td>${item.especialidad}</td>
                    <td><span class="badge bg-danger">${item.totalAtendidas}</span></td>
                </tr>


            `);

        });

    }

});