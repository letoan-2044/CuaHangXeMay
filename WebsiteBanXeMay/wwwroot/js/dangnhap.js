document.addEventListener('DOMContentLoaded', function () {
    // 1. Toggle Password Visibility
    initPasswordToggle();

    // 2. Auto focus first input
    autoFocusFirstInput();

    // 3. Form validation & submit handling
    initFormValidation();

    // 4. Enter key submit
    initEnterSubmit();

    // 5. Success redirect
    handleSuccessRedirect();

    // 6. Forgot password modal (demo)
    initForgotPassword();
});

function initPasswordToggle() {
    document.querySelectorAll('.btn-toggle-password').forEach(btn => {
        btn.addEventListener('click', function () {
            const target = document.querySelector(this.dataset.target);
            const icon = this.querySelector('i');

            if (target.type === 'password') {
                target.type = 'text';
                icon.classList.replace('fa-eye', 'fa-eye-slash');
                this.title = 'Ẩn mật khẩu';
            } else {
                target.type = 'password';
                icon.classList.replace('fa-eye-slash', 'fa-eye');
                this.title = 'Hiện mật khẩu';
            }
        });
    });
}

function autoFocusFirstInput() {
    const firstInput = document.getElementById('TenDangNhap');
    if (firstInput) {
        firstInput.focus();
        firstInput.select();
    }
}

function initFormValidation() {
    const form = document.getElementById('loginForm');
    const inputs = form.querySelectorAll('input[required]');

    inputs.forEach(input => {
        input.addEventListener('blur', function () {
            validateField(this);
        });

        input.addEventListener('input', function () {
            validateField(this);
        });
    });

    function validateField(input) {
        const value = input.value.trim();
        const feedback = input.parentElement.querySelector('.invalid-feedback');

        if (value === '') {
            input.classList.remove('is-valid');
            input.classList.add('is-invalid');
            if (feedback) feedback.style.display = 'block';
        } else {
            input.classList.remove('is-invalid');
            input.classList.add('is-valid');
            if (feedback) feedback.style.display = 'none';
        }
    }
}

function initEnterSubmit() {
    const form = document.querySelector('form');
    form.addEventListener('keypress', function (e) {
        if (e.key === 'Enter' && !e.ctrlKey && !e.shiftKey) {
            e.preventDefault();
            form.submit();
        }
    });
}

function handleSuccessRedirect() {
    // Nếu có success message, auto redirect sau 2s
    const successAlert = document.querySelector('.alert-success');
    if (successAlert) {
        const vaiTro = '@TempData["VaiTro"]';
        setTimeout(() => {
            // Redirect theo vai trò
            const redirectUrls = {
                'admin': '@Url.Action("Dashboard", "Admin")',
                'manager': '@Url.Action("QuanLy", "QuanLy")',
                'nhanvien': '@Url.Action("NhanVien", "NhanVien")',
                'user': '@Url.Action("Index", "Home")'
            };

            const url = redirectUrls[vaiTro?.toLowerCase()] || '@Url.Action("Index", "Home")';
            window.location.href = url;
        }, 2000);
    }
}

function initForgotPassword() {
    const forgotLink = document.getElementById('forgotPassword');
    if (forgotLink) {
        forgotLink.addEventListener('click', function (e) {
            e.preventDefault();
            alert('Tính năng quên mật khẩu sẽ sớm có! Vui lòng liên hệ admin.');
        });
    }
}

// Form submit loading
document.getElementById('loginForm')?.addEventListener('submit', function () {
    const btn = document.getElementById('btnSubmit');
    const btnText = btn.querySelector('.btn-text');
    const btnLoading = btn.querySelector('.btn-loading');

    btn.disabled = true;
    btnText.classList.add('d-none');
    btnLoading.classList.remove('d-none');

    // Re-enable sau 5s nếu cần
    setTimeout(() => {
        btn.disabled = false;
        btnText.classList.remove('d-none');
        btnLoading.classList.add('d-none');
    }, 5000);
});

// Real-time validation cho username (demo)
document.getElementById('TenDangNhap')?.addEventListener('input', function () {
    const value = this.value.trim();
    if (value.length > 2) {
        // Simulate API check
        this.parentElement.querySelector('.valid-feedback').textContent = 'Tên đăng nhập có sẵn!';
    }
});