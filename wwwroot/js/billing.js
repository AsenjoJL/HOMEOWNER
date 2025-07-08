$(document).ready(function () {
    // Edit/Create Bill Modal
    $(document).on('click', '.edit-bill', function () {
        var billId = $(this).data('id');
        if (billId) {
            $.ajax({
                url: '/Billing/GetBillDetails',
                type: 'GET',
                data: { id: billId },
                success: function (data) {
                    $('#Amount').val(data.Amount);
                    $('#DueDate').val(data.DueDate);
                    $('#PaymentStatus').val(data.PaymentStatus);
                    $('#BillID').val(data.BillingID);
                    $('#billModal').modal('show');
                }
            });
        } else {
            $('#billForm')[0].reset();
            $('#billModal').modal('show');
        }
    });

    $('#billForm').submit(function (e) {
        e.preventDefault();
        var formData = $(this).serialize();
        var billId = $('#BillID').val();
        var url = billId ? '/Billing/EditBill' : '/Billing/CreateBill';

        $.ajax({
            url: url,
            type: 'POST',
            data: formData,
            success: function (data) {
                $('#billModal').modal('hide');
                location.reload();
            }
        });
    });

    // View Payment Details (Admin)
    $(document).on('click', '.view-payment', function () {
        var paymentId = $(this).data('id');
        $.ajax({
            url: '/Admin/GetPaymentDetails',
            type: 'GET',
            data: { id: paymentId },
            success: function (data) {
                $('#paymentHomeowner').text(data.Homeowner);
                $('#paymentAmount').text(data.AmountPaid);
                $('#paymentMethod').text(data.PaymentMethod);
                $('#paymentDate').text(data.PaymentDate);
                $('#paymentModal').modal('show');
            }
        });
    });
});
