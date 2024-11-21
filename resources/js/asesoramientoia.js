let map = null;

function initializeMap(data) {
    try {
        if (!data || !Array.isArray(data)) {
            console.error('Datos inválidos para el mapa');
            return;
        }

        if (map) {
            map.remove();
        }

        const mapContainer = document.getElementById('mapContainer');
        if (!mapContainer) {
            console.error('Contenedor del mapa no encontrado');
            return;
        }
        mapContainer.style.display = 'block';

        map = L.map('map', {
            minZoom: 5,
            maxZoom: 18
        });

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© OpenStreetMap contributors',
            maxZoom: 19
        }).addTo(map);

        let bounds = L.latLngBounds();
        let hasValidGeometry = false;

        function getColor(aptitud) {
            const colorMap = {
                'Apta': '#2ecc71',
                'Aptitud alta': '#2ecc71',
                'Aptitud media': '#f1c40f',
                'Aptitud baja': '#e74c3c',
                'Exclusión legal': '#8B4513',
                'No apta': '#000000'
            };
            return colorMap[aptitud] || '#95a5a6';
        }

        data.forEach(function (feature) {
            if (feature.the_geom?.coordinates?.[0]?.[0]) {
                const coordinates = feature.the_geom.coordinates[0][0].map(coord => [coord[1], coord[0]]);
                if (coordinates.length > 0) {
                    const polygon = L.polygon(coordinates, {
                        color: getColor(feature.aptitud),
                        weight: 2,
                        opacity: 0.8,
                        fillOpacity: 0.5
                    }).addTo(map);

                    polygon.bindPopup(`
                        <strong>Aptitud:</strong> ${feature.aptitud}<br>
                        ${feature.area ? `<strong>Área:</strong> ${feature.area} ha<br>` : ''}
                    `);

                    coordinates.forEach(coord => bounds.extend(coord));
                    hasValidGeometry = true;
                }
            }
        });

        if (hasValidGeometry) {
            map.fitBounds(bounds, { padding: [20, 20] });
        } else {
            map.setView([4.570868, -74.297333], 5);
        }

        L.control.scale({ imperial: false }).addTo(map);
        setTimeout(() => {
            map.invalidateSize();
        }, 100);

    } catch (err) {
        console.error('Error al inicializar el mapa:', err);
    }
}

function loadMapData(geoData) {
    if (!geoData) {
        console.error('No hay datos para cargar en el mapa');
        return;
    }
    initializeMap(geoData);
}

function createPieChart(data) {
    Highcharts.chart('pieChartContainer', {
        chart: {
            plotBackgroundColor: null,
            plotBorderWidth: null,
            plotShadow: false,
            type: 'pie'
        },
        title: {
            text: 'Distribución de Aptitud del Suelo',
            style: {
                fontSize: '18px',
                fontWeight: 'bold',
                color: '#333'
            }
        },
        tooltip: {
            pointFormat: '{series.name}: <b>{point.percentage:.1f}%</b>',
            style: {
                color: '#333'
            }
        },
        accessibility: {
            point: {
                valueSuffix: '%'
            }
        },
        plotOptions: {
            pie: {
                allowPointSelect: true,
                cursor: 'pointer',
                dataLabels: {
                    enabled: true,
                    format: '<b>{point.name}</b>: {point.percentage:.1f} %',
                    style: {
                        fontSize: '14px',
                        color: '#333'
                    }
                }
            }
        },
        series: [{
            name: 'Porcentaje',
            colorByPoint: true,
            data: [
                {
                    name: 'Aptitud Alta',
                    y: parseFloat(data.aptitudAlta),
                    color: '#2ecc71'
                },
                {
                    name: 'Aptitud Media',
                    y: parseFloat(data.aptitudMedia),
                    color: '#f1c40f'
                },
                {
                    name: 'Aptitud Baja',
                    y: parseFloat(data.aptitudBaja),
                    color: '#e74c3c'
                },
                {
                    name: 'Exclusión Legal',
                    y: parseFloat(data.exclusionLegal),
                    color: '#8B4513'
                },
                {
                    name: 'No Apta',
                    y: parseFloat(data.noApta),
                    color: '#000000'
                }
            ]
        }]
    });
}

