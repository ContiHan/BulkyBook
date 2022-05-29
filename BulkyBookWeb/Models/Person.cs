using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BulkyBookWeb.Models
{
	public class Person
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int Id { get; set; }
		[Required]
		public string? Jmeno { get; set; }
		[Required]
		public string? Prijmeni { get; set; }
		[Required]
		public DateTime Date { get; set; }
	}
}
