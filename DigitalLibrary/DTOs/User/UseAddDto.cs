using System.ComponentModel.DataAnnotations;

namespace DigitalLibrary.DTOs.User
{
    public class UserAddDto
    {
        public string Email{ get; set; }
        public string Password{ get; set; }
        public string Name{ get; set; }
        public string Phone{ get; set; }
        public string Class { get; set; }
        public string RoldId{ get; set; }
    }

}
