// ========================================
// SANPHAM - Interactive JS
// ========================================

document.addEventListener('DOMContentLoaded', function () {
    // Quick view modal
    const quickViewButtons = document.querySelectorAll('[data-quick-view]');
    quickViewButtons.forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            const productId = this.dataset.productId;
            loadQuickView(productId);
        });
    });

    // Add to cart animation
    const addToCartForms = document.querySelectorAll('form[action*="AddToCart"]');
    addToCartForms.forEach(form => {
        form.addEventListener('submit', function (e) {
            const btn = this.querySelector('button[type="submit"]');
            addToCartAnimation(btn);
        });
    });

    // Image lazy load
    const images = document.querySelectorAll('img[data-src]');
    const imageObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                img.src = img.dataset.src;
                img.classList.remove('lazy');
                imageObserver.unobserve(img);
            }
        });
    });
    images.forEach(img => imageObserver.observe(img));
});

function addToCartAnimation(button) {
    const originalText = button.innerHTML;
    button.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Đang thêm...';
    button.disabled = true;

    setTimeout(() => {
        button.innerHTML = '<i class="fas fa-check me-2"></i>Đã thêm!';
        button.classList.add('btn-success');

        setTimeout(() => {
            button.innerHTML = originalText;
            button.disabled = false;
            button.classList.remove('btn-success');
        }, 1500);
    }, 1000);
}

function loadQuickView(productId) {
    // AJAX load product details
    fetch(`/SanPham/QuickView/${productId}`)
        .then(response => response.text())
        .then(html => {
            document.getElementById('quickview-modal-body').innerHTML = html;
            new bootstrap.Modal(document.getElementById('quickviewModal')).show();
        });
}