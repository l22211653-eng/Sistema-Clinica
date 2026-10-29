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
let FechaSeleccionadaTexto = "";

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
        Swal.fire("Error!", "No se encontraron coincidencias.", "warning");
    });

    $("#tabs").tabs("option", "disabled", [1, 2]);
    $("#btnSiguiente").prop("disabled", true);
});

$("#txtBuscarEspecialidad").on("input", function () {
    IdEspecialidadSeleccionada = 0;
    BuscarEspecialidad($(this).val());
    $("#btnSiguiente").prop("disabled", true);
    $(".card-especialidad").removeClass("selected");
});

$("#txtBuscarDoctor").on("input", function () {
    IdDoctorSeleccionado = 0;
    BuscarDoctor($(this).val());
    $("#btnSiguiente").prop("disabled", true);
    $(".card-doctor").removeClass("selected");
});

// Selección de Especialidad
$(document).on("click", ".card-especialidad", function () {
    $(".card-especialidad").removeClass("selected");
    $(this).addClass("selected");
    IdEspecialidadSeleccionada = $(this).data("id");
    EspecialidadSeleccionada = $(this).data("text");
    $("#btnSiguiente").prop("disabled", false);
});

// Selección de Doctor
$(document).on("click", ".card-doctor", function () {
    $(".card-doctor").removeClass("selected");
    $(this).addClass("selected");
    IdDoctorSeleccionado = $(this).data("id");
    DoctorSeleccionado = $(this).data("text");
    $("#btnSiguiente").prop("disabled", false);
});

// ⚠️ Selección de Hora (Corregido)
$(document).on("click", ".card-hora", function (e) {
    e.preventDefault();

    // Quitamos la clase 'active' a todos los botones de hora
    $("#contenedor-am .card-hora").removeClass("active");
    $("#contenedor-pm .card-hora").removeClass("active");

    // Se la ponemos solo al botón clickeado
    $(this).addClass("active");

    IdHoraSeleccionado = $(this).data("id");
    HoraSeleccionado = $(this).data("text");

    if (FechaSeleccionadaTexto !== "" && IdHoraSeleccionado != 0) {
        $("#btnSiguiente").prop("disabled", false);
    }
});

function BuscarEspecialidad(valor) {
    $("#contenedor-especialidades").html("");
    const resultadosFiltrados = Especialidades.filter(element => element.nombre.toLowerCase().includes(valor.toLowerCase()));
    const resumenFiltro = resultadosFiltrados.slice(0, 6);

    resumenFiltro.forEach(function (item) {
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
        let foto = item.fotoUrl ? item.fotoUrl : '/img/default-doctor.png';

        $("#contenedor-doctores").append(
            `<div class="col mb-3">
                <div class="card-select card-doctor" data-id="${item.idDoctor}" data-text="${acronimo} ${item.nombres} ${item.apellidos}">
                    <img src="${foto}" alt="Foto" style="width: 70px; height: 70px; border-radius: 50%; object-fit: cover; margin-bottom: 10px; border: 3px solid #e2e8f0;">
                    <h6>${acronimo} ${item.nombres} ${item.apellidos}</h6>
                </div>
            </div>`
        );
    });
}

$("#btnSiguiente").on("click", function (e) {
    e.preventDefault();

    const indexTab = $("#tabs ul li.ui-tabs-active").index();
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
            $("#contenedor-am").html("");
            $("#contenedor-pm").html("");
            ObtenerDoctoreHorarioDetalle();
        }
    }

    if (indexTab == 2) {
        $("#txtEspecialidad").val(EspecialidadSeleccionada);
        $("#txtDoctor").val(DoctorSeleccionado);
        $("#txtFechadeCita").val(FechaSeleccionadaTexto);
        $("#txtHoraCita").val(HoraSeleccionado);
        $("#mdData").modal("show");
    }
});

$("#btnRegresar").on("click", function () {
    const indexTab = $("#tabs ul li.ui-tabs-active").index();

    if (indexTab === 0) {
        window.location.href = '/Citas/Index';
        return;
    }

    let habilitarIndex = indexTab - 1;
    let bloquearIndex = IndexTabs.filter((i) => i != habilitarIndex);
    $("#tabs").tabs("option", "disabled", bloquearIndex);
    $("#tabs").tabs({ active: habilitarIndex });

    if (habilitarIndex == 0) {
        $("#btnSiguiente").prop("disabled", false);
    } else if (habilitarIndex == 1) {
        $("#btnSiguiente").prop("disabled", false);
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
        Swal.fire("Error!", "No se encontraron coincidencias.", "warning");
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
                    FechaSeleccionadaTexto = dateText;

                    $("#btnSiguiente").prop("disabled", true);
                    $("#contenedor-am").html("");
                    $("#contenedor-pm").html("");

                    const Fecha = DoctorHorario.find(element => element.fecha == dateText);

                    if (Fecha) {
                        const HorarioAM = Fecha.horarioDTO.filter(element => element.turno == "AM");
                        const HorarioPM = Fecha.horarioDTO.filter(element => element.turno == "PM");

                        HorarioAM.forEach(function (item) {
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
            Swal.fire("Aviso", "No hay horarios disponibles para este doctor.", "warning");
        }
        $("#tab-horario").LoadingOverlay("hide");
    }).catch((error) => {
        $("#tab-horario").LoadingOverlay("hide");
        Swal.fire("Error!", "No se encontraron coincidencias.", "warning");
    });
}

$("#btnAgendar").on("click", function () {
    if (FechaSeleccionadaTexto == "" || IdHoraSeleccionado == 0) {
        Swal.fire("Error!", "Falta seleccionar la fecha o la hora.", "warning");
        return;
    }

    let objeto = {
        DoctorHorarioDetalle: { IdDoctorHorarioDetalle: IdHoraSeleccionado },
        EstadoCita: { IdEstadoCita: 1 },
        FechaCita: moment(FechaSeleccionadaTexto, "DD/MM/YYYY").format('DD/MM/YYYY')
    };

    fetch(`/Citas/Guardar`, {
        method: "POST",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(objeto)
    }).then(response => {
        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {
        if (responseJson.data == "") {
            Swal.fire("¡Felicidades!", "Su cita fue registrada!", "success").then(function () {
                window.location.href = '/Citas/Index';
            });
            $(`#${modal}`).modal('hide');
        } else {
            Swal.fire("Error!", responseJson.data, "warning");
        }
    }).catch((error) => {
        Swal.fire("Error!", "No se pudo registrar.", "warning");
    });
});