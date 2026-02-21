using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EimmyTool.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string Name { get; set; } = "";
        public string DNI { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
        public int IsAdmin { get; set; } // 0 o 1
        public int IsActive { get; set; } // 0 o 1

        // Propiedad calculada para mostrar texto en lugar de números en la UI
        public string Role => IsAdmin == 1 ? "Administrador" : "Usuario";
        public string Status => IsActive == 1 ? "Activo" : "Inactivo";
    }
}