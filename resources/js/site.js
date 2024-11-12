// Mostrar el contenedor de carga cuando la página comienza a recargarse
window.addEventListener('beforeunload', function () {
    document.getElementById('loading').style.display = 'flex';
});

// Ocultar el contenedor de carga cuando la página se haya cargado
window.addEventListener('load', function () {
    document.getElementById('loading').style.display = 'none';
});

// Constantes para el tamaño de fuente
const MIN_FONT_SIZE = 16;
const MAX_FONT_SIZE = 24;
let currentFontSize = parseInt(localStorage.getItem('fontSize')) || 16;

// Función auxiliar para ocultar todos los tooltips
function hideAllTooltips() {
    const tooltips = document.querySelectorAll('.barra-accesibilidad-govco button span');
    tooltips.forEach(tooltip => {
        tooltip.style.visibility = 'hidden';
        tooltip.style.opacity = '0';
        tooltip.style.display = 'none';
    });
}

// Función para cambiar el contraste
function cambiarContexto(event) {
    event.preventDefault();
    const body = document.body;
    body.classList.toggle('modo_oscuro-govco');

    // Asegurar que todos los elementos de texto sean visibles
    const textElements = document.querySelectorAll('h1, h2, h3, h4, h5, h6, p, span:not(.barra-accesibilidad-govco button span), div, label, input, textarea');
    textElements.forEach(element => {
        element.style.visibility = 'visible';
        element.style.opacity = '1';

        if (body.classList.contains('modo_oscuro-govco')) {
            element.style.color = '#fff';
        } else {
            element.style.color = '';
        }
    });

    // Manejar específicamente los contenedores de asesoramiento y detección
    const specialContainers = document.querySelectorAll('.cia-container, .ge-container, .cia-panel, .ge-panel');
    specialContainers.forEach(container => {
        container.style.visibility = 'visible';
        container.style.opacity = '1';
    });

    // Restaurar estilos específicos para botones de carga de archivos
    const fileUploadLabels = document.querySelectorAll('.ge-file-upload-label');
    fileUploadLabels.forEach(label => {
        if (body.classList.contains('modo_oscuro-govco')) {
            label.style.backgroundColor = '#1f1f1f';
            label.style.color = '#fff';
            label.style.border = '1px solid #fff';
        } else {
            label.style.backgroundColor = '#3498db';
            label.style.color = '#fff';
            label.style.border = 'none';
        }
    });

    // Guardar preferencia en localStorage
    localStorage.setItem('darkMode', body.classList.contains('modo_oscuro-govco'));

    // Asegurarse de ocultar todos los tooltips
    hideAllTooltips();
}

// Función para cambiar el tamaño de fuente
function cambiarTamanio(event, operador) {
    event.preventDefault();

    if (operador === 'aumentar' && currentFontSize < MAX_FONT_SIZE) {
        currentFontSize += 1;
    } else if (operador === 'disminuir' && currentFontSize > MIN_FONT_SIZE) {
        currentFontSize -= 1;
    }

    document.documentElement.style.fontSize = `${currentFontSize}px`;
    localStorage.setItem('fontSize', currentFontSize);

    // Ocultar los tooltips después del clic
    hideAllTooltips();
}

// Función para manejar el indicador de navegación
function handleIndicator(el) {
    if (!el) return;

    const indicator = document.querySelector('.nav-indicator');
    const items = document.querySelectorAll('.nav-item');

    items.forEach(item => {
        item.classList.remove('is-active');
        item.removeAttribute('style');
    });

    if (indicator) {
        indicator.style.width = `${el.offsetWidth}px`;
        indicator.style.left = `${el.offsetLeft}px`;
        indicator.style.backgroundColor = el.getAttribute('active-color');

        el.classList.add('is-active');
        el.style.color = el.getAttribute('active-color');
    }
}

// Función para establecer el elemento de menú activo
function setActiveMenuItem() {
    const currentPath = window.location.pathname.toLowerCase();
    const menuItems = document.querySelectorAll('.nav-item');

    menuItems.forEach(item => {
        const itemPath = item.getAttribute('href').split('/').pop().toLowerCase();
        const itemPathWithoutExtension = itemPath.replace('.aspx', '');

        if (currentPath.endsWith(itemPathWithoutExtension) ||
            (currentPath === '/' && itemPathWithoutExtension === 'index')) {
            handleIndicator(item);
        }
    });
}

