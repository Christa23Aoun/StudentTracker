// ===== Mobile menu toggle =====
const menuBtn = document.querySelector('.menu-toggle');
const mobileMenu = document.getElementById('mobileMenu');

if (menuBtn && mobileMenu) {
    menuBtn.addEventListener('click', () => {
        mobileMenu.classList.toggle('active');
    });

    document.addEventListener('click', (e) => {
        if (mobileMenu.classList.contains('active') &&
            !mobileMenu.contains(e.target) &&
            !menuBtn.contains(e.target)) {
            mobileMenu.classList.remove('active');
        }
    });
}

// ===== News slider =====
(function () {
    const track = document.getElementById('newsTrack');
    const prev = document.querySelector('.news-prev');
    const next = document.querySelector('.news-next');
    if (!track || !prev || !next) return;

    const getStep = () => {
        const card = track.querySelector('.news-card');
        return card ? card.getBoundingClientRect().width + 16 : 320;
    };

    prev.addEventListener('click', () => {
        track.scrollBy({ left: -getStep(), behavior: 'smooth' });
    });
    next.addEventListener('click', () => {
        track.scrollBy({ left: getStep(), behavior: 'smooth' });
    });
})();

// ===== Stats count-up animation =====
(function () {
    const stats = document.querySelectorAll('.stat h3');
    let done = false;

    function animate() {
        if (done) return;
        const trigger = window.innerHeight * 0.85;
        const band = document.querySelector('.stats-band');
        if (!band) return;
        const top = band.getBoundingClientRect().top;
        if (top < trigger) {
            done = true;
            stats.forEach(el => {
                const target = +el.textContent.replace(/,/g, '');
                let count = 0;
                const step = Math.ceil(target / 100);
                const timer = setInterval(() => {
                    count += step;
                    if (count >= target) {
                        count = target;
                        clearInterval(timer);
                    }
                    el.textContent = count.toLocaleString();
                }, 15);
            });
        }
    }

    window.addEventListener('scroll', animate);
    animate();
})();
