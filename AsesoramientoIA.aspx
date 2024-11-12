<%@ Page Title="Asesoramiento Inteligente en Producción Lechera" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AsesoramientoIA.aspx.cs" Inherits="LecheMap.AsesoramientoIA" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="https://code.highcharts.com/highcharts.js"></script>
    <link rel="stylesheet" href="resources/css/asesoramientoia.css" />
    <link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css">
    <link rel="stylesheet" href="https://unpkg.com/leaflet/dist/leaflet.css" />
    <script src="https://unpkg.com/leaflet/dist/leaflet.js"></script>
    <script src="resources/js/asesoramientoia.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="cia-container">
        <h1 class="cia-title">Asesoramiento Inteligente en Producción Lechera</h1>
        <div class="cia-description">
            <div class="welcome-text">
                <div class="welcome-image">
                    <img src="resources/image/contenido/vaca.webp" alt="Vaca" />
                    <div class="image-credits">Foto de Chris: <a href="https://www.pexels.com/es-es/foto/montanas-naturaleza-puesta-de-sol-nubes-9802904/" target="_blank">Pexels</a></div>
                </div>
                <div class="welcome-content">
                    <h2 class="welcome-heading">¡Bienvenido a un Futuro Sostenible!</h2>
                    <p>
                        Enfocados en la producción lechera en Colombia, te damos la bienvenida a nuestro innovador servicio de asesoramiento. 
                        Aquí, la inteligencia artificial se convierte en tu mejor aliada, ayudándote a evaluar la viabilidad de tus inversiones en el sector lechero.
                    </p>
                    <p>
                        Nuestro modelo de IA está diseñado para ofrecerte análisis precisos y recomendaciones personalizadas, adaptadas a las necesidades de tu región. 
                        Con nosotros, no solo obtendrás datos, sino también estrategias efectivas para maximizar tu producción y rentabilidad.
                    </p>
                    <p>
                        <strong>Prepárate para transformar tu enfoque hacia la producción lechera y alcanzar nuevas alturas de éxito!</strong>
                    </p>
                </div>
            </div>
        </div>

        <!-- Banner de datos oficiales -->
        <div class="banner">
            <p>El sistema usa los datos oficiales publicados en la plataforma de datos abiertos del gobierno.</p>
            <a href="https://www.datos.gov.co/Agricultura-y-Desarrollo-Rural/Zonificaci-n-de-aptitud-para-la-producci-n-de-lech/bfuy-8yvf/about_data" target="_blank">
                <img src="https://herramientas.datos.gov.co/sites/default/files/pictures/logo_pda.png" alt="Logo PDA" />
            </a>
        </div>

        <div class="row mb-4">
            <div class="col">
                <div id="topDepartmentsChart" style="height: 400px; margin: 20px auto;"></div>
            </div>
        </div>
        <div class="steps-container">
            <h2 class="steps-title">Pasos para realizar tu consulta</h2>
            <ul class="steps-list">
                <li class="step-item">
                    <span class="step-number">1</span>
                    <span class="step-text">Selecciona el departamento y municipio</span>
                </li>
                <li class="step-item">
                    <span class="step-number">2</span>
                    <span class="step-text">Haz clic en el botón "Iniciar"</span>
                </li>
                <li class="step-item">
                    <span class="step-number">3</span>
                    <span class="step-text">Interactúa con el asistente virtual y realiza tus consultas</span>
                </li>
            </ul>
        </div>

        <asp:Label ID="lblErrorMessage" runat="server" CssClass="error-message" Visible="false"></asp:Label>
        <asp:ScriptManager ID="ScriptManager1" runat="server" />

        <!-- Selección de departamento y municipio -->
        <div class="selection-container">
            <div class="row justify-content-center">
                <div class="col-md-6">
                    <div class="card shadow-sm">
                        <div class="card-body">
                            <h4 class="card-title text-center mb-4">Selecciona tu ubicación</h4>

                            <div class="form-group mb-4">
                                <label for="ddlDepartamentos" class="form-label">Departamento</label>
                                <asp:DropDownList ID="ddlDepartamentos" runat="server"
                                    CssClass="form-select form-control cia-dropdown"
                                    AutoPostBack="true"
                                    OnSelectedIndexChanged="ddlDepartamentos_SelectedIndexChanged">
                                    <asp:ListItem Text="-- Selecciona un departamento --" Value="" />
                                </asp:DropDownList>
                            </div>

                            <div class="form-group mb-4">
                                <label for="ddlMunicipios" class="form-label">Municipio</label>
                                <asp:DropDownList ID="ddlMunicipios" runat="server"
                                    CssClass="form-select form-control cia-dropdown">
                                    <asp:ListItem Text="-- Selecciona un municipio --" Value="" />
                                </asp:DropDownList>
                            </div>

                            <div class="text-center">
                                <asp:Button ID="btnIniciar" runat="server"
                                    Text="Comenzar Validación"
                                    CssClass="btn btn-primary cia-btn-iniciar w-100"
                                    OnClick="btnIniciar_Click"
                                    OnClientClick="return validateStartForm();"
                                    Enabled="true" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Contenedor del chat -->
        <asp:UpdatePanel ID="ChatUpdatePanel" runat="server">
            <ContentTemplate>
                <div class="cia-panel" id="chatPanel" runat="server" style="display: none;">
                    <h3 class="cia-subtitle"><span id="greeting" runat="server"></span></h3>
                    <div class="cia-chat-container" id="chatContainer" runat="server" role="log" aria-live="polite"></div>

                    <div class="cia-input-container">
                        <asp:TextBox ID="txtMessage" runat="server" CssClass="cia-input"
                            placeholder="Escribe tu mensaje aquí"
                            onkeyup="checkChatFormValidity()"
                            aria-label="Mensaje para consultar"
                            AutoComplete="off"
                            Enabled="false">
                        </asp:TextBox>
                        <asp:Button ID="btnSend" runat="server"
                            Text="Enviar"
                            CssClass="btn btn-success cia-btn-enviar"
                            OnClick="btnSend_Click"
                            OnClientClick="return validateChatForm();"
                            Enabled="false"
                            aria-label="Enviar mensaje" />
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>

        <!-- Tabla para mostrar los datos de aptitud -->
        <div id="aptitudDataContainer" runat="server" style="display: none;">
            <h2 class="steps-title">Datos de Aptitud</h2>
            <div class="table-responsive">
                <asp:GridView ID="gvAptitudData" runat="server"
                    AutoGenerateColumns="False"
                    CssClass="table"
                    OnRowDataBound="gvAptitudData_RowDataBound">
                    <Columns>
                        <asp:TemplateField HeaderText="Departamento">
                            <ItemTemplate>
                                <asp:Label runat="server" Text='<%# Eval("Departamento") %>' data-label="Departamento:"></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Municipio">
                            <ItemTemplate>
                                <asp:Label runat="server" Text='<%# Eval("Municipio") %>' data-label="Municipio:"></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Área Total">
                            <ItemTemplate>
                                <asp:Label runat="server" Text='<%# Eval("TotalArea") %>' data-label="Área Total:"></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Aptitud Alta">
                            <ItemTemplate>
                                <asp:Label runat="server" Text='<%# Eval("AptitudAlta") %>'
                                    data-label="Aptitud Alta:"
                                    CssClass="aptitud-alta"></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Aptitud Media">
                            <ItemTemplate>
                                <asp:Label runat="server" Text='<%# Eval("AptitudMedia") %>'
                                    data-label="Aptitud Media:"
                                    CssClass="aptitud-media"></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Aptitud Baja">
                            <ItemTemplate>
                                <asp:Label runat="server" Text='<%# Eval("AptitudBaja") %>'
                                    data-label="Aptitud Baja:"
                                    CssClass="aptitud-baja"></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Exclusión Legal">
                            <ItemTemplate>
                                <asp:Label runat="server" Text='<%# Eval("ExclusionLegal") %>'
                                    data-label="Exclusión Legal:"
                                    CssClass="exclusion-legal"></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="No Apta">
                            <ItemTemplate>
                                <asp:Label runat="server" Text='<%# Eval("NoApta") %>'
                                    data-label="No Apta:"
                                    CssClass="no-apta"></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            <div class="row mt-4">
                <div class="col-12">
                    <div id="pieChartContainer"></div>
                </div>
            </div>
            <div id="mapContainer" style="display: none;" class="mt-4">
                <div class="card">
                    <div class="card-header">
                        <h3 class="card-title">Mapa de Aptitud del Suelo</h3>
                    </div>
                    <div class="card-body">
                        <div id="map" style="height: 500px; width: 100%;"></div>
                        <div class="mt-3">
                            <div class="d-flex flex-wrap">
                                <div class="me-3 mb-2">
                                    <span style="background-color: #2ecc71; width: 20px; height: 20px; display: inline-block; margin-right: 5px;"></span>Apta/Aptitud alta
                                </div>
                                <div class="me-3 mb-2">
                                    <span style="background-color: #f1c40f; width: 20px; height: 20px; display: inline-block; margin-right: 5px;"></span>Aptitud media
                                </div>
                                <div class="me-3 mb-2">
                                    <span style="background-color: #e74c3c; width: 20px; height: 20px; display: inline-block; margin-right: 5px;"></span>Aptitud baja
                                </div>
                                <div class="me-3 mb-2">
                                    <span style="background-color: #8B4513; width: 20px; height: 20px; display: inline-block; margin-right: 5px;"></span>Exclusión legal
                                </div>
                                <div class="me-3 mb-2">
                                    <span style="background-color: #000000; width: 20px; height: 20px; display: inline-block; margin-right: 5px;"></span>No apta
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <asp:HiddenField ID="hdnChartData" runat="server" />
            <script type="text/javascript">
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
                                    color: '#2ecc71' // Verde
                                },
                                {
                                    name: 'Aptitud Media',
                                    y: parseFloat(data.aptitudMedia),
                                    color: '#f1c40f' // Amarillo
                                },
                                {
                                    name: 'Aptitud Baja',
                                    y: parseFloat(data.aptitudBaja),
                                    color: '#e74c3c' // Rojo
                                },
                                {
                                    name: 'Exclusión Legal',
                                    y: parseFloat(data.exclusionLegal),
                                    color: '#8B4513' // Café
                                },
                                {
                                    name: 'No Apta',
                                    y: parseFloat(data.noApta),
                                    color: '#000000' // Negro
                                }
                            ]
                        }]
                    });
                }
            </script>
        </div>
    </div>
    <div class="text-center mt-4" id="newConsultationContainer" style="display: none;">
        <p>Si deseas nuevamente empezar una nueva asesoría en otro departamento y municipio, presiona aquí:</p>
        <asp:Button ID="btnTerminar" runat="server"
            Text="Terminar Asesoría"
            CssClass="btn btn-danger"
            OnClientClick="return endConsultation();" />
    </div>
    <script type="text/javascript">
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
    </script>
    <script>
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

            // Crear el gráfico
            Highcharts.chart('topDepartmentsChart', {
                chart: {
                    type: 'column',
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
                        rotation: -45,
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
                    column: {
                        borderRadius: 5,
                        dataLabels: {
                            enabled: true
                        }
                    }
                }
            });
        }

        window.createTopDepartmentsChart = createTopDepartmentsChart;

        function endConsultation() {
            // Recargar la página completamente
            window.location.href = window.location.href.split('?')[0]; // Esto recarga la página sin parámetros de consulta
            return false; // Evitar el postback del botón
        }
    </script>
</asp:Content>