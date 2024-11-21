// Event listeners para carga de página
window.addEventListener('beforeunload', function () {
    document.getElementById('loading').style.display = 'flex';
});

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

// Función mejorada para cambiar el contraste
function cambiarContexto(event) {
    event.preventDefault();
    const body = document.body;
    body.classList.toggle('modo_oscuro-govco');

    // Lista de elementos para aplicar contraste
    const elements = {
        text: 'h1, h2, h3, h4, h5, h6, p, span:not(.barra-accesibilidad-govco button span), div, label, input, textarea',
        containers: '.cia-container, .ge-container, .cia-panel, .ge-panel, .selection-container, .steps-container',
        buttons: '.cia-btn-iniciar, .cia-btn-enviar, .btn',
        inputs: '.form-control, .cia-input, .cia-dropdown',
        steps: '.step-item, .step-number, .step-text',
        chat: '.cia-chat-container, .cia-input-container',
        tables: '.table, .table th, .table td',
        cards: '.card, .card-header, .card-body, .card-title'
    };

    const isDarkMode = body.classList.contains('modo_oscuro-govco');
    const darkModeStyles = {
        backgroundColor: '#1f1f1f',
        color: '#ffffff',
        borderColor: '#ffffff'
    };

    const lightModeStyles = {
        backgroundColor: '',
        color: '',
        borderColor: ''
    };

    // Aplicar estilos basados en el modo
    const currentStyles = isDarkMode ? darkModeStyles : lightModeStyles;

    // Aplicar estilos a elementos de texto
    document.querySelectorAll(elements.text).forEach(element => {
        if (!element.closest('.highcharts-container')) {
            element.style.color = currentStyles.color;
            element.style.visibility = 'visible';
            element.style.opacity = '1';
        }
    });

    // Aplicar estilos a contenedores
    document.querySelectorAll(elements.containers).forEach(container => {
        container.style.backgroundColor = currentStyles.backgroundColor;
        container.style.color = currentStyles.color;
        container.style.borderColor = currentStyles.borderColor;
    });

    // Aplicar estilos a botones
    document.querySelectorAll(elements.buttons).forEach(button => {
        if (isDarkMode) {
            button.style.backgroundColor = '#3498db';
            button.style.color = '#ffffff';
            button.style.borderColor = '#ffffff';
        } else {
            button.style.backgroundColor = '';
            button.style.color = '';
            button.style.borderColor = '';
        }
    });

    // Aplicar estilos a inputs
    document.querySelectorAll(elements.inputs).forEach(input => {
        if (isDarkMode) {
            input.style.backgroundColor = '#2c2c2c';
            input.style.color = '#ffffff';
            input.style.borderColor = '#ffffff';
        } else {
            input.style.backgroundColor = '';
            input.style.color = '';
            input.style.borderColor = '';
        }
    });

    // Aplicar estilos a los pasos
    document.querySelectorAll(elements.steps).forEach(step => {
        step.style.backgroundColor = currentStyles.backgroundColor;
        step.style.color = currentStyles.color;
        if (isDarkMode) {
            if (step.classList.contains('step-number')) {
                step.style.backgroundColor = '#3498db';
            }
        } else {
            if (step.classList.contains('step-number')) {
                step.style.backgroundColor = '';
            }
        }
    });

    // Aplicar estilos al chat
    document.querySelectorAll(elements.chat).forEach(chatElement => {
        chatElement.style.backgroundColor = currentStyles.backgroundColor;
        chatElement.style.borderColor = currentStyles.borderColor;
    });

    // Aplicar estilos a las tablas
    document.querySelectorAll(elements.tables).forEach(tableElement => {
        tableElement.style.backgroundColor = currentStyles.backgroundColor;
        tableElement.style.color = currentStyles.color;
        tableElement.style.borderColor = currentStyles.borderColor;
    });

    // Aplicar estilos a las cards
    document.querySelectorAll(elements.cards).forEach(card => {
        if (isDarkMode) {
            card.style.backgroundColor = '#2c2c2c';
            card.style.color = '#ffffff';
            card.style.borderColor = '#ffffff';
        } else {
            card.style.backgroundColor = '';
            card.style.color = '';
            card.style.borderColor = '';
        }
    });

    // Ajustar estilos de los gráficos de Highcharts
    const charts = Highcharts.charts.filter(chart => chart);
    charts.forEach(chart => {
        const theme = isDarkMode ? {
            backgroundColor: '#1f1f1f',
            textColor: '#ffffff',
            lineColor: '#ffffff'
        } : {
            backgroundColor: '#ffffff',
            textColor: '#333333',
            lineColor: '#cccccc'
        };

        chart.update({
            chart: {
                backgroundColor: theme.backgroundColor
            },
            title: {
                style: { color: theme.textColor }
            },
            xAxis: {
                labels: { style: { color: theme.textColor } },
                lineColor: theme.lineColor,
                gridLineColor: theme.lineColor
            },
            yAxis: {
                labels: { style: { color: theme.textColor } },
                lineColor: theme.lineColor,
                gridLineColor: theme.lineColor
            },
            legend: {
                itemStyle: { color: theme.textColor }
            }
        });
    });

    // Guardar preferencia en localStorage
    localStorage.setItem('darkMode', isDarkMode);
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

// Función para validar el formulario de inicio
function checkStartFormValidity() {
    const departamento = document.getElementById('ddlDepartamentos');
    const municipio = document.getElementById('ddlMunicipios');
    const btnIniciar = document.querySelector('.cia-btn-iniciar');

    if (departamento && municipio && btnIniciar) {
        btnIniciar.disabled = !departamento.value || !municipio.value;
        btnIniciar.style.opacity = btnIniciar.disabled ? '0.6' : '1';
    }
}

// Función para mostrar/ocultar elementos de carga
function toggleLoadingElements(show) {
    const loadingElements = document.querySelectorAll('.loading-indicator');
    loadingElements.forEach(element => {
        element.style.display = show ? 'block' : 'none';
    });
}

// Función para restaurar las preferencias del usuario
function restoreUserPreferences() {
    const savedDarkMode = localStorage.getItem('darkMode') === 'true';
    if (savedDarkMode) {
        document.body.classList.add('modo_oscuro-govco');
        cambiarContexto({ preventDefault: () => { } });
    }

    const savedFontSize = localStorage.getItem('fontSize');
    if (savedFontSize) {
        currentFontSize = parseInt(savedFontSize);
        document.documentElement.style.fontSize = `${currentFontSize}px`;
    }
}

// Inicialización cuando el DOM está listo
document.addEventListener('DOMContentLoaded', function () {
    restoreUserPreferences();
    setupAccessibilityTooltips();
    setupNavigation();
    checkStartFormValidity();

    // Configurar eventos para los dropdowns
    const departamentos = document.getElementById('ddlDepartamentos');
    const municipios = document.getElementById('ddlMunicipios');
    if (departamentos && municipios) {
        departamentos.addEventListener('change', checkStartFormValidity);
        municipios.addEventListener('change', checkStartFormValidity);
    }

    // Configurar el chat si está presente
    const chatContainer = document.getElementById('chatContainer');
    if (chatContainer) {
        setupChat();
    }
});

// Configuración de navegación
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

// Configuración de tooltips de accesibilidad
function setupAccessibilityTooltips() {
    const buttons = document.querySelectorAll('.barra-accesibilidad-govco button');
    buttons.forEach(button => {
        const span = button.querySelector('span');
        if (!span) return;

        button.addEventListener('mouseenter', () => {
            hideAllTooltips();
            span.style.visibility = 'visible';
            span.style.opacity = '1';
            span.style.display = 'block';
        });

        button.addEventListener('mouseleave', () => {
            span.style.visibility = 'hidden';
            span.style.opacity = '0';
            span.style.display = 'none';
        });

        button.addEventListener('focus', () => {
            hideAllTooltips();
            span.style.visibility = 'visible';
            span.style.opacity = '1';
            span.style.display = 'block';
        });

        button.addEventListener('blur', () => {
            span.style.visibility = 'hidden';
            span.style.opacity = '0';
            span.style.display = 'none';
        });
    });
}

// Configuración del chat
function setupChat() {
    const chatInput = document.getElementById('txtMessage');
    const sendButton = document.getElementById('btnSend');

    if (chatInput && sendButton) {
        chatInput.addEventListener('input', () => {
            sendButton.disabled = !chatInput.value.trim();
            sendButton.style.opacity = sendButton.disabled ? '0.6' : '1';
        });
    }
}

// Event listeners para la ventana
window.addEventListener('resize', function () {
    const activeItem = document.querySelector('.nav-item.is-active');
    if (activeItem) {
        handleIndicator(activeItem);
    }
});

window.addEventListener('load', function () {
    setActiveMenuItem();
    hideAllTooltips();
});