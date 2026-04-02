// 🔥 DONHANG.JS V2.4 - FIX MULTIPLE CONFIRM
console.log('🔥 DONHANG.JS V2.4 - SINGLE BIND');

var donHangManager = {
    capNhatTrangThai: async function (maDonHang, trangThaiMoi) {
        console.log(`🎯 SINGLE UPDATE: #${maDonHang} → ${trangThaiMoi}`);

        const $btn = $(`button[data-id="${maDonHang}"][data-trangthai="${trangThaiMoi}"]`);
        const $row = $(`tr[data-dh="${maDonHang}"]`);

        // ✅ DISABLE TẤT CẢ BUTTON CÙNG ĐƠN HÀNG
        $(`button[data-id="${maDonHang}"]`).prop('disabled', true);

        if (!confirm(`Cập nhật đơn #${maDonHang} → ${trangThaiMoi}?`)) {
            // Re-enable nếu cancel
            $(`button[data-id="${maDonHang}"]`).prop('disabled', false);
            return;
        }

        $btn.html('⏳ Đang cập nhật...');

        try {
            const token = $('input[name="__RequestVerificationToken"]').val();
            const response = await $.ajax({
                url: '/DonHang/CapNhatTrangThai',
                method: 'POST',
                data: {
                    __RequestVerificationToken: token,
                    maDonHang: parseInt(maDonHang),
                    trangThaiMoi: trangThaiMoi.trim()
                }
            });

            if (response.success) {
                alert('✅ ' + response.message);
                // Update UI ngay
                $row.find('.badge-trangthai').text(trangThaiMoi);
                $row.addClass('table-success');
                // Reload trang để sync
                setTimeout(() => location.reload(), 1000);
            } else {
                alert('❌ ' + response.message);
            }
        } catch (error) {
            console.error(error);
            alert('❌ Lỗi server!');
        } finally {
            // Re-enable
            $(`button[data-id="${maDonHang}"]`).prop('disabled', false);
            $btn.html('<i class="fas fa-truck"></i>'); // Reset icon
        }
    }
};

// 🔥 SINGLE BIND - KHÔNG LẶP!
$(document).ready(function () {
    console.log('🔗 SINGLE BIND EVENTS');

    // ✅ OFF TRƯỚC KHI ON - TRÁNH MULTIPLE!
    $(document).off('click.btn-capnhat').on('click.btn-capnhat', '.btn-capnhat', function (e) {
        e.preventDefault();
        e.stopImmediatePropagation(); // ✅ NGAN CHẶN PROPAGATION

        console.log('🖱️ SINGLE CLICK!');

        const $btn = $(this);
        const maDH = $btn.data('id');
        const trangThai = $btn.data('trangthai');

        donHangManager.capNhatTrangThai(maDH, trangThai);
        return false;
    });

    console.log('✅ SINGLE BIND COMPLETE');
});