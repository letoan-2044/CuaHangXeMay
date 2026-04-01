$(document).ready(function () {
    let ticking = false;

    // 🔥 CHỈ SHRINK USER INFO KHI SCROLL
    function handleScroll() {
        const scrollTop = $(window).scrollTop();
        const navbar = $('#mainNavbar');

        if (scrollTop > 50) {
            navbar.addClass('scroll-shrinked');
        } else {
            navbar.removeClass('scroll-shrinked');
        }

        // Scroll top button
        if (scrollTop > 300) {
            $('#scrollTopBtn').addClass('show');
        } else {
            $('#scrollTopBtn').removeClass('show');
        }

        ticking = false;
    }

    // Smooth performance
    $(window).on('scroll', function () {
        if (!ticking) {
            requestAnimationFrame(handleScroll);
            ticking = true;
        }
    });

    // Scroll top
    $('#scrollTopBtn').click(function (e) {
        e.preventDefault();
        $('html, body').animate({ scrollTop: 0 }, 800);
    });
});