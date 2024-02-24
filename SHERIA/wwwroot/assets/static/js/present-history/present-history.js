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
                        /*{ "data": "client_id", "visible": false },*/
                        { "data": "full_name", "autoWidth": true, "sDefaultContent": "n/a" },
                        { "data": "present_history_date", "autoWidth": true, "sDefaultContent": "n/a" },

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

                    //console.log($(this).parents('tr').attr("recid"));

                    //console.log(nRow);

                    if (isViewing !== null && isViewing != nRow) {
                        restoreRow(oTable, isViewing);
                        viewRow(oTable, nRow);
                        isViewing = nRow;
                    } else {
                        viewRow(oTable, nRow);
                        isViewing = nRow;
                    }
                });

                function viewRow(oTable, nRow) {
                    var aData = oTable.fnGetData(nRow);
                    console.log("aData:", aData);

                    var jqTds = $('>td', nRow);
                    
                    

                    /*console.log(nRow);*/

                    var json = JSON.parse(JSON.stringify(aData));
                    console.log("json:", json);

                    /*console.log(aData);*/

                    $('.modal-body #recordid').val($(nRow).attr("recid"));
                    $('.modal-body #client_id').val(json["full_name"]);
                    $('.modal-body #present_history_date').val(json["present_history_date"]);
                    $('.modal-body #remarks').val(json["remarks"]);
                    $("#view-present-history").appendTo("body").modal("show");
                }
                


                //Edit
                $('#editabledatatable').on("click", 'a.edit', function (e) {
                    e.preventDefault();

                    nRow = $(this).parents('tr')[0];

                    //console.log($(this).parents('tr').attr("recid"));

                    //console.log(nRow);

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

                    console.log(aData);

                    $('.modal-body #recordid').val($(nRow).attr("recid"));
                    $('.modal-body #client_id').val(json["client_id"]);
                    $('.modal-body #present_history_date').val(json["present_history_date"]);
                    $('.modal-body #remarks').val(json["remarks"]);
                    $("#capture-present-history").appendTo("body").modal("show");
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
                            var parameters = { module: 'present_history_record', id: rec };
                            console.log(parameters);
                            $.ajax({
                                url: "/PresentHistory/Delete",
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
                                    GetPresentHistory();
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

    GetPresentHistory();
    GetClient();
});


function GetPresentHistory() {
    $.get('GetRecords', { module: 'present_history_record' }, function (data) {
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


function GetClient() {
    $.get('GetRecords', { module: 'client_record' }, function (data) {
        $("#client_id").get(0).options.length = 0;
        $("#client_id").get(0).options[0] = new Option("Please Select Client ", "-1");

        $.each(data, function (index, item) {
            // Assuming 'first_name' and 'last_name' are returned in the data
            $("#client_id").get(0).options[$("#client_id").get(0).options.length] = new Option(item.first_name + ' ' + item.last_name, item.id);
        });

        $("#client_id").bind("change", function () {
            // Your change event logic here
        });
    });
}



$('#save').click(function () {
    var a = $(this).closest(".panel");

    var id = document.getElementById('recordid').value;
    var client_id = document.getElementById('client_id').value;
    var present_history_date = document.getElementById('present_history_date').value;
    var remarks = document.getElementById('remarks').value;

    var parameters = {
        id: id,
        client_id: client_id,
        present_history_date: present_history_date,
        remarks: remarks
    };

    console.log(parameters);

    $.ajax({
        url: "/PresentHistory/CreatePresentHistory",
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

                $("#capture-present-history").modal("hide").data("bs.modal", null);

                GetPresentHistory();

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

$("#capture-present-history").on("hidden.bs.modal", function (e) {
    $('#recordid').val("");
    $('#client_id').val("");
    $('#patient_complain_date').val("");
    $('#remarks').val("");
});

$('#present_history_date').datepicker({
    todayHighlight: true,
    startDate: '',
    //endDate: '0',
    format: 'yyyy-mm-dd',
    changeMonth: true,
    changeYear: true,
    autoclose: true,
    todayBtn: 'linked'
});