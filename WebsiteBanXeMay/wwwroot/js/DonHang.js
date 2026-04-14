// 🔥 DONHANG.JS V3.2 - EXPORT ĐỘC LẬP + CHARTS + UPDATE
console.log('🚀 DONHANG.JS V3.2 LOADED');

class DonHangManager {
    constructor() {
        this.charts = {};
        this.lastStats = { stats: null, orders: [], filters: {} }; // ✅ FULL CACHE
        this.isLoading = false; // 🔒 Prevent double-click
        this.init();

        this.STATUS_CONFIG = {
            'Đã giao': 'badge-success',
            'Đang giao': 'badge-info',
            'Chờ xử lý': 'badge-warning',
            'Hủy': 'badge-danger'
        };
    }

    init() {
        console.log('🔗 Binding events...');
        this.bindEvents();
    }

    bindEvents() {
        // 📊 Stats
        $(document).off('click.btn-stats').on('click.btn-stats', '.btn-stats', (e) => {
            e.preventDefault();
            e.stopPropagation();
            this.loadStats();
        });

        // 📥 Export độc lập
        $(document).off('click.btn-export').on('click.btn-export', '.btn-export, .btn-export-dh', (e) => {
            e.preventDefault();
            e.stopPropagation();
            this.exportStats();
        });

        // 🔄 Update status
        $(document).off('click.btn-capnhat').on('click.btn-capnhat', '.btn-capnhat', (e) => {
            e.preventDefault();
            this.updateStatus($(e.currentTarget));
        });
    }

    /** 🔥 THỐNG KÊ DB REAL-TIME + CACHE */
    async loadStats() {
        if (this.isLoading) return;
        this.isLoading = true;

        console.log('📊 Loading DB stats...');
        const swal = Swal.fire({
            title: 'Đang tải thống kê...',
            allowOutsideClick: false,
            didOpen: () => Swal.showLoading()
        });

        try {
            const filters = this.getFilters();
            const response = await $.get('/DonHang/ThongKeDonHang', filters);

            // ✅ FULL CACHE: stats + orders + filters
            this.lastStats = {
                stats: this.calculateStatsFromApi(response),
                orders: response.orders || [],
                filters
            };

            this.displayStats(this.lastStats.stats);
            this.renderCharts(this.lastStats.stats, this.lastStats.orders);

            // ✅ Auto thêm nút export vào modal
            this.showExportButton();

            new bootstrap.Modal(document.getElementById('statsModal')).show();
            swal.close();
            Swal.fire('✅', `📊 ${response.totalOrders} đơn hàng`, 'success');
        } catch (error) {
            console.error('Stats error:', error);
            Swal.fire('❌', 'Lỗi tải thống kê!', 'error');
        } finally {
            this.isLoading = false;
        }
    }

    /** 🔥 GET FILTERS - REUSABLE */
    getFilters() {
        return {
            tuKhoa: $('input[name="tuKhoa"]').val()?.trim() || '',
            trangThai: $('select[name="trangThai"]').val() || ''
        };
    }

    calculateStatsFromApi(apiData) {
        const totalOrders = apiData.totalOrders || 0;
        const totalRevenue = apiData.totalRevenue || 0;
        const avgOrder = totalOrders ? Math.round(totalRevenue / totalOrders) : 0;
        const statusCount = apiData.statusCount || {};

        return {
            totalOrders,
            totalRevenue,
            avgOrder,
            statusCount,
            pendingOrders: statusCount['Chờ xử lý'] || 0,
            shippingOrders: statusCount['Đang giao'] || 0,
            completedOrders: statusCount['Đã giao'] || 0,
            cancelledOrders: statusCount['Hủy'] || 0,
            completionRate: totalOrders ? ((statusCount['Đã giao'] || 0) / totalOrders * 100).toFixed(1) : 0
        };
    }

    displayStats(stats) {
        $('#totalOrders').text(stats.totalOrders?.toLocaleString() || 0);
        $('#totalRevenue').text(this.formatCurrency(stats.totalRevenue));
        $('#pendingOrders').text(stats.pendingOrders);
        $('#shippingOrders').text(stats.shippingOrders);
        $('#completedOrders').text(stats.completedOrders);
        $('#cancelledOrders').text(stats.cancelledOrders);
        $('#avgOrder').text(this.formatCurrency(stats.avgOrder));
        $('#completionRate').text(stats.completionRate + '%');
        $('#completionBar').css('width', stats.completionRate + '%');
    }

