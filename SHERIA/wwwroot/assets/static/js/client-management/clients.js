$(document).ready(function () {
    App.init();

    //TableManageResponsive.init();

    var InitiateEditableDataTable = function () {
        return {
            init: function () {
                //Datatable Initiating
                var oTable = $('#editabledatatable').dataTable({
                    "responsive": true,
                    "createdRow": function (row, data, dataIndex) {
                        $(row).attr("recid", data.id);
                    },
                    "aoColumns": [
                        { "data": "first_name", "autoWidth": true, "sDefaultContent": "n/a" },
                        { "data": "last_name", "autoWidth": true, "sDefaultContent": "n/a" },
                        { "data": "phone_number", "autoWidth": true, "sDefaultContent": "n/a" },
                        { "data": "email", "autoWidth": true, "sDefaultContent": "n/a" },
                        { "data": "sex", "autoWidth": true, "sDefaultContent": "n/a" },
                        { "data": "nationality", "autoWidth": true, "sDefaultContent": "n/a" },

                        {
                            "bSortable": false,
                            "sDefaultContent": "<a href='#' class='btn btn-success btn-xs view'><i class='fa fa-eye'></i> View</a>"
                        },
                        {
                            "bSortable": false,
                            "sDefaultContent": "<a href='#' class='btn btn-info btn-xs edit'><i class='fa fa-edit'></i> Edit</a>"
                        },
                        {
                            "bSortable": false,
                            "sDefaultContent": "<a href='#' class='btn btn-danger btn-xs delete'><i class='fas fa-trash-alt'></i> Delete</a>"
                        }
                    ],

                   

                });

                var isViewing = null;
                var isEditing = null;

                //View
                $('#editabledatatable').on("click", 'a.view', function (e) {
                    e.preventDefault();

                    nRow = $(this).parents('tr')[0];

                    if (isViewing !== null && isViewing != nRow) {
                        //restoreRow(oTable, isViewing);
                        viewRow(oTable, nRow);
                        isViewing = nRow;
                    } else {
                        viewRow(oTable, nRow);
                        isViewing = nRow;
                    }
                });

                function viewRow(oTable, nRow) {
                    var aData = oTable.fnGetData(nRow);
                    var jqTds = $('>td', nRow);

                    var json = JSON.parse(JSON.stringify(aData));


                    $('.modal-body #applicantrecordid').val($(nRow).attr("recid"));
                    $('.modal-body #first_name').val(json["first_name"]);
                    $('.modal-body #last_name').val(json["last_name"]);
                    $('.modal-body #phone_number').val(json["phone_number"]);
                    $('.modal-body #email').val(json["email"]);
                    $('.modal-body #id_number').val(json["id_number"]);
                    $('.modal-body #sex').val(json["sex"]);
                    $('.modal-body #occupation').val(json["occupation"]);
                    $('.modal-body #nationality').val(json["nationality"]);
                    $('.modal-body #physical_address').val(json["physical_address"]);
                    $('.modal-body #next_of_kin_name').val(json["next_of_kin_name"]);
                    $('.modal-body #next_of_kin_phone_number').val(json["next_of_kin_phone_number"]);
                    $('.modal-body #date_of_birth').val(json["date_of_birth"]);
                    $('.modal-body #remarks').val(json["remarks"]);
                    $("#view-clients").appendTo("body").modal("show");
                }
                var isViewing = null;

                //Edit
                $('#editabledatatable').on("click", 'a.edit', function (e) {
                    e.preventDefault();

                    nRow = $(this).parents('tr')[0];

                    if (isEditing !== null && isEditing != nRow) {
                        //restoreRow(oTable, isEditing);
                        editRow(oTable, nRow);
                        isEditing = nRow;
                    } else {
                        editRow(oTable, nRow);
                        isEditing = nRow;
                    }
                });

                function editRow(oTable, nRow) {
                    var aData = oTable.fnGetData(nRow);
                    var jqTds = $('>td', nRow);

                    var json = JSON.parse(JSON.stringify(aData));


                    $('.modal-body #applicantrecordid').val($(nRow).attr("recid"));
                    $('.modal-body #first_name').val(json["first_name"]);
                    $('.modal-body #last_name').val(json["last_name"]);
                    $('.modal-body #phone_number').val(json["phone_number"]);
                    $('.modal-body #email').val(json["email"]);
                    $('.modal-body #id_number').val(json["id_number"]);
                    $('.modal-body #sex').val(json["sex"]);
                    $('.modal-body #occupation').val(json["occupation"]);
                    $('.modal-body #nationality').val(json["nationality"]);
                    $('.modal-body #physical_address').val(json["physical_address"]);
                    $('.modal-body #next_of_kin_name').val(json["next_of_kin_name"]);
                    $('.modal-body #next_of_kin_phone_number').val(json["next_of_kin_phone_number"]);
                    $('.modal-body #date_of_birth').val(json["date_of_birth"]);
                    $('.modal-body #remarks').val(json["remarks"]);
                    $("#capture-clients").appendTo("body").modal("show");
                }

                //Delete an Existing Row
                $('#editabledatatable').on("click", 'a.delete', function (e) {
                    e.preventDefault();

                    var a = $(this).closest(".panel");

                    var nRow = $(this).parents('tr')[0];

                    var rec = $(this).parents('tr').attr("recid");

                    //console.log($(this).parents('tr').attr("recid"));
                    Swal.fire({
                        title: "Are you sure?",
                        text: "You want to delete this record",
                        icon: "question",
                        showCancelButton: true,
                        confirmButtonText: "Proceed!",
                        reverseButtons: true
                    }).then((result) => {
                        if (result.isConfirmed) {

                            oTable.fnDeleteRow(nRow);
                            //Ajax to flag as deleted
                            var parameters = { module: 'client_record', id: rec };
                            console.log(parameters);
                            $.ajax({
                                url: "/Client/Delete",
                                type: "GET",
                                data: parameters,
                                beforeSend: function () {
                                    if (!$(a).hasClass("panel-loading")) {
                                        var t = $(a).find(".panel-body"),
                                            i = '<div class="panel-loader"><span class="spinner-small"></span></div>';

                                        $(a).addClass("panel-loading"), $(t).prepend(i);
                                    }
                                },


                                success: function (data) {
                                    $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();

                                    Swal.fire({
                                        title: "Deleted",
                                        text: "Record has been deleted",
                                        icon: "success",
                                        confirmButtonText: "Ok"
                                    });
                                    GetClients();
                                },
                                error: function (xhr, textStatus, errorThrown) {
                                    $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();

                                    Swal.fire({
                                        title: "Failed",
                                        text: "Operation could not be completed " + errorThrown,
                                        icon: "error",
                                        confirmButtonText: "Ok"
                                    });
                                }
                            });
                        } else {
                            e.preventDefault();
                        }
                    });
                });
            }
        };
    }();

    InitiateEditableDataTable.init();

    GetClients();

});


