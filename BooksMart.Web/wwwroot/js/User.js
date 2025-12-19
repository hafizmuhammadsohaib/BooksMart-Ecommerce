var dataTable;
$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/User/GetAllUsers' },
        "columns": [
            { data: 'name', "width":"15px" },
            { data: 'email', "width": "15px" },
            { data: 'phoneNumber', "width": "15px" },
            { data: 'company.name', "width": "15px" },
            { data: 'role', "width": "15px" },
            {
                data: { id:"id", lockoutEnd:"lockoutEnd" },
                "render": function (data) {
                    var currentDate = new Date().getTime();
                    var lockoutEndDate = data.lockoutEnd
                        ? new Date(data.lockoutEnd).getTime()
                        : 0;
                    if (lockoutEndDate > currentDate) {
                        return `
                        <div class="text-center">
                         <a onClick=LockUnlockUser('${data.id}') class="btn btn-danger text-white" style="cursor:pointer; width:100px;">
                            <i class="bi bi-lock-fill"></i> Lock
                            </a>
                            
                            <a href="/Admin/User/ManageUserRoles?userId=${data.id}" class="btn btn-danger text-white" style="cursor:pointer; width:100px;">
                            <i class="bi bi-pencil-square"></i> Permission
                            </a>
                        </div>
                     `
                    }
                    else {
                        return `
                        <div class="text-center">
                           <a onClick=LockUnlockUser('${data.id}') class="btn btn-success text-white" style="cursor:pointer; width:100px;">
                            <i class="bi bi-unlock-fill"></i> UnLock
                            </a>
                            <a href="/Admin/User/ManageUserRoles?userId=${data.id}" class="btn btn-danger text-white" style="cursor:pointer; width:100px;">
                            <i class="bi bi-pencil-square"></i> Permission
                            </a>
                        </div>
                     `
                    }
                   
                },
                "width": "25px"
            },
        ]
    });
}

function LockUnlockUser(id) {
    $.ajax({
        type: "POST",
        url: '/Admin/User/LockUnlockUser',
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {
            toastr.success(data.message);
            dataTable.ajax.reload();
        }

    });
}