function validateStartForm() {
    var departamento = document.getElementById('<%= ddlDepartamentos.ClientID %>').value;
    var municipio = document.getElementById('<%= ddlMunicipios.ClientID %>').value;

    if (!departamento || departamento === "") {
        showError('Por favor, selecciona un departamento.');
        return false;
    }

    if (!municipio || municipio === "") {
        showError('Por favor, selecciona un municipio.');
        return false;
    }

    return true;
}

function checkChatFormValidity() {
    var message = document.getElementById('<%= txtMessage.ClientID %>').value.trim();
    var btnSend = document.getElementById('<%= btnSend.ClientID %>');

    if (btnSend) {
        btnSend.disabled = message === "";
        btnSend.style.opacity = btnSend.disabled ? '0.6' : '1';
    }
}

function validateChatForm() {
    var message = document.getElementById('<%= txtMessage.ClientID %>').value.trim();

    if (!message) {
        showError('Por favor, ingresa un mensaje antes de enviar.');
        return false;
    }

    showLoadingMessage();
    return true;
}

function showError(message) {
    var errorLabel = document.getElementById('<%= lblErrorMessage.ClientID %>');
    errorLabel.innerHTML = message;
    errorLabel.style.display = 'block';
    setTimeout(() => {
        errorLabel.style.display = 'none';
    }, 5000);
}

function showLoadingMessage() {
    var chatContainer = document.getElementById('<%= chatContainer.ClientID %>');
    const loadingMessage = document.createElement('p');
    loadingMessage.className = 'loading-message';
    loadingMessage.textContent = 'Generando...';
    chatContainer.appendChild(loadingMessage);
    chatContainer.scrollTop = chatContainer.scrollHeight;
    return true;
}

function saveScrollPosition() {
    sessionStorage.setItem('scrollPosition', window.scrollY);
}

function restoreScrollPosition() {
    const savedPosition = sessionStorage.getItem('scrollPosition');
    if (savedPosition !== null) {
        window.scrollTo(0, parseInt(savedPosition));
        sessionStorage.removeItem('scrollPosition');
    }
}

document.addEventListener('DOMContentLoaded', function () {
    // Restaurar la posición del scroll cuando la página se carga
    restoreScrollPosition();

    // Agregar el evento para guardar la posición antes del postback
    const ddlDepartamentos = document.getElementById('<%= ddlDepartamentos.ClientID %>');
    if (ddlDepartamentos) {
        ddlDepartamentos.addEventListener('change', function () {
            saveScrollPosition();
        });
    }

    // Código existente del DOMContentLoaded
    checkStartFormValidity();

    // Configurar el saludo
    var now = new Date();
    var hour = now.getHours();
    var greetingText = hour < 12 ? 'Buenos días' :
        hour < 19 ? 'Buenas tardes' :
            'Buenas noches';

    var greetingElement = document.getElementById('greeting');
    if (greetingElement) {
        greetingElement.textContent = greetingText + ' 👋';
    }

    // Agregar eventos para validación
    var ddlMunicipios = document.getElementById('<%= ddlMunicipios.ClientID %>');
    var txtMessage = document.getElementById('<%= txtMessage.ClientID %>');

    if (ddlMunicipios) ddlMunicipios.addEventListener('change', checkStartFormValidity);
    if (txtMessage) txtMessage.addEventListener('input', checkChatFormValidity);
});

