# 🔧 Guía de Configuración del Sistema

La web se ha modificado un poco solo en diseño dado que indicaron por correo electronico que podiamos seguir mejorando el entregable, sin embargo el repositorio esta igual para más transparencia, link de la pagina https://lechemap.gestionproyectoiot.com/

## ℹ️ Información para Jurados concurso datos a la U

**¡IMPORTANTE!** Para la evaluación del concurso de datos del MinTIC, por favor revisar el archivo `AsesoramientoIA.aspx.cs`. En este archivo encontrarán todas las URLs de consulta a la API de datos abiertos implementadas en el sistema.

## ⚙️ Configuración de Variables 

### Archivo Web.config

El sistema requiere la configuración de tres variables esenciales en el archivo `Web.config`. Estas variables son críticas para el funcionamiento correcto de las diferentes integraciones con APIs externas.

### Instrucciones de Configuración

1. Abra el archivo `Web.config` en su editor preferido
2. Localice las siguientes líneas:
3. Modificar las keys con unas propias para funcionamiento.

```xml
<appSettings>
   <add key="CohereApiKey" value="APIKEY DE COHERE"/>
   <add key="AppToken" value="TOKEN DE SOCRATA DATOS ABIERTOS MINTIC"/>
   <add key="BingApiKey" value="API DE BING PARA USO DE NOTICIAS"/>
</appSettings>
```
### Si se probara ejecutar el proyecto deben establecer a AsesoramientoIA.aspx como página de inicio dándole clic derecho Establecer como página de inicio.
⌨️Programado con ❤️ por [Equipo LECHEMAP](https://lechemap.gestionproyectoiot.com/) | 2024
