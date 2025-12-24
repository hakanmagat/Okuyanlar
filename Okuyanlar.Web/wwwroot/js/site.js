// Okuyanlar Library Management System - Frontend JavaScript

// Toast Notification System
const OkToast = {
    container: null,

    init() {
        if (!this.container) {
            this.container = document.createElement('div');
            this.container.className = 'ok-toast-container';
            document.body.appendChild(this.container);
        }
    },

    show(message, type = 'info', title = '', duration = 4000) {
        this.init();

        const toast = document.createElement('div');
        toast.className = `ok-toast ${type}`;

        const icons = {
            success: '✓',
            error: '✕',
            warning: '⚠',
            info: 'ℹ'
        };

        toast.innerHTML = `
            <div class="ok-toast-icon">${icons[type] || icons.info}</div>
            <div class="ok-toast-content">
                ${title ? `<div class="ok-toast-title">${title}</div>` : ''}
                <div class="ok-toast-message">${message}</div>
            </div>
            <button class="ok-toast-close">×</button>
        `;

        this.container.appendChild(toast);

        const closeBtn = toast.querySelector('.ok-toast-close');
        closeBtn.addEventListener('click', () => this.remove(toast));

        if (duration > 0) {
            setTimeout(() => this.remove(toast), duration);
        }
    },

    remove(toast) {
        toast.classList.add('removing');
        setTimeout(() => {
            if (toast.parentNode) {
                toast.parentNode.removeChild(toast);
            }
        }, 300);
    },

    success(message, title = 'Success') {
        this.show(message, 'success', title);
    },

    error(message, title = 'Error') {
        this.show(message, 'error', title);
    },

    warning(message, title = 'Warning') {
        this.show(message, 'warning', title);
    },

    info(message, title = 'Info') {
        this.show(message, 'info', title);
    }
};

// Modal System
const OkModal = {
    create(options) {
        const { title, content, onConfirm, onCancel, confirmText = 'Confirm', cancelText = 'Cancel' } = options;

        const overlay = document.createElement('div');
        overlay.className = 'ok-modal-overlay';

        overlay.innerHTML = `
            <div class="ok-modal">
                <div class="ok-modal-header">
                    <h3 class="ok-modal-title">${title}</h3>
                    <button class="ok-modal-close" data-action="close">×</button>
                </div>
                <div class="ok-modal-body">${content}</div>
                <div class="ok-modal-footer">
                    ${cancelText ? `<button class="btn-ok-secondary" data-action="cancel">${cancelText}</button>` : ''}
                    <button class="btn-ok-primary" data-action="confirm">${confirmText}</button>
                </div>
            </div>
        `;

        document.body.appendChild(overlay);

        // Animate in
        setTimeout(() => overlay.classList.add('active'), 10);

        // Event handlers
        overlay.addEventListener('click', (e) => {
            if (e.target === overlay) {
                this.close(overlay, onCancel);
            }
        });

        overlay.querySelectorAll('[data-action]').forEach(btn => {
            btn.addEventListener('click', () => {
                const action = btn.dataset.action;
                if (action === 'confirm' && onConfirm) {
                    onConfirm();
                } else if (action === 'cancel' && onCancel) {
                    onCancel();
                }
                this.close(overlay);
            });
        });

        return overlay;
    },

    close(overlay, callback) {
        overlay.classList.remove('active');
        setTimeout(() => {
            if (overlay.parentNode) {
                overlay.parentNode.removeChild(overlay);
            }
            if (callback) callback();
        }, 300);
    },

    confirm(title, message, onConfirm, onCancel) {
        return this.create({
            title,
            content: `<p>${message}</p>`,
            onConfirm,
            onCancel,
            confirmText: 'Yes',
            cancelText: 'No'
        });
    }
};

// Star Rating Component
class StarRating {
    constructor(element, options = {}) {
        this.element = element;
        this.rating = options.rating || 0;
        this.readonly = options.readonly || false;
        this.onChange = options.onChange || null;
        this.render();
    }

    render() {
        this.element.innerHTML = '';
        this.element.className = 'ok-star-rating';

        for (let i = 1; i <= 5; i++) {
            const star = document.createElement('span');
            star.className = `ok-star ${i <= this.rating ? 'filled' : ''}`;
            star.textContent = '★';
            star.dataset.value = i;

            if (!this.readonly) {
                star.addEventListener('click', () => this.setRating(i));
                star.addEventListener('mouseenter', () => this.highlightStars(i));
                star.addEventListener('mouseleave', () => this.highlightStars(this.rating));
            }

            this.element.appendChild(star);
        }
    }

