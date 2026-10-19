let Especialidades = [];
let Doctores = [];
let DoctorHorario = [];
const modal = "mdData";
let IdEspecialidadSeleccionada = 0;
let IdDoctorSeleccionado = 0;
let IdHoraSeleccionado = 0;
let EspecialidadSeleccionada = "";
let DoctorSeleccionado = "";
let HoraSeleccionado = "";

const IndexTabs = [0, 1, 2];

document.addEventListener("DOMContentLoaded", function (event) {
    $.datepicker.setDefaults($.datepicker.regional['es']);
    $("#tabs").tabs();
    $("#txtBuscarEspecialidad").trigger("focus");

    $("#tab-especialidad").LoadingOverlay("show");
    fetch(`/Especialidad/Lista`, {
        method: "GET",
        headers: { 'Content-Type': 'application/json;charset=utf-8' }
    }).then(response => {
        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {
        $("#tab-especialidad").LoadingOverlay("hide");
        if (responseJson.data.length > 0) {
            Especialidades = responseJson.data;
            BuscarEspecialidad("");
        }
    }).catch((error) => {
        $("#tab-especialidad").LoadingOverlay("hide");
        console.log(error);
        Swal.fire({
            title: "Error!",
            text: "No se encontraron coincidencias.",
            icon: "warning"
        });
    });

    $("#tabs").tabs("option", "disabled", [1, 2]);
    $("#btnSiguiente").prop("disabled", true);
});

$("#txtBuscarEspecialidad").on("input", function () {
    IdEspecialidadSeleccionada = 0;
    BuscarEspecialidad($(this).val());
    $("#btnSiguiente").prop("disabled", true);
    $(".card-select").removeClass("selected");
});

$("#txtBuscarDoctor").on("input", function () {
    IdDoctorSeleccionado = 0;
    BuscarDoctor($(this).val());
    $("#btnSiguiente").prop("disabled", true);
    $(".card-select").removeClass("selected");
});

// Selección de Especialidad
$(document).on("click", "div.card-especialidad", function () {
    $(".card-especialidad").removeClass("selected");
    $(this).addClass("selected");
    IdEspecialidadSeleccionada = $(this).data("id");
    EspecialidadSeleccionada = $(this).data("text");
    $("#btnSiguiente").prop("disabled", false);
});

// Selección de Doctor
$(document).on("click", "div.card-doctor", function () {
    $(".card-doctor").removeClass("selected");
    $(this).addClass("selected");
    IdDoctorSeleccionado = $(this).data("id");
    DoctorSeleccionado = $(this).data("text");
    $("#btnSiguiente").prop("disabled", false);
});

// Selección de Hora
$(document).on("click", "div.card-hora", function () {
    $(".card-hora").removeClass("active");
    $(this).addClass("active");
    IdHoraSeleccionado = $(this).data("id");
    HoraSeleccionado = $(this).data("text");

    if ($("#txtFechaCita").val() != "" && IdHoraSeleccionado != 0)
        $("#btnSiguiente").prop("disabled", false);
});

function BuscarEspecialidad(valor) {
    $("#contenedor-especialidades").html("");
    const resultadosFiltrados = Especialidades.filter(element => element.nombre.toLowerCase().includes(valor.toLowerCase()));
    const resumenFiltro = resultadosFiltrados.slice(0, 6); // Mostramos 6 para que llene bien la pantalla

    resumenFiltro.forEach(function (item) {
        // Usamos las clases nuevas: card-select, card-especialidad
        $("#contenedor-especialidades").append(
            `<div class="col mb-3">
                <div class="card-select card-especialidad" data-id="${item.idEspecialidad}" data-text="${item.nombre}">
                    <i class="fa-solid fa-clipboard"></i>
                    <h6>${item.nombre}</h6>
                </div>
            </div>`
        );
    });
}

function BuscarDoctor(valor) {
    $("#contenedor-doctores").html("");
    const resultadosFiltrados = Doctores.filter(element => (element.nombres + " " + element.apellidos).toLowerCase().includes(valor.toLowerCase()));
    resultadosFiltrados.forEach(function (item) {
        const acronimo = item.genero == "F" ? "Dra." : "Dr.";
        // Usamos las clases nuevas: card-select, card-doctor
        $("#contenedor-doctores").append(
            `<div class="col mb-3">
                <div class="card-select card-doctor" data-id="${item.idDoctor}" data-text="${acronimo} ${item.nombres} ${item.apellidos}">
                    <i class="fa-solid fa-user-doctor"></i>
                    <h6>${acronimo} ${item.nombres} ${item.apellidos}</h6>
                </div>
            </div>`
        );
    });
}

$("#btnSiguiente").on("click", function () {
    const indexTab = $("ul li.ui-state-active").index();
    let habilitarIndex = indexTab;

    if (indexTab + 1 <= 2) {
        habilitarIndex = indexTab + 1;
        let bloquearIndex = IndexTabs.filter((i) => i != habilitarIndex);
        $("#tabs").tabs("option", "disabled", bloquearIndex);
        $("#tabs").tabs({ active: habilitarIndex });
        $("#btnSiguiente").prop("disabled", true);

        if (habilitarIndex == 1) {
            $("#txtBuscarDoctor").val("");
            $("#txtBuscarDoctor").trigger("focus");
            ObtenerDoctores();
        } else if (habilitarIndex == 2) {
            $("#txtFechaCita").val("");
            $("#contenedor-am").html("");
            $("#contenedor-pm").html("");
            ObtenerDoctoreHorarioDetalle();
        }
    }

    if (indexTab == 2) {
        $("#txtEspecialidad").val(EspecialidadSeleccionada);
        $("#txtDoctor").val(DoctorSeleccionado);
        $("#txtFechadeCita").val($("#txtFechaCita").val());
        $("#txtHoraCita").val(HoraSeleccionado);
        $("#mdData").modal("show");
    }
});

$("#btnRegresar").on("click", function () {
    const indexTab = $("ul li.ui-state-active").index();

    if (indexTab === 0) {
        window.location.href = '/Citas/Index';
        return;
    }

    let habilitarIndex = indexTab - 1;
    let bloquearIndex = IndexTabs.filter((i) => i != habilitarIndex);
    $("#tabs").tabs("option", "disabled", bloquearIndex);
    $("#tabs").tabs({ active: habilitarIndex });

    $("#btnSiguiente").prop("disabled", false);

    if (habilitarIndex == 0) {
        $("#txtBuscarEspecialidad").val("");
        $("#txtBuscarEspecialidad").trigger("focus");
        $(".card-especialidad").removeClass("selected");
        BuscarEspecialidad("");
    } else if (habilitarIndex == 1) {
        $("#txtBuscarDoctor").val("");
        $("#txtBuscarDoctor").trigger("focus");
        $(".card-doctor").removeClass("selected");
        BuscarDoctor("");
    }
});

function ObtenerDoctores() {
    $("#tab-doctor").LoadingOverlay("show");
    fetch(`/Doctor/Lista`, {
        method: "GET",
        headers: { 'Content-Type': 'application/json;charset=utf-8' }
    }).then(response => {
        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {
        if (responseJson.data.length > 0) {
            Doctores = responseJson.data.filter(element => element.especialidad.idEspecialidad == IdEspecialidadSeleccionada);
            BuscarDoctor("");
        }
        $("#tab-doctor").LoadingOverlay("hide");
    }).catch((error) => {
        $("#tab-doctor").LoadingOverlay("hide");
        console.log(error);
        Swal.fire({
            title: "Error!",
            text: "No se encontraron coincidencias.",
            icon: "warning"
        });
    });
}

function ObtenerDoctoreHorarioDetalle() {
    $("#txtFechaCita").datepicker("destroy");
    $("#tab-horario").LoadingOverlay("show");
    $("#contenedor-am").html("");
    $("#contenedor-pm").html("");

    fetch(`/Citas/ListaDoctorHorarioDetalle?Id=${IdDoctorSeleccionado}`, {
        method: "GET",
        headers: { 'Content-Type': 'application/json;charset=utf-8' }
    }).then(response => {
        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {
        if (responseJson.data.length > 0) {
            const arraySoloFechas = responseJson.data.map(item => item.fecha);
            DoctorHorario = responseJson.data;

            $("#txtFechaCita").datepicker({
                defaultDate: "",
                minDate: 0,
                beforeShowDay: function (date) {
                    var formattedDate = $.datepicker.formatDate("dd/mm/yy", date);
                    var esFechaPermitida = ($.inArray(formattedDate, arraySoloFechas) !== -1);
                    return [esFechaPermitida, ""];
                },
                onSelect: function (dateText, inst) {
                    $("#btnSiguiente").prop("disabled", true);
                    $("#contenedor-am").html("");
                    $("#contenedor-pm").html("");

                    const selectedDate = $(this).val();
                    const Fecha = DoctorHorario.find(element => element.fecha == selectedDate);

                    if (Fecha) {
                        const HorarioAM = Fecha.horarioDTO.filter(element => element.turno == "AM");
                        const HorarioPM = Fecha.horarioDTO.filter(element => element.turno == "PM");

                        HorarioAM.forEach(function (item) {
                            // Usamos las clases nuevas: btn-hora y card-hora
                            $("#contenedor-am").append(
                                `<div class="col">
                                    <button type="button" class="btn btn-hora card-hora" data-id="${item.idDoctorHorarioDetalle}" data-text="${item.turnoHora}">
                                        ${item.turnoHora}
                                    </button>
                                </div>`
                            );
                        });

                        HorarioPM.forEach(function (item) {
                            $("#contenedor-pm").append(
                                `<div class="col">
                                    <button type="button" class="btn btn-hora card-hora" data-id="${item.idDoctorHorarioDetalle}" data-text="${item.turnoHora}">
                                        ${item.turnoHora}
                                    </button>
                                </div>`
                            );
                        });
                    }
                }
            });
        } else {
            Swal.fire({
                text: "No hay horarios disponibles.",
                icon: "warning"
            });
        }
        $("#tab-horario").LoadingOverlay("hide");
    }).catch((error) => {
        $("#tab-horario").LoadingOverlay("hide");
        Swal.fire({
            title: "Error!",
            text: "No se encontraron coincidencias.",
            icon: "warning"
        });
    });
}

$("#btnAgendar").on("click", function () {
    if ($("#txtFechaCita").val() == "" || IdHoraSeleccionado == 0) {
        Swal.fire({
            title: "Error!",
            text: "Falta seleccionar la fecha o la hora.",
            icon: "warning"
        });
        return;
    }

    let objeto = {
        DoctorHorarioDetalle: {
            IdDoctorHorarioDetalle: IdHoraSeleccionado
        },
        EstadoCita: {
            IdEstadoCita: 1
        },
        FechaCita: moment($("#txtFechaCita").val(), "DD/MM/YYYY").format('DD/MM/YYYY')
    };

    fetch(`/Citas/Guardar`, {
        method: "POST",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(objeto)
    }).then(response => {
        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {
        if (responseJson.data == "") {
            Swal.fire({
                title: "¡Felicidades!",
                text: "Su cita fue registrada!",
                icon: "success"
            }).then(function () {
                window.location.href = '/Citas/Index';
            });
            $(`#${modal}`).modal('hide');
        } else {
            Swal.fire({
                title: "Error!",
                text: responseJson.data,
                icon: "warning"
            });
        }
    }).catch((error) => {
        Swal.fire({
            title: "Error!",
            text: "No se pudo registrar.",
            icon: "warning"
        });
    });
});