    renderCharts(stats, orders) {
        this.destroyCharts();

        // Doughnut Chart - Status
        const ctx1 = document.getElementById('chartStatus')?.getContext('2d');
        if (ctx1) {
            this.charts.status = new Chart(ctx1, {
                type: 'doughnut',
                data: {
                    labels: Object.keys(stats.statusCount).filter(k => stats.statusCount[k] > 0),
                    datasets: [{
                        data: Object.values(stats.statusCount).filter(v => v > 0),
                        backgroundColor: ['#ffc107', '#17a2b8', '#28a745', '#dc3545', '#6c757d']
                    }]
                },
                options: {
                    responsive: true,
                    plugins: { legend: { position: 'bottom' } },
                    cutout: '60%'
                }
            });
        }

        // Line Chart - Doanh thu
        const ctx2 = document.getElementById('chartRevenue')?.getContext('2d');
        if (ctx2 && orders.length) {
            const daily = {};
            orders.slice(0, 30).forEach(o => { // Limit 30 days
                const date = o.ngayDat.split(' ')[0];
                daily[date] = (daily[date] || 0) + o.tongTien;
            });
            this.charts.revenue = new Chart(ctx2, {
                type: 'line',
                data: {
                    labels: Object.keys(daily),
                    datasets: [{
                        label: 'Doanh thu',
                        data: Object.values(daily),
                        borderColor: '#28a745',
                        backgroundColor: 'rgba(40, 167, 69, 0.1)',
                        tension: 0.4,
                        fill: true
                    }]
                },
                options: {
                    responsive: true,
                    scales: { y: { beginAtZero: true } }
                }
            });
        }
    }

    /** 🔥 EXPORT EXCEL - ĐỘC LẬP HOÀN TOÀN */
    async exportStats() {
        if (this.isLoading) return;
        if (!this.lastStats.orders?.length) {
            Swal.fire('⚠️', 'Chưa load thống kê! Nhấn "Thống kê" trước!', 'warning');
            return;
        }

        this.isLoading = true;
        console.log('📥 Exporting...', this.lastStats.orders.length, 'orders');

        const swal = Swal.fire({
            title: 'Đang xuất Excel...',
            allowOutsideClick: false,
            didOpen: () => Swal.showLoading()
        });

        try {
            const orders = this.lastStats.orders;
            await this.generateExcel(orders);
            swal.close();
            Swal.fire('✅', `Đã xuất ${orders.length} đơn hàng! 📥`, 'success');
        } catch (error) {
            console.error('Export error:', error);
            swal.close();
            Swal.fire('❌', 'Lỗi xuất file! ' + error.message, 'error');
        } finally {
            this.isLoading = false;
        }
    }

    /** 🔥 GENERATE EXCEL - PRO */
    async generateExcel(orders) {
        const wsData = orders.map((o, index) => ({
            'STT': index + 1,
            'Mã ĐH': o.maDH || '',
            'Khách hàng': o.tenKH || 'N/A',
            'SĐT': o.sdt || '',
            'Email': o.email || '',
            'Ngày đặt': o.ngayDat || '',
            'Tổng tiền': this.formatCurrency(o.tongTien || 0),
            'Trạng thái': o.trangThai || '',
            'Ghi chú': o.ghiChu || ''
        }));

        // Header info
        wsData.unshift(
            { 'STT': '', 'Mã ĐH': '', 'Khách hàng': '🔥 THỐNG KÊ ĐƠN HÀNG', 'SĐT': '', 'Email': '', 'Ngày đặt': '', 'Tổng tiền': '', 'Trạng thái': '', 'Ghi chú': '' },
            { 'STT': '', 'Mã ĐH': `Tổng: ${this.lastStats.orders.length} đơn`, 'Khách hàng': `Doanh thu: ${this.formatCurrency(this.lastStats.stats?.totalRevenue || 0)}`, 'SĐT': `Từ khóa: ${this.lastStats.filters.tuKhoa || 'Tất cả'}`, 'Email': `Trạng thái: ${this.lastStats.filters.trangThai || 'Tất cả'}`, 'Ngày đặt': '', 'Tổng tiền': '', 'Trạng thái': '', 'Ghi chú': '' }
        );

        const wb = XLSX.utils.book_new();
        const ws = XLSX.utils.json_to_sheet(wsData);

        // Column widths
        ws['!cols'] = [
            { wch: 5 }, { wch: 12 }, { wch: 20 }, { wch: 15 }, { wch: 25 },
            { wch: 15 }, { wch: 15 }, { wch: 12 }, { wch: 25 }
        ];

        XLSX.utils.book_append_sheet(wb, ws, "📋 Đơn hàng");

        // Smart filename
        const filterText = this.lastStats.filters.tuKhoa ? `_${this.lastStats.filters.tuKhoa}` : '_tatca';
        const statusText = this.lastStats.filters.trangThai ? `_${this.lastStats.filters.trangThai}` : '';
        const fileName = `ThongKeDonHang_${new Date().toISOString().slice(0, 10)}${filterText}${statusText}.xlsx`;

        XLSX.writeFile(wb, fileName);
    }