function createTopDepartmentsChart(data) {
    // Agrupar datos por departamento y sumar aptitud_alta_ha
    const departmentData = {};
    data.forEach(item => {
        const dept = item.departamen;
        const aptitudAlta = parseFloat(item.aptitud_alta_ha || 0);

        if (!departmentData[dept]) {
            departmentData[dept] = 0;
        }
        departmentData[dept] += aptitudAlta;
    });

    // Convertir a array y ordenar
    const sortedData = Object.entries(departmentData)
        .map(([dept, value]) => ({ name: dept, y: value }))
        .sort((a, b) => b.y - a.y)
        .slice(0, 10);

    // Crear el gráfico dependiendo del tamaño de la pantalla
    const isMobile = window.innerWidth <= 768; // Detectar si es móvil
    const chartType = isMobile ? 'bar' : 'column'; // Gráfico de barras para móviles, columnas para escritorio

    Highcharts.chart('topDepartmentsChart', {
        chart: {
            type: chartType, // Cambiar tipo de gráfico según el tamaño de pantalla
            backgroundColor: '#f9f9f9', // Fondo claro
            borderColor: '#ccc',
            borderWidth: 1,
            borderRadius: 5
        },
        title: {
            text: 'Top 10 Departamentos con Mayor Área de Aptitud Alta',
            style: {
                fontSize: '18px',
                fontWeight: 'bold',
                color: '#333'
            }
        },
        xAxis: {
            type: 'category',
            labels: {
                style: {
                    fontSize: '13px',
                    color: '#333'
                }
            }
        },
        yAxis: {
            title: {
                text: 'Hectáreas',
                style: {
                    color: '#333'
                }
            },
            labels: {
                formatter: function () {
                    return Highcharts.numberFormat(this.value, 0);
                }
            }
        },
        legend: {
            enabled: false
        },
        tooltip: {
            formatter: function () {
                return '<b>' + this.point.name + '</b><br/>' +
                    'Área de aptitud alta: ' + Highcharts.numberFormat(this.y, 0) + ' ha';
            }
        },
        series: [{
            name: 'Aptitud Alta',
            data: sortedData,
            color: '#2f7ed8',
            dataLabels: {
                enabled: true,
                format: '{point.y:.1f} ha', // Formato del texto
                style: {
                    color: '#000000', // Cambiar el color a negro
                    textOutline: 'none' // Sin contorno
                }
            }
        }],
        plotOptions: {
            [chartType]: {
                borderRadius: 5,
                dataLabels: {
                    enabled: true
                }
            }
        }
    });
}

window.createTopDepartmentsChart = createTopDepartmentsChart;

function createTopMunicipiosChart(data) {
    // Agrupar datos por municipio y sumar aptitud_alta_ha
    const municipioData = {};
    data.forEach(item => {
        const municipio = item.municipio;
        const aptitudAlta = parseFloat(item.aptitud_alta_ha || 0);

        if (!municipioData[municipio]) {
            municipioData[municipio] = 0;
        }
        municipioData[municipio] += aptitudAlta;
    });

    // Convertir a array y ordenar
    const sortedData = Object.entries(municipioData)
        .map(([municipio, value]) => ({ name: municipio, y: value }))
        .sort((a, b) => b.y - a.y)
        .slice(0, 10);

    // Crear el gráfico dependiendo del tamaño de la pantalla
    const isMobile = window.innerWidth <= 768; // Detectar si es móvil
    const chartType = isMobile ? 'bar' : 'column'; // Gráfico de barras para móviles, columnas para escritorio

    Highcharts.chart('topMunicipiosChart', {
        chart: {
            type: chartType, // Cambiar tipo de gráfico según el tamaño de pantalla
            backgroundColor: '#f9f9f9', // Fondo claro
            borderColor: '#ccc',
            borderWidth: 1,
            borderRadius: 5
        },
        title: {
            text: 'Top 10 Municipios con Mayor Área de Aptitud Alta',
            style: {
                fontSize: '18px',
                fontWeight: 'bold',
                color: '#333'
            }
        },
        xAxis: {
            type: 'category',
            labels: {
                style: {
                    fontSize: '13px',
                    color: '#333'
                }
            }
        },
        yAxis: {
            title: {
                text: 'Hectáreas',
                style: {
                    color: '#333'
                }
            },
            labels: {
                formatter: function () {
                    return Highcharts.numberFormat(this.value, 0);
                }
            }
        },
        legend: {
            enabled: false
        },
        tooltip: {
            formatter: function () {
                return '<b>' + this.point.name + '</b><br/>' +
                    'Área de aptitud alta: ' + Highcharts.numberFormat(this.y, 0) + ' ha';
            }
        },
        series: [{
            name: 'Aptitud Alta',
            data: sortedData,
            color: '#2f7ed8',
            dataLabels: {
                enabled: true,
                format: '{point.y:.1f} ha', // Formato del texto
                style: {
                    color: '#000000', // Cambiar el color a negro
                    textOutline: 'none' // Sin contorno
                }
            }
        }],
        plotOptions: {
            [chartType]: {
                borderRadius: 5,
                dataLabels: {
                    enabled: true
                }
            }
        }
    });
}

function endConsultation() {
    // Recargar la página completamente
    window.location.href = window.location.href.split('?')[0]; // Esto recarga la página sin parámetros de consulta
    return false; // Evitar el postback del botón
}