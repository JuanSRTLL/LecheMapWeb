# 🔧 Guía de Configuración del Sistema

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

⌨️ con ❤️ por [Equipo LECHEMAP](https://lechemap.gestionproyectoiot.com/) | 2024
