using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E1.Models
{
    [Table("Ventas")]
    public class Venta
    {
        [Key]
        public int id_venta { get; set; }

        public int id_cliente { get; set; }
        public int id_usuario { get; set; }
        public int id_caja { get; set; }
        public int id_estado_venta { get; set; }
        public int? id_cotizacion { get; set; }

        [Required]
        [StringLength(20)]
        public string numero_venta { get; set; } = "";

        [StringLength(20)]
        public string tipo_comprobante { get; set; } = "";

        [StringLength(20)]
        public string metodo_pago { get; set; } = "";

        [StringLength(10)]
        public string? tipo_documento_cli { get; set; }

        [StringLength(11)]
        public string? numero_documento_cli { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal subtotal { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal descuento { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal igv { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal total { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal monto_pagado { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal vuelto { get; set; } = 0;

        public DateTime fecha_venta { get; set; } = DateTime.Now;
        public DateTime fecha_creacion { get; set; } = DateTime.Now;
        public DateTime fecha_modificacion { get; set; } = DateTime.Now;

        // Navegación
        [ForeignKey("id_cliente")]
        public Cliente? Cliente { get; set; }

        [ForeignKey("id_estado_venta")]
        public EstadoVenta? EstadoVenta { get; set; }
    }
}
