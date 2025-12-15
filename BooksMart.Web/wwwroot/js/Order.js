var dataTable;
$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("inprocess")) {
        loadDataTable("inprocess");
    }
    else {
        if (url.includes("completed")) {
            loadDataTable("completed");
        }
        else {
            if (url.includes("pending")) {
                loadDataTable("pending");
            }
            else {
                if (url.includes("approved")) {
                    loadDataTable("approved");
                }
                else {
                    loadDataTable("all");
                }
            }
        }
    }
});

function loadDataTable(status) {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/Order/GetAllOrders?status=' + status },
        "columns": [
            { data: 'id', "width":"25px" },
            { data: 'name', "width": "15px" },
            { data: 'applicationUser.email', "width": "10px" },
            { data: 'phoneNumber', "width": "15px" },
            { data: 'orderStatus', "width": "10px" },
            { data: 'orderTotal', "width": "10px" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                    <a href="/admin/order/details?orderId=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i></a>
                    </div>`
                },
                "width": "25px"
            },
        ]
    });
}