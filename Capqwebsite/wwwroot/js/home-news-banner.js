(() => {
    const banner = document.querySelector('[data-news-banner]');
    if (!banner) return;
    const slides = Array.from(banner.querySelectorAll('.home-news-slide'));
    const fallbackImage = '/img/hero-fields-DJLmT-UN.jpg';
    banner.querySelectorAll('.home-news-image').forEach(image => {
        const fallback = () => {
            image.onerror = null;
            if (!image.src.endsWith(fallbackImage)) image.src = fallbackImage;
        };
        image.onerror = fallback;
        if (image.complete && !image.naturalWidth) fallback();
    });
    if (slides.length < 2) return;
    let current = 0;
    let timer;
    let hovering = false;
    const showNext = () => {
        slides[current].hidden = true;
        slides[current].classList.remove('is-entering');
        current = (current + 1) % slides.length;
        slides[current].hidden = false;
        slides[current].classList.add('is-entering');
    };
    const schedule = () => {
        window.clearInterval(timer);
        if (!hovering && !document.hidden && !banner.contains(document.activeElement)) {
            timer = window.setInterval(showNext, 5000);
        }
    };
    banner.addEventListener('mouseenter', () => { hovering = true; schedule(); });
    banner.addEventListener('mouseleave', () => { hovering = false; schedule(); });
    banner.addEventListener('focusin', schedule);
    banner.addEventListener('focusout', () => window.setTimeout(schedule, 0));
    document.addEventListener('visibilitychange', schedule);
    window.addEventListener('pagehide', () => window.clearInterval(timer));
    window.addEventListener('pageshow', schedule);
    schedule();
})();