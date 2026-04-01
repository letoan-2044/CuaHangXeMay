$(document).ready(function () {
    // 🔥 1. SIDEBAR TOGGLE
    const sidebarToggle = $('#sidebarToggle');
    const sidebar = $('#sidebar');
    const body = $('body');
    const mainWrapper = $('.main-wrapper');

    sidebarToggle.click(function (e) {
        e.stopPropagation();
        body.toggleClass('sidebar-collapsed');

        // Rotate icon
        const icon = $(this).find('i');
        icon.toggleClass('fa-bars fa-times rotate-180');

        // Update tooltip
        updateTooltip();
    });

    // 🔥 2. MOBILE SIDEBAR
    $(document).on('click', function (e) {
        if ($(window).width() < 1200) {
            if (!$(e.target).closest('.sidebar, .sidebar-toggle').length) {
                sidebar.removeClass('show');
                body.removeClass('sidebar-collapsed');
            }
        }
    });

    // 🔥 3. SUBMENU TOGGLE
    $('.nav-link').click(function (e) {
        if ($(this).next('.submenu').length) {
            e.preventDefault();
            e.stopPropagation();

            const parent = $(this).parent();
            const submenu = $(this).next('.submenu');

            // Close others
            $('.nav-item').not(parent).removeClass('show');
            $('.nav-item').not(parent).find('.submenu').slideUp(200);

            // Toggle current
            parent.toggleClass('show');
            submenu.slideToggle(300);
        }
    });

    // 🔥 4. PERFECT SCROLLBAR
    if (typeof OverlayScrollbars !== 'undefined') {
        OverlayScrollbars(document.querySelector('#sidebar'), {
            scrollbars: {
                theme: 'os-theme-dark',
                autoHide: 'scroll',
                autoHideDelay: 800
            },
            overflow: {
                overflowedX: false
            }
        });
    }

    // 🔥 5. PAGE LOADER
    window.addEventListener('load', function () {
        $('#loader').fadeOut(500);
    });

    // 🔥 6. ACTIVE MENU BASED ON URL
    const currentPath = window.location.pathname;
    $('.nav-link').each(function () {
        const href = $(this).attr('href');
        if (href === currentPath || $(this).find('a').attr('href') === currentPath) {
            $(this).addClass('active');
            $(this).parent().addClass('show');
        }
    });

    // 🔥 7. TOOLTIPS & POPOVERS
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // 🔥 8. SIDEBAR COLLAPSE ON MOBILE
    $(window).resize(function () {
        if ($(window).width() > 1200) {
            body.removeClass('sidebar-collapsed');
            sidebar.removeClass('show');
        }
    });

    // 🔥 9. NOTIFICATION DISMISS
    $('.notification').each(function () {
        const notification = $(this);
        setTimeout(function () {
            notification.fadeOut(500);
        }, 5000);
    });

    // 🔥 10. CHART INIT (Dashboard)
    initCharts();

    // 🔥 11. DARK MODE TOGGLE (Optional)
    $('.dark-mode-toggle').click(function () {
        $('body').toggleClass('dark-mode');
    });
});

// 🔥 CHART FUNCTIONS
function initCharts() {
    // Sample chart for dashboard
    const ctx = document.getElementById('salesChart');
    if (ctx && typeof Chart !== 'undefined') {
        new Chart(ctx, {
            type: 'line',
            data: {
                labels: ['Th1', 'Th2', 'Th3', 'Th4', 'Th5', 'Th6'],
                datasets: [{
                    label: 'Doanh thu',
                    data: [12000000, 19000000, 15000000, 28000000, 22000000, 35000000],
                    borderColor: '#3b82f6',
                    backgroundColor: 'rgba(59, 130, 246, 0.1)',
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: {
                        position: 'top',
                    }
                }
            }
        });
    }
}

// 🔥 HELPER FUNCTIONS
function updateTooltip() {
    const toggle = $('#sidebarToggle');
    const body = $('body');
    if (body.hasClass('sidebar-collapsed')) {
        toggle.attr('data-bs-title', 'Mở sidebar');
    } else {
        toggle.attr('data-bs-title', 'Thu gọn sidebar');
    }
}

// 🔥 AJAX ERROR HANDLER
$(document).ajaxError(function (event, xhr, settings, error) {
    console.error('AJAX Error:', error);
    showToast('Có lỗi xảy ra! Vui lòng thử lại.', 'error');
});

// 🔥 TOAST NOTIFICATION
function showToast(message, type = 'success') {
    const toastHtml = `
        <div class="toast align-items-center text-white bg-${type === 'error' ? 'danger' : type} border-0 position-fixed shadow-lg"
             style="top: 20px; right: 20px; z-index: 1055; min-width: 300px;" role="alert">
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;
    $('body').append(toastHtml);

    const toastEl = $('.toast:last');
    const toast = new bootstrap.Toast(toastEl[0]);
    toast.show();

    toastEl.on('hidden.bs.toast', function () {
        toastEl.remove();
    });
}

// 🔥 AUTO HIDE ALERTS
setTimeout(function () {
    $('.alert').each(function () {
        $(this).fadeOut(500);
    });
}, 5000);

// 🔥 UTILITY FUNCTIONS
// Success toast
function showSuccess(message) {
    showToast(message, 'success');
}

// Error toast
function showError(message) {
    showToast(message, 'error');
}

// Warning toast
function showWarning(message) {
    showToast(message, 'warning');
}

// Info toast
function showInfo(message) {
    showToast(message, 'info');
}

// 🔥 SIDEBAR UTILITIES
function toggleSidebar() {
    $('body').toggleClass('sidebar-collapsed');
    updateTooltip();
}

function openSidebar() {
    $('body').removeClass('sidebar-collapsed');
}

function closeSidebar() {
    $('body').addClass('sidebar-collapsed');
}

// 🔥 EXPORT FUNCTIONS FOR GLOBAL USE
window.AdminLTE = {
    initCharts: initCharts,
    showToast: showToast,
    showSuccess: showSuccess,
    showError: showError,
    showWarning: showWarning,
    showInfo: showInfo,
    toggleSidebar: toggleSidebar,
    openSidebar: openSidebar,
    closeSidebar: closeSidebar
};