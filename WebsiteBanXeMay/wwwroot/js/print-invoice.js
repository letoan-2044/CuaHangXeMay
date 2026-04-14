// print-invoice.js
window.printInvoice = function (maDonHang, donHangData, chiTietGioHangs) {
    console.log('🖨️ In hóa đơn:', maDonHang);

    const statusColors = {
        'Chờ xử lý': '#ffc107',
        'Đang giao': '#fd7e14',
        'Đã giao': '#28a745',
        'Hủy': '#dc3545'
    };
    const color = statusColors[donHangData.trangThai] || '#6c757d';

    // Tạo HTML chi tiết sản phẩm
    let chiTietHTML = '';
    chiTietGioHangs.forEach(ct => {
        const thanhTien = (ct.soLuong * (ct.gia || 0)).toLocaleString('vi-VN');
        chiTietHTML += `
            <tr>
                <td style="border: 1px solid #ddd; padding: 12px;">${ct.tenSanPham || 'N/A'}</td>
                <td style="border: 1px solid #ddd; padding: 12px; text-align: center;">${ct.soLuong}</td>
                <td style="border: 1px solid #ddd; padding: 12px; text-align: right; font-weight: bold;">
                    ${thanhTien} VNĐ
                </td>
            </tr>`;
    });

    const hoaDonHTML = `
        <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; line-height: 1.4;">
            <div style="text-align: center; border-bottom: 3px solid #333; padding-bottom: 20px; margin-bottom: 25px;">
                <h2 style="color: #0d6efd; margin: 0 0 10px 0; font-size: 24px;">🏍️ CỬA HÀNG XE MÁY</h2>
                <p style="margin: 5px 0; font-size: 16px; font-weight: bold;">HÓA ĐƠN BÁN HÀNG</p>
                <p style="margin: 0; font-size: 14px;">Mã đơn: <strong>#${maDonHang}</strong></p>
                <p style="margin: 5px 0 0 0; font-size: 12px;">In ngày: ${new Date().toLocaleString('vi-VN')}</p>
            </div>
            <div style="margin-bottom: 25px; padding: 15px; background: #f8f9fa; border-radius: 8px;">
                <h4 style="margin: 0 0 15px 0;">📱 Thông tin khách hàng</h4>
                <p><strong>Điện thoại:</strong> ${donHangData.soDienThoai}</p>
                <p><strong>Địa chỉ:</strong> ${donHangData.diaChi}</p>
                <p><strong>Trạng thái:</strong> 
                    <span style="color: ${color}; font-weight: bold; padding: 4px 8px; background: rgba(255,255,255,0.8); border-radius: 4px;">
                        ${donHangData.trangThai}
                    </span>
                </p>
            </div>
            <table style="width: 100%; border-collapse: collapse; margin-bottom: 25px; font-size: 14px;">
                <thead>
                    <tr style="background: linear-gradient(135deg, #0d6efd, #6610f2); color: white;">
                        <th style="border: 1px solid #ddd; padding: 12px; text-align: left;">Sản phẩm</th>
                        <th style="border: 1px solid #ddd; padding: 12px; text-align: center; width: 80px;">SL</th>
                        <th style="border: 1px solid #ddd; padding: 12px; text-align: right; width: 120px;">Thành tiền</th>
                    </tr>
                </thead>
                <tbody>${chiTietHTML}</tbody>
            </table>
            <div style="border-top: 3px solid #333; padding-top: 20px; text-align: right;">
                <div style="font-size: 22px; font-weight: bold; color: #28a745;">
                    <span>TỔNG CỘNG:</span>
                    <span style="margin-left: 20px;">${donHangData.tongTien.toLocaleString('vi-VN')} VNĐ</span>
                </div>
            </div>
            <div style="text-align: center; margin-top: 30px; font-size: 12px; color: #666; border-top: 1px dashed #ccc; padding-top: 15px;">
                <p>Cảm ơn quý khách đã tin dùng dịch vụ! 🏍️</p>
                <p style="margin: 5px 0 0 0;">Liên hệ: 0123 456 789 | shopxemay@gmail.com</p>
            </div>
        </div>
    `;

    const printWin = window.open('', '_blank', 'width=850,height=1100,scrollbars=yes');
    printWin.document.write(`
        <!DOCTYPE html>
        <html>
        <head>
            <title>Hóa đơn #${maDonHang}</title>
            <style>
                body { margin: 0; padding: 10px; background: white; }
                @media print { body { margin: 0; padding: 0; } button { display: none !important; } }
            </style>
        </head>
        <body onload="setTimeout(() => window.print(), 500);">${hoaDonHTML}</body>
        </html>
    `);
    printWin.document.close();
};