// Función para manejar los tooltips de accesibilidad
function setupAccessibilityTooltips() {
    const buttons = document.querySelectorAll('.barra-accesibilidad-govco button');
    let activeTooltip = null;

    buttons.forEach(button => {
        const span = button.querySelector('span');
        if (!span) return;

        // Ocultar tooltip al inicio
        span.style.visibility = 'hidden';
        span.style.opacity = '0';
        span.style.display = 'none';

        // Mostrar tooltip al pasar el mouse
        button.addEventListener('mouseenter', () => {
            hideAllTooltips();
            span.style.visibility = 'visible';
            span.style.opacity = '1';
            span.style.display = 'block';
            activeTooltip = span;
        });

        // Ocultar tooltip al salir el mouse
        button.addEventListener('mouseleave', () => {
            span.style.visibility = 'hidden';
            span.style.opacity = '0';
            span.style.display = 'none';
            if (activeTooltip === span) {
                activeTooltip = null;
            }
        });

        // Ocultar tooltip después del clic
        button.addEventListener('click', () => {
            hideAllTooltips();
        });

        // Accesibilidad del teclado
        button.addEventListener('focus', () => {
            hideAllTooltips();
            span.style.visibility = 'visible';
            span.style.opacity = '1';
            span.style.display = 'block';
            activeTooltip = span;
        });

        button.addEventListener('blur', () => {
            span.style.visibility = 'hidden';
            span.style.opacity = '0';
            span.style.display = 'none';
            if (activeTooltip === span) {
                activeTooltip = null;
            }
        });

        button.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                button.click();
                hideAllTooltips();
            }
        });
    });

    // Cerrar tooltips al hacer clic fuera
    document.addEventListener('click', (e) => {
        if (!e.target.closest('.barra-accesibilidad-govco')) {
            hideAllTooltips();
            activeTooltip = null;
        }
    });
}

// Función para inicializar la navegación
function setupNavigation() {
    const items = document.querySelectorAll('.nav-item');

    items.forEach(item => {
        item.addEventListener('click', (e) => {
            handleIndicator(e.target);
        });

        if (item.classList.contains('is-active')) {
            handleIndicator(item);
        }
    });
}

// Función para restaurar las preferencias del usuario
function restoreUserPreferences() {
    // Restaurar modo oscuro
    const savedDarkMode = localStorage.getItem('darkMode');
    if (savedDarkMode === 'true') {
        document.body.classList.add('modo_oscuro-govco');
        const textElements = document.querySelectorAll('h1, h2, h3, h4, h5, h6, p, span:not(.barra-accesibilidad-govco button span), div, label, input, textarea');
        textElements.forEach(element => {
            element.style.visibility = 'visible';
            element.style.opacity = '1';
            element.style.color = '#fff';
        });
    }

    // Restaurar tamaño de fuente
    const savedFontSize = localStorage.getItem('fontSize');
    if (savedFontSize) {
        currentFontSize = parseInt(savedFontSize);
        document.documentElement.style.fontSize = `${currentFontSize}px`;
    }
}

// Event Listener principal cuando el DOM está cargado
document.addEventListener('DOMContentLoaded', function () {
    // Restaurar preferencias del usuario
    restoreUserPreferences();

    // Configurar navegación
    setupNavigation();

    // Configurar tooltips de accesibilidad
    setupAccessibilityTooltips();

    // Establecer menú activo inicial
    setActiveMenuItem();

    // Asegurarse de que los tooltips estén ocultos al inicio
    hideAllTooltips();
});

// Event Listener para cuando la ventana se redimensiona
window.addEventListener('resize', function () {
    const activeItem = document.querySelector('.nav-item.is-active');
    if (activeItem) {
        handleIndicator(activeItem);
    }
});

// Event Listener para cuando se completa la carga de la página
window.addEventListener('load', function () {
    setActiveMenuItem();
    // Asegurarse de que los tooltips estén ocultos al cargar la página
    hideAllTooltips();
});