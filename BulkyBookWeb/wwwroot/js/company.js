var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Company/GetAll"
        },
        "columns": [
            { "data": "name", "width": "12%" },
            { "data": "streetAddress", "width": "12%" },
            { "data": "city", "width": "12%" },
            { "data": "state", "width": "12%" },
            { "data": "postalCode", "width": "12%" },
            { "data": "phoneNumber", "width": "12%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="btn-group w-100" role="group">
						    <a href="/Admin/Company/Upsert?id=${data}" class="btn btn-outline-primary mx-2">
							    <i class="bi bi-pencil-square"></i> Edit
						    </a>
						    <a onClick=deleteProduct('/Admin/Company/Delete/${data}') class="btn btn-outline-danger mx-2">
							    <i class="bi bi-x-square"></i> Delete
						    </a>
					    </div>
                           `
                },
                "width": "25%"
            },
        ]
    });
}

function deleteProduct(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload();
                        toastr.success(data.message);
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}