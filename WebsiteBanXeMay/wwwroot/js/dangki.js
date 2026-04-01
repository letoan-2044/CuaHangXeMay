// wwwroot/js/dangki.js
document.addEventListener('DOMContentLoaded', function () {
    // Toggle password
    document.querySelectorAll('.btn-toggle-password').forEach(btn => {
        btn.addEventListener('click', function () {
            const targetId = this.dataset.target;
            const input = document.querySelector(targetId);
            const icon = this.querySelector('i');

            if (input.type === 'password') {
                input.type = 'text';
                icon.classList.replace('fa-eye', 'fa-eye-slash');
            } else {
                input.type = 'password';
                icon.classList.replace('fa-eye-slash', 'fa-eye');
            }
        });
    });

    // Phone format
    const phoneInput = document.getElementById('SoDienThoai');
    if (phoneInput) {
        phoneInput.addEventListener('input', function (e) {
            let value = e.target.value.replace(/\D/g, '');
            if (value.length > 10) value = value.slice(0, 10);
            e.target.value = value;
        });
    }

    // Auto focus
    document.getElementById('TenDangNhap')?.focus();
});