using System;

namespace LecheMap
{
    public class AptitudData
    {
        public string departamen { get; set; }
        public string municipio { get; set; }
        public string total_area_ha { get; set; }
        public string aptitud_alta_ha { get; set; }
        public string aptitud_alta_pct { get; set; }
        public string aptitud_media_ha { get; set; }
        public string aptitud_media_pct { get; set; }
        public string aptitud_baja_ha { get; set; }
        public string aptitud_baja_pct { get; set; }
        public string exclusion_legal_ha { get; set; }
        public string exclusion_legal_pct { get; set; }
        public string no_apta_ha { get; set; }
        public string no_apta_pct { get; set; }
        public object the_geom { get; set; }
    }
}