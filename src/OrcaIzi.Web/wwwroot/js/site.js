// Navbar Scroll Effect
document.addEventListener('DOMContentLoaded', function() {
    const navbar = document.querySelector('.navbar');
    
    // Initial check
    if (window.scrollY > 50) {
        navbar.classList.add('bg-dark-glass');
        navbar.style.backgroundColor = 'rgba(0, 0, 0, 0.95)';
    } else {
        navbar.classList.remove('bg-dark-glass');
        navbar.style.backgroundColor = 'rgba(0, 0, 0, 0.0)';
    }

    window.addEventListener('scroll', function() {
        if (window.scrollY > 50) {
            navbar.style.backgroundColor = 'rgba(0, 0, 0, 0.95)';
            navbar.style.boxShadow = '0 2px 10px rgba(0,0,0,0.5)';
        } else {
            navbar.style.backgroundColor = 'rgba(0, 0, 0, 0.0)';
            navbar.style.boxShadow = 'none';
        }
    });

    // Fade In Animation on Scroll
    const observerOptions = {
        root: null,
        rootMargin: '0px',
        threshold: 0.1
    };

    const observer = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('visible');
                observer.unobserve(entry.target); // Only animate once
            }
        });
    }, observerOptions);

    const animatedElements = document.querySelectorAll('.fade-in-up');
    animatedElements.forEach(el => observer.observe(el));
});
