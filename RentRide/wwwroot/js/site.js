// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

$(document).ready(function () {
    if ($("#recent-users-content").length) {
        $.get("/Admin/RecentUsers", function (data) {
            $("#recent-users-content").html(data);
        });
    }

    const carContent = $("#admin-cars-content, #member-cars-content");
    if (carContent.length) {
        $.get("/Fleet/AvailableCars", function (data) {
            carContent.html(data);
        });
    }
});
