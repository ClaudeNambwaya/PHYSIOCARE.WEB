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
                    "columnDefs": [
                        {
                            "targets": 7,
                            "render": function (data, type, row, meta) {
                                if (row.matter_status === false) {
                                    return '<a href="#" class="btn btn-danger btn-xs flagclosed">Open</a>';
                                } else {
                                    return '<a href="#" class="btn btn-success btn-xs flagunOpen"> Closed</a>';
                                }
                            }
                        }
                    ],
                    "aoColumns": [
                        { "data": "matter_name", "autoWidth": true, "sDefaultContent": "n/a" },
                        { "data": "matter_number", "autoWidth": true, "sDefaultContent": "n/a" },
                        { "data": "assigned_to", "autoWidth": true, "sDefaultContent": "n/a" },
                        { "data": "client_name", "autoWidth": true, "sDefaultContent": "n/a" },
                        { "data": "start_date", "autoWidth": true, "sDefaultContent": "n/a" },
                        { "data": "close_date", "autoWidth": true, "sDefaultContent": "n/a" },
                        { "data": "practice_area", "autoWidth": true, "sDefaultContent": "n/a" },
                        {
                            "data": "matters_status",
                            "autoWidth": true,
                            "bSearchable": false,
                            "bSortable": false,
                            "sDefaultContent": "n/a"
                        },
                        {
                            "bSortable": false,
                            "sDefaultContent": "<a href='#' class='btn btn-danger btn-xs delete'><i class='fas fa-trash-alt'></i> Delete</a>"
                        }
                    ]
                });

                $('#editabledatatable').on("click", 'a.flagunOpen', function (e) {
                    e.preventDefault();

                    nRow = $(this).parents('tr')[0];

                    var aData = oTable.fnGetData(nRow);

                    var json = JSON.parse(JSON.stringify(aData));
                    console.log(json);

                    var matter_id = json["id"];

                    console.log(matter_id);

                    //ajax call to update debit_credit_note table - paid = 1

                    Swal.fire({
                        title: "Are you sure?",
                        text: "You want to Change Matters Status ?",
                        icon: "question",
                        showCancelButton: true,
                        confirmButtonText: "YES!",
                        reverseButtons: true
                    }).then((result) => {
                        if (result.isConfirmed) {
                            //$.blockUI();

                            oTable.fnDeleteRow(nRow);
                            //Ajax to flag as deleted
                            var parameters = { module: 'close_matter_status', id: matter_id };
                            $.ajax({
                                url: "/Matters/UpdateMatters",
                                type: "POST",
                                data: parameters,
                                success: function (data) {
                                    Swal.fire({
                                        title: "Confirmed",
                                        text: "Matter has been Closed",
                                        icon: "success",
                                        confirmButtonText: "Ok"
                                    });

                                    GetMattersRecords();
                                },
                                error: function (xhr, textStatus, errorThrown) {
                                    //$.unblockUI();

                                    Swal.fire({
                                        title: "Failed",
                                        text: "Matter could not be updated " + errorThrown,
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
                            var parameters = { module: 'open_matters', id: rec };
                            console.log(parameters);
                            $.ajax({
                                url: "/Matters/Delete",
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
                                    GetTopics();
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


    $('#start_date').datepicker({
        todayHighlight: true,
        startDate: '-6m',
        //endDate: '0',
        format: 'yyyy-mm-dd',
        changeMonth: true,
        changeYear: true,
        autoclose: true,
        todayBtn: 'linked'
    });

    $('#close_date').datepicker({
        todayHighlight: true,
        startDate: '-6m',
        //endDate: '0',
        format: 'yyyy-mm-dd',
        changeMonth: true,
        changeYear: true,
        autoclose: true,
        todayBtn: 'linked'
    });

    GetMattersRecords();

});


function GetMattersRecords() {
    $.get('GetRecords', { module: 'closed_matters_record' }, function (data) {
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

