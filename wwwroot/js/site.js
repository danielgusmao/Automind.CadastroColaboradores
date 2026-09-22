document.addEventListener('DOMContentLoaded', () => {
    const ticketForm = document.querySelector('[data-ticket-search]');
    const ticketInput = ticketForm?.querySelector('input[name="chamado"]');

    if (ticketInput) {
        ticketInput.addEventListener('input', () => {
            ticketInput.value = ticketInput.value.toUpperCase().replace(/\s+/g, '');
        });
    }

    document.querySelectorAll('.page-enter').forEach((element) => {
        requestAnimationFrame(() => element.classList.add('is-visible'));
    });
});
