document.addEventListener('DOMContentLoaded', function () {
    // Reduced loading animation
    const cards = document.querySelectorAll('.dashboard-card');
    cards.forEach((card, index) => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(20px)'; // Smaller initial translation

        setTimeout(() => {
            card.style.transition = 'all 0.5s ease-out'; // Faster and simpler
            card.style.opacity = '1';
            card.style.transform = 'translateY(0)';
        }, index * 50); // Faster staggered delay
    });

    // Reduced click effects
    const buttons = document.querySelectorAll('.btn-enhanced, .btn-secondary-enhanced');
    buttons.forEach(button => {
        button.addEventListener('click', function (e) {
            const ripple = document.createElement('span');
            const rect = this.getBoundingClientRect();
            const size = Math.max(rect.width, rect.height);
            const x = e.clientX - rect.left - size / 2;
            const y = e.clientY - rect.top - size / 2;

            ripple.style.width = ripple.style.height = size + 'px';
            ripple.style.left = x + 'px';
            ripple.style.top = y + 'px';
            ripple.classList.add('ripple');

            this.appendChild(ripple);

            setTimeout(() => {
                ripple.remove();
            }, 500); // Faster ripple animation
        });
    });
});