    /** 🔥 AUTO EXPORT BUTTON TRONG MODAL */
    showExportButton() {
        const $modalHeader = $('#statsModal .modal-header');
        let $exportBtn = $modalHeader.find('.btn-export-dh');

        if (this.lastStats.orders?.length && !$exportBtn.length) {
            $exportBtn = $(`
                <button class="btn btn-success btn-export-dh" style="position: absolute; top: 12px; right: 60px; z-index: 10;">
                    📥 Xuất Excel <span class="badge bg-light text-dark ms-1">${this.lastStats.orders.length}</span>
                </button>
            `);
            $modalHeader.append($exportBtn);
        } else if ($exportBtn.length) {
            $exportBtn.find('.badge').text(this.lastStats.orders.length);
            $exportBtn.toggle(this.lastStats.orders?.length > 0);
        }
    }

    destroyCharts() {
        Object.values(this.charts).forEach(chart => chart?.destroy());
        this.charts = {};
    }

    formatCurrency(num) {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(num).replace('₫', 'đ');
    }

    /** 🔥 UPDATE STATUS - FULL */
    async updateStatus($btn) {
        const maDH = $btn.data('id');
        const trangThai = $btn.data('trangthai');

        if (!maDH || !trangThai) return;

        const result = await Swal.fire({
            title: `🔄 Cập nhật trạng thái?`,
            html: `Đơn #${maDH} → <span class="fw-bold">${trangThai}</span>`,  
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Xác nhận',
            cancelButtonText: 'Hủy'
        });

        if (!result.isConfirmed) return;

        $btn.prop('disabled', true).html('⏳ Đang cập nhật...');

        try {
            const token = $('input[name="__RequestVerificationToken"]').val();
            const response = await $.ajax({
                url: '/DonHang/CapNhatTrangThai',
                method: 'POST',
                data: {
                    __RequestVerificationToken: token,
                    maDonHang: maDH,
                    trangThaiMoi: trangThai
                }
            });

            if (response.success) {
                Swal.fire('✅', response.message || 'Cập nhật thành công!', 'success');

                // Update UI
                $(`tr[data-dh="${maDH}"] .badge-trangthai`).text(trangThai).removeClass('badge-warning badge-info badge-success badge-danger')
                    .addClass(this.STATUS_CONFIG[trangThai] || 'badge-warning');

                // Reload stats nếu modal đang mở
                if ($('#statsModal').hasClass('show')) {
                    this.loadStats();
                }
            } else {
                Swal.fire('❌', response.message || 'Cập nhật thất bại!', 'error');
            }
        } catch (error) {
            console.error('Update error:', error);
            Swal.fire('❌', 'Lỗi server! Vui lòng thử lại.', 'error');
        } finally {
            $btn.prop('disabled', false).html('<i class="fas fa-sync-alt fa-spin"></i> Cập nhật');
            setTimeout(() => $btn.html('<i class="fas fa-truck"></i>'), 1000);
        }
    }

    /** 🧹 CLEANUP */
    destroy() {
        $(document).off('.donhang .btn-stats .btn-export .btn-capnhat');
        this.destroyCharts();
        $('#statsModal .btn-export-dh').remove();
        console.log('🧹 DonHangManager destroyed');
    }
}

// 🔥 GLOBAL INSTANCE + CLEANUP
let donHangManager = null;
$(document).ready(() => {
    if (donHangManager) donHangManager.destroy();
    window.donHangManager = donHangManager = new DonHangManager();
    console.log('✅ V3.2 READY - FULL FEATURE!');
});

// Cleanup
$(window).on('beforeunload', () => donHangManager?.destroy());