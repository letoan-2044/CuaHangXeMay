// ========================================
// GIỎ HÀNG - XE MÁY VN
// ========================================

document.addEventListener('DOMContentLoaded', function () {
    initGioHang();
    updateCartBadge();
});

function initGioHang() {
    // Quantity controls
    document.querySelectorAll('.qty-btn').forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            const maChiTietGio = parseInt(this.dataset.maChiTietGio);
            const change = this.dataset.change;
            const soLuongTon = parseInt(this.dataset.soLuongTon);
            updateQuantity(maChiTietGio, parseInt(change), soLuongTon);
        });
    });

    // Auto-submit on quantity change
    document.querySelectorAll('input[name="soLuong"]').forEach(input => {
        input.addEventListener('change', function () {
            const form = this.closest('form');
            setTimeout(() => form.submit(), 300);
        });
    });

    // Delete confirm
    document.querySelectorAll('.delete-btn').forEach(btn => {
        btn.addEventListener('click', function (e) {
            return confirm('🗑️ Xóa sản phẩm này khỏi giỏ hàng?\n\nHành động này không thể hoàn tác!');
        });
    });
}

function updateQuantity(maChiTietGio, change, soLuongTon) {
    // Tìm đúng input
    const input = document.querySelector(`input[value="${maChiTietGio}"]`).closest('form')
        .querySelector('input[name="soLuong"]');

    let current = parseInt(input.value) || 1;
    let newValue = current + change;

    // Giới hạn
    newValue = Math.max(1, Math.min(soLuongTon, newValue));

    // Update value
    input.value = newValue;

    // Animate
    input.animate([
        { transform: 'scale(1)', backgroundColor: '#0d6efd' },
        { transform: 'scale(1.15)', backgroundColor: '#ffffff' },
        { transform: 'scale(1)', backgroundColor: '#f8f9ff' }
    ], {
        duration: 200,
        easing: 'ease-out'
    });

    // Auto submit sau 500ms
    setTimeout(() => {
        input.closest('form').submit();
    }, 500);
}

// Real-time cart badge
async function updateCartBadge() {
    try {
        const response = await fetch('/GioHang/GetSoLuongGioHang');
        const soLuong = await response.json();

        const badge = document.getElementById('cartBadge');
        if (badge) {
            badge.textContent = soLuong;
            badge.classList.toggle('d-none', soLuong === 0);

            // Animate
            badge.classList.add('animate__animated', 'animate__pulse');
            setTimeout(() => {
                badge.classList.remove('animate__pulse');
            }, 1000);
        }
    } catch (error) {
        console.log('Cart badge update failed:', error);
    }
}

// Update mỗi 10s
setInterval(updateCartBadge, 10000);

// Scroll to top
window.addEventListener('scroll', function () {
    const scrollBtn = document.getElementById('scrollTopBtn');
    if (window.scrollY > 300) {
        scrollBtn.style.opacity = '1';
    } else {
        scrollBtn.style.opacity = '0';
    }
});