    setRating(value) {
        this.rating = value;
        this.highlightStars(value);
        if (this.onChange) {
            this.onChange(value);
        }
    }

    highlightStars(value) {
        const stars = this.element.querySelectorAll('.ok-star');
        stars.forEach((star, index) => {
            if (index < value) {
                star.classList.add('filled');
            } else {
                star.classList.remove('filled');
            }
        });
    }
}

// Form Validation Helper
const OkValidation = {
    validate(form) {
        let isValid = true;
        const inputs = form.querySelectorAll('[required]');

        inputs.forEach(input => {
            this.clearError(input);

            if (!input.value.trim()) {
                this.showError(input, 'This field is required');
                isValid = false;
            } else if (input.type === 'email' && !this.isValidEmail(input.value)) {
                this.showError(input, 'Please enter a valid email');
                isValid = false;
            }
        });

        return isValid;
    },

    isValidEmail(email) {
        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
    },

    showError(input, message) {
        input.classList.add('is-invalid');
        const error = document.createElement('div');
        error.className = 'text-danger small mt-1';
        error.textContent = message;
        input.parentNode.appendChild(error);
    },

    clearError(input) {
        input.classList.remove('is-invalid');
        const error = input.parentNode.querySelector('.text-danger');
        if (error) {
            error.remove();
        }
    }
};

// Loading Overlay
const OkLoading = {
    show() {
        if (document.getElementById('ok-loading')) return;

        const overlay = document.createElement('div');
        overlay.id = 'ok-loading';
        overlay.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(253, 247, 237, 0.9);
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 99999;
        `;
        overlay.innerHTML = '<div class="ok-loading-spinner"></div>';
        document.body.appendChild(overlay);
    },

    hide() {
        const overlay = document.getElementById('ok-loading');
        if (overlay) {
            overlay.remove();
        }
    }
};

// Search Debounce
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Initialize on DOM ready
document.addEventListener('DOMContentLoaded', function () {

    // Initialize all star ratings
    document.querySelectorAll('[data-star-rating]').forEach(el => {
        const rating = parseFloat(el.dataset.starRating) || 0;
        const readonly = el.dataset.readonly === 'true';
        new StarRating(el, {
            rating,
            readonly,
            onChange: (value) => {
                console.log('Rating changed:', value);
                // Could dispatch custom event here for form handling
            }
        });
    });

    // Confirm delete actions
    document.querySelectorAll('[data-confirm-delete]').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            const message = btn.dataset.confirmDelete || 'Are you sure you want to delete this item?';
            OkModal.confirm('Confirm Delete', message, () => {
                btn.closest('form')?.submit();
            });
        });
    });

    // Auto-hide alerts after 5 seconds
    document.querySelectorAll('.alert:not(.alert-permanent)').forEach(alert => {
        setTimeout(() => {
            alert.style.opacity = '0';
            alert.style.transition = 'opacity 0.3s';
            setTimeout(() => alert.remove(), 300);
        }, 5000);
    });

    // Search with debounce
    const searchInputs = document.querySelectorAll('[data-search-live]');
    searchInputs.forEach(input => {
        input.addEventListener('input', debounce((e) => {
            const form = input.closest('form');
            if (form) {
                form.submit();
            }
        }, 500));
    });

    // Show toast for TempData messages
    if (window.tempDataMessage) {
        const { message, type, title } = window.tempDataMessage;
        OkToast.show(message, type || 'info', title || '');
    }

    // Fade in animations for cards
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach((entry, index) => {
            if (entry.isIntersecting) {
                setTimeout(() => {
                    entry.target.style.opacity = '1';
                    entry.target.style.transform = 'translateY(0)';
                }, index * 50);
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    document.querySelectorAll('.ok-card, .ok-book-card, .ok-stat-card').forEach(card => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(20px)';
        card.style.transition = 'opacity 0.5s, transform 0.5s';
        observer.observe(card);
    });

    // Mobile menu toggle
    const navbarToggler = document.querySelector('.navbar-toggler');
    if (navbarToggler) {
        navbarToggler.addEventListener('click', () => {
            const navbar = document.querySelector('.navbar-collapse');
            navbar.classList.toggle('show');
        });
    }
});

// Export for use in other scripts
window.OkToast = OkToast;
window.OkModal = OkModal;
window.OkLoading = OkLoading;
window.OkValidation = OkValidation;
window.StarRating = StarRating;
