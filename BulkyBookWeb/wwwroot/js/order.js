var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Order/GetAll"
        },
        "columns": [
            { "data": "id", "width": "5%" },
            { "data": "name", "width": "15%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "applicationUser.email", "width": "15%" },
            { "data": "orderStatus", "width": "15%" },
            { "data": "orderTotal", "width": "10%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="btn-group w-100" role="group">
						    <a href="/Admin/Order/Details?orderId=${data}" class="btn btn-outline-primary mx-2">
							    <i class="bi bi-pencil-square"></i>Detail
						    </a>
					    </div>
                           `
                },
                "width": "15%"
            },
        ]
    });
}
