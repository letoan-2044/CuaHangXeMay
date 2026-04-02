// 🔥 XE MÁY VN - PURE CSS/JS DROPDOWN (FINAL VERSION)
(function () {
    'use strict';

    let ticking = false;
    let isDropdownOpen = false;

    function initPureDropdown() {
        const toggle = document.getElementById('userDropdownPure');
        const menu = toggle?.parentElement.querySelector('.dropdown-menu');

        if (!toggle || !menu) {
            console.log('⚠️ Dropdown elements not found');
            return;
        }

        toggle.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            isDropdownOpen = !isDropdownOpen;

            if (isDropdownOpen) {
                menu.classList.add('show');
                menu.style.display = 'block';
                menu.style.opacity = '1';
                menu.style.visibility = 'visible';
                menu.style.transform = 'translateY(0)';
                document.body.classList.add('dropdown-open');
            } else {
                menu.classList.remove('show');
                menu.style.display = 'none';
                document.body.classList.remove('dropdown-open');
            }
        });

        document.addEventListener('click', function (e) {
            if (!toggle.contains(e.target) && !menu.contains(e.target)) {
                menu.classList.remove('show');
                menu.style.display = 'none';
                isDropdownOpen = false;
                document.body.classList.remove('dropdown-open');
            }
        });
    }

    function initScroll() {
        const handleScroll = () => {
            const scrollTop = window.pageYOffset;
            const navbar = document.getElementById('mainNavbar');
            const scrollBtn = document.getElementById('scrollTopBtn');

            if (scrollTop > 50) {
                navbar?.classList.add('scroll-shrinked', 'navbar-scrolled');
            } else {
                navbar?.classList.remove('scroll-shrinked', 'navbar-scrolled');
            }

            if (scrollTop > 300) {
                scrollBtn?.classList.add('show');
            } else {
                scrollBtn?.classList.remove('show');
            }
            ticking = false;
        };

        window.addEventListener('scroll', () => {
            if (!ticking) {
                requestAnimationFrame(handleScroll);
                ticking = true;
            }
        }, { passive: true });
    }

    function initSearch() {
        const searchInput = document.getElementById('searchInput');
        if (searchInput) {
            searchInput.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    document.getElementById('searchForm')?.submit();
                }
            });
        }
    }

    function initScrollTop() {
        document.getElementById('scrollTopBtn')?.addEventListener('click', (e) => {
            e.preventDefault();
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });
    }

    function initNavActive() {
        document.querySelectorAll('.nav-link:not(.dropdown-toggle)').forEach(link => {
            link.addEventListener('click', function () {
                document.querySelectorAll('.nav-link').forEach(l => l.classList.remove('active-link'));
                this.classList.add('active-link');
            });
        });
    }

    // 🔥 INIT CHECKOUT ALERT
    function initAlerts() {
        // Đọc TempData từ data attribute (set từ Layout)
        const successMsg = document.body.dataset.successMsg;
        if (successMsg) {
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    icon: 'success',
                    title: '🎉 Thành công!',
                    text: successMsg,
                    timer: 2500,
                    timerProgressBar: true,
                    showConfirmButton: false
                });
            } else {
                alert(successMsg);
            }
        }
    }

    function init() {
        console.log('🚀 Xe Máy VN - Pure JS Loaded!');
        initPureDropdown();
        initScroll();
        initSearch();
        initScrollTop();
        initNavActive();
        initAlerts();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();