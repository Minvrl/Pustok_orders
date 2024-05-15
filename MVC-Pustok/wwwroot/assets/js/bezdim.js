$(document).ready(function () {
    $(".order-detail-view").click(function (e) {
        e.preventDefault();
        let url = this.getAttribute("href");

        fetch(url)
            .then(response => response.text())
            .then(data => {
                $("#orderDetailModal .modal-dialog").html(data)
            })

        $("#orderDetailModal").modal('show');
    })


})