function GetClients() {
    $.get('GetRecords', { module: 'client_record' }, function (data) {
        getData(data);
    });
}

function getData(jsonstring) {
    table = $('#editabledatatable').dataTable();
    oSettings = table.fnSettings();
    table.fnClearTable(this);

    var json = $.parseJSON(JSON.stringify(jsonstring));
    //var json = JSON.parse(jsonstring);
    for (var i = 0; i < json.length; i++) {
        var item = json[i];
        table.oApi._fnAddData(oSettings, item);
    }
    oSettings.aiDisplay = oSettings.aiDisplayMaster.slice();
    table.fnDraw();
}

$('#save').click(function () {
    var a = $(this).closest(".panel");

    var id = document.getElementById('applicantrecordid').value;
    var first_name = document.getElementById('first_name').value;
    var last_name = document.getElementById('last_name').value;
    var phone_number = document.getElementById('phone_number').value;
    var email = document.getElementById('email').value;
    var id_number = document.getElementById('id_number').value;
    var sex = document.getElementById('sex').value;
    var occupation = document.getElementById('occupation').value;
    var nationality = document.getElementById('nationality').value;
    var physical_address = document.getElementById('physical_address').value;
    var next_of_kin_name = document.getElementById('next_of_kin_name').value;
    var next_of_kin_phone_number = document.getElementById('next_of_kin_phone_number').value;
    var date_of_birth = document.getElementById('date_of_birth').value;
    var remarks = document.getElementById('remarks').value;

    var parameters = {
        id: id,
        first_name: first_name,
        last_name: last_name,
        phone_number: phone_number,
        email: email,
        id_number: id_number,
        sex: sex,
        occupation: occupation,
        nationality: nationality,
        physical_address: physical_address,
        next_of_kin_name: next_of_kin_name,
        next_of_kin_phone_number: next_of_kin_phone_number,
        date_of_birth: date_of_birth,
        remarks: remarks
    };

    console.log(parameters);

    $.ajax({
        url: "/Client/Client",
        type: "POST",
        data: parameters,
        beforeSend: function () {
            if (!$(a).hasClass("panel-loading")) {
                var t = $(a).find(".panel-body"),
                    i = '<div class="panel-loader"><span class="spinner-small"></span></div>';

                $(a).addClass("panel-loading"), $(t).prepend(i);
            }
        },
        success: function (data) {
            $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();

            if (data.error_code === '00') {
                Swal.fire({
                    title: "Success",
                    text: data.error_desc,
                    icon: "success",
                    confirmButtonText: "Ok"
                });

                $("#capture-tasks").modal("hide").data("bs.modal", null);

                GetClients();

            } else {
                Swal.fire({
                    title: "Failed",
                    text: data.error_desc,
                    icon: "error",
                    confirmButtonText: "Ok"
                });
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();

            Swal.fire({
                title: "Failed",
                text: "Record could not be saved " + errorThrown,
                icon: "error",
                confirmButtonText: "Ok"
            });
        }
    });
});

$("#capture-clients").on("hidden.bs.modal", function (e) {
    $('#applicantrecordid').val("");
    $('#first_name').val("");
    $('#last_name').val("");
    $('#phone_number').val("");
    $('#email').val("");
    $('#id_number').val("");
    $('#sex').val("");
    $('#occupation').val("");
    $('#nationality').val("");
    $('#physical_address').val("");
    $('#next_of_kin_name').val("");
    $('#next_of_kin_phone_number').val("");
    $('#date_of_birth').val("");
    $('#remarks').val("");
});

$('#date_of_birth').datepicker({
    todayHighlight: true,
    startDate: '',
    //endDate: '0',
    format: 'yyyy-mm-dd',
    changeMonth: true,
    changeYear: true,
    autoclose: true,
    todayBtn: 